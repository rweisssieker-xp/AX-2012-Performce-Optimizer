namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Performance metrics for the AOS Server layer
/// </summary>
public class AosLayerMetrics
{
    /// <summary>
    /// AOS Server CPU usage percentage (0-100)
    /// </summary>
    public double CpuUsagePercent { get; set; }

    /// <summary>
    /// Memory usage in megabytes
    /// </summary>
    public long MemoryUsageMB { get; set; }

    /// <summary>
    /// Number of active user sessions
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Number of requests in the queue
    /// </summary>
    public int RequestQueueLength { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AvgResponseTimeMs { get; set; }

    /// <summary>
    /// Number of active threads
    /// </summary>
    public int ActiveThreads { get; set; }

    /// <summary>
    /// Server name
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}
