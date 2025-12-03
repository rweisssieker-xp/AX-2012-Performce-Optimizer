namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Represents a performance bottleneck detected at a specific layer
/// </summary>
public class Bottleneck
{
    /// <summary>
    /// Unique identifier for the bottleneck
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Layer where the bottleneck was detected
    /// </summary>
    public LayerType Layer { get; set; }

    /// <summary>
    /// Severity level of the bottleneck
    /// </summary>
    public BottleneckSeverity Severity { get; set; }

    /// <summary>
    /// Description of the bottleneck
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Impact score (0-100) indicating the severity of the bottleneck
    /// </summary>
    public double ImpactScore { get; set; }

    /// <summary>
    /// Metric name that triggered the bottleneck detection
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the metric
    /// </summary>
    public double CurrentValue { get; set; }

    /// <summary>
    /// Threshold value that was exceeded
    /// </summary>
    public double ThresholdValue { get; set; }

    /// <summary>
    /// Timestamp when the bottleneck was detected
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Severity levels for bottlenecks
/// </summary>
public enum BottleneckSeverity
{
    /// <summary>
    /// Low severity - minor impact
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium severity - moderate impact
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High severity - significant impact
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical severity - severe impact requiring immediate attention
    /// </summary>
    Critical = 3
}
