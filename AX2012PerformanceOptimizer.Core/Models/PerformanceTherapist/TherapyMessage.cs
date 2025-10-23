namespace AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

/// <summary>
/// Message in therapy conversation
/// </summary>
public class TherapyMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> SuggestedResponses { get; set; } = new();
    public List<SystemSymptom> RelatedSymptoms { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool RequiresResponse { get; set; } = true;
}
