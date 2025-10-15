using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IBatchJobMonitorService
{
    Task<List<BatchJobMetric>> GetRunningBatchJobsAsync();
    Task<List<BatchJobMetric>> GetBatchJobHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<BatchJobMetric>> GetFailedBatchJobsAsync();
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
    event EventHandler<BatchJobMetric>? NewMetricCollected;
}

