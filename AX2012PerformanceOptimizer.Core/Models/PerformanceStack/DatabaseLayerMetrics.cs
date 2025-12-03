namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Performance metrics for the Database layer
/// </summary>
public class DatabaseLayerMetrics
{
    /// <summary>
    /// Average query execution time in milliseconds
    /// </summary>
    public double AvgQueryExecutionTimeMs { get; set; }

    /// <summary>
    /// Total I/O operations per second
    /// </summary>
    public double IoOperationsPerSecond { get; set; }

    /// <summary>
    /// Database CPU usage percentage (0-100)
    /// </summary>
    public double CpuUsagePercent { get; set; }

    /// <summary>
    /// Average lock wait time in milliseconds
    /// </summary>
    public double AvgLockWaitTimeMs { get; set; }

    /// <summary>
    /// Total number of active connections
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Total logical reads per second
    /// </summary>
    public long LogicalReadsPerSecond { get; set; }

    /// <summary>
    /// Total physical reads per second
    /// </summary>
    public long PhysicalReadsPerSecond { get; set; }

    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}
