using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IHistoricalDataService
{
    Task InitializeDatabaseAsync();

    // Save metrics
    Task SaveDashboardMetricsAsync(int activeUsers, int runningJobs, long dbSizeMB, int expensiveQueries);
    Task SaveQueryMetricsAsync(IEnumerable<SqlQueryMetric> queries);
    Task SaveAosMetricsAsync(AosMetric metrics);
    Task SaveBatchJobMetricsAsync(int runningCount, int failedCount);
    Task SaveDatabaseMetricsAsync(DatabaseMetric metrics);

    // Retrieve historical data
    Task<List<DashboardSnapshot>> GetDashboardHistoryAsync(DateTime from, DateTime to);
    Task<List<QueryPerformanceHistory>> GetQueryPerformanceHistoryAsync(string queryHash, DateTime from, DateTime to);
    Task<List<AosMetricHistory>> GetAosHistoryAsync(DateTime from, DateTime to);
    Task<List<BatchJobHistory>> GetBatchJobHistoryAsync(DateTime from, DateTime to);
    Task<List<DatabaseSizeHistory>> GetDatabaseSizeHistoryAsync(DateTime from, DateTime to);

    // Trend analysis
    Task<TrendAnalysis> AnalyzeTrendAsync(string metricType, DateTime from, DateTime to);
    Task<Dictionary<string, double>> GetBaselineMetricsAsync();

    // Cleanup
    Task CleanupOldDataAsync(int daysToKeep);
}

public class DashboardSnapshot
{
    public DateTime Timestamp { get; set; }
    public int ActiveUsers { get; set; }
    public int RunningJobs { get; set; }
    public long DatabaseSizeMB { get; set; }
    public int ExpensiveQueries { get; set; }
}

public class QueryPerformanceHistory
{
    public DateTime Timestamp { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public double AvgCpuTimeMs { get; set; }
    public long ExecutionCount { get; set; }
    public long AvgLogicalReads { get; set; }
}

public class AosMetricHistory
{
    public DateTime Timestamp { get; set; }
    public int ActiveSessions { get; set; }
    public double CpuPercent { get; set; }
    public long MemoryMB { get; set; }
}

public class BatchJobHistory
{
    public DateTime Timestamp { get; set; }
    public int RunningCount { get; set; }
    public int FailedCount { get; set; }
}

public class DatabaseSizeHistory
{
    public DateTime Timestamp { get; set; }
    public long TotalSizeMB { get; set; }
    public long DataSizeMB { get; set; }
    public long LogSizeMB { get; set; }
}

public class TrendAnalysis
{
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double AverageValue { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double StandardDeviation { get; set; }
    public TrendDirection Trend { get; set; }
    public double ChangePercent { get; set; }
}

public enum TrendDirection
{
    Stable,
    Increasing,
    Decreasing,
    Volatile
}
