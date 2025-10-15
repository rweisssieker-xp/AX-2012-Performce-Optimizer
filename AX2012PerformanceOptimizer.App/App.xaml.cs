using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Data.SqlServer;
using AX2012PerformanceOptimizer.Data.AxConnector;
using AX2012PerformanceOptimizer.Data.Configuration;
using AX2012PerformanceOptimizer.App.ViewModels;
using AX2012PerformanceOptimizer.App.Views;

namespace AX2012PerformanceOptimizer.App;

public partial class App : Application
{
    private Window? _window;
    private readonly IHost _host;

    public App()
    {
        InitializeComponent();

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

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<SqlPerformanceViewModel>();
                services.AddTransient<AosMonitoringViewModel>();
                services.AddTransient<BatchJobsViewModel>();
                services.AddTransient<DatabaseHealthViewModel>();
                services.AddTransient<RecommendationsViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _host.StartAsync();

        _window = _host.Services.GetRequiredService<MainWindow>();
        _window.Activate();
    }

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!._host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }
}

