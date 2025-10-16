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

                // Monitoring Services
                services.AddSingleton<ISqlQueryMonitorService, SqlQueryMonitorService>();
                services.AddSingleton<IAosMonitorService, AosMonitorService>();
                services.AddSingleton<IBatchJobMonitorService, BatchJobMonitorService>();
                services.AddSingleton<IAifMonitorService, AifMonitorService>();
                services.AddSingleton<ISsrsMonitorService, SsrsMonitorService>();
                services.AddSingleton<IDatabaseStatsService, DatabaseStatsService>();

                // Analysis
                services.AddSingleton<IRecommendationEngine, RecommendationEngine>();

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
