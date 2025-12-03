namespace AX2012PerformanceOptimizer.Core.Models.ChainReaction;

/// <summary>
/// Represents the impact of optimizing a query on a dependent query
/// </summary>
public class QueryImpact
{
    /// <summary>
    /// Hash of the affected query
    /// </summary>
    public string QueryHash { get; set; } = string.Empty;

    /// <summary>
    /// Current execution time in milliseconds
    /// </summary>
    public double CurrentExecutionTime { get; set; }

    /// <summary>
    /// Predicted execution time after optimization in milliseconds
    /// </summary>
    public double PredictedExecutionTime { get; set; }

    /// <summary>
    /// Improvement percentage (positive = improvement, negative = degradation)
    /// </summary>
    public double ImprovementPercentage { get; set; }

    /// <summary>
    /// Type of impact (Positive, Neutral, Negative)
    /// </summary>
    public ImpactType ImpactType { get; set; }

    /// <summary>
    /// Confidence score for this prediction (0-100)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Description of the impact
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
