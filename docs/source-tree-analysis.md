# Source Tree Analysis - AX 2012 Performance Optimizer

## Project Root Structure

```
AX-2012-Performce-Optimizer/
├── AX2012PerformanceOptimizer.sln          # Solution file
├── README.md                                # Project overview
├── CORE_DOCUMENTATION.md                   # Complete core documentation
├── AI_FEATURES.md                          # AI features documentation
├── INNOVATIVE_FEATURES.md                  # Innovative USP features
├── LICENSE                                  # MIT License
│
├── AX2012PerformanceOptimizer.WpfApp/       # WPF Application (UI Layer)
│   ├── App.xaml                            # Application entry point XAML
│   ├── App.xaml.cs                         # Application entry point (DI setup)
│   ├── MainWindow.xaml                     # Main window XAML
│   ├── MainWindow.xaml.cs                 # Main window code-behind
│   │
│   ├── ViewModels/                        # MVVM ViewModels (23 files)
│   │   ├── MainViewModel.cs               # Main navigation ViewModel
│   │   ├── DashboardViewModel.cs          # Dashboard ViewModel
│   │   ├── SqlPerformanceViewModel.cs     # SQL Performance ViewModel
│   │   ├── AosMonitoringViewModel.cs      # AOS Monitoring ViewModel
│   │   ├── BatchJobsViewModel.cs          # Batch Jobs ViewModel
│   │   ├── DatabaseHealthViewModel.cs     # Database Health ViewModel
│   │   ├── RecommendationsViewModel.cs    # Recommendations ViewModel
│   │   ├── SettingsViewModel.cs           # Settings ViewModel
│   │   ├── HistoricalTrendingViewModel.cs  # Historical Trending ViewModel
│   │   ├── NaturalLanguageAssistantViewModel.cs  # AI Assistant ViewModel
│   │   ├── AiInsightsDashboardViewModel.cs # AI Insights ViewModel
│   │   ├── AiHealthDashboardViewModel.cs   # AI Health Dashboard ViewModel
│   │   ├── NaturalLanguageToSqlViewModel.cs # NL to SQL ViewModel
│   │   ├── QueryRiskScoringViewModel.cs   # Query Risk Scoring ViewModel
│   │   ├── ExecutiveDashboardViewModel.cs # Executive Dashboard ViewModel
│   │   ├── ExecutiveScorecardViewModel.cs  # Executive Scorecard ViewModel
│   │   ├── PerformanceTherapistViewModel.cs # Performance Therapist ViewModel
│   │   ├── PerformanceDnaViewModel.cs      # Performance DNA ViewModel
│   │   ├── PerformanceCrystalBallViewModel.cs # Crystal Ball ViewModel
│   │   ├── PerformancePersonasViewModel.cs # Personas ViewModel
│   │   ├── PerformanceTimeMachineViewModel.cs # Time Machine ViewModel
│   │   ├── PerformanceCommunityViewModel.cs # Community ViewModel
│   │   └── ServerSettingsViewModel.cs      # Server Settings ViewModel
│   │
│   ├── Views/                             # XAML Views (22 XAML + 22 code-behind)
│   │   ├── DashboardView.xaml             # Dashboard view
│   │   ├── SqlPerformanceView.xaml        # SQL Performance view
│   │   ├── AosMonitoringView.xaml         # AOS Monitoring view
│   │   ├── BatchJobsView.xaml             # Batch Jobs view
│   │   ├── DatabaseHealthView.xaml        # Database Health view
│   │   ├── RecommendationsView.xaml       # Recommendations view
│   │   ├── SettingsView.xaml              # Settings view
│   │   ├── HistoricalTrendingView.xaml    # Historical Trending view
│   │   ├── NaturalLanguageAssistantView.xaml # AI Assistant view
│   │   ├── AiInsightsDashboardView.xaml   # AI Insights view
│   │   ├── AiHealthDashboardView.xaml     # AI Health Dashboard view
│   │   ├── NaturalLanguageToSqlView.xaml  # NL to SQL view
│   │   ├── QueryRiskScoringView.xaml      # Query Risk Scoring view
│   │   ├── ExecutiveDashboardView.xaml    # Executive Dashboard view
│   │   ├── ExecutiveScorecardView.xaml     # Executive Scorecard view
│   │   ├── PerformanceTherapistView.xaml  # Performance Therapist view
│   │   ├── PerformanceDnaView.xaml        # Performance DNA view
│   │   ├── PerformanceCrystalBallView.xaml # Crystal Ball view
│   │   ├── PerformancePersonasView.xaml   # Personas view
│   │   ├── PerformanceTimeMachineView.xaml # Time Machine view
│   │   ├── PerformanceCommunityView.xaml   # Community view
│   │   └── ServerSettingsView.xaml        # Server Settings view
│   │
│   ├── Dialogs/                           # Dialog windows
│   │   ├── AiExplainerDialog.xaml         # AI Explainer dialog
│   │   ├── DetailsDialog.xaml             # Details dialog
│   │   ├── ErrorDialog.xaml               # Error dialog
│   │   └── SummaryDialog.xaml              # Summary dialog
│   │
│   ├── Converters/                        # Value converters (10 files)
│   │   ├── BooleanToVisibilityConverter.cs
│   │   ├── BoolToActiveInactiveConverter.cs
│   │   ├── BoolToYesNoConverter.cs
│   │   ├── CountToVisibilityConverter.cs
│   │   ├── IntToVisibilityConverter.cs
│   │   ├── InverseBoolConverter.cs
│   │   ├── NullToBoolConverter.cs
│   │   ├── NullToVisibilityConverter.cs
│   │   ├── ProfileIsActiveConverter.cs
│   │   └── StringToVisibilityConverter.cs
│   │
│   └── Services/                         # Application services
│       ├── DialogService.cs              # Dialog service
│       └── IDialogService.cs             # Dialog service interface
│
├── AX2012PerformanceOptimizer.Core/      # Business Logic Layer
│   ├── Models/                           # Domain models (30+ files)
│   │   ├── SqlQueryMetric.cs            # SQL query metrics model
│   │   ├── AosMetric.cs                  # AOS metrics model
│   │   ├── BatchJobMetric.cs            # Batch job metrics model
│   │   ├── DatabaseMetric.cs            # Database metrics model
│   │   ├── Recommendation.cs            # Recommendation model
│   │   ├── SystemHealthScore.cs         # System health score model
│   │   ├── PerformanceAnomaly.cs        # Performance anomaly model
│   │   ├── PerformanceBenchmark.cs       # Performance benchmark model
│   │   ├── PerformanceDNA.cs             # Performance DNA model
│   │   ├── PerformancePersona.cs         # Performance persona model
│   │   ├── PerformanceScenario.cs        # Performance scenario model
│   │   ├── PerformanceSnapshot.cs        # Performance snapshot model
│   │   ├── PerformanceHealthScorecard.cs # Health scorecard model
│   │   ├── QueryOptimizationSuggestion.cs # Query optimization model
│   │   ├── AlertRule.cs                  # Alert rule model
│   │   ├── ServerConfiguration.cs        # Server configuration model
│   │   ├── ConfigurationRecommendation.cs # Configuration recommendation
│   │   ├── AiQueryAnalysisResult.cs      # AI query analysis result
│   │   ├── ExtendedAiModels.cs           # Extended AI models
│   │   │
│   │   ├── ExecutiveDashboard/           # Executive dashboard models (7 files)
│   │   ├── NaturalLanguageToSql/         # NL to SQL models (3 files)
│   │   ├── PerformanceTherapist/         # Performance therapist models (7 files)
│   │   └── QueryRiskScoring/             # Query risk scoring models (4 files)
│   │
│   └── Services/                         # Business services (81 files)
│       ├── SqlQueryMonitorService.cs     # SQL query monitoring
│       ├── AosMonitorService.cs          # AOS monitoring
│       ├── BatchJobMonitorService.cs     # Batch job monitoring
│       ├── DatabaseStatsService.cs       # Database statistics
│       ├── AifMonitorService.cs          # AIF monitoring
│       ├── SsrsMonitorService.cs         # SSRS monitoring
│       ├── RecommendationEngine.cs      # Recommendation engine
│       ├── AlertService.cs               # Alert service
│       ├── TrendingService.cs            # Trending service
│       ├── SystemHealthScoreService.cs   # System health scoring
│       │
│       ├── AI Services/                  # AI-powered services
│       │   ├── AiQueryOptimizerService.cs # AI query optimizer
│       │   ├── AiQueryExplainerService.cs # AI query explainer
│       │   ├── AiPerformanceInsightsService.cs # AI performance insights
│       │   ├── NaturalLanguageQueryAssistant.cs # Natural language assistant
│       │   ├── IntelligentQueryRewriter.cs # Intelligent query rewriter
│       │   ├── QueryAutoFixerService.cs   # Query auto-fixer
│       │   ├── QueryDocumentationService.cs # Query documentation generator
│       │   ├── PerformanceCostCalculatorService.cs # Cost calculator
│       │   ├── QueryPerformanceForecastingService.cs # Performance forecasting
│       │   ├── NaturalLanguageToSql/     # NL to SQL service (2 files)
│       │   └── QueryRiskScoring/         # Query risk scoring (2 files)
│       │
│       ├── Innovative Features/          # USP features
│       │   ├── PerformanceDNAService.cs  # Performance DNA
│       │   ├── PerformanceCrystalBallService.cs # Crystal Ball
│       │   ├── PerformancePersonaService.cs # Personas
│       │   ├── PerformanceTimeMachineService.cs # Time Machine
│       │   ├── PerformanceCommunityService.cs # Community
│       │   ├── SelfHealingQueryService.cs # Self-healing queries
│       │   ├── QueryCorrelationEngine.cs # Query correlation
│       │   ├── QueryClusteringService.cs # Query clustering
│       │   └── SmartBatchingAdvisor.cs   # Smart batching advisor
│       │
│       ├── Executive Dashboard/          # Executive features (4 files)
│       ├── Performance Therapist/        # Performance therapist (2 files)
│       │
│       └── Supporting Services/         # Supporting services
│           ├── HistoricalDataService.cs  # Historical data
│           ├── DataCollectionService.cs  # Data collection
│           ├── QueryAnalyzerService.cs   # Query analyzer
│           ├── PerformanceAnomalyDetectionService.cs # Anomaly detection
│           ├── PerformanceHealthScorecardService.cs # Health scorecard
│           └── ServerConfigurationService.cs # Server configuration
│
├── AX2012PerformanceOptimizer.Data/      # Data Access Layer
│   ├── SqlServer/                        # SQL Server access
│   │   ├── SqlConnectionManager.cs       # SQL connection manager
│   │   ├── ISqlConnectionManager.cs      # Connection manager interface
│   │   ├── ExecutionPlanService.cs       # Execution plan service
│   │   └── ISqlExecutionPlanService.cs   # Execution plan interface
│   │
│   ├── AxConnector/                      # AX Business Connector
│   │   ├── AxConnectorService.cs         # AX connector service
│   │   └── IAxConnectorService.cs        # AX connector interface
│   │
│   ├── Configuration/                    # Configuration management
│   │   ├── ConfigurationService.cs       # Configuration service
│   │   └── IConfigurationService.cs      # Configuration interface
│   │
│   └── Models/                          # Data models
│       ├── ConnectionProfile.cs          # Connection profile model
│       └── AiConfiguration.cs           # AI configuration model
│
├── docs/                                 # Documentation
│   ├── bmm-workflow-status.yaml         # BMM workflow status
│   ├── project-scan-report.json         # Project scan report
│   └── sprint-artifacts/                # Sprint artifacts folder
│
└── Scripts/                             # Build and deployment scripts
    ├── build-and-run.ps1                # Build and run script
    ├── publish-release.ps1              # Publish release script
    ├── debug-start.ps1                   # Debug start script
    ├── diagnose-detailed.ps1             # Detailed diagnostics
    ├── diagnose-error.ps1                # Error diagnostics
    ├── start-app.bat                     # Start application batch file
    └── TestInnovativeFeatures.ps1       # Test innovative features
```

