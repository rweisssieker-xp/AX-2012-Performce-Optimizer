# Technology Stack - AX 2012 Performance Optimizer

## Project Classification

- **Project Type**: Desktop Application
- **Framework**: .NET 8 WPF
- **Architecture Pattern**: 3-Layer Architecture (Presentation → Business Logic → Data Access)
- **Repository Type**: Monolith

## Technology Stack Table

| Category | Technology | Version | Justification |
|----------|-----------|---------|---------------|
| **Runtime** | .NET | 8.0 | Modern, performant, cross-platform foundation |
| **UI Framework** | WPF (Windows Presentation Foundation) | Built-in | Native Windows desktop UI framework |
| **Language** | C# | Latest (12.0) | Primary development language |
| **MVVM Framework** | CommunityToolkit.Mvvm | 8.3.2 | MVVM helpers (ObservableProperty, RelayCommand) |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection | 9.0.10 | IoC container for service registration |
| **Hosting** | Microsoft.Extensions.Hosting | 9.0.10 | Application lifetime management |
| **Charts/Visualization** | LiveChartsCore.SkiaSharpView.WPF | 2.0.0-rc4.3 | Modern charting library for WPF |
| **XAML Behaviors** | Microsoft.Xaml.Behaviors.Wpf | 1.1.122 | XAML behaviors for UI interactions |
| **HTTP Client** | Microsoft.Extensions.Http | 9.0.10 | HTTP client factory for AI services |
| **SQL Server** | Microsoft.Data.SqlClient | 5.2.2 | SQL Server connectivity (DMV queries) |
| **SQLite** | Microsoft.Data.Sqlite | 8.0.0 | Local data storage (if needed) |
| **Logging** | Microsoft.Extensions.Logging.Abstractions | 8.2.0 | Logging abstractions |
| **Security** | System.Security.Cryptography.ProtectedData | 8.0.0 | DPAPI encryption for credentials |
| **Build System** | MSBuild | Built-in | .NET SDK build system |
| **IDE** | Visual Studio 2022 | 17.8+ | Primary development environment |

## Architecture Pattern

**3-Layer Clean Architecture:**

```
┌─────────────────────────────────────────┐
│  WpfApp (Presentation Layer)             │
│  • ViewModels (MVVM)                     │
│  • Views (XAML)                          │
│  • Dependency Injection Setup            │
└─────────────────────────────────────────┘
              ↓ depends on
┌─────────────────────────────────────────┐
│  Core (Business Logic Layer)             │
│  • Models                                │
│  • Service Interfaces                    │
│  • Service Implementations               │
│  • Recommendation Engine                 │
│  • AI Services                           │
└─────────────────────────────────────────┘
              ↓ depends on
┌─────────────────────────────────────────┐
│  Data (Data Access Layer)                │
│  • SQL Connection Manager                │
│  • AX Connector Service                  │
│  • Configuration Service                 │
└─────────────────────────────────────────┘
```

## Project Structure

- **AX2012PerformanceOptimizer.WpfApp**: WPF application (UI layer)
- **AX2012PerformanceOptimizer.Core**: Business logic and services
- **AX2012PerformanceOptimizer.Data**: Data access and external integrations

## Key Architectural Decisions

1. **MVVM Pattern**: Separation of concerns between View and ViewModel
2. **Dependency Injection**: Service registration via Microsoft.Extensions.DependencyInjection
3. **Interface-Based Design**: All services expose interfaces for testability
4. **Async/Await**: Asynchronous operations throughout for UI responsiveness
5. **Read-Only Operations**: All database operations are read-only for safety

