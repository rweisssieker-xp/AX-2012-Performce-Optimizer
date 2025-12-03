namespace AX2012PerformanceOptimizer.Core.Models.QuickFix;

/// <summary>
/// Result of applying a quick fix
/// </summary>
public class ApplyResult
{
    /// <summary>
    /// Whether the fix was applied successfully
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error message if application failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when fix was applied
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the fix that was applied
    /// </summary>
    public string FixId { get; set; } = string.Empty;

    /// <summary>
    /// Whether rollback is available
    /// </summary>
    public bool CanRollback { get; set; }

    /// <summary>
    /// Rollback script if available
    /// </summary>
    public string? RollbackScript { get; set; }
}
