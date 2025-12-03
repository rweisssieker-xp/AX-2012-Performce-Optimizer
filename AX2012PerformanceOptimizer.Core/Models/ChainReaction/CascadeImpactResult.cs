namespace AX2012PerformanceOptimizer.Core.Models.ChainReaction;

/// <summary>
/// Result of cascade impact prediction for a query optimization
/// </summary>
public class CascadeImpactResult
{
    /// <summary>
    /// Hash of the source query being optimized
    /// </summary>
    public string SourceQueryHash { get; set; } = string.Empty;

    /// <summary>
    /// Type of optimization applied
    /// </summary>
    public string OptimizationType { get; set; } = string.Empty;

    /// <summary>
    /// List of queries affected by this optimization
    /// </summary>
    public List<QueryImpact> AffectedQueries { get; set; } = new();

    /// <summary>
    /// Total time saved across all affected queries (in milliseconds)
    /// </summary>
    public double TotalTimeSaved { get; set; }

    /// <summary>
    /// Total time improved (sum of improvements, in milliseconds)
    /// </summary>
    public double TotalTimeImproved { get; set; }

    /// <summary>
    /// Number of queries affected
    /// </summary>
    public int QueriesAffected { get; set; }

    /// <summary>
    /// Overall confidence score for this prediction (0-100)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Summary description of the cascade impact
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when prediction was made
    /// </summary>
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}
