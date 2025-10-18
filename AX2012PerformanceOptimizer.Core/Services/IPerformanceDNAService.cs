using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Genetic algorithm-based performance optimization service
/// </summary>
public interface IPerformanceDNAService
{
    Task<GeneticOptimizationResult> EvolveOptimalSolutionAsync(
        OptimizationProblem problem, 
        int generations = 50,
        int populationSize = 20);
    
    Task<PerformanceDNA> GenerateRandomDNAAsync();
    Task<double> EvaluateFitnessAsync(PerformanceDNA dna, OptimizationProblem problem);
}

public class OptimizationProblem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, double> CurrentMetrics { get; set; } = new();
    public Dictionary<string, double> TargetMetrics { get; set; } = new();
}
