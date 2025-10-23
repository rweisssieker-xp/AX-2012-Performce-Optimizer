namespace AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

/// <summary>
/// Complete executive dashboard
/// </summary>
public class ExecutiveDashboard
{
    /// <summary>
    /// Dashboard ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Dashboard title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Dashboard subtitle/description
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Target stakeholder type
    /// </summary>
    public StakeholderType StakeholderType { get; set; }

    /// <summary>
    /// Time period covered
    /// </summary>
    public string TimePeriod { get; set; } = string.Empty;

    /// <summary>
    /// Generated timestamp
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Dashboard widgets
    /// </summary>
    public List<DashboardWidget> Widgets { get; set; } = new();

    /// <summary>
    /// Executive summary
    /// </summary>
    public string ExecutiveSummary { get; set; } = string.Empty;

    /// <summary>
    /// Key insights
    /// </summary>
    public List<string> KeyInsights { get; set; } = new();

    /// <summary>
    /// Top priorities/action items
    /// </summary>
    public List<string> ActionItems { get; set; } = new();

    /// <summary>
    /// Overall health score (0-100)
    /// </summary>
    public double OverallHealthScore { get; set; }

    /// <summary>
    /// Health trend
    /// </summary>
    public string HealthTrend { get; set; } = "Stable";

    /// <summary>
    /// Critical alerts count
    /// </summary>
    public int CriticalAlertsCount { get; set; }

    /// <summary>
    /// Warnings count
    /// </summary>
    public int WarningsCount { get; set; }

    /// <summary>
    /// Estimated cost impact
    /// </summary>
    public double EstimatedCostImpact { get; set; }

    /// <summary>
    /// Potential savings
    /// </summary>
    public double PotentialSavings { get; set; }

    /// <summary>
    /// Performance improvement opportunities
    /// </summary>
    public List<string> ImprovementOpportunities { get; set; } = new();

    /// <summary>
    /// Data sources used
    /// </summary>
    public List<string> DataSources { get; set; } = new();

    /// <summary>
    /// Report format metadata
    /// </summary>
    public Dictionary<string, string> ReportMetadata { get; set; } = new();

    /// <summary>
    /// Dashboard layout (grid rows x columns)
    /// </summary>
    public string Layout { get; set; } = "3x3";

    /// <summary>
    /// Export-ready HTML
    /// </summary>
    public string HtmlExport { get; set; } = string.Empty;

    /// <summary>
    /// Export-ready Markdown
    /// </summary>
    public string MarkdownExport { get; set; } = string.Empty;
}
