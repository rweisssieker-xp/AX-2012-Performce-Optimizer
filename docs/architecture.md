# Architecture Documentation - AX 2012 Performance Optimizer

## Executive Summary

The AX 2012 Performance Optimizer is a **native Windows WPF desktop application** built with .NET 8, following a **clean 3-layer architecture** pattern. It provides real-time performance monitoring and optimization recommendations for Microsoft Dynamics AX 2012 R3 CU13 systems.

## Architecture Pattern

**3-Layer Clean Architecture:**

```
┌─────────────────────────────────────────┐
│  Presentation Layer (WpfApp)            │
│  • ViewModels (MVVM)                    │
│  • Views (XAML)                         │
│  • Dependency Injection Setup           │
└─────────────────────────────────────────┘
              ↓ depends on
┌─────────────────────────────────────────┐
│  Business Logic Layer (Core)            │
│  • Models                                │
│  • Service Interfaces                    │
│  • Service Implementations               │
│  • Recommendation Engine                 │
│  • AI Services                           │
└─────────────────────────────────────────┘
              ↓ depends on
┌─────────────────────────────────────────┐
│  Data Access Layer (Data)                │
│  • SQL Connection Manager                │
│  • AX Connector Service                  │
│  • Configuration Service                 │
└─────────────────────────────────────────┘
              ↓ connects to
┌─────────────────────────────────────────┐
│  External Systems                       │
│  • SQL Server 2016 (DMVs)               │
│  • AX 2012 R3 CU13 (Business Connector) │
│  • OpenAI/Azure OpenAI (AI Features)    │
└─────────────────────────────────────────┘
```

## Technology Stack

See [Technology Stack](./technology-stack.md) for complete details.

**Key Technologies:**
- **.NET 8**: Runtime framework
- **WPF**: UI framework
- **CommunityToolkit.Mvvm**: MVVM helpers
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **LiveChartsCore**: Charting library
- **Microsoft.Data.SqlClient**: SQL Server connectivity

## Layer Responsibilities

### Presentation Layer (WpfApp)

**Responsibilities:**
- User interface rendering (XAML)
- User interaction handling
- Data presentation and visualization
- Navigation and routing

**Key Components:**
- 23 ViewModels (MVVM pattern)
- 22 Views (XAML)
- 4 Dialog windows
- 10 Value converters
- Dependency injection setup

**Patterns:**
- MVVM (Model-View-ViewModel)
- Command pattern (RelayCommand)
- Data binding (two-way and one-way)
- Dependency injection

### Business Logic Layer (Core)

**Responsibilities:**
- Business logic implementation
- Data processing and analysis
- Recommendation generation
- AI service integration
- Performance calculations

**Key Components:**
- 81 Service files (interfaces + implementations)
- 30+ Domain models
- Recommendation engine
- AI services (8 services)
- Innovative features (5 USP features)

**Patterns:**
- Service-oriented architecture
- Interface-based design
- Dependency injection
- Async/await patterns

### Data Access Layer (Data)

**Responsibilities:**
- Database connectivity
- SQL query execution
- AX Business Connector integration
- Configuration management
- Credential encryption

**Key Components:**
- SqlConnectionManager: SQL Server connectivity
- AxConnectorService: AX Business Connector
- ConfigurationService: Settings management
- ExecutionPlanService: SQL execution plans

**Patterns:**
- Repository pattern (implicit)
- Connection pooling
- Async data access
- Encrypted storage (DPAPI)

## Data Architecture

### Data Flow

```
External Systems (SQL Server, AX)
    ↓
Data Access Layer (Data)
    ↓
Business Logic Layer (Core Services)
    ↓
Presentation Layer (ViewModels)
    ↓
UI (Views)
```

### Data Models

**Core Models:**
- `SqlQueryMetric`: SQL query performance metrics
- `AosMetric`: AOS server metrics
- `BatchJobMetric`: Batch job metrics
- `DatabaseMetric`: Database health metrics
- `Recommendation`: Optimization recommendations
- `SystemHealthScore`: Overall system health

**AI Models:**
- `AiQueryAnalysisResult`: AI query analysis
- `PerformanceAnomaly`: Performance anomalies
- `PerformanceDNA`: Performance DNA analysis
- `PerformancePersona`: AI expert personas

