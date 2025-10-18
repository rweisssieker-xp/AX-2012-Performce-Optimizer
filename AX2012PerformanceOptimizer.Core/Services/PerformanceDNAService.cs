using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class PerformanceDNAService : IPerformanceDNAService
{
    private readonly ILogger<PerformanceDNAService> _logger;
    private readonly Random _random = new();

    public PerformanceDNAService(ILogger<PerformanceDNAService> logger)
    {
        _logger = logger;
    }

    public async Task<GeneticOptimizationResult> EvolveOptimalSolutionAsync(
        OptimizationProblem problem,
        int generations = 50,
        int populationSize = 20)
    {
        _logger.LogInformation("🧬 Starting genetic evolution for {Problem}", problem.Description);
        
        var startTime = DateTime.UtcNow;
        var population = await GenerateInitialPopulationAsync(populationSize);
        var history = new List<PerformanceDNA>();

        for (int gen = 0; gen < generations; gen++)
        {
            // Evaluate fitness for all DNAs
            foreach (var dna in population)
            {
                dna.FitnessScore = await EvaluateFitnessAsync(dna, problem);
                dna.Generation = gen;
            }

            // Keep best performers
            var sorted = population.OrderByDescending(d => d.FitnessScore).ToList();
            history.Add(CloneDNA(sorted[0])); // Track best of generation

            // Selection, Crossover, Mutation
            population = await EvolvePopulationAsync(sorted, populationSize);

            _logger.LogDebug("Generation {Gen}: Best fitness = {Fitness:F2}", gen, sorted[0].FitnessScore);
        }

        var best = population.OrderByDescending(d => d.FitnessScore).First();
        
        return new GeneticOptimizationResult
        {
            BestSolution = best,
            EvolutionHistory = history,
            EvolutionTime = DateTime.UtcNow - startTime,
            Summary = $"Evolved optimal solution with fitness {best.FitnessScore:F2} over {generations} generations"
        };
    }

    public async Task<PerformanceDNA> GenerateRandomDNAAsync()
    {
        await Task.Delay(1); // Simulate async work
        
        var dna = new PerformanceDNA();
        var geneCount = _random.Next(3, 8);

        var geneTypes = new[] { "IndexRebuild", "StatisticsUpdate", "QueryRewrite", "ConfigOptimization", "CacheWarming" };

        for (int i = 0; i < geneCount; i++)
        {
            dna.Genes.Add(new OptimizationGene
            {
                Type = geneTypes[_random.Next(geneTypes.Length)],
                Weight = _random.NextDouble(),
                Parameters = GenerateRandomParameters()
            });
        }

        return dna;
    }

    public async Task<double> EvaluateFitnessAsync(PerformanceDNA dna, OptimizationProblem problem)
    {
        await Task.Delay(1);
        
        double fitness = 0;

        // Evaluate based on gene composition
        foreach (var gene in dna.Genes)
        {
            fitness += gene.Weight * EvaluateGene(gene, problem);
        }

        // Penalize overly complex solutions
        fitness -= dna.Genes.Count * 0.1;

        return Math.Max(0, fitness);
    }

    // Private helper methods

    private async Task<List<PerformanceDNA>> GenerateInitialPopulationAsync(int size)
    {
        var population = new List<PerformanceDNA>();
        for (int i = 0; i < size; i++)
        {
            population.Add(await GenerateRandomDNAAsync());
        }
        return population;
    }

    private async Task<List<PerformanceDNA>> EvolvePopulationAsync(List<PerformanceDNA> sorted, int targetSize)
    {
        await Task.Delay(1);
        
        var newPopulation = new List<PerformanceDNA>();

        // Keep top 20% (elitism)
        int eliteCount = Math.Max(2, targetSize / 5);
        newPopulation.AddRange(sorted.Take(eliteCount).Select(CloneDNA));

        // Fill rest with crossover and mutation
        while (newPopulation.Count < targetSize)
        {
            var parent1 = SelectParent(sorted);
            var parent2 = SelectParent(sorted);
            var child = Crossover(parent1, parent2);
            Mutate(child);
            newPopulation.Add(child);
        }

        return newPopulation;
    }

    private PerformanceDNA SelectParent(List<PerformanceDNA> sorted)
    {
        // Tournament selection
        int tournamentSize = 3;
        var tournament = sorted.OrderBy(_ => _random.Next()).Take(tournamentSize).ToList();
        return tournament.OrderByDescending(d => d.FitnessScore).First();
    }

    private PerformanceDNA Crossover(PerformanceDNA parent1, PerformanceDNA parent2)
    {
        var child = new PerformanceDNA();
        int splitPoint = _random.Next(Math.Min(parent1.Genes.Count, parent2.Genes.Count));

        child.Genes.AddRange(parent1.Genes.Take(splitPoint).Select(CloneGene));
        child.Genes.AddRange(parent2.Genes.Skip(splitPoint).Select(CloneGene));

        return child;
    }

    private void Mutate(PerformanceDNA dna)
    {
        double mutationRate = 0.1;

        foreach (var gene in dna.Genes)
        {
            if (_random.NextDouble() < mutationRate)
            {
                gene.Weight = Math.Clamp(_random.NextDouble(), 0, 1);
            }
        }

        // Occasionally add/remove genes
        if (_random.NextDouble() < 0.05 && dna.Genes.Count > 2)
        {
            dna.Genes.RemoveAt(_random.Next(dna.Genes.Count));
        }
    }

    private double EvaluateGene(OptimizationGene gene, OptimizationProblem problem)
    {
        // Simplified evaluation logic
        return gene.Type switch
        {
            "IndexRebuild" => 2.0,
            "StatisticsUpdate" => 1.5,
            "QueryRewrite" => 3.0,
            "ConfigOptimization" => 1.2,
            "CacheWarming" => 1.0,
            _ => 0.5
        };
    }

    private Dictionary<string, object> GenerateRandomParameters()
    {
        return new Dictionary<string, object>
        {
            ["Priority"] = _random.Next(1, 10),
            ["Threshold"] = _random.NextDouble()
        };
    }

    private PerformanceDNA CloneDNA(PerformanceDNA original)
    {
        return new PerformanceDNA
        {
            Id = Guid.NewGuid().ToString(),
            Genes = original.Genes.Select(CloneGene).ToList(),
            FitnessScore = original.FitnessScore,
            Generation = original.Generation
        };
    }

    private OptimizationGene CloneGene(OptimizationGene original)
    {
        return new OptimizationGene
        {
            Type = original.Type,
            Weight = original.Weight,
            Parameters = new Dictionary<string, object>(original.Parameters)
        };
    }
}
