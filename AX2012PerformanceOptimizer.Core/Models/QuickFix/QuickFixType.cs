namespace AX2012PerformanceOptimizer.Core.Models.QuickFix;

/// <summary>
/// Types of quick fixes that can be applied
/// </summary>
public enum QuickFixType
{
    /// <summary>
    /// Create a new index
    /// </summary>
    CreateIndex = 0,

    /// <summary>
    /// Update statistics
    /// </summary>
    UpdateStatistics = 1,

    /// <summary>
    /// Rebuild an index
    /// </summary>
    RebuildIndex = 2,

    /// <summary>
    /// Clear cache
    /// </summary>
    ClearCache = 3,

    /// <summary>
    /// Kill a blocking query
    /// </summary>
    KillBlockingQuery = 4,

    /// <summary>
    /// Optimize a query
    /// </summary>
    OptimizeQuery = 5,

    /// <summary>
    /// Adjust configuration
    /// </summary>
    AdjustConfiguration = 6
}
