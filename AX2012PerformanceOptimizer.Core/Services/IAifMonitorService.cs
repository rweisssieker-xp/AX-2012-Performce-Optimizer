namespace AX2012PerformanceOptimizer.Core.Services;

public interface IAifMonitorService
{
    Task<int> GetInboundQueueCountAsync();
    Task<int> GetOutboundQueueCountAsync();
    Task<int> GetErrorQueueCountAsync();
    Task<double> GetAvgProcessingTimeAsync();
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
}

