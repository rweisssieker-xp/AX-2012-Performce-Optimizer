namespace AX2012PerformanceOptimizer.Core.Models;

public class PerformancePersona
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExpertName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double SuccessRate { get; set; }
    public List<string> KnownPatterns { get; set; } = new();
    public string Avatar { get; set; } = string.Empty; // Emoji or icon
}

public class ExpertRecommendation
{
    public string PersonaId { get; set; } = string.Empty;
    public string ExpertName { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public string Caution { get; set; } = string.Empty;
}

public class ConsensusRecommendation
{
    public List<ExpertRecommendation> ExpertOpinions { get; set; } = new();
    public string ConsensusAdvice { get; set; } = string.Empty;
    public double AgreementScore { get; set; }
    public string MinorityOpinion { get; set; } = string.Empty;
}
