# Project Documentation Index - AX 2012 Performance Optimizer

## Project Overview

- **Type**: Monolith Desktop Application
- **Primary Language**: C#
- **Architecture**: 3-Layer Clean Architecture
- **Framework**: .NET 8 WPF

## Quick Reference

- **Tech Stack**: .NET 8, WPF, CommunityToolkit.Mvvm, LiveChartsCore
- **Entry Point**: `App.xaml.cs` (Application startup with DI)
- **Architecture Pattern**: 3-Layer (Presentation → Business Logic → Data Access)
- **Project Structure**: 3 projects (WpfApp, Core, Data)

## Generated Documentation

### Core Documentation
- [Project Overview](./project-overview.md) - Project summary and quick reference
- [Architecture](./architecture.md) - Complete architecture documentation
- [Technology Stack](./technology-stack.md) - Technology details and versions
- [Source Tree Analysis](./source-tree-analysis.md) - Complete directory structure

### Development Documentation
- [Development Guide](./development-guide.md) - Setup, build, and development workflow
- [State Management Patterns](./state-management-patterns.md) - MVVM and state management
- [UI Component Inventory](./ui-component-inventory.md) - UI components catalog

### Deployment Documentation
- [Deployment Guide](./deployment-guide.md) - Publishing and deployment instructions

## Existing Documentation

### User Documentation
- [README.md](../README.md) - Project overview and quick start
- [CORE_DOCUMENTATION.md](../CORE_DOCUMENTATION.md) - Complete core features documentation
- [AI_FEATURES.md](../AI_FEATURES.md) - AI-powered features guide
- [INNOVATIVE_FEATURES.md](../INNOVATIVE_FEATURES.md) - Innovative USP features documentation

### Configuration Files
- [LICENSE](../LICENSE) - MIT License

## Project Structure Summary

```
AX-2012-Performce-Optimizer/
├── AX2012PerformanceOptimizer.WpfApp/    # WPF Application (UI Layer)
│   ├── ViewModels/ (23 files)            # MVVM ViewModels
│   ├── Views/ (22 views)                 # XAML Views
│   ├── Dialogs/ (4 dialogs)              # Dialog windows
│   └── Converters/ (10 converters)       # Value converters
│
├── AX2012PerformanceOptimizer.Core/      # Business Logic Layer
│   ├── Models/ (30+ files)               # Domain models
│   └── Services/ (81 files)              # Business services
│
├── AX2012PerformanceOptimizer.Data/      # Data Access Layer
│   ├── SqlServer/                        # SQL Server connectivity
│   ├── AxConnector/                      # AX Business Connector
│   └── Configuration/                    # Configuration management
│
└── docs/                                 # Documentation
    ├── index.md                          # This file (master index)
    ├── architecture.md                   # Architecture documentation
    ├── technology-stack.md               # Technology stack
    └── [other documentation files]
```

## Key Features

### Core Monitoring (7 modules)
1. Dashboard - System health overview
2. SQL Performance - Query performance analysis
3. AOS Monitoring - AOS server health
4. Batch Jobs - Batch job monitoring
5. Database Health - Database metrics
6. Recommendations - Optimization suggestions
7. Settings - Application configuration

### AI Features (8 services)
1. Natural Language Query Assistant
2. AI Performance Insights Dashboard
3. Intelligent Query Rewriter
4. Query Auto-Fixer
5. Query Documentation Generator
6. AI Query Explainer
7. Performance Cost Calculator
8. Query Performance Forecasting

### Innovative Features (5 USP)
1. Performance DNA
2. Performance Crystal Ball
3. Performance Personas
4. Performance Time Machine
5. Performance Community

## Getting Started

### For Developers
1. Read [Development Guide](./development-guide.md)
2. Review [Architecture](./architecture.md)
3. Check [Technology Stack](./technology-stack.md)
4. Explore [Source Tree](./source-tree-analysis.md)

### For Users
1. Read [README.md](../README.md)
2. Review [CORE_DOCUMENTATION.md](../CORE_DOCUMENTATION.md)
3. Check [AI_FEATURES.md](../AI_FEATURES.md) for AI features
4. See [INNOVATIVE_FE_FEATURES.md](../INNOVATIVE_FEATURES.md) for innovative features

### For Deployment
1. Read [Deployment Guide](./deployment-guide.md)
2. Review build-and-run.ps1` for local
3. Use `publish-release.ps1` for publishing

## Architecture Overview

-Architecture Patterns

- **MVVM Pattern**: Model-View-ViewModel (MVVM)**
-
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Service-Oriented**: Business logic as services
- **Interface-Based Design**: All services expose interfaces

## Technology Highlights

- **.NET 8**: Modern Windows Presentation Foundation (WPF): UI framework
- **CommunityToolkit.Mvvm**: MVVM helpers**
- **LiveChartsCore**: Charting library
- **Microsoft.Data.SqlClient**: SQL Server connectivity
- **DPAPI Encryption**: Secure credential storage

## Next Steps

### Development

- **Brownfield PRD**: Reference this index when planning new features, reference this index
- **Architecture Reference**: Use**: `architecture.md` for system design
- **UI Features**: Reference `ui-component-inventory.md` for UI components
- **Development**: Reference `development-guide.md` for development workflow

## Documentation**: Reference `deployment-guide.md` for deployment

## Documentation Status

- ✅ Project Overview: Complete
- ✅ Architecture: Complete
- ✅ Technology Stack: Complete
- ✅ Source Tree: Complete
- ✅ Development Guide: Complete
- ✅ State Management: Complete
- ✅ UI Components: Complete
- ✅ Deployment Guide: Complete
- ✅ Master Index: Complete

---

**Last Updated**: 2025-12-02
**Workflow**: Document Project (Exhaustive Scan)
**Status**: Complete

