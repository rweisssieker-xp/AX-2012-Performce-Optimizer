using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Windows;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Data.SqlServer;
using AX2012PerformanceOptimizer.Data.AxConnector;
using AX2012PerformanceOptimizer.Data.Configuration;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton<IConfigurationService, ConfigurationService>();

                // Data Access
                services.AddSingleton<ISqlConnectionManager, SqlConnectionManager>();
                services.AddSingleton<IAxConnectorService, AxConnectorService>();
                services.AddSingleton<ISqlExecutionPlanService, ExecutionPlanService>();

                // Monitoring Services
                services.AddSingleton<ISqlQueryMonitorService, SqlQueryMonitorService>();
                services.AddSingleton<IAosMonitorService, AosMonitorService>();
                services.AddSingleton<IBatchJobMonitorService, BatchJobMonitorService>();
                services.AddSingleton<IAifMonitorService, AifMonitorService>();
                services.AddSingleton<ISsrsMonitorService, SsrsMonitorService>();
                services.AddSingleton<IDatabaseStatsService, DatabaseStatsService>();
                services.AddSingleton<IServerConfigurationService, ServerConfigurationService>();

                // Analysis
                services.AddSingleton<IRecommendationEngine, RecommendationEngine>();
                services.AddSingleton<ITrendingService, TrendingService>();
                services.AddSingleton<IAlertService, AlertService>();

                // 🚀 Innovative Features (Top 5 USPs)
                services.AddSingleton<IPerformanceDNAService, PerformanceDNAService>();
                services.AddSingleton<IPerformanceCrystalBallService, PerformanceCrystalBallService>();
                services.AddSingleton<IPerformancePersonaService, PerformancePersonaService>();
                services.AddSingleton<IPerformanceTimeMachineService, PerformanceTimeMachineService>();
                services.AddSingleton<IPerformanceCommunityService, PerformanceCommunityService>();

                // Historical Data
                services.AddSingleton<IHistoricalDataService, HistoricalDataService>();
                services.AddSingleton<IDataCollectionService, DataCollectionService>();
                services.AddSingleton<IQueryAnalyzerService, QueryAnalyzerService>();

                // AI Services
                services.AddHttpClient();
                services.AddSingleton<IAiQueryOptimizerService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AiQueryOptimizerService>>();
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("AI");
                    var aiService = new AiQueryOptimizerService(logger, httpClient);

                    // Load configuration and configure the service
                    var configService = sp.GetRequiredService<IConfigurationService>();
                    var aiConfig = configService.GetAiConfigurationAsync().Result;
                    if (aiConfig != null && aiConfig.IsEnabled && !string.IsNullOrEmpty(aiConfig.EncryptedApiKey))
                    {
                        var apiKey = configService.DecryptPassword(aiConfig.EncryptedApiKey);
                        aiService.Configure(
                            apiKey,
                            aiConfig.Endpoint,
                            aiConfig.Model,
                            aiConfig.Provider == Data.Models.AiProvider.AzureOpenAI);
                    }

                    return aiService;
                });

                // AI Query Explainer Service
                services.AddSingleton<IAiQueryExplainerService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AiQueryExplainerService>>();
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("AI");
                    var explainerService = new AiQueryExplainerService(logger, httpClient);

                    // Load configuration and configure the service
                    var configService = sp.GetRequiredService<IConfigurationService>();
                    var aiConfig = configService.GetAiConfigurationAsync().Result;
                    if (aiConfig != null && aiConfig.IsEnabled && !string.IsNullOrEmpty(aiConfig.EncryptedApiKey))
                    {
                        var apiKey = configService.DecryptPassword(aiConfig.EncryptedApiKey);
                        explainerService.Configure(
                            apiKey,
                            aiConfig.Endpoint,
                            aiConfig.Model,
                            aiConfig.Provider == Data.Models.AiProvider.AzureOpenAI);
                    }

                    return explainerService;
                });

                // NEW: Advanced AI Features
                services.AddSingleton<IQueryAutoFixerService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<QueryAutoFixerService>>();
                    var aiOptimizer = sp.GetRequiredService<IAiQueryOptimizerService>();
                    return new QueryAutoFixerService(logger, aiOptimizer);
                });

                services.AddSingleton<IQueryDocumentationService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<QueryDocumentationService>>();
                    var aiOptimizer = sp.GetRequiredService<IAiQueryOptimizerService>();
                    return new QueryDocumentationService(logger, aiOptimizer);
                });

                // NEW: Performance Cost Calculator
                services.AddSingleton<IPerformanceCostCalculatorService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<PerformanceCostCalculatorService>>();
                    return new PerformanceCostCalculatorService(logger);
                });

                // NEW: Query Performance Forecasting
                services.AddSingleton<IQueryPerformanceForecastingService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<QueryPerformanceForecastingService>>();
                    return new QueryPerformanceForecastingService(logger);
                });

                // NEW: Self-Healing Queries
                services.AddSingleton<ISelfHealingQueryService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<SelfHealingQueryService>>();
                    var autoFixer = sp.GetRequiredService<IQueryAutoFixerService>();
                    return new SelfHealingQueryService(logger, autoFixer);
                });

                // NEW: Query Correlation Engine
                services.AddSingleton<IQueryCorrelationEngine>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<QueryCorrelationEngine>>();
                    return new QueryCorrelationEngine(logger);
                });

                // NEW: Smart Batching Advisor
                services.AddSingleton<ISmartBatchingAdvisor>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<SmartBatchingAdvisor>>();
                    return new SmartBatchingAdvisor(logger);
                });

                // NEW: Query Clustering Service
                services.AddSingleton<IQueryClusteringService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<QueryClusteringService>>();
                    return new QueryClusteringService(logger);
                });

                // PHASE 1 AI FEATURES: Natural Language Query Assistant (with real data services)
                services.AddSingleton<INaturalLanguageQueryAssistant>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<NaturalLanguageQueryAssistant>>();
                    var aiService = sp.GetRequiredService<IAiQueryOptimizerService>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var historicalData = sp.GetRequiredService<IHistoricalDataService>();
                    var costCalculator = sp.GetRequiredService<IPerformanceCostCalculatorService>();
                    var queryAnalyzer = sp.GetRequiredService<IQueryAnalyzerService>();
                    return new NaturalLanguageQueryAssistant(logger, aiService, queryMonitor, historicalData, costCalculator, queryAnalyzer);
                });

                // PHASE 1 AI FEATURES: AI Performance Insights Dashboard (with real data services)
                services.AddSingleton<IAiPerformanceInsightsService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AiPerformanceInsightsService>>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var historicalData = sp.GetRequiredService<IHistoricalDataService>();
                    var costCalculator = sp.GetRequiredService<IPerformanceCostCalculatorService>();
                    var queryAnalyzer = sp.GetRequiredService<IQueryAnalyzerService>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    return new AiPerformanceInsightsService(logger, queryMonitor, historicalData, costCalculator, queryAnalyzer, databaseStats);
                });

                // PHASE 1 AI FEATURES: Intelligent Query Rewriter
                services.AddSingleton<IIntelligentQueryRewriter>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<IntelligentQueryRewriter>>();
                    var aiService = sp.GetRequiredService<IAiQueryOptimizerService>();
                    return new IntelligentQueryRewriter(logger, aiService);
                });

                // NEW: AI Health Dashboard - System Health Score Service
                services.AddSingleton<ISystemHealthScoreService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<SystemHealthScoreService>>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    var batchJobMonitor = sp.GetRequiredService<IBatchJobMonitorService>();
                    return new SystemHealthScoreService(logger, queryMonitor, databaseStats, batchJobMonitor);
                });

                // NEW: Dialog Service for modern popups
                services.AddSingleton<Services.IDialogService, Services.DialogService>();

                // NEW: Natural Language to SQL Generator (Phase 1 AI Feature)
                services.AddSingleton<AX2012PerformanceOptimizer.Core.Services.NaturalLanguageToSql.INaturalLanguageToSqlService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AX2012PerformanceOptimizer.Core.Services.NaturalLanguageToSql.NaturalLanguageToSqlService>>();
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("AI");
                    var nlToSqlService = new AX2012PerformanceOptimizer.Core.Services.NaturalLanguageToSql.NaturalLanguageToSqlService(logger, httpClient);

                    // Load configuration and configure the service
                    var configService = sp.GetRequiredService<IConfigurationService>();
                    var aiConfig = configService.GetAiConfigurationAsync().Result;
                    if (aiConfig != null && aiConfig.IsEnabled && !string.IsNullOrEmpty(aiConfig.EncryptedApiKey))
                    {
                        var apiKey = configService.DecryptPassword(aiConfig.EncryptedApiKey);
                        nlToSqlService.Configure(
                            apiKey,
                            aiConfig.Endpoint,
                            aiConfig.Model,
                            aiConfig.Provider == Data.Models.AiProvider.AzureOpenAI);
                    }

                    return nlToSqlService;
                });

                // NEW: Query Risk Scoring with ML (Phase 1 AI Feature)
                services.AddSingleton<AX2012PerformanceOptimizer.Core.Services.QueryRiskScoring.IQueryRiskScoringService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AX2012PerformanceOptimizer.Core.Services.QueryRiskScoring.QueryRiskScoringService>>();
                    return new AX2012PerformanceOptimizer.Core.Services.QueryRiskScoring.QueryRiskScoringService(logger);
                });

                // NEW: Executive Dashboard Generator (Phase 1 AI Feature)
                services.AddSingleton<AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard.IExecutiveDashboardGeneratorService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard.ExecutiveDashboardGeneratorService>>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    var historicalData = sp.GetRequiredService<IHistoricalDataService>();
                    return new AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard.ExecutiveDashboardGeneratorService(logger, queryMonitor, databaseStats, historicalData);
                });

                // PHASE 2 AI FEATURES: Performance Therapist (interactive diagnosis)
                services.AddSingleton<AX2012PerformanceOptimizer.Core.Services.PerformanceTherapist.IPerformanceTherapistService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AX2012PerformanceOptimizer.Core.Services.PerformanceTherapist.PerformanceTherapistService>>();
                    var aiService = sp.GetRequiredService<IAiQueryOptimizerService>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    var historicalData = sp.GetRequiredService<IHistoricalDataService>();
                    return new AX2012PerformanceOptimizer.Core.Services.PerformanceTherapist.PerformanceTherapistService(logger, aiService, queryMonitor, databaseStats, historicalData);
                });

                // PHASE 1 QUICK WINS: Executive Performance Scorecard
                services.AddSingleton<AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard.IExecutiveScorecardService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard.ExecutiveScorecardService>>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var aosMonitor = sp.GetRequiredService<IAosMonitorService>();
                    var batchJobMonitor = sp.GetRequiredService<IBatchJobMonitorService>();
                    var healthScore = sp.GetRequiredService<ISystemHealthScoreService>();
                    return new AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard.ExecutiveScorecardService(databaseStats, queryMonitor, aosMonitor, batchJobMonitor, healthScore, logger);
                });

                // PHASE 1 QUICK WINS: Performance Anomaly Detection
                services.AddSingleton<IPerformanceAnomalyDetectionService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<PerformanceAnomalyDetectionService>>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    var aosMonitor = sp.GetRequiredService<IAosMonitorService>();
                    return new PerformanceAnomalyDetectionService(queryMonitor, databaseStats, aosMonitor, logger);
                });

                // PHASE 1 QUICK WINS: Performance Health Scorecard
                services.AddSingleton<IPerformanceHealthScorecardService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<PerformanceHealthScorecardService>>();
                    var queryMonitor = sp.GetRequiredService<ISqlQueryMonitorService>();
                    var databaseStats = sp.GetRequiredService<IDatabaseStatsService>();
                    var aosMonitor = sp.GetRequiredService<IAosMonitorService>();
                    var batchJobMonitor = sp.GetRequiredService<IBatchJobMonitorService>();
                    return new PerformanceHealthScorecardService(queryMonitor, databaseStats, aosMonitor, batchJobMonitor, logger);
                });

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<SqlPerformanceViewModel>();
                services.AddTransient<AosMonitoringViewModel>();
                services.AddTransient<BatchJobsViewModel>();
                services.AddTransient<DatabaseHealthViewModel>();
                services.AddTransient<RecommendationsViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<HistoricalTrendingViewModel>();

                // PHASE 1 AI FEATURES: ViewModels
                services.AddTransient<NaturalLanguageAssistantViewModel>();
                services.AddTransient<AiInsightsDashboardViewModel>();
                services.AddTransient<AiHealthDashboardViewModel>();
                services.AddTransient<NaturalLanguageToSqlViewModel>();
                services.AddTransient<QueryRiskScoringViewModel>();
                services.AddTransient<ExecutiveDashboardViewModel>();

                // PHASE 2 AI FEATURES: ViewModels
                services.AddTransient<PerformanceTherapistViewModel>();

                // PHASE 1 QUICK WINS: ViewModels
                services.AddTransient<ExecutiveScorecardViewModel>();

                // 🚀 INNOVATIVE USP FEATURES: ViewModels
                services.AddTransient<PerformanceDnaViewModel>();
                services.AddTransient<PerformanceCrystalBallViewModel>();
                services.AddTransient<PerformancePersonasViewModel>();
                services.AddTransient<PerformanceTimeMachineViewModel>();
                services.AddTransient<PerformanceCommunityViewModel>();

                // Server Settings ViewModel
                services.AddTransient<ServerSettingsViewModel>();

                // Windows (not registered as singleton - created via XAML)
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetRequiredService<T>();
    }
}
