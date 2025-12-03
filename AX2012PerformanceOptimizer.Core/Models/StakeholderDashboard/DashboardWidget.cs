namespace AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;

/// <summary>
/// Represents a dashboard widget
/// </summary>
public class DashboardWidget
{
    /// <summary>
    /// Unique identifier for the widget
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of widget
    /// </summary>
    public WidgetType Type { get; set; }

    /// <summary>
    /// Title of the widget
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the widget
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Widget data (varies by widget type)
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Widget position/order
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Whether widget is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Widget size (Small, Medium, Large)
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

/// <summary>
/// Widget size options
/// </summary>
public enum WidgetSize
{
    Small = 0,
    Medium = 1,
    Large = 2
}
