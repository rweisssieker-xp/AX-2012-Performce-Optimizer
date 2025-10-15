using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface ISqlQueryMonitorService
{
    Task<List<SqlQueryMetric>> GetTopExpensiveQueriesAsync(int topCount = 20);
    Task<List<SqlQueryMetric>> GetQueryStatisticsAsync();
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
    event EventHandler<SqlQueryMetric>? NewMetricCollected;
}

