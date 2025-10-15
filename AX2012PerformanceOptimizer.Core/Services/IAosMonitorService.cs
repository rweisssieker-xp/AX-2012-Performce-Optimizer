using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IAosMonitorService
{
    Task<AosMetric> GetAosMetricsAsync();
    Task<List<UserSession>> GetActiveUserSessionsAsync();
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
    event EventHandler<AosMetric>? NewMetricCollected;
}

