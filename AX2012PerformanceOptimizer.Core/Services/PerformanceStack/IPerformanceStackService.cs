using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

namespace AX2012PerformanceOptimizer.Core.Services.PerformanceStack;

/// <summary>
/// Service for retrieving and analyzing performance metrics across all system layers
/// </summary>
public interface IPerformanceStackService
{
    /// <summary>
    /// Gets performance metrics for all layers in the stack
    /// </summary>
    /// <param name="timeRange">Time range for which to retrieve metrics</param>
    /// <returns>Performance stack data containing metrics for all layers</returns>
    Task<PerformanceStackData> GetStackMetricsAsync(TimeRange timeRange);

    /// <summary>
    /// Gets detailed metrics for a specific layer
    /// </summary>
    /// <param name="layer">The layer to get details for</param>
    /// <param name="timeRange">Time range for which to retrieve metrics</param>
    /// <returns>Layer-specific metrics (DatabaseLayerMetrics, AosLayerMetrics, NetworkLayerMetrics, or ClientLayerMetrics)</returns>
    Task<object> GetLayerDetailsAsync(LayerType layer, TimeRange timeRange);

    /// <summary>
    /// Detects bottlenecks across all layers in the performance stack
    /// </summary>
    /// <param name="stackData">The performance stack data to analyze</param>
    /// <returns>List of detected bottlenecks</returns>
    Task<List<Bottleneck>> DetectBottlenecksAsync(PerformanceStackData stackData);
}

/// <summary>
/// Time range for querying performance metrics
/// </summary>
public class TimeRange
{
    /// <summary>
    /// Start time of the range
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the range
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Creates a time range for the last hour
    /// </summary>
    public static TimeRange LastHour => new()
    {
        StartTime = DateTime.UtcNow.AddHours(-1),
        EndTime = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a time range for the last 24 hours
    /// </summary>
    public static TimeRange Last24Hours => new()
    {
        StartTime = DateTime.UtcNow.AddHours(-24),
        EndTime = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a time range for the last week
    /// </summary>
    public static TimeRange LastWeek => new()
    {
        StartTime = DateTime.UtcNow.AddDays(-7),
        EndTime = DateTime.UtcNow
    };
}

