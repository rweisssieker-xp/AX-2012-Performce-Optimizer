using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for automatically healing queries that have performance issues
/// </summary>
public interface ISelfHealingQueryService
{
    /// <summary>
    /// Analyze query and attempt automatic healing
    /// </summary>
    Task<HealingResult> HealQueryAsync(
        SqlQueryMetric query,
        HealingOptions options);

    /// <summary>
    /// Test if a healed query is actually better
    /// </summary>
    Task<HealingValidation> ValidateHealingAsync(
        string originalQuery,
        string healedQuery,
        SqlQueryMetric originalMetrics);

    /// <summary>
    /// Get healing recommendations without applying
    /// </summary>
    Task<List<HealingRecommendation>> GetHealingRecommendationsAsync(
        SqlQueryMetric query);

    /// <summary>
    /// Rollback a healing if it made things worse
    /// </summary>
    Task<RollbackResult> RollbackHealingAsync(string queryHash);

    /// <summary>
    /// Get healing history for a query
    /// </summary>
    Task<HealingHistory> GetHealingHistoryAsync(string queryHash);

    /// <summary>
    /// Enable/disable auto-healing for specific queries
    /// </summary>
    Task EnableAutoHealingAsync(string queryHash, bool enable);
}

/// <summary>
/// Options for healing behavior
/// </summary>
public class HealingOptions
{
    /// <summary>
    /// Automatically apply healing without user approval
    /// </summary>
    public bool AutoApply { get; set; } = false;

    /// <summary>
    /// Require user approval before healing
    /// </summary>
    public bool RequireApproval { get; set; } = true;

    /// <summary>
    /// Automatically rollback if healing makes things worse
    /// </summary>
    public bool AutoRollback { get; set; } = true;

    /// <summary>
    /// Maximum performance degradation allowed (%)
    /// </summary>
    public double MaxDegradationPercent { get; set; } = 5.0;

    /// <summary>
    /// Minimum improvement required to keep healing (%)
    /// </summary>
    public double MinImprovementPercent { get; set; } = 10.0;

    /// <summary>
    /// Enable learning from successful healings
    /// </summary>
    public bool EnableLearning { get; set; } = true;

    /// <summary>
    /// Test healed query before applying
    /// </summary>
    public bool TestBeforeApply { get; set; } = true;
}

/// <summary>
/// Result of a healing attempt
/// </summary>
public class HealingResult
{
    public string QueryHash { get; set; } = string.Empty;
    public DateTime HealingDate { get; set; }
    public bool Success { get; set; }
    public bool Applied { get; set; }
    public bool RequiresApproval { get; set; }

    // Original query
    public string OriginalQuery { get; set; } = string.Empty;
    public SqlQueryMetric OriginalMetrics { get; set; } = new();

    // Healed query
    public string HealedQuery { get; set; } = string.Empty;
    public SqlQueryMetric PredictedMetrics { get; set; } = new();

    // Healing actions
    public List<HealingAction> ActionsApplied { get; set; } = new();

    // Performance impact
    public double ImprovementPercent { get; set; }
    public double TimeReduction { get; set; }
    public string ImpactLevel { get; set; } = string.Empty; // Minor, Moderate, Significant, Major

    // Status
    public string Status { get; set; } = string.Empty; // Pending, Applied, Rejected, RolledBack
    public string Message { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual healing action
/// </summary>
public class HealingAction
{
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double EstimatedImpact { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // Safe, Low, Medium, High

    // Before/After
    public string Before { get; set; } = string.Empty;
    public string After { get; set; } = string.Empty;

    // Results
    public bool Applied { get; set; }
    public double ActualImpact { get; set; }
}

/// <summary>
/// Validation of healing effectiveness
/// </summary>
public class HealingValidation
{
    public string QueryHash { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public bool IsBetter { get; set; }

    // Original metrics
    public double OriginalAvgElapsedTime { get; set; }
    public long OriginalExecutionCount { get; set; }

    // Healed metrics
    public double HealedAvgElapsedTime { get; set; }
    public long HealedExecutionCount { get; set; }

    // Comparison
    public double ImprovementPercent { get; set; }
    public double TimeReduction { get; set; }

    // Validation checks
    public List<ValidationCheck> Checks { get; set; } = new();

    // Decision
    public string Recommendation { get; set; } = string.Empty; // Keep, Rollback, Monitor
    public string Reason { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual validation check
/// </summary>
public class ValidationCheck
{
    public string CheckName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Healing recommendation
/// </summary>
public class HealingRecommendation
{
    public string ActionType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public double EstimatedImprovementPercent { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // Critical, High, Medium, Low

    // Implementation details
    public string Implementation { get; set; } = string.Empty;
    public int EstimatedEffortMinutes { get; set; }

    // Dependencies
    public List<string> RequiredPermissions { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
}

/// <summary>
/// Rollback result
/// </summary>
public class RollbackResult
{
    public string QueryHash { get; set; } = string.Empty;
    public DateTime RollbackDate { get; set; }
    public bool Success { get; set; }

    public string OriginalQuery { get; set; } = string.Empty;
    public string RolledBackQuery { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Healing history for a query
/// </summary>
public class HealingHistory
{
    public string QueryHash { get; set; } = string.Empty;
    public int TotalHealings { get; set; }
    public int SuccessfulHealings { get; set; }
    public int FailedHealings { get; set; }
    public int RolledBack { get; set; }

    // Performance trends
    public double InitialAvgElapsedTime { get; set; }
    public double CurrentAvgElapsedTime { get; set; }
    public double TotalImprovementPercent { get; set; }

    // History entries
    public List<HealingHistoryEntry> Entries { get; set; } = new();

    // Learning data
    public List<string> SuccessfulPatterns { get; set; } = new();
    public List<string> FailedPatterns { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual history entry
/// </summary>
public class HealingHistoryEntry
{
    public DateTime Date { get; set; }
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public double ImprovementPercent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
