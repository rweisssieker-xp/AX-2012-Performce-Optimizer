namespace AX2012PerformanceOptimizer.Core.Services;

public interface ISsrsMonitorService
{
    Task<Dictionary<string, int>> GetTopExecutedReportsAsync(int topCount = 10);
    Task<Dictionary<string, double>> GetLongRunningReportsAsync(double thresholdSeconds = 30);
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
}

