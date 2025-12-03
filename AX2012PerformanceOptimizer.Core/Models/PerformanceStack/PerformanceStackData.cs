namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Complete performance stack data containing metrics for all layers
/// </summary>
public class PerformanceStackData
{
    /// <summary>
    /// Timestamp when the stack data was collected
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Database layer metrics
    /// </summary>
    public DatabaseLayerMetrics Database { get; set; } = new();

    /// <summary>
    /// AOS Server layer metrics
    /// </summary>
    public AosLayerMetrics AosServer { get; set; } = new();

    /// <summary>
    /// Network layer metrics
    /// </summary>
    public NetworkLayerMetrics Network { get; set; } = new();

    /// <summary>
    /// Client layer metrics
    /// </summary>
    public ClientLayerMetrics Client { get; set; } = new();

    /// <summary>
    /// List of detected bottlenecks across all layers
    /// </summary>
    public List<Bottleneck> Bottlenecks { get; set; } = new();
}
