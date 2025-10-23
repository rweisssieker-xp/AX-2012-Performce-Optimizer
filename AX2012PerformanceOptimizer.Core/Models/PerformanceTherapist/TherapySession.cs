namespace AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

/// <summary>
/// Interactive therapy session for system diagnosis
/// </summary>
public class TherapySession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<TherapyMessage> Messages { get; set; } = new();
    public List<SystemSymptom> DetectedSymptoms { get; set; } = new();
    public DiagnosisResult? Diagnosis { get; set; }
    public Dictionary<string, object> SessionContext { get; set; } = new();
    public int MessageCount => Messages.Count;
    public string CurrentPhase { get; set; } = "Introduction"; // Introduction, Exploration, Diagnosis, Treatment
}
