namespace AX2012PerformanceOptimizer.Core.Services;

public class DataCollectionService : IDataCollectionService
{
    private readonly ISqlQueryMonitorService _sqlMonitor;
    private readonly IAosMonitorService _aosMonitor;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IHistoricalDataService _historyService;

    private Timer? _timer;
    private bool _isCollecting;
    private DateTime? _lastCollectionTime;

    public bool IsCollecting => _isCollecting;
    public DateTime? LastCollectionTime => _lastCollectionTime;

    public DataCollectionService(
        ISqlQueryMonitorService sqlMonitor,
        IAosMonitorService aosMonitor,
        IBatchJobMonitorService batchJobMonitor,
        IDatabaseStatsService databaseStats,
        IHistoricalDataService historyService)
    {
        _sqlMonitor = sqlMonitor;
        _aosMonitor = aosMonitor;
        _batchJobMonitor = batchJobMonitor;
        _databaseStats = databaseStats;
        _historyService = historyService;
    }

    public async Task StartCollectionAsync(int intervalMinutes = 15)
    {
        if (_isCollecting)
            return;

        // Initialize database
        await _historyService.InitializeDatabaseAsync();

        _isCollecting = true;

        // Collect immediately
        await CollectDataAsync();

        // Setup timer for recurring collection
        var interval = TimeSpan.FromMinutes(intervalMinutes);
        _timer = new Timer(async _ => await CollectDataAsync(), null, interval, interval);
    }

    public Task StopCollectionAsync()
    {
        _timer?.Dispose();
        _timer = null;
        _isCollecting = false;
        return Task.CompletedTask;
    }

    private async Task CollectDataAsync()
    {
        try
        {
            // Collect AOS metrics
            var aosMetrics = await _aosMonitor.GetAosMetricsAsync();
            await _historyService.SaveAosMetricsAsync(aosMetrics);

            // Collect batch job metrics
            var runningJobs = await _batchJobMonitor.GetRunningBatchJobsAsync();
            var failedJobs = await _batchJobMonitor.GetFailedBatchJobsAsync();
            await _historyService.SaveBatchJobMetricsAsync(runningJobs.Count, failedJobs.Count);

            // Collect database metrics
            var dbMetrics = await _databaseStats.GetDatabaseMetricsAsync();
            await _historyService.SaveDatabaseMetricsAsync(dbMetrics);

            // Collect query metrics
            var queries = await _sqlMonitor.GetTopExpensiveQueriesAsync(10);
            await _historyService.SaveQueryMetricsAsync(queries);

            // Save dashboard snapshot
            await _historyService.SaveDashboardMetricsAsync(
                aosMetrics.ActiveUserSessions,
                runningJobs.Count,
                dbMetrics.TotalSizeMB,
                queries.Count
            );

            _lastCollectionTime = DateTime.UtcNow;

            // Cleanup old data (keep 90 days)
            if (_lastCollectionTime.Value.Hour == 2) // Run cleanup at 2 AM
            {
                await _historyService.CleanupOldDataAsync(90);
            }
        }
        catch
        {
            // Silent fail - don't interrupt the monitoring
        }
    }
}
