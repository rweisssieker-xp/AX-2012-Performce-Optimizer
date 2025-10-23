namespace AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

/// <summary>
/// Individual widget on an executive dashboard
/// </summary>
public class DashboardWidget
{
    /// <summary>
    /// Unique widget identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Widget title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Widget description/subtitle
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of widget
    /// </summary>
    public WidgetType Type { get; set; }

    /// <summary>
    /// Primary value to display
    /// </summary>
    public string PrimaryValue { get; set; } = string.Empty;

    /// <summary>
    /// Secondary value (optional)
    /// </summary>
    public string SecondaryValue { get; set; } = string.Empty;

    /// <summary>
    /// Trend indicator (-1 = down, 0 = stable, 1 = up)
    /// </summary>
    public int TrendDirection { get; set; }

    /// <summary>
    /// Trend percentage change
    /// </summary>
    public double TrendPercentage { get; set; }

    /// <summary>
    /// Is this trend positive or negative for business
    /// </summary>
    public bool IsTrendPositive { get; set; }

    /// <summary>
    /// Priority/importance (1-10)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Status (OK, Warning, Critical)
    /// </summary>
    public string Status { get; set; } = "OK";

    /// <summary>
    /// Color code for the widget
    /// </summary>
    public string ColorCode { get; set; } = "#4CAF50";

    /// <summary>
    /// Icon for the widget
    /// </summary>
    public string Icon { get; set; } = "📊";

    /// <summary>
    /// Additional data for charts/tables (JSON)
    /// </summary>
    public string DataJson { get; set; } = string.Empty;

    /// <summary>
    /// Action recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Related metrics
    /// </summary>
    public Dictionary<string, string> RelatedMetrics { get; set; } = new();

    /// <summary>
    /// Grid position (row)
    /// </summary>
    public int GridRow { get; set; }

    /// <summary>
    /// Grid position (column)
    /// </summary>
    public int GridColumn { get; set; }

    /// <summary>
    /// Grid span (rows)
    /// </summary>
    public int GridRowSpan { get; set; } = 1;

    /// <summary>
    /// Grid span (columns)
    /// </summary>
    public int GridColumnSpan { get; set; } = 1;

    /// <summary>
    /// Drill-down query or link
    /// </summary>
    public string DrillDownLink { get; set; } = string.Empty;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
