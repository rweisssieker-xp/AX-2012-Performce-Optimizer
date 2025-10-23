namespace AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

/// <summary>
/// Types of stakeholders for dashboard generation
/// </summary>
public enum StakeholderType
{
    /// <summary>
    /// Chief Executive Officer - High-level business impact
    /// </summary>
    CEO = 0,

    /// <summary>
    /// Chief Technology Officer - Technical performance metrics
    /// </summary>
    CTO = 1,

    /// <summary>
    /// Chief Financial Officer - Cost and ROI metrics
    /// </summary>
    CFO = 2,

    /// <summary>
    /// Chief Information Officer - IT operations and infrastructure
    /// </summary>
    CIO = 3,

    /// <summary>
    /// Database Administrator - Database health and optimization
    /// </summary>
    DBA = 4,

    /// <summary>
    /// Development Team - Code and query optimization
    /// </summary>
    Developer = 5,

    /// <summary>
    /// Operations Team - System stability and monitoring
    /// </summary>
    Operations = 6,

    /// <summary>
    /// Business Analyst - Business process performance
    /// </summary>
    BusinessAnalyst = 7,

    /// <summary>
    /// General/Custom dashboard
    /// </summary>
    General = 8
}
