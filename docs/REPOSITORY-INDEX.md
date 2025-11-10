# ScoutVision Repository Index

## 📁 **Core Structure**

### **Source Code (`src/`)**
```
src/
├── ScoutVision.Core/           # Domain entities and business logic
│   ├── Entities/              # Player, Team, Match, Performance models
│   ├── Enums/                 # Position, InjuryType, InjurySeverity
│   └── Interfaces/            # Service contracts
├── ScoutVision.Infrastructure/ # Data access and external services
│   ├── Data/                  # Entity Framework DbContext
│   ├── Repositories/          # Data access patterns
│   └── Services/              # External service integrations
├── ScoutVision.API/           # REST API endpoints
│   ├── Controllers/           # API controllers
│   ├── DTOs/                  # Data transfer objects
│   └── Middleware/            # Custom middleware
├── ScoutVision.Web/           # Blazor Server frontend
│   ├── Pages/                 # Razor pages and components
│   ├── Services/              # Frontend services
│   └── Shared/                # Shared components
└── ScoutVision.AI/            # Python AI/ML services
    ├── models/                # ML model definitions
    ├── services/              # AI service implementations
    └── tests/                 # Python test suite
```

### **Tests (`tests/`)**
```
tests/
├── ScoutVision.Tests/         # Unit tests for .NET components
├── ScoutVision.IntegrationTests/ # Integration tests
└── ScoutVision.AI/tests/      # Python AI service tests
```

### **Documentation (`docs/`)**
```
docs/
├── Architecture-Diagrams.md   # System architecture
├── Hybrid-Architecture-Guide.md # Hybrid solution guide
├── API-Documentation.md       # API endpoints
├── Deployment-Guide.md        # Deployment instructions
└── Development-Guide.md       # Development setup
```

## 🔍 **Key Files Index**

### **Configuration Files**
| File | Purpose | Location |
|------|---------|----------|
| `ScoutVision.sln` | Solution file | Root |
| `appsettings.json` | App configuration | `src/ScoutVision.*/` |
| `requirements.txt` | Python dependencies | `src/ScoutVision.AI/` |
| `Dockerfile` | Container definitions | `src/ScoutVision.*/` |
| `docker-compose.yml` | Multi-container setup | Root |

### **Core Entities**
| Entity | File | Purpose |
|--------|------|---------|
| `Player` | `src/ScoutVision.Core/Entities/Player.cs` | Player data model |
| `Team` | `src/ScoutVision.Core/Entities/Team.cs` | Team information |
| `Match` | `src/ScoutVision.Core/Entities/Match.cs` | Match data |
| `Performance` | `src/ScoutVision.Core/Entities/Performance.cs` | Player performance metrics |
| `InjuryReport` | `src/ScoutVision.Core/Entities/InjuryReport.cs` | Injury tracking |

### **API Endpoints**
| Endpoint | Controller | Purpose |
|----------|------------|---------|
| `/api/players` | `PlayersController` | Player management |
| `/api/teams` | `TeamsController` | Team operations |
| `/api/matches` | `MatchesController` | Match data |
| `/api/analytics` | `AnalyticsController` | Performance analytics |
| `/ai/analyze` | AI Service | ML analysis |

## 🚀 **Quick Navigation**

### **Getting Started**
1. [Installation Guide](../README.md#installation)
2. [Development Setup](Development-Guide.md)
3. [API Documentation](API-Documentation.md)

### **Architecture**
1. [System Overview](Architecture-Diagrams.md)
2. [Database Schema](Database-Schema.md)
3. [AI/ML Pipeline](AI-Architecture.md)

### **Deployment**
1. [Docker Deployment](Deployment-Guide.md#docker)
2. [Azure Deployment](Deployment-Guide.md#azure)
3. [CI/CD Pipeline](.github/workflows/ci-cd.yml)

## 🔧 **Development Workflows**

### **Adding New Features**
1. Create feature branch: `git checkout -b feature/new-feature`
2. Add entity to `ScoutVision.Core/Entities/`
3. Update `ApplicationDbContext` in `Infrastructure/Data/`
4. Create API controller in `ScoutVision.API/Controllers/`
5. Add Blazor page in `ScoutVision.Web/Pages/`
6. Write tests in `tests/ScoutVision.Tests/`

### **AI/ML Development**
1. Add model to `src/ScoutVision.AI/models/`
2. Create service in `src/ScoutVision.AI/services/`
3. Add endpoint to `ai_service.py`
4. Write tests in `src/ScoutVision.AI/tests/`

## 📊 **Metrics & Monitoring**

### **Code Quality**
- **Lines of Code**: ~15,000+ (estimated)
- **Test Coverage**: Target 80%+
- **Code Quality**: SonarQube integration ready

### **Performance Targets**
- **API Response**: < 100ms average
- **Page Load**: < 2s average
- **AI Processing**: < 5s per analysis

## 🔍 **Search Tips**

### **GitHub Search Queries**
```
# Find all player-related code
filename:Player language:csharp

# Find API controllers
path:Controllers filename:Controller

# Find test files
path:tests extension:cs

# Find Python AI code
path:ScoutVision.AI language:python

# Find configuration files
filename:appsettings.json OR filename:requirements.txt
```

### **IDE Navigation**
- **Go to Definition**: F12
- **Find All References**: Shift+F12
- **Search Solution**: Ctrl+Shift+F
- **Go to File**: Ctrl+T