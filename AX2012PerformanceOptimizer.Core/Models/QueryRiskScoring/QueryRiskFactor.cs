namespace AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;

/// <summary>
/// Individual risk factor contributing to overall query risk score
/// </summary>
public class QueryRiskFactor
{
    /// <summary>
    /// Name of the risk factor
    /// </summary>
    public string FactorName { get; set; } = string.Empty;

    /// <summary>
    /// Category of the risk (Performance, Security, Complexity, etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Risk score for this specific factor (0-100)
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Weight/importance of this factor in overall calculation (0-1)
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Weighted contribution to total risk
    /// </summary>
    public double WeightedScore => Score * Weight;

    /// <summary>
    /// Description of what this factor measures
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Evidence or reasoning for this risk score
    /// </summary>
    public string Evidence { get; set; } = string.Empty;

    /// <summary>
    /// Recommended action to mitigate this risk
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Impact level if this risk materializes
    /// </summary>
    public string ImpactLevel { get; set; } = string.Empty;
}
