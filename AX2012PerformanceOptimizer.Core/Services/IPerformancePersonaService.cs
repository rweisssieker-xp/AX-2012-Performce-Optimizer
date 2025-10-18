using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Expert AI personas trained on best practices from top AX consultants
/// </summary>
public interface IPerformancePersonaService
{
    Task<List<PerformancePersona>> GetAvailablePersonasAsync();
    Task<ExpertRecommendation> GetExpertAdviceAsync(string personaId, PerformanceProblem problem);
    Task<ConsensusRecommendation> GetConsensusAdviceAsync(PerformanceProblem problem);
}
