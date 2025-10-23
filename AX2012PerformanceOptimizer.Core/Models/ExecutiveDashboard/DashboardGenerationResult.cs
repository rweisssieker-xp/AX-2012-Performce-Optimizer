namespace AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

/// <summary>
/// Result of dashboard generation
/// </summary>
public class DashboardGenerationResult
{
    /// <summary>
    /// Whether generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Generated dashboard
    /// </summary>
    public ExecutiveDashboard? Dashboard { get; set; }

    /// <summary>
    /// Generation time in milliseconds
    /// </summary>
    public double GenerationTimeMs { get; set; }

    /// <summary>
    /// AI confidence in the generated dashboard (0-100)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Data quality score (0-100)
    /// </summary>
    public double DataQuality { get; set; }

    /// <summary>
    /// Warnings during generation
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Alternative dashboards (if any)
    /// </summary>
    public List<ExecutiveDashboard> AlternativeDashboards { get; set; } = new();
}
