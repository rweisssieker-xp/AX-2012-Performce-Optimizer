namespace AX2012PerformanceOptimizer.Core.Models;

/// <summary>
/// Represents a "genetic" solution for performance optimization
/// </summary>
public class PerformanceDNA
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<OptimizationGene> Genes { get; set; } = new();
    public double FitnessScore { get; set; }
    public int Generation { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}

public class OptimizationGene
{
    public string Type { get; set; } = string.Empty; // "IndexRebuild", "StatisticsUpdate", "QueryRewrite"
    public Dictionary<string, object> Parameters { get; set; } = new();
    public double Weight { get; set; } = 1.0;
}

public class GeneticOptimizationResult
{
    public PerformanceDNA BestSolution { get; set; } = new();
    public List<PerformanceDNA> EvolutionHistory { get; set; } = new();
    public TimeSpan EvolutionTime { get; set; }
    public string Summary { get; set; } = string.Empty;
}
