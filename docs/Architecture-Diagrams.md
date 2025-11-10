# ScoutVision Hybrid Architecture - Visual Overview

## System Architecture Diagram

```text

┌─────────────────────────────────────────────────────────────────────────────┐
│                         ScoutVision Hybrid Platform                          │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                                USER LAYER                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐     │
│  │  Web Browser     │    │  GMod Client     │    │  Mobile App      │     │
│  │  (Chrome/Edge)   │    │  (3D View)       │    │  (Future)        │     │
│  └────────┬─────────┘    └────────┬─────────┘    └────────┬─────────┘     │
│           │                       │                        │               │
└───────────┼───────────────────────┼────────────────────────┼───────────────┘
            │                       │                        │
            │                       │                        │
┌───────────┼───────────────────────┼────────────────────────┼───────────────┐
│           │         PRESENTATION & VISUALIZATION LAYER     │               │
├───────────┼───────────────────────┼────────────────────────┼───────────────┤
│           ▼                       ▼                        ▼               │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌──────────────────┐  │
│  │  Blazor Web UI      │  │  GMod 3D Renderer   │  │  Mobile UI       │  │
│  │  ─────────────      │  │  ─────────────────  │  │  ─────────       │  │
│  │  • Dashboard        │  │  • 3D Visualization │  │  • Quick View    │  │
│  │  • Search           │  │  • Player Tracking  │  │  • Notifications │  │
│  │  • Analytics        │  │  • Heat Maps        │  │  • Reports       │  │
│  │  • Reports          │  │  • Formation View   │  │                  │  │
│  │  • User Manual      │  │  • Collaboration    │  │                  │  │
│  └─────────┬───────────┘  └─────────┬───────────┘  └────────┬─────────┘  │
│            │                        │                        │             │
└────────────┼────────────────────────┼────────────────────────┼─────────────┘
             │                        │                        │
             │                        │                        │
┌────────────┼────────────────────────┼────────────────────────┼─────────────┐
│            │           APPLICATION & BUSINESS LOGIC LAYER    │             │
├────────────┼────────────────────────┼────────────────────────┼─────────────┤
│            ▼                        │                        ▼             │
│  ┌──────────────────────────────────┼────────────────────────────────────┐ │
│  │  ScoutVision.Web (Blazor Server) │                                    │ │
│  │  ────────────────────────────────┴─────────────                       │ │
│  │                                                                        │ │
│  │  ┌──────────────────────────────────────────────────────────────┐    │ │
│  │  │  HybridAnalyticsService (Core Service)                       │    │ │
│  │  │  ──────────────────────────────────────                      │    │ │
│  │  │  • GetPlayerAnalyticsAsync()                                 │    │ │
│  │  │  • GetMatchAnalyticsAsync()                                  │    │ │
│  │  │  • AnalyzeFormationAsync()                                   │    │ │
│  │  │  • StartGModSessionAsync()                                   │    │ │
│  │  │  • SynchronizeWithGModAsync()                                │    │ │
│  │  │  • CreateAnalysisSessionAsync()                              │    │ │
│  │  └──────────────────────────────────────────────────────────────┘    │ │
│  │                                                                        │ │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐   │ │
│  │  │ Localization     │  │ Theme Service    │  │ Session Manager  │   │ │
│  │  │ Service          │  │                  │  │                  │   │ │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────┘   │ │
│  └────────────────────────────────┬───────────────────────────────────┘ │
│                                   │                                      │
│                                   ▼                                      │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  Bridge Service (Communication Hub)                             │    │
│  │  ───────────────────────────────────                            │    │
│  │  • HTTP Relay                                                   │    │
│  │  • WebSocket Management         ◄────────┐                      │    │
│  │  • Data Transformation                   │                      │    │
│  │  • Session Coordination                  │                      │    │
│  └─────────────────────────────────────────┬┘                      │    │
│                                             │                       │    │
└─────────────────────────────────────────────┼───────────────────────┼────┘
                                              │                       │
                                              │                       │
┌─────────────────────────────────────────────┼───────────────────────┼────┐
│                          DATA & INTEGRATION LAYER                   │    │
├─────────────────────────────────────────────┼───────────────────────┼────┤
│                                             ▼                       │    │
│  ┌────────────────────────────────────────────────────────────┐    │    │
│  │  ScoutVision.Core (Domain Models)                          │    │    │
│  │  ─────────────────────────────────                         │    │    │
│  │  • Player, Team, Match Entities                            │    │    │
│  │  • PlayerAnalytics, MatchAnalytics DTOs                    │    │    │
│  │  • AnalysisSession, GModSessionConfig                      │    │    │
│  └────────────────────────────────────────────────────────────┘    │    │
│                                                                     │    │
│  ┌────────────────────────────────────────────────────────────┐    │    │
│  │  ScoutVision.Infrastructure (Data Access)                  │    │    │
│  │  ────────────────────────────────────────                  │    │    │
│  │  • Entity Framework Core                                   │    │    │
│  │  • Repository Pattern                                      │    │    │
│  │  • Database Context                                        │    │    │
│  └─────────────────────────┬──────────────────────────────────┘    │    │
│                            │                                        │    │
└────────────────────────────┼────────────────────────────────────────┼────┘
                             │                                        │
                             ▼                                        │
┌─────────────────────────────────────────────────────────────────────┼────┐
│                          PERSISTENCE LAYER                          │    │
├─────────────────────────────────────────────────────────────────────┼────┤
│                                                                     │    │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐    │    │
│  │  SQL Server     │  │  Redis Cache    │  │  File Storage   │    │    │
│  │  (Primary DB)   │  │  (Sessions)     │  │  (Media)        │    │    │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘    │    │
│                                                                     │    │
└─────────────────────────────────────────────────────────────────────┼────┘
                                                                      │
                                                                      │
┌─────────────────────────────────────────────────────────────────────┼────┐
│                         EXTERNAL INTEGRATIONS                       │    │
├─────────────────────────────────────────────────────────────────────┼────┤
│                                                                     ▼    │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │  GMod Addon (Lua Scripts)                                          ││
│  │  ─────────────────────────                                         ││
│  │  Server-side:                           Client-side:               ││
│  │  • sv_init.lua                          • cl_init.lua              ││
│  │  • sv_network.lua                       • cl_visualizer.lua        ││
│  │  • sv_data_manager.lua                  • cl_ui.lua                ││
│  │  • sv_session_manager.lua               • cl_input.lua             ││
│  │                                                                     ││
│  │  Shared:                                                           ││
│  │  • sh_config.lua                                                   ││
│  │  • sh_utils.lua                                                    ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘

```text