### Data Storage

**Configuration Storage:**
- Location: `%LocalAppData%\AX2012PerformanceOptimizer\`
- Files: `profiles.json`, `ai-config.json`
- Encryption: Windows DPAPI for credentials
- Format: JSON

**Runtime Data:**
- In-memory only
- No persistent database
- Real-time queries to SQL Server DMVs

## Service Architecture

### Service Categories

1. **Monitoring Services**
   - SqlQueryMonitorService
   - AosMonitorService
   - BatchJobMonitorService
   - DatabaseStatsService
   - AifMonitorService
   - SsrsMonitorService

2. **Analysis Services**
   - RecommendationEngine
   - QueryAnalyzerService
   - TrendingService
   - AlertService

3. **AI Services**
   - AiQueryOptimizerService
   - AiQueryExplainerService
   - AiPerformanceInsightsService
   - NaturalLanguageQueryAssistant
   - IntelligentQueryRewriter
   - QueryAutoFixerService
   - QueryDocumentationService
   - PerformanceCostCalculatorService
   - QueryPerformanceForecastingService

4. **Innovative Features**
   - PerformanceDNAService
   - PerformanceCrystalBallService
   - PerformancePersonaService
   - PerformanceTimeMachineService
   - PerformanceCommunityService

## Dependency Injection

**Container**: Microsoft.Extensions.DependencyInjection

**Registration**: `App.xaml.cs`

**Lifetimes:**
- **Singleton**: Services (monitoring, AI, analysis)
- **Transient**: ViewModels (created per view instance)

**Service Registration Pattern:**
```csharp
services.AddSingleton<IServiceInterface, ServiceImplementation>();
services.AddTransient<ViewModelClass>();
```

## Security Architecture

### Credential Management
- **Encryption**: Windows DPAPI (Data Protection API)
- **Storage**: Local user profile only
- **Access**: Per-user encryption key

### Database Access
- **Read-Only**: All operations are read-only
- **Permissions**: db_datareader + VIEW SERVER STATE
- **Connection**: Encrypted (SSL/TLS supported)

### Application Security
- **No Admin Required**: Standard user permissions
- **Local Storage**: No network storage
- **No Registry**: Portable application

## Performance Architecture

### Async Patterns
- All I/O operations are async
- UI remains responsive during operations
- Proper async/await usage throughout

### Caching Strategy
- **AI Responses**: In-memory cache (95%+ hit rate)
- **Query Results**: No caching (real-time data)
- **Configuration**: Loaded once at startup

### Resource Management
- **Connection Pooling**: SQL Server connection pooling
- **Memory Management**: Proper disposal patterns
- **Thread Safety**: UI updates on UI thread

## Extension Points

### Adding New Monitoring
1. Create service interface in `Core/Services/`
2. Implement service
3. Register in DI container
4. Create ViewModel and View

### Adding New AI Features
1. Create AI service interface
2. Implement with AI service integration
3. Register with HTTP client factory
4. Create ViewModel and View

### Adding New Data Sources
1. Create data access service in `Data/`
2. Implement connection management
3. Register in DI container
4. Use in monitoring services

## Testing Strategy

### Unit Testing
- Test services in isolation
- Mock dependencies
- Test business logic

### Integration Testing
- Test service interactions
- Test data access layer
- Test AI service integration

### UI Testing
- Manual testing (WPF UI testing complex)
- Focus on ViewModel testing
- Test data binding

## Deployment Architecture

### Single-File Deployment
- Self-contained executable
- All dependencies included
- No installation required
- Portable deployment

### Configuration Management
- Runtime configuration
- User-specific settings
- Encrypted credentials
- No central configuration server

## Scalability Considerations

### Current Limitations
- Single-user application
- Single database connection
- In-memory state only

### Future Scalability Options
- Multi-instance support
- Centralized configuration
- Database for historical data
- Web API backend

## Monitoring and Observability

### Logging
- Uses `ILogger<T>` throughout
- Log levels: Debug, Information, Warning, Error
- Logs to Visual Studio Output window

### Error Handling
- Try-catch blocks in async methods
- User-friendly error messages
- Status messages for user feedback

### Performance Monitoring
- Built-in performance metrics
- Query execution time tracking
- Memory usage monitoring

