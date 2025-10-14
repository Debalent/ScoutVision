# ScoutVision Hybrid Solution - Deployment Guide

## Overview

This guide walks you through deploying the complete ScoutVision hybrid solution, combining web-based analytics with GMod 3D visualization capabilities.

## Prerequisites

### Web Component Requirements

- .NET 8.0 SDK or later
- SQL Server 2019+ or PostgreSQL 13+
- IIS or Kestrel web server
- 4GB RAM minimum (8GB recommended)
- Modern web browser (Chrome, Edge, Firefox)

### GMod Component Requirements

- Garry's Mod (Steam)
- Source SDK Base 2013 Multiplayer
- 8GB RAM minimum
- Graphics card with OpenGL 3.3+ support
- Windows 10/11 or Linux with Wine

### Network Requirements

- HTTP communication between components
- Open ports: 7000 (API), 7001 (Web), 8080 (GMod Bridge)
- Local network or VPN for collaborative sessions

## Installation Steps

### Phase 1: Web Application Setup

#### 1. Clone Repository

```powershell
git clone https://github.com/Debalent/ScoutVision.git
cd ScoutVision
```

#### 2. Configure Database

Update `appsettings.json` in ScoutVision.Web:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ScoutVision;Trusted_Connection=True;"
  },
  "GMod": {
    "BridgeUrl": "http://localhost:8080",
    "EnableHybridMode": true,
    "AutoStartSessions": false
  }
}
```

#### 3. Run Migrations

```powershell
cd src/ScoutVision.Infrastructure
dotnet ef database update --project ../ScoutVision.Web
```

#### 4. Build and Run Web Application

```powershell
cd ../ScoutVision.Web
dotnet build
dotnet run
```

Access at: `https://localhost:7001`

### Phase 2: GMod Integration Setup

#### 1. Install GMod Addon

Copy the addon to your GMod addons folder:

```powershell
# Windows
xcopy /E /I gmod-addon "%USERPROFILE%\Steam\steamapps\common\GarrysMod\garrysmod\addons\scoutvision"

# Linux
cp -r gmod-addon ~/.steam/steam/steamapps/common/GarrysMod/garrysmod/addons/scoutvision
```

#### 2. Configure GMod Addon

Edit `garrysmod/addons/scoutvision/lua/autorun/server/sv_config.lua`:

```lua
ScoutVision.Config = {
    APIEndpoint = "http://localhost:7000/api",
    APIKey = "your-api-key-here",
    EnableHTTP = true,
    EnableWebSocket = true,
    AutoConnect = true,
    LogLevel = "INFO"
}
```

#### 3. Start GMod Server

Launch dedicated server or listen server with the addon loaded:

```powershell
# Windows
"C:\Program Files (x86)\Steam\steamapps\common\GarrysMod\srcds.exe" -console -game garrysmod +map gm_flatgrass +maxplayers 16

# Linux
./srcds_run -console -game garrysmod +map gm_flatgrass +maxplayers 16
```

#### 4. Verify Connection

In GMod console, run:

```lua
lua_run_cl print(ScoutVision.ConnectionStatus())
```

Should return: `Connected to ScoutVision API`

### Phase 3: Bridge Service Setup

#### 1. Install Bridge Dependencies

```powershell
cd bridge-service
pip install -r requirements.txt
```

#### 2. Configure Bridge

Edit `bridge-service/config.yaml`:

```yaml
server:
  host: "0.0.0.0"
  port: 8080
  
web_api:
  url: "http://localhost:7000"
  timeout: 30
  
gmod:
  ports: [27015, 27016, 27017]
  protocol: "http"
  
security:
  api_key_required: true
  allowed_origins: ["http://localhost:7001"]
```

#### 3. Start Bridge Service

```powershell
python bridge-service/main.py
```

Verify at: `http://localhost:8080/health`

## Configuration

### Web Application Settings

#### Enable Hybrid Mode

In `ScoutVision.Web/Program.cs`, register the hybrid service:

```csharp
builder.Services.AddScoped<IHybridAnalyticsService, HybridAnalyticsService>();
builder.Services.AddHttpClient<IHybridAnalyticsService, HybridAnalyticsService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

#### Localization for Hybrid Features

Add to `LocalizationService.cs`:

```csharp
["HybridAnalytics"] = "Hybrid Analytics",
["NewSession"] = "New Session",
["HybridAnalyticsDescription"] = "Combine web-based analytics with 3D GMod visualization",
["3DVisualization"] = "3D Visualization",
["SyncWithGMod"] = "Sync with GMod"
```

### GMod Addon Configuration

#### Network Settings

Edit `lua/autorun/server/sv_network.lua`:

```lua
-- HTTP Configuration
http.Fetch(ScoutVision.Config.APIEndpoint .. "/health",
    function(body) 
        print("[ScoutVision] Connected to API")
    end,
    function(error)
        print("[ScoutVision] Connection failed: " .. error)
    end
)

