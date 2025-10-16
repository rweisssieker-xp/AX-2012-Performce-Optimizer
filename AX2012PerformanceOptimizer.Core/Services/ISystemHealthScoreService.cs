using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for calculating overall system health score
/// </summary>
public interface ISystemHealthScoreService
{
    /// <summary>
    /// Calculate the current system health score
    /// </summary>
    Task<SystemHealthScore> CalculateHealthScoreAsync();

    /// <summary>
    /// Get health score history for trending
    /// </summary>
    Task<List<HealthScoreHistory>> GetHealthScoreHistoryAsync(int days = 30);

    /// <summary>
    /// Save current health score to history
    /// </summary>
    Task SaveHealthScoreToHistoryAsync(SystemHealthScore score);

    /// <summary>
    /// Get recommended actions sorted by impact
    /// </summary>
    Task<List<HealthAction>> GetPrioritizedActionsAsync();

    /// <summary>
    /// Calculate score for SQL Performance category
    /// </summary>
    Task<HealthCategory> CalculateSqlPerformanceScoreAsync();

    /// <summary>
    /// Calculate score for Index Health category
    /// </summary>
    Task<HealthCategory> CalculateIndexHealthScoreAsync();

    /// <summary>
    /// Calculate score for Batch Jobs category
    /// </summary>
    Task<HealthCategory> CalculateBatchJobsScoreAsync();

    /// <summary>
    /// Calculate score for Database Size category
    /// </summary>
    Task<HealthCategory> CalculateDatabaseSizeScoreAsync();
}
