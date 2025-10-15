using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IDatabaseStatsService
{
    Task<DatabaseMetric> GetDatabaseMetricsAsync();
    Task<List<TableMetric>> GetTopTablesBySize(int topCount = 20);
    Task<List<IndexFragmentation>> GetFragmentedIndexesAsync(double thresholdPercent = 30);
    Task<List<MissingIndex>> GetMissingIndexesAsync();
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
    event EventHandler<DatabaseMetric>? NewMetricCollected;
}

