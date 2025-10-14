# ScoutVision Hybrid Solution - Quick Reference

## 🚀 Quick Start Commands

### Build and Run Web Application
```powershell
cd src\ScoutVision.Web
dotnet restore
dotnet build
dotnet run
```

Access at: `https://localhost:7001`

### Run Database Migrations
```powershell
cd src\ScoutVision.Infrastructure
dotnet ef database update --project ..\ScoutVision.Web
```

### Run Tests
```powershell
cd src\ScoutVision.Tests
dotnet test
```

## 📁 Project Structure

```
ScoutVision/
├── src/
│   ├── ScoutVision.Web/              # Main Blazor application
│   │   ├── Pages/
│   │   │   ├── HybridAnalytics.razor # Hybrid analytics UI
│   │   │   ├── SearchSimple.razor    # Player search
│   │   │   └── UserManual.razor      # User guide
│   │   ├── Services/
│   │   │   ├── HybridAnalyticsService.cs  # Core hybrid service
│   │   │   ├── LocalizationService.cs     # Multi-language
│   │   │   └── ThemeService.cs            # Theme management
│   │   └── Shared/
│   │       ├── NavMenu.razor         # Navigation
│   │       └── ThemeProvider.razor   # Theme provider
│   ├── ScoutVision.Core/             # Domain models & DTOs
│   ├── ScoutVision.Infrastructure/   # Data access
│   └── ScoutVision.API/              # REST API
├── docs/
│   ├── Hybrid-Architecture-Guide.md
│   ├── Hybrid-Solution-Deployment.md
│   ├── GMod-SDK-Integration.md
│   └── Hybrid-Solution-Implementation-Summary.md
├── gmod-addon/                       # GMod integration (future)
└── bridge-service/                   # Python/Node bridge (future)
```

## 🔑 Key Files Reference

### HybridAnalyticsService.cs
**Location**: `src/ScoutVision.Web/Services/HybridAnalyticsService.cs`

**Key Methods**:
```csharp
// Player Analysis
Task<PlayerAnalytics> GetPlayerAnalyticsAsync(int playerId)

// Match Analysis
Task<MatchAnalytics> GetMatchAnalyticsAsync(int matchId)

// Formation Analysis
Task<TeamFormationAnalysis> AnalyzeFormationAsync(int teamId, int matchId)

// GMod Session Management
Task<bool> StartGModSessionAsync(GModSessionConfig config)
Task<bool> StopGModSessionAsync(string sessionId)
Task<GModSessionStatus> GetGModSessionStatusAsync(string sessionId)

// Hybrid Workflows
Task<AnalysisSession> CreateAnalysisSessionAsync(AnalysisSessionRequest request)
Task<bool> SynchronizeWithGModAsync(string sessionId)
```

### HybridAnalytics.razor
**Location**: `src/ScoutVision.Web/Pages/HybridAnalytics.razor`

**Route**: `/hybrid-analytics`

**Key Features**:
- Mode selection (Web/GMod/Hybrid)
- Active session management
- Quick action buttons
- GMod connection status
- Session creation modal

### LocalizationService.cs
**Location**: `src/ScoutVision.Web/Services/LocalizationService.cs`

