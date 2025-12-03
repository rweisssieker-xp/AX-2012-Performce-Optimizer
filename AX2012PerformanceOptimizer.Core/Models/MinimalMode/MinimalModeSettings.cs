namespace AX2012PerformanceOptimizer.Core.Models.MinimalMode;

/// <summary>
/// Settings for minimal mode (resource-efficient configuration)
/// </summary>
public class MinimalModeSettings
{
    /// <summary>
    /// Whether minimal mode is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Refresh interval in seconds (default: 60s, minimal: 300s)
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Whether to show simplified UI
    /// </summary>
    public bool UseSimplifiedUI { get; set; } = true;

    /// <summary>
    /// Whether animations are disabled
    /// </summary>
    public bool DisableAnimations { get; set; } = true;

    /// <summary>
    /// Whether to reduce data collection frequency
    /// </summary>
    public bool ReduceDataCollection { get; set; } = true;

    /// <summary>
    /// Data collection interval multiplier (1.0 = normal, 5.0 = 5x slower)
    /// </summary>
    public double DataCollectionMultiplier { get; set; } = 5.0;

    /// <summary>
    /// Whether to disable non-essential features
    /// </summary>
    public bool DisableNonEssentialFeatures { get; set; } = true;

    /// <summary>
    /// Maximum memory usage in MB (0 = no limit)
    /// </summary>
    public int MaxMemoryUsageMB { get; set; } = 0;

    /// <summary>
    /// Maximum CPU usage percentage (0 = no limit)
    /// </summary>
    public double MaxCpuUsagePercent { get; set; } = 0;
}