## Data Flow Diagram

### Web Analytics Flow

```text

User ──┐
       ├─► Web Browser ──► Blazor Component ──► HybridAnalyticsService
       │                                              │
       │                                              ├─► Core Domain Models
       │                                              │
       │                                              ├─► Infrastructure Layer
       │                                              │
       │                                              ▼
       └◄─────────────────────────────────────── SQL Server
                    (Results displayed)

```text

### Hybrid Analytics Flow

```text

User ──► Web Browser ──► Create Hybrid Session
                                 │
                                 ├─► HybridAnalyticsService
                                 │      │
                                 │      ├─► Generate Web Analytics
                                 │      │
                                 │      └─► Start GMod Session
                                 │              │
                                 ▼              ▼
                        Bridge Service ◄─► GMod Server
                                 │              │
                                 │              ├─► Load 3D Data
                                 │              │
                                 │              └─► Render Visualization
                                 │
                                 ├─► Sync Data (Real-time)
                                 │
                                 └─► Combined Results
                                          │
User ◄───────────────────────────────────┘
       (View in Web + GMod simultaneously)

```text

### GMod Visualization Flow

```text

GMod Client ──► cl_init.lua ──► Request Data ──► HTTP Client
                                                      │
                                                      ▼
                                              Bridge Service
                                                      │
                                                      ▼
                                            ScoutVision API
                                                      │
                                                      ├─► Get Match Data
                                                      │
                                                      ├─► Get Player Positions
                                                      │
                                                      └─► Get Heat Maps
                                                           │
cl_visualizer.lua ◄─── Process Data ◄─── Return Results ─┘
         │
         ├─► Render 3D Players
         │
         ├─► Draw Movement Trails
         │
         ├─► Project Heat Maps
         │
         └─► Display Formations
                  │
         Player sees 3D Visualization

```text

## Component Interaction Matrix

```text

┌──────────────────┬──────────┬──────────┬──────────┬──────────┐
│    Component     │   Web    │  Bridge  │   GMod   │    DB    │
├──────────────────┼──────────┼──────────┼──────────┼──────────┤
│ Web Application  │    -     │   HTTP   │    -     │  Direct  │

├──────────────────┼──────────┼──────────┼──────────┼──────────┤
│ Bridge Service   │   HTTP   │    -     │   HTTP   │    -     │

├──────────────────┼──────────┼──────────┼──────────┼──────────┤
│ GMod Addon       │    -     │   HTTP   │    -     │    -     │

├──────────────────┼──────────┼──────────┼──────────┼──────────┤
│ Database         │  Direct  │    -     │    -     │    -     │

└──────────────────┴──────────┴──────────┴──────────┴──────────┘

```text

