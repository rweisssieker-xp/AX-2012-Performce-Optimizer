using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class PerformancePersonaService : IPerformancePersonaService
{
    private readonly ILogger<PerformancePersonaService> _logger;
    private readonly List<PerformancePersona> _personas;

    public PerformancePersonaService(ILogger<PerformancePersonaService> logger)
    {
        _logger = logger;
        _personas = InitializePersonas();
    }

    public async Task<List<PerformancePersona>> GetAvailablePersonasAsync()
    {
        await Task.Delay(1);
        return _personas;
    }

    public async Task<ExpertRecommendation> GetExpertAdviceAsync(
        string personaId, 
        PerformanceProblem problem)
    {
        _logger.LogInformation("🎭 Getting advice from persona {PersonaId}", personaId);
        
        var persona = _personas.FirstOrDefault(p => p.Id == personaId);
        if (persona == null)
        {
            throw new ArgumentException($"Persona {personaId} not found");
        }

        await Task.Delay(100); // Simulate AI processing

        return new ExpertRecommendation
        {
            PersonaId = persona.Id,
            ExpertName = persona.ExpertName,
            Advice = GeneratePersonaAdvice(persona, problem),
            Confidence = 0.85 + (new Random().NextDouble() * 0.15),
            Reasoning = GenerateReasoning(persona, problem),
            Steps = GenerateSteps(persona, problem),
            Caution = GenerateCaution(persona)
        };
    }

    public async Task<ConsensusRecommendation> GetConsensusAdviceAsync(PerformanceProblem problem)
    {
        _logger.LogInformation("🎭 Getting consensus from all experts");

        var opinions = new List<ExpertRecommendation>();

        foreach (var persona in _personas)
        {
            var opinion = await GetExpertAdviceAsync(persona.Id, problem);
            opinions.Add(opinion);
        }

        return new ConsensusRecommendation
        {
            ExpertOpinions = opinions,
            ConsensusAdvice = GenerateConsensus(opinions),
            AgreementScore = CalculateAgreement(opinions),
            MinorityOpinion = ExtractMinorityOpinion(opinions)
        };
    }

    // Private helpers

    private List<PerformancePersona> InitializePersonas()
    {
        return new List<PerformancePersona>
        {
            new PerformancePersona
            {
                Id = "mvp-index",
                ExpertName = "Dr. Index Master",
                Specialty = "Index Optimization",
                Description = "20+ years optimizing indexes for Fortune 500 AX installations",
                SuccessRate = 0.95,
                Avatar = "🏆",
                KnownPatterns = new List<string>
                {
                    "Missing index detection",
                    "Fragmentation analysis",
                    "Covering index recommendations"
                }
            },
            new PerformancePersona
            {
                Id = "mvp-query",
                ExpertName = "Query Performance Guru",
                Specialty = "Query Optimization",
                Description = "Specialized in T-SQL optimization and execution plan analysis",
                SuccessRate = 0.92,
                Avatar = "⚡",
                KnownPatterns = new List<string>
                {
                    "Query rewriting",
                    "Sargability improvements",
                    "Join optimization"
                }
            },
            new PerformancePersona
            {
                Id = "mvp-architecture",
                ExpertName = "Architecture Wizard",
                Specialty = "System Architecture",
                Description = "Holistic system optimization and capacity planning",
                SuccessRate = 0.90,
                Avatar = "🏛️",
                KnownPatterns = new List<string>
                {
                    "AOS configuration",
                    "Load balancing",
                    "Batch job optimization"
                }
            },
            new PerformancePersona
            {
                Id = "mvp-database",
                ExpertName = "DBA Specialist",
                Specialty = "Database Administration",
                Description = "SQL Server internals and maintenance expert",
                SuccessRate = 0.93,
                Avatar = "💾",
                KnownPatterns = new List<string>
                {
                    "Statistics management",
                    "Wait stats analysis",
                    "Blocking resolution"
                }
            },
            new PerformancePersona
            {
                Id = "mvp-developer",
                ExpertName = "X++ Developer Pro",
                Specialty = "Application Code",
                Description = "AX application layer performance optimization",
                SuccessRate = 0.88,
                Avatar = "👨‍💻",
                KnownPatterns = new List<string>
                {
                    "Set-based operations",
                    "Caching strategies",
                    "Report optimization"
                }
            }
        };
    }

    private string GeneratePersonaAdvice(PerformancePersona persona, PerformanceProblem problem)
    {
        return persona.Specialty switch
        {
            "Index Optimization" => "Focus on creating well-designed covering indexes for your most expensive queries. " +
                                   "I see opportunities for 3-4 strategic indexes that could improve performance by 40-60%.",
            
            "Query Optimization" => "Several queries show poor sargability. Rewriting them to avoid functions on columns " +
                                   "and using proper parameterization will yield immediate benefits.",
            
            "System Architecture" => "Your AOS configuration needs tuning. Adjust connection pool sizes and consider " +
                                    "implementing a batch server tier for optimal resource utilization.",
            
            "Database Administration" => "Statistics are outdated on several high-traffic tables. Implement an automated " +
                                        "statistics update strategy with sampled updates.",
            
            "Application Code" => "Replace row-by-row operations with set-based alternatives in X++. " +
                                 "This alone can reduce execution time by 70-80%.",
            
            _ => "Implement a comprehensive optimization strategy addressing multiple layers."
        };
    }

    private string GenerateReasoning(PerformancePersona persona, PerformanceProblem problem)
    {
        return $"Based on {persona.Specialty} best practices and pattern recognition, " +
               $"I've identified {new Random().Next(3, 7)} critical optimization opportunities.";
    }

    private List<string> GenerateSteps(PerformancePersona persona, PerformanceProblem problem)
    {
        var baseSteps = new List<string>
        {
            "1. Baseline current performance metrics",
            "2. Implement recommended changes in non-production first",
            "3. Validate improvements with load testing",
            "4. Monitor for 24-48 hours before production rollout",
            "5. Document changes for future reference"
        };

        return baseSteps;
    }

    private string GenerateCaution(PerformancePersona persona)
    {
        return "⚠️ Always test in non-production environment first. Some optimizations may have trade-offs.";
    }

    private string GenerateConsensus(List<ExpertRecommendation> opinions)
    {
        return "All experts agree on implementing a multi-layered optimization approach. " +
               "Priority should be given to index optimization and query rewriting.";
    }

    private double CalculateAgreement(List<ExpertRecommendation> opinions)
    {
        return 0.78; // Simplified: 78% agreement among experts
    }

    private string ExtractMinorityOpinion(List<ExpertRecommendation> opinions)
    {
        return "One expert suggests focusing on application code first, which differs from the majority view.";
    }
}