## Critical Directories

### Entry Points
- **App.xaml.cs**: Application entry point with DI container setup
- **MainWindow.xaml.cs**: Main window entry point

### Core Business Logic
- **Core/Services/**: All business logic services (81 files)
- **Core/Models/**: Domain models (30+ files)

### UI Layer
- **WpfApp/ViewModels/**: MVVM ViewModels (23 files)
- **WpfApp/Views/**: XAML views (22 views)

### Data Access
- **Data/SqlServer/**: SQL Server connectivity
- **Data/AxConnector/**: AX Business Connector integration
- **Data/Configuration/**: Configuration management

## File Statistics

- **Total C# Files**: ~194 files
- **Total XAML Files**: ~22 views + 4 dialogs = 26 XAML files
- **Total Models**: 30+ domain models
- **Total Services**: 81 service files (interfaces + implementations)
- **Total ViewModels**: 23 ViewModels
- **Total Views**: 22 main views + 4 dialogs

## Architecture Notes

1. **Clean Separation**: Clear separation between UI (WpfApp), Business Logic (Core), and Data Access (Data)
2. **Interface-Based**: All services expose interfaces for testability
3. **MVVM Pattern**: Strict MVVM pattern with ViewModels and Views
4. **Dependency Injection**: Comprehensive DI setup in App.xaml.cs
5. **Service-Oriented**: Business logic organized as services with clear responsibilities

