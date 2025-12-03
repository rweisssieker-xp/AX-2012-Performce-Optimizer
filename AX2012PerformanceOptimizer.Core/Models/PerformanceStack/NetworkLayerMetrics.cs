namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Performance metrics for the Network layer
/// </summary>
public class NetworkLayerMetrics
{
    /// <summary>
    /// Average network latency in milliseconds
    /// </summary>
    public double AvgLatencyMs { get; set; }

    /// <summary>
    /// Bandwidth usage in megabits per second
    /// </summary>
    public double BandwidthUsageMbps { get; set; }

    /// <summary>
    /// Packet loss percentage (0-100)
    /// </summary>
    public double PacketLossPercent { get; set; }

    /// <summary>
    /// Total number of active connections
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Network throughput in bytes per second
    /// </summary>
    public long ThroughputBytesPerSecond { get; set; }

    /// <summary>
    /// Number of network errors
    /// </summary>
    public int NetworkErrors { get; set; }

    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}
