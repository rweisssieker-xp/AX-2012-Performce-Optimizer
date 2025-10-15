using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ISqlQueryMonitorService _sqlMonitor;
    private readonly IAosMonitorService _aosMonitor;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly IDatabaseStatsService _databaseStats;

    [ObservableProperty]
    private int activeUsers;

    [ObservableProperty]
    private int runningBatchJobs;

    [ObservableProperty]
    private long databaseSizeMB;

    [ObservableProperty]
    private int expensiveQueries;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public DashboardViewModel(
        ISqlQueryMonitorService sqlMonitor,
        IAosMonitorService aosMonitor,
        IBatchJobMonitorService batchJobMonitor,
        IDatabaseStatsService databaseStats)
    {
        _sqlMonitor = sqlMonitor;
        _aosMonitor = aosMonitor;
        _batchJobMonitor = batchJobMonitor;
        _databaseStats = databaseStats;

        // Initialize with demo data
        LoadDemoData();
    }

    private void LoadDemoData()
    {
        // Show demo data when no connection is available
        ActiveUsers = 42;
        RunningBatchJobs = 7;
        DatabaseSizeMB = 15360; // 15 GB
        ExpensiveQueries = 23;
        StatusMessage = "Demo Mode - Connect to database for live data";
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading dashboard data...";

        try
        {
            var aosMetrics = await _aosMonitor.GetAosMetricsAsync();
            ActiveUsers = aosMetrics.ActiveUserSessions;

            var batchJobs = await _batchJobMonitor.GetRunningBatchJobsAsync();
            RunningBatchJobs = batchJobs.Count;

            var dbMetrics = await _databaseStats.GetDatabaseMetricsAsync();
            DatabaseSizeMB = dbMetrics.TotalSizeMB;

            var queries = await _sqlMonitor.GetTopExpensiveQueriesAsync(10);
            ExpensiveQueries = queries.Count;

            StatusMessage = "Data loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection not configured - Showing demo data";
            // Keep demo data on connection error
            LoadDemoData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }
}

