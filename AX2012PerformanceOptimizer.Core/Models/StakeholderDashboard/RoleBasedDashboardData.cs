namespace AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;

/// <summary>
/// Role-based dashboard data
/// </summary>
public class RoleBasedDashboardData
{
    /// <summary>
    /// Selected user role
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// List of widgets for this role
    /// </summary>
    public List<DashboardWidget> Widgets { get; set; } = new();

    /// <summary>
    /// Dashboard summary for this role
    /// </summary>
    public DashboardSummary Summary { get; set; } = new();

    /// <summary>
    /// Action items for this role
    /// </summary>
    public List<ActionItem> ActionItems { get; set; } = new();

    /// <summary>
    /// Role-specific metrics (key-value pairs)
    /// </summary>
    public Dictionary<string, object> RoleSpecificMetrics { get; set; } = new();

    /// <summary>
    /// Time range for the data
    /// </summary>
    public TimeRange TimeRange { get; set; } = new();

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dashboard summary
/// </summary>
public class DashboardSummary
{
    /// <summary>
    /// Summary title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Summary description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Key metrics (key-value pairs)
    /// </summary>
    public Dictionary<string, string> KeyMetrics { get; set; } = new();

    /// <summary>
    /// Overall status (Good/Fair/Poor)
    /// </summary>
    public string Status { get; set; } = "Good";
}

/// <summary>
/// Action item for dashboard
/// </summary>
public class ActionItem
{
    /// <summary>
    /// Action item ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Action title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Action description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Priority (High/Medium/Low)
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Due date (optional)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Related query or object ID
    /// </summary>
    public string? RelatedObjectId { get; set; }
}

/// <summary>
/// Time range for dashboard data
/// </summary>
public class TimeRange
{
    /// <summary>
    /// Start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(-24);

    /// <summary>
    /// End time
    /// </summary>
    public DateTime EndTime { get; set; } = DateTime.UtcNow;
}
