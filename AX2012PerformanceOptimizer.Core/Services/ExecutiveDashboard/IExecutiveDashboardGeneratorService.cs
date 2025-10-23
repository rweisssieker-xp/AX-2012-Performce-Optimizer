using AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

namespace AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;

/// <summary>
/// Service for AI-powered executive dashboard generation
/// </summary>
public interface IExecutiveDashboardGeneratorService
{
    /// <summary>
    /// Generate an executive dashboard for a specific stakeholder
    /// </summary>
    Task<DashboardGenerationResult> GenerateDashboardAsync(
        DashboardGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available stakeholder types
    /// </summary>
    Task<List<StakeholderType>> GetAvailableStakeholderTypesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recommended dashboard for current system state
    /// </summary>
    Task<DashboardGenerationResult> GetRecommendedDashboardAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export dashboard to HTML
    /// </summary>
    Task<string> ExportDashboardToHtmlAsync(
        Models.ExecutiveDashboard.ExecutiveDashboard dashboard,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export dashboard to Markdown
    /// </summary>
    Task<string> ExportDashboardToMarkdownAsync(
        Models.ExecutiveDashboard.ExecutiveDashboard dashboard,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh dashboard data
    /// </summary>
    Task<DashboardGenerationResult> RefreshDashboardAsync(
        string dashboardId,
        CancellationToken cancellationToken = default);
}
