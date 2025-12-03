namespace AX2012PerformanceOptimizer.Core.Models.ChainReaction;

/// <summary>
/// Type of impact that an optimization has on dependent queries
/// </summary>
public enum ImpactType
{
    /// <summary>
    /// Positive impact - query performance improves
    /// </summary>
    Positive = 0,

    /// <summary>
    /// Neutral impact - no significant change
    /// </summary>
    Neutral = 1,

    /// <summary>
    /// Negative impact - query performance degrades
    /// </summary>
    Negative = 2
}
