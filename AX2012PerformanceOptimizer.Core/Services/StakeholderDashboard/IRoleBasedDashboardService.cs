using AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;

namespace AX2012PerformanceOptimizer.Core.Services.StakeholderDashboard;

/// <summary>
/// Service for role-based dashboard data
/// </summary>
public interface IRoleBasedDashboardService
{
    /// <summary>
    /// Gets dashboard data for a specific role
    /// </summary>
    Task<RoleBasedDashboardData> GetDashboardDataAsync(UserRole role, TimeRange timeRange);

    /// <summary>
    /// Gets role-specific widgets
    /// </summary>
    Task<List<DashboardWidget>> GetRoleSpecificWidgetsAsync(UserRole role);

    /// <summary>
    /// Gets widget configuration for a role
    /// </summary>
    Task<RoleWidgetConfiguration> GetRoleConfigurationAsync(UserRole role);

    /// <summary>
    /// Gets available roles
    /// </summary>
    List<UserRole> GetAvailableRoles();
}
