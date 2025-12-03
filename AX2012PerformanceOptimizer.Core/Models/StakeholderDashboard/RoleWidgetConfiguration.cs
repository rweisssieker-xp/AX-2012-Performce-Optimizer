namespace AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;

/// <summary>
/// Widget configuration for a specific role
/// </summary>
public class RoleWidgetConfiguration
{
    /// <summary>
    /// User role
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// List of enabled widget types for this role
    /// </summary>
    public List<WidgetType> EnabledWidgets { get; set; } = new();

    /// <summary>
    /// Widget-specific settings
    /// </summary>
    public Dictionary<WidgetType, WidgetSettings> WidgetSettings { get; set; } = new();

    /// <summary>
    /// Default widget layout (grid positions)
    /// </summary>
    public Dictionary<WidgetType, WidgetLayout> WidgetLayouts { get; set; } = new();
}

/// <summary>
/// Settings for a specific widget
/// </summary>
public class WidgetSettings
{
    /// <summary>
    /// Widget size
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Medium;

    /// <summary>
    /// Whether widget is collapsible
    /// </summary>
    public bool IsCollapsible { get; set; } = true;

    /// <summary>
    /// Refresh interval in seconds
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Custom settings (key-value pairs)
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Layout information for a widget
/// </summary>
public class WidgetLayout
{
    /// <summary>
    /// Grid row position
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Grid column position
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Row span
    /// </summary>
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// Column span
    /// </summary>
    public int ColumnSpan { get; set; } = 1;
}
