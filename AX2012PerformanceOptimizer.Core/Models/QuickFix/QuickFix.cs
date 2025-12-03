namespace AX2012PerformanceOptimizer.Core.Models.QuickFix;

/// <summary>
/// Represents a quick fix suggestion
/// </summary>
public class QuickFix
{
    /// <summary>
    /// Unique identifier for the fix
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the fix
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the fix
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of quick fix
    /// </summary>
    public QuickFixType Type { get; set; }

    /// <summary>
    /// Impact score (0-100) indicating expected improvement
    /// </summary>
    public double Impact { get; set; }

    /// <summary>
    /// Effort score (0-100) indicating implementation difficulty
    /// </summary>
    public double Effort { get; set; }

    /// <summary>
    /// Confidence score (0-100) indicating prediction accuracy
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Whether this fix can be applied directly without confirmation
    /// </summary>
    public bool CanApplyDirectly { get; set; }

    /// <summary>
    /// SQL script to apply the fix
    /// </summary>
    public string SqlScript { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time saved (human-readable string)
    /// </summary>
    public string EstimatedTimeSaved { get; set; } = string.Empty;

    /// <summary>
    /// Related query hash or object identifier
    /// </summary>
    public string? RelatedObjectId { get; set; }

    /// <summary>
    /// Priority level
    /// </summary>
    public QuickFixPriority Priority { get; set; } = QuickFixPriority.Medium;
}

/// <summary>
/// Priority levels for quick fixes
/// </summary>
public enum QuickFixPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
