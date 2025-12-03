using AX2012PerformanceOptimizer.Core.Models.QuickFix;

namespace AX2012PerformanceOptimizer.Core.Services.QuickFix;

/// <summary>
/// Service for rapid quick fix analysis and application
/// </summary>
public interface IQuickFixService
{
    /// <summary>
    /// Analyzes system and returns quick fixes (must complete within 30 seconds)
    /// </summary>
    Task<QuickFixAnalysisResult> AnalyzeQuickFixesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a quick fix
    /// </summary>
    Task<ApplyResult> ApplyQuickFixAsync(string fixId);

    /// <summary>
    /// Checks if a fix can be applied directly without confirmation
    /// </summary>
    Task<bool> CanApplyDirectlyAsync(string fixId);

    /// <summary>
    /// Rolls back a previously applied fix
    /// </summary>
    Task<ApplyResult> RollbackFixAsync(string fixId);
}
