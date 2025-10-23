namespace AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

/// <summary>
/// Response from therapy session
/// </summary>
public class TherapyResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TherapySession? Session { get; set; }
    public TherapyMessage? LatestMessage { get; set; }
    public List<string> SuggestedQuestions { get; set; } = new();
    public bool SessionComplete { get; set; }
    public DiagnosisResult? FinalDiagnosis { get; set; }
}