## Deployment Topology

### Development Environment

```text

┌────────────────────────────────────────┐
│         Developer Workstation           │
│  ┌───────────────────────────────────┐ │
│  │  IIS Express (Port 7001)          │ │
│  │  ├─ ScoutVision.Web               │ │
│  └───────────────────────────────────┘ │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │  SQL Server LocalDB               │ │
│  └───────────────────────────────────┘ │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │  Bridge Service (Port 8080)       │ │
│  └───────────────────────────────────┘ │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │  GMod Client/Server               │ │
│  └───────────────────────────────────┘ │
└────────────────────────────────────────┘

```text

### Production Environment

```text

┌──────────────────────────────────────────────────────────┐
│                    Load Balancer                          │
└─────────────┬────────────────────────────────┬───────────┘
              │                                │
┌─────────────┴────────────┐    ┌──────────────┴──────────┐
│   Web Server 1           │    │   Web Server 2          │
│   ─────────────          │    │   ─────────────         │
│   ScoutVision.Web        │    │   ScoutVision.Web       │
│   (IIS/Kestrel)          │    │   (IIS/Kestrel)         │
└─────────────┬────────────┘    └──────────────┬──────────┘
              │                                │
              └────────────┬───────────────────┘
                           │
              ┌────────────┴────────────┐
              │   Database Cluster      │
              │   ─────────────────     │
              │   SQL Server            │
              │   (High Availability)   │
              └────────────┬────────────┘
                           │
              ┌────────────┴────────────┐
              │   Bridge Service Pool   │
              │   ────────────────────  │
              │   Load Balanced         │
              └────────────┬────────────┘
                           │
              ┌────────────┴────────────┐
              │   GMod Server Farm      │
              │   ─────────────────     │
              │   Multiple Instances    │
              └─────────────────────────┘

```text

## Security Architecture

```text

┌─────────────────────────────────────────────────────────┐
│                    Security Layers                       │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Layer 1: Network Security                              │
│  ┌────────────────────────────────────────────────┐    │
│  │  • Firewall Rules                              │    │
│  │  • HTTPS/TLS Encryption                        │    │
│  │  • VPN for GMod Connections                    │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
│  Layer 2: Authentication & Authorization                │
│  ┌────────────────────────────────────────────────┐    │
│  │  • JWT Tokens                                  │    │
│  │  • Role-Based Access Control (RBAC)           │    │
│  │  • API Key Authentication                      │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
│  Layer 3: Application Security                          │
│  ┌────────────────────────────────────────────────┐    │
│  │  • Input Validation                            │    │
│  │  • SQL Injection Prevention                    │    │
│  │  • XSS Protection                              │    │
│  │  • CSRF Tokens                                 │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
│  Layer 4: Data Security                                 │
│  ┌────────────────────────────────────────────────┐    │
│  │  • Encrypted Data at Rest                      │    │
│  │  • Encrypted Data in Transit                   │    │
│  │  • Secure Key Management                       │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
│  Layer 5: Audit & Monitoring                            │
│  ┌────────────────────────────────────────────────┐    │
│  │  • Activity Logging                            │    │
│  │  • Intrusion Detection                         │    │
│  │  • Anomaly Detection                           │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘

```text

## Technology Stack Visualization

```text

┌─────────────────────────────────────────────────────────┐
│                  Frontend Technologies                   │
├─────────────────────────────────────────────────────────┤
│  Blazor Server │ Bootstrap 5 │ JavaScript │ SignalR    │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  Backend Technologies                    │
├─────────────────────────────────────────────────────────┤
│  .NET 8.0 │ C# 12 │ ASP.NET Core │ Entity Framework    │

└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  3D Visualization                        │
├─────────────────────────────────────────────────────────┤
│  GMod SDK │ Lua 5.1 │ Source Engine │ OpenGL           │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  Data & Storage                          │
├─────────────────────────────────────────────────────────┤
│  SQL Server │ Entity Framework │ Redis │ File Storage  │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  DevOps & Tools                          │
├─────────────────────────────────────────────────────────┤
│  Git │ Docker │ Azure DevOps │ GitHub Actions          │
└─────────────────────────────────────────────────────────┘

```text

---

**Document Version**: 1.0
**Last Updated**: October 13, 2025
**Status**: Current Architecture
