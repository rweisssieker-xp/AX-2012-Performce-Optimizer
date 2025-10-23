namespace AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

/// <summary>
/// Result of therapy diagnosis
/// </summary>
public class DiagnosisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<SystemSymptom> Symptoms { get; set; } = new();
    public List<string> RootCauses { get; set; } = new();
    public List<string> ImmediateActions { get; set; } = new();
    public List<string> LongTermRecommendations { get; set; } = new();
    public Dictionary<string, string> TreatmentPlan { get; set; } = new();
    public List<string> PreventiveMeasures { get; set; } = new();
    public string Prognosis { get; set; } = string.Empty;
    public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;
    public int SessionDuration { get; set; } // in seconds
}
