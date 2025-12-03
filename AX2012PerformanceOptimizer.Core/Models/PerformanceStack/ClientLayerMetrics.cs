namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Performance metrics for the Client layer
/// </summary>
public class ClientLayerMetrics
{
    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AvgResponseTimeMs { get; set; }

    /// <summary>
    /// Average user wait time in milliseconds
    /// </summary>
    public double AvgUserWaitTimeMs { get; set; }

    /// <summary>
    /// Total number of requests
    /// </summary>
    public long RequestCount { get; set; }

    /// <summary>
    /// Error rate percentage (0-100)
    /// </summary>
    public double ErrorRatePercent { get; set; }

    /// <summary>
    /// Number of active users
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Requests per second
    /// </summary>
    public double RequestsPerSecond { get; set; }

    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}