**Hybrid Analytics Keys**:
```csharp
"HybridAnalytics"            // "Hybrid Analytics"
"NewSession"                 // "New Session"
"HybridAnalyticsDescription" // Description text
"3DVisualization"            // "3D Visualization"
"SyncWithGMod"              // "Sync with GMod"
```

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ScoutVision;Trusted_Connection=True;"
  },
  "GMod": {
    "BridgeUrl": "http://localhost:8080",
    "EnableHybridMode": true,
    "AutoStartSessions": false,
    "TimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Program.cs - Register Services
```csharp
// Hybrid Analytics Service
builder.Services.AddScoped<IHybridAnalyticsService, HybridAnalyticsService>();

// HTTP Client for GMod communication
builder.Services.AddHttpClient<IHybridAnalyticsService, HybridAnalyticsService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Localization
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Theme Service
builder.Services.AddScoped<IThemeService, ThemeService>();
```

## 🎯 Common Tasks

### Create New Analysis Session
```csharp
var request = new AnalysisSessionRequest
{
    MatchId = 101,
    AnalysisType = "player",
    Include3DVisualization = true,
    VisualizationType = "Hybrid",
    Participants = new List<string> { "analyst@team.com" }
};

var session = await HybridAnalytics.CreateAnalysisSessionAsync(request);
```

### Get Player Analytics
```csharp
var analytics = await HybridAnalytics.GetPlayerAnalyticsAsync(playerId);

// Access data
var metrics = analytics.PerformanceMetrics;
var patterns = analytics.MovementPatterns;
var heatMap = analytics.HeatMapData;
```

### Start GMod Session
```csharp
var config = new GModSessionConfig
{
    MatchId = 101,
    VisualizationType = "FullMatch",
    PlayerIds = new List<int> { 1, 2, 3 },
    AnalysisMode = "Formation"
};

var started = await HybridAnalytics.StartGModSessionAsync(config);
```

### Sync Data with GMod
```csharp
var success = await HybridAnalytics.SynchronizeWithGModAsync(sessionId);
```

## 🌍 Supported Languages

| Code | Language | Flag |
|------|----------|------|
| `en` | English | 🇺🇸 |
| `es` | Español | 🇪🇸 |
| `fr` | Français | 🇫🇷 |
| `de` | Deutsch | 🇩🇪 |
| `it` | Italiano | 🇮🇹 |
| `pt` | Português | 🇵🇹 |
| `ru` | Русский | 🇷🇺 |
| `ja` | 日本語 | 🇯🇵 |
| `ko` | 한국어 | 🇰🇷 |
| `zh` | 中文 | 🇨🇳 |
| `ar` | العربية | 🇸🇦 |
| `hi` | हिन्दी | 🇮🇳 |

## 🎨 Theme Management

### Get Current Theme
```csharp
@inject IThemeService ThemeService

var isDark = await ThemeService.GetIsDarkModeAsync();
```

### Toggle Theme
```csharp
await ThemeService.ToggleThemeAsync();
```

## 🧪 Testing

### Unit Test Example
```csharp
[Fact]
public async Task CreateAnalysisSession_ShouldReturnSession()
{
    // Arrange
    var service = new HybridAnalyticsService(logger, httpClient);
    var request = new AnalysisSessionRequest { MatchId = 1 };
    
    // Act
    var session = await service.CreateAnalysisSessionAsync(request);
    
    // Assert
    Assert.NotNull(session);
    Assert.Equal(1, session.MatchId);
}
```

## 🐛 Debugging

### Enable Detailed Logging
```json
{
  "Logging": {
    "LogLevel": {
      "ScoutVision.Web.Services": "Debug",
      "Default": "Information"
    }
  }
}
```

### Check GMod Connection
```csharp
var status = await HybridAnalytics.GetGModSessionStatusAsync("test");
Console.WriteLine($"GMod Status: {status.Status}");
```

## 📊 Data Models

### AnalysisSession
```csharp
public class AnalysisSession
{
    public string SessionId { get; set; }
    public int MatchId { get; set; }
    public string AnalysisType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public List<string> Participants { get; set; }
    public bool Include3DVisualization { get; set; }
    public string? GModSessionId { get; set; }
    public object? WebAnalyticsData { get; set; }
}
```

### PlayerAnalytics
```csharp
public class PlayerAnalytics
{
    public int PlayerId { get; set; }
    public PerformanceMetrics PerformanceMetrics { get; set; }
    public List<MovementPattern> MovementPatterns { get; set; }
    public List<HeatMapPoint> HeatMapData { get; set; }
    public TacticalPositioning TacticalPositioning { get; set; }
    public ComparisonData ComparisonData { get; set; }
    public List<ImprovementSuggestion> ImprovementSuggestions { get; set; }
}
```

### GModSessionConfig
```csharp
public class GModSessionConfig
{
    public string SessionId { get; set; }
    public int MatchId { get; set; }
    public string VisualizationType { get; set; }
    public List<int> PlayerIds { get; set; }
    public TimeRange TimeRange { get; set; }
    public string AnalysisMode { get; set; }
}
```

## 🔗 Useful Links

- **Web Interface**: https://localhost:7001
- **API Docs**: https://localhost:7000/swagger
- **GMod Bridge**: http://localhost:8080
- **GitHub Repo**: https://github.com/Debalent/ScoutVision

## 💡 Tips & Tricks

### Performance
- Use async/await for all I/O operations
- Enable response caching for static data
- Implement pagination for large result sets
- Cache frequently accessed translations

### Security
- Always validate user input
- Use HTTPS in production
- Implement rate limiting
- Enable CORS properly
- Use API keys for GMod communication

### Best Practices
- Follow C# naming conventions
- Write unit tests for services
- Document public APIs with XML comments
- Use dependency injection
- Handle exceptions gracefully

## 🆘 Common Issues

### Build Errors
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Database Connection Issues
```powershell
# Update connection string in appsettings.json
# Run migrations
dotnet ef database update
```

### GMod Connection Failed
- Check bridge service is running on port 8080
- Verify firewall settings
- Confirm GMod addon is loaded
- Review application logs

## 📞 Support

- **Documentation**: `/docs` folder
- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions

---

**Last Updated**: October 13, 2025  
**Version**: 1.0.0  
**Status**: Beta