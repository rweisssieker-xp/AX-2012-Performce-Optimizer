namespace AX2012PerformanceOptimizer.Core.Models;

/// <summary>
/// Represents a recommendation for changing a SQL Server configuration setting
/// </summary>
public class ConfigurationRecommendation
{
    /// <summary>
    /// Configuration setting name
    /// </summary>
    public string SettingName { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the setting
    /// </summary>
    public int CurrentValue { get; set; }

    /// <summary>
    /// Recommended value
    /// </summary>
    public int RecommendedValue { get; set; }

    /// <summary>
    /// Priority of this recommendation (Critical, High, Medium, Low)
    /// </summary>
    public RecommendationPriority Priority { get; set; }

    /// <summary>
    /// Reason for the recommendation
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Expected impact of applying this recommendation
    /// </summary>
    public string ExpectedImpact { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description and considerations
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// SQL script to apply this recommendation
    /// </summary>
    public string ApplyScript { get; set; } = string.Empty;

    /// <summary>
    /// Whether this change requires a SQL Server restart
    /// </summary>
    public bool RequiresRestart { get; set; }

    /// <summary>
    /// Category of the recommendation
    /// </summary>
    public RecommendationCategory Category { get; set; }

    /// <summary>
    /// Estimated impact score (0-100)
    /// </summary>
    public int ImpactScore { get; set; }
}
