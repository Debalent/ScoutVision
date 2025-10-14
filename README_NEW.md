# 🎯 ScoutVision 

**Next-Generation AI-Powered Athletic Scouting Platform**

ScoutVision revolutionizes talent identification through advanced AI video analysis, predictive modeling, and comprehensive player profiling. Built for modern sports organizations that demand data-driven insights and collaborative scouting workflows.

[![Build Status](https://github.com/Debalent/ScoutVision/workflows/CI/badge.svg)](https://github.com/Debalent/ScoutVision/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Python 3.8+](https://img.shields.io/badge/Python-3.8+-green.svg)](https://www.python.org/)

## 🌟 Key Features

### 🎥 AI-Powered Video Analysis
- **Computer Vision**: Advanced motion tracking using MediaPipe and TensorFlow
- **Performance Metrics**: Real-time analysis of speed, agility, technique, and tactical awareness  
- **Highlight Detection**: Automatic identification of key moments and skills
- **Comparative Analysis**: Benchmark players against position averages and elite standards

### 🧠 Predictive Talent Modeling
- **Machine Learning**: Ensemble models predict professional success likelihood
- **Market Valuation**: AI-driven current and future market value projections
- **Career Trajectory**: Peak performance age and longevity predictions
- **Risk Assessment**: Injury susceptibility and career risk factors

### 👥 Mindset & Psychology Profiling  
- **Leadership Assessment**: Quantified leadership potential scoring
- **Resilience Testing**: Mental toughness and pressure handling evaluation
- **Team Chemistry**: Collaboration and communication style analysis
- **Development Planning**: Personalized psychological development recommendations

### 🚀 Modern Technology Stack
- **Backend**: ASP.NET Core 8.0 Web API with Entity Framework
- **Frontend**: Blazor Server with MudBlazor UI components
- **AI Services**: Python FastAPI with TensorFlow and scikit-learn
- **Database**: SQL Server with comprehensive relational design
- **Real-time**: SignalR for collaborative features

## 📋 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB
- [Python 3.8+](https://www.python.org/downloads/)
- [Node.js](https://nodejs.org/) (for build tools)

### 🚀 Installation

1. **Clone the repository**
```bash
git clone https://github.com/Debalent/ScoutVision.git
cd ScoutVision
```

2. **Setup the database**
```bash
cd src/ScoutVision.API
dotnet ef database update
```

3. **Start the AI service**
```bash
cd ../ScoutVision.AI
pip install -r requirements.txt
python ai_service.py
```

4. **Launch the application**
```bash
# Terminal 1 - API
cd src/ScoutVision.API
dotnet run

# Terminal 2 - Web UI  
cd ../ScoutVision.Web
dotnet run
```

5. **Access the application**
- Web UI: https://localhost:7001
- API Documentation: https://localhost:7000/swagger
- AI Service: http://localhost:8000/docs

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Blazor Web    │───▶│   ASP.NET API    │───▶│  SQL Server DB  │
│   (Frontend)    │    │    (Backend)     │    │   (Data Layer)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
          │                       │                       
          │                       ▼                       
          │            ┌──────────────────┐               
          └───────────▶│   Python AI      │               
                       │   Services       │               
                       └──────────────────┘               
```

### Project Structure
```
ScoutVision/
├── src/
│   ├── ScoutVision.Core/          # Domain models and entities
│   ├── ScoutVision.Infrastructure/ # Data access and EF Core
│   ├── ScoutVision.API/           # REST API endpoints
│   ├── ScoutVision.Web/           # Blazor Server UI
│   └── ScoutVision.AI/            # Python AI services
├── tests/                         # Unit and integration tests
├── docs/                          # Documentation
├── .github/workflows/             # CI/CD pipelines
└── assets/                        # Static assets and media
```

## 📊 Core Entities

### Player Profile
```csharp
public class Player : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Position { get; set; }
    public string CurrentTeam { get; set; }
    public ScoutingPriority Priority { get; set; }
    
    // Navigation properties
    public ICollection<VideoAnalysis> VideoAnalyses { get; set; }
    public ICollection<ScoutingReport> ScoutingReports { get; set; }
    public ICollection<TalentPrediction> TalentPredictions { get; set; }
    // ... more collections
}
```

### Video Analysis Results
```csharp
public class VideoAnalysis : BaseEntity
{
    public VideoAnalysisType Type { get; set; }
    public string VideoUrl { get; set; }
    public decimal? OverallScore { get; set; }
    public string TechnicalAnalysis { get; set; }
    public string MotionData { get; set; } // JSON
    public ICollection<VideoTimestamp> Timestamps { get; set; }
}
```

## 🔧 Configuration

### Database Connection
Update `appsettings.json` in the API project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ScoutVisionDB;Trusted_Connection=true"
  }
}
```

### AI Service Integration  
Configure AI service endpoints:
```json
{
  "AIServices": {
    "PythonServiceUrl": "http://localhost:8000",
    "VideoAnalysisEndpoint": "/api/analyze-video",
    "TalentPredictionEndpoint": "/api/predict-talent"
  }
}
```

## 📈 API Endpoints

### Players
- `GET /api/players` - List all players with pagination
- `GET /api/players/{id}` - Get detailed player profile
- `POST /api/players` - Create new player
- `PUT /api/players/{id}` - Update player information
- `DELETE /api/players/{id}` - Soft delete player

### Video Analysis
- `POST /api/video-analysis` - Submit video for AI analysis
- `GET /api/video-analysis/{playerId}` - Get player's video analyses
- `GET /api/video-analysis/{id}/results` - Get detailed analysis results

### Talent Predictions
- `POST /api/talent-predictions` - Generate talent prediction
- `GET /api/talent-predictions/{playerId}` - Get player predictions
- `GET /api/talent-predictions/batch` - Bulk prediction processing

## 🧪 Testing

Run the test suite:
```bash
# Unit tests
dotnet test tests/ScoutVision.Tests/

# Integration tests  
dotnet test tests/ScoutVision.IntegrationTests/

# AI service tests
cd src/ScoutVision.AI
pytest tests/
```

## 🚀 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up --build

# Or build individual services
docker build -t scoutvision-api ./src/ScoutVision.API
docker build -t scoutvision-ai ./src/ScoutVision.AI
```

### Azure Deployment
The application is configured for Azure App Service deployment with:
- Azure SQL Database
- Azure Container Instances for AI services
- Azure Blob Storage for video files
- Application Insights for monitoring

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Workflow
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏆 Roadmap

### Phase 1 - Foundation ✅
- [x] Core player management system
- [x] Basic video analysis pipeline  
- [x] RESTful API architecture
- [x] Modern Blazor UI

### Phase 2 - AI Enhancement 🚧
- [x] Advanced motion tracking
- [x] Talent prediction modeling
- [ ] Real-time video processing
- [ ] Multi-sport analysis support

### Phase 3 - Collaboration 📋
- [ ] Real-time scout collaboration
- [ ] Mobile companion app
- [ ] Advanced reporting system
- [ ] Integration marketplace

### Phase 4 - Enterprise 🎯
- [ ] Multi-tenant architecture
- [ ] Advanced analytics dashboard
- [ ] API marketplace
- [ ] White-label solutions

## 🆘 Support

- 📧 Email: support@scoutvision.com
- 💬 Discord: [ScoutVision Community](https://discord.gg/scoutvision)
- 📖 Documentation: [docs.scoutvision.com](https://docs.scoutvision.com)
- 🐛 Issues: [GitHub Issues](https://github.com/Debalent/ScoutVision/issues)

## ⭐ Acknowledgments

- Built with ❤️ by the ScoutVision team
- Special thanks to the sports analytics community
- Powered by open-source technologies

---

**Ready to revolutionize your scouting process? [Get Started Now](https://scoutvision.com/get-started) 🚀**