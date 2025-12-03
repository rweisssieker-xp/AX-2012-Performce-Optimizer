namespace AX2012PerformanceOptimizer.Core.Models.Sonification;

/// <summary>
/// Settings for performance sonification
/// </summary>
public class SonificationSettings
{
    /// <summary>
    /// Whether sonification is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Master volume (0-100)
    /// </summary>
    public int Volume { get; set; } = 50;

    /// <summary>
    /// Minimum pitch in Hz (for slow queries)
    /// </summary>
    public double MinPitchHz { get; set; } = 200.0;

    /// <summary>
    /// Maximum pitch in Hz (for fast queries)
    /// </summary>
    public double MaxPitchHz { get; set; } = 2000.0;

    /// <summary>
    /// Whether alerts are enabled
    /// </summary>
    public bool EnableAlerts { get; set; } = true;

    /// <summary>
    /// Slow query threshold in milliseconds
    /// </summary>
    public double SlowQueryThresholdMs { get; set; } = 1000.0;

    /// <summary>
    /// High CPU threshold percentage
    /// </summary>
    public double HighCpuThresholdPercent { get; set; } = 80.0;

    /// <summary>
    /// Audio mapping for query performance
    /// </summary>
    public AudioMapping QueryPerformanceMapping { get; set; } = new();

    /// <summary>
    /// Audio mapping for CPU usage
    /// </summary>
    public AudioMapping CpuUsageMapping { get; set; } = new();

    /// <summary>
    /// Audio mapping for database health
    /// </summary>
    public AudioMapping DatabaseHealthMapping { get; set; } = new();
}

/// <summary>
/// Audio mapping configuration for a metric
/// </summary>
public class AudioMapping
{
    /// <summary>
    /// Whether this mapping is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Minimum value for mapping
    /// </summary>
    public double MinValue { get; set; } = 0.0;

    /// <summary>
    /// Maximum value for mapping
    /// </summary>
    public double MaxValue { get; set; } = 100.0;

    /// <summary>
    /// Minimum pitch in Hz
    /// </summary>
    public double MinPitchHz { get; set; } = 200.0;

    /// <summary>
    /// Maximum pitch in Hz
    /// </summary>
    public double MaxPitchHz { get; set; } = 2000.0;

    /// <summary>
    /// Volume multiplier (0.0-1.0)
    /// </summary>
    public double VolumeMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Alert threshold configuration
/// </summary>
public class AlertThreshold
{
    /// <summary>
    /// Alert type
    /// </summary>
    public AlertType Type { get; set; }

    /// <summary>
    /// Threshold value
    /// </summary>
    public double ThresholdValue { get; set; }

    /// <summary>
    /// Alert severity
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    /// <summary>
    /// Whether this alert is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Alert type
/// </summary>
public enum AlertType
{
    SlowQuery = 0,
    HighCpuUsage = 1,
    DatabaseError = 2,
    HighMemoryUsage = 3,
    ConnectionTimeout = 4
}

/// <summary>
/// Alert severity
/// </summary>
public enum AlertSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Sonification event
/// </summary>
public class SonificationEvent
{
    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Metric type
    /// </summary>
    public MetricType MetricType { get; set; }

    /// <summary>
    /// Metric value
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Calculated pitch in Hz
    /// </summary>
    public double PitchHz { get; set; }

    /// <summary>
    /// Calculated volume (0-1)
    /// </summary>
    public double Volume { get; set; }
}

/// <summary>
/// Metric type for sonification
/// </summary>
public enum MetricType
{
    QueryPerformance = 0,
    CpuUsage = 1,
    DatabaseHealth = 2,
    MemoryUsage = 3,
    NetworkLatency = 4
}
