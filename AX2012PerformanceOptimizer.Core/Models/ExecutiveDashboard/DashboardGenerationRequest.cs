namespace AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

/// <summary>
/// Request to generate an executive dashboard
/// </summary>
public class DashboardGenerationRequest
{
    /// <summary>
    /// Target stakeholder
    /// </summary>
    public StakeholderType StakeholderType { get; set; }

    /// <summary>
    /// Time range for data (in days)
    /// </summary>
    public int TimeRangeDays { get; set; } = 7;

    /// <summary>
    /// Include cost analysis
    /// </summary>
    public bool IncludeCostAnalysis { get; set; } = true;

    /// <summary>
    /// Include trend analysis
    /// </summary>
    public bool IncludeTrendAnalysis { get; set; } = true;

    /// <summary>
    /// Include recommendations
    /// </summary>
    public bool IncludeRecommendations { get; set; } = true;

    /// <summary>
    /// Include drill-down links
    /// </summary>
    public bool IncludeDrillDown { get; set; } = true;

    /// <summary>
    /// Maximum number of widgets
    /// </summary>
    public int MaxWidgets { get; set; } = 12;

    /// <summary>
    /// Dashboard layout preference
    /// </summary>
    public string PreferredLayout { get; set; } = "3x4";

    /// <summary>
    /// Focus areas (optional filters)
    /// </summary>
    public List<string> FocusAreas { get; set; } = new();

    /// <summary>
    /// Custom title (optional)
    /// </summary>
    public string CustomTitle { get; set; } = string.Empty;

    /// <summary>
    /// Generate export-ready formats
    /// </summary>
    public bool GenerateExportFormats { get; set; } = true;
}
