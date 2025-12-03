namespace AX2012PerformanceOptimizer.Core.Models.QuickFix;

/// <summary>
/// Result of quick fix analysis
/// </summary>
public class QuickFixAnalysisResult
{
    /// <summary>
    /// Date and time when analysis was performed
    /// </summary>
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the analysis
    /// </summary>
    public TimeSpan AnalysisDuration { get; set; }

    /// <summary>
    /// List of quick fixes found
    /// </summary>
    public List<QuickFix> QuickFixes { get; set; } = new();

    /// <summary>
    /// Summary of the analysis
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Whether analysis completed successfully
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
