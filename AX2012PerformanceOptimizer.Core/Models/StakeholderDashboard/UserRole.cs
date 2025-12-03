namespace AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;

/// <summary>
/// User roles for role-based dashboard views
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Executive role - Business-focused metrics
    /// </summary>
    Executive = 0,

    /// <summary>
    /// DBA role - Technical database metrics
    /// </summary>
    DBA = 1,

    /// <summary>
    /// Developer role - Code-level performance metrics
    /// </summary>
    Developer = 2,

    /// <summary>
    /// End-User role - User experience metrics
    /// </summary>
    EndUser = 3
}
