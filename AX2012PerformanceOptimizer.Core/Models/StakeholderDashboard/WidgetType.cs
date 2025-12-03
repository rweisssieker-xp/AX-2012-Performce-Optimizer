namespace AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;

/// <summary>
/// Types of dashboard widgets
/// </summary>
public enum WidgetType
{
    /// <summary>
    /// Cost impact widget
    /// </summary>
    CostImpact = 0,

    /// <summary>
    /// ROI widget
    /// </summary>
    ROI = 1,

    /// <summary>
    /// System health score widget
    /// </summary>
    SystemHealthScore = 2,

    /// <summary>
    /// Query performance widget
    /// </summary>
    QueryPerformance = 3,

    /// <summary>
    /// Database health widget
    /// </summary>
    DatabaseHealth = 4,

    /// <summary>
    /// Code performance widget
    /// </summary>
    CodePerformance = 5,

    /// <summary>
    /// User experience widget
    /// </summary>
    UserExperience = 6,

    /// <summary>
    /// System availability widget
    /// </summary>
    SystemAvailability = 7,

    /// <summary>
    /// Action items widget
    /// </summary>
    ActionItems = 8,

    /// <summary>
    /// Trend visualization widget
    /// </summary>
    TrendVisualization = 9,

    /// <summary>
    /// Summary card widget
    /// </summary>
    SummaryCard = 10
}
