namespace AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;

/// <summary>
/// Comprehensive risk score for a SQL query
/// </summary>
public class QueryRiskScore
{
    /// <summary>
    /// Overall risk score (0-100)
    /// 0-25: Low, 26-50: Medium, 51-75: High, 76-100: Critical
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Risk level category
    /// </summary>
    public QueryRiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Individual risk factors that contribute to the overall score
    /// </summary>
    public List<QueryRiskFactor> RiskFactors { get; set; } = new();

    /// <summary>
    /// Performance risk score (0-100)
    /// </summary>
    public double PerformanceRisk { get; set; }

    /// <summary>
    /// Security risk score (0-100)
    /// </summary>
    public double SecurityRisk { get; set; }

    /// <summary>
    /// Complexity risk score (0-100)
    /// </summary>
    public double ComplexityRisk { get; set; }

    /// <summary>
    /// Resource usage risk score (0-100)
    /// </summary>
    public double ResourceRisk { get; set; }

    /// <summary>
    /// Data integrity risk score (0-100)
    /// </summary>
    public double DataIntegrityRisk { get; set; }

    /// <summary>
    /// Confidence level in this risk assessment (0-100)
    /// </summary>
    public double ConfidenceLevel { get; set; }

    /// <summary>
    /// The SQL query being assessed
    /// </summary>
    public string SqlQuery { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the risk was calculated
    /// </summary>
    public DateTime AssessmentTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estimated execution time based on risk factors (milliseconds)
    /// </summary>
    public double EstimatedExecutionTimeMs { get; set; }

    /// <summary>
    /// Estimated CPU usage percentage
    /// </summary>
    public double EstimatedCpuUsagePercent { get; set; }

    /// <summary>
    /// Estimated memory usage in MB
    /// </summary>
    public double EstimatedMemoryUsageMb { get; set; }

    /// <summary>
    /// Whether this query should be executed
    /// </summary>
    public bool ShouldExecute => RiskLevel != QueryRiskLevel.Critical;

    /// <summary>
    /// Whether this query requires review before execution
    /// </summary>
    public bool RequiresReview => RiskLevel >= QueryRiskLevel.Medium;

    /// <summary>
    /// Color code for UI display
    /// </summary>
    public string ColorCode => RiskLevel switch
    {
        QueryRiskLevel.Low => "#4CAF50",      // Green
        QueryRiskLevel.Medium => "#FF9800",   // Orange
        QueryRiskLevel.High => "#F44336",     // Red
        QueryRiskLevel.Critical => "#9C27B0", // Purple
        _ => "#757575"                         // Gray
    };

    /// <summary>
    /// Icon for UI display
    /// </summary>
    public string Icon => RiskLevel switch
    {
        QueryRiskLevel.Low => "✅",
        QueryRiskLevel.Medium => "⚠️",
        QueryRiskLevel.High => "🔴",
        QueryRiskLevel.Critical => "🚨",
        _ => "❓"
    };
}
