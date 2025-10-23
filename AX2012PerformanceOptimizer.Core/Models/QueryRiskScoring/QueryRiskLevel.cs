namespace AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;

/// <summary>
/// Risk level categories for query execution
/// </summary>
public enum QueryRiskLevel
{
    /// <summary>
    /// Low risk - Safe to execute
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium risk - Requires review
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High risk - Proceed with caution
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical risk - Should not be executed
    /// </summary>
    Critical = 3
}