-- WebSocket for real-time updates
ScoutVision.WebSocket = WebSocket.create(
    ScoutVision.Config.APIEndpoint:gsub("http", "ws") .. "/ws"
)
```

#### Visualization Settings

Edit `lua/scoutvision/client/cl_visualizer.lua`:

```lua
ScoutVision.Visualization = {
    PlayerScale = 1.0,
    TrailDuration = 5.0,
    HeatMapResolution = 64,
    CameraSpeed = 2.0,
    EnableEffects = true
}
```

## Usage Workflows

### Basic Hybrid Session

1. **Web Interface**: Navigate to `/hybrid-analytics`
2. **Select Mode**: Choose "Hybrid Mode"
3. **Create Session**: Click "New Session" and configure:
   - Match ID
   - Analysis type
   - Enable 3D visualization
4. **Launch GMod**: Session automatically connects to GMod
5. **Collaborate**: Multiple analysts can join
6. **Export Results**: Save findings back to web database

### Advanced Workflows

#### Player Performance Analysis

```csharp
// Web API Call
var playerAnalytics = await HybridAnalytics.GetPlayerAnalyticsAsync(playerId);

// Start 3D visualization
var gmodConfig = new GModSessionConfig
{
    MatchId = matchId,
    VisualizationType = "PlayerFocus",
    PlayerIds = new List<int> { playerId }
};
await HybridAnalytics.StartGModSessionAsync(gmodConfig);
```

#### Formation Study

```csharp
// Analyze formation
var formationAnalysis = await HybridAnalytics.AnalyzeFormationAsync(teamId, matchId);

// Visualize in 3D
var transitions = await HybridAnalytics.GetFormationTransitionsAsync(matchId);
await HybridAnalytics.SendDataToGModAsync(sessionId, transitions);
```

## Troubleshooting

### Web Application Issues

**Problem**: Cannot connect to GMod
**Solution**: 
- Verify bridge service is running on port 8080
- Check firewall settings
- Confirm GMod addon is loaded

**Problem**: Session creation fails
**Solution**:
- Check database connection
- Verify all required services are running
- Review application logs

### GMod Integration Issues

**Problem**: Addon not loading
**Solution**:
- Verify addon is in correct directory
- Check `addon.json` is valid
- Enable developer mode in GMod

**Problem**: No data visualization
**Solution**:
- Check network connectivity
- Verify API endpoint configuration
- Review GMod console for errors

### Bridge Service Issues

**Problem**: Connection timeouts
**Solution**:
- Increase timeout values in config
- Check network latency
- Verify both endpoints are accessible

## Performance Optimization

### Web Application

- Enable response caching
- Use CDN for static assets
- Implement connection pooling
- Optimize database queries

### GMod Visualization

- Limit player trail history
- Reduce heat map resolution for lower-end systems
- Disable expensive effects
- Use LOD (Level of Detail) for distant objects

### Bridge Service

- Implement request queuing
- Use async processing
- Cache frequently accessed data
- Enable compression for API responses

## Security Considerations

### API Security

- Use HTTPS in production
- Implement API key authentication
- Rate limit requests
- Validate all inputs

### GMod Security

- Restrict server access
- Use Steam authentication
- Validate data from web API
- Implement anti-cheat measures

### Bridge Security

- Use secure WebSocket connections (WSS)
- Implement CORS properly
- Encrypt sensitive data
- Log all access attempts

## Monitoring and Maintenance

### Health Checks

Implement monitoring endpoints:

- Web: `https://localhost:7001/health`
- Bridge: `http://localhost:8080/health`
- GMod: Console command `scoutvision_status`

### Logging

Configure comprehensive logging:

```csharp
// Web application
builder.Logging.AddFile("logs/scoutvision-{Date}.log");

// Bridge service
logging:
  level: INFO
  file: logs/bridge-{date}.log
```

### Backup Strategy

- Database: Daily automated backups
- Configuration: Version controlled
- Session data: Retained for 30 days
- User data: GDPR compliant retention

## Support and Resources

- **Documentation**: `/docs` directory
- **API Reference**: `https://localhost:7001/swagger`
- **GMod Lua API**: `/gmod-addon/docs/api.md`
- **Community**: GitHub Discussions
- **Issues**: GitHub Issues tracker

## Conclusion

The ScoutVision hybrid solution provides a powerful platform for sports analytics, combining the accessibility of web applications with the immersive power of 3D visualization. Follow this guide carefully for successful deployment, and refer to the troubleshooting section if you encounter issues.

For production deployment, ensure all security measures are implemented and performance optimization is completed.