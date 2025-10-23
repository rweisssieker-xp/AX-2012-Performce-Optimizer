namespace AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

/// <summary>
/// System symptom detected during diagnosis
/// </summary>
public class SystemSymptom
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical
    public double ImpactScore { get; set; }
    public List<string> Indicators { get; set; } = new();
    public List<string> PossibleCauses { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public Dictionary<string, object> MetricValues { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}
