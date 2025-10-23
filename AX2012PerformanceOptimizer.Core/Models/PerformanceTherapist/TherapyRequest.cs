namespace AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

/// <summary>
/// Request to start or continue therapy session
/// </summary>
public class TherapyRequest
{
    public string? SessionId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public bool IncludeSystemMetrics { get; set; } = true;
    public bool IncludeHistoricalData { get; set; } = true;
    public List<string> FocusAreas { get; set; } = new();
}
