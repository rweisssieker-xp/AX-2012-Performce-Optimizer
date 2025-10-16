namespace AX2012PerformanceOptimizer.Core.Models;

/// <summary>
/// Represents the overall system health score and breakdown
/// </summary>
public class SystemHealthScore
{
    /// <summary>
    /// Overall health score (0-100)
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Score from last calculation (for trend)
    /// </summary>
    public int PreviousScore { get; set; }

    /// <summary>
    /// Change in points since last calculation
    /// </summary>
    public int ScoreChange => OverallScore - PreviousScore;

    /// <summary>
    /// Timestamp of this health check
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// SQL Performance sub-score
    /// </summary>
    public HealthCategory SqlPerformance { get; set; } = new();

    /// <summary>
    /// Index Health sub-score
    /// </summary>
    public HealthCategory IndexHealth { get; set; } = new();

    /// <summary>
    /// Batch Jobs sub-score
    /// </summary>
    public HealthCategory BatchJobs { get; set; } = new();

    /// <summary>
    /// Database Size/Growth sub-score
    /// </summary>
    public HealthCategory DatabaseSize { get; set; } = new();

    /// <summary>
    /// Overall health status
    /// </summary>
    public HealthStatus Status => OverallScore switch
    {
        >= 90 => HealthStatus.Excellent,
        >= 75 => HealthStatus.Good,
        >= 60 => HealthStatus.Fair,
        >= 40 => HealthStatus.Poor,
        _ => HealthStatus.Critical
    };

    /// <summary>
    /// Top recommended action for biggest impact
    /// </summary>
    public HealthAction? TopImpactAction { get; set; }

    /// <summary>
    /// All recommended actions sorted by impact
    /// </summary>
    public List<HealthAction> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Individual health category score
/// </summary>
public class HealthCategory
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Weight { get; set; } = 25; // Default weight 25% (4 categories)
    public HealthStatus Status => Score switch
    {
        >= 90 => HealthStatus.Excellent,
        >= 75 => HealthStatus.Good,
        >= 60 => HealthStatus.Fair,
        >= 40 => HealthStatus.Poor,
        _ => HealthStatus.Critical
    };
    public string StatusText => Status.ToString();
    public string StatusColor => Status switch
    {
        HealthStatus.Excellent => "#4CAF50", // Green
        HealthStatus.Good => "#8BC34A",      // Light Green
        HealthStatus.Fair => "#FF9800",      // Orange
        HealthStatus.Poor => "#FF5722",      // Deep Orange
        HealthStatus.Critical => "#F44336",  // Red
        _ => "#9E9E9E"                       // Gray
    };
    public string StatusIcon => Status switch
    {
        HealthStatus.Excellent => "✅",
        HealthStatus.Good => "✅",
        HealthStatus.Fair => "⚠️",
        HealthStatus.Poor => "❌",
        HealthStatus.Critical => "❌",
        _ => "⚪"
    };
    public List<string> Issues { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
}

/// <summary>
/// Recommended action to improve health score
/// </summary>
public class HealthAction
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int EstimatedScoreImpact { get; set; }
    public string Priority => EstimatedScoreImpact switch
    {
        >= 10 => "Critical",
        >= 7 => "High",
        >= 4 => "Medium",
        _ => "Low"
    };
    public string PriorityColor => Priority switch
    {
        "Critical" => "#F44336",
        "High" => "#FF9800",
        "Medium" => "#FFC107",
        _ => "#2196F3"
    };
    public string ActionType { get; set; } = string.Empty; // "Index", "Query", "Batch", "Database"
    public string? Script { get; set; }
    public double EstimatedTimeMinutes { get; set; }
    public bool IsAutomatable { get; set; }
}

/// <summary>
/// Health status levels
/// </summary>
public enum HealthStatus
{
    Critical = 0,  // 0-39
    Poor = 1,      // 40-59
    Fair = 2,      // 60-74
    Good = 3,      // 75-89
    Excellent = 4  // 90-100
}

/// <summary>
/// Historical health score entry for trending
/// </summary>
public class HealthScoreHistory
{
    public DateTime Timestamp { get; set; }
    public int OverallScore { get; set; }
    public int SqlPerformanceScore { get; set; }
    public int IndexHealthScore { get; set; }
    public int BatchJobsScore { get; set; }
    public int DatabaseSizeScore { get; set; }
}
