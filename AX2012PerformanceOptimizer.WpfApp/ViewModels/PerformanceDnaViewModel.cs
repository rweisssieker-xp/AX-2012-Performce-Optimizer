using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class PerformanceDnaViewModel : ObservableObject
{
    private readonly IPerformanceDNAService _dnaService;

    [ObservableProperty]
    private string problemDescription = "Optimize slow query performance";

    [ObservableProperty]
    private int generations = 50;

    [ObservableProperty]
    private int populationSize = 20;

    [ObservableProperty]
    private bool isEvolving;

    [ObservableProperty]
    private string statusMessage = "Ready to evolve optimal solution";

    [ObservableProperty]
    private PerformanceDNA? bestSolution;

    [ObservableProperty]
    private double bestFitness;

    [ObservableProperty]
    private ObservableCollection<GenerationHistory> evolutionHistory = new();

    [ObservableProperty]
    private string evolutionSummary = string.Empty;

    public PerformanceDnaViewModel(IPerformanceDNAService dnaService)
    {
        _dnaService = dnaService;
    }

    [RelayCommand]
    private async Task EvolveOptimalSolutionAsync()
    {
        IsEvolving = true;
        StatusMessage = "🧬 Evolving optimal solution...";
        EvolutionHistory.Clear();

        try
        {
            var problem = new OptimizationProblem
            {
                Description = ProblemDescription,
                CurrentMetrics = new Dictionary<string, double>
                {
                    ["AvgQueryTime"] = 500,
                    ["CpuUsage"] = 75
                },
                TargetMetrics = new Dictionary<string, double>
                {
                    ["AvgQueryTime"] = 100,
                    ["CpuUsage"] = 40
                }
            };

            var result = await Task.Run(() => 
                _dnaService.EvolveOptimalSolutionAsync(problem, Generations, PopulationSize));

            BestSolution = result.BestSolution;
            BestFitness = result.BestSolution.FitnessScore;
            EvolutionSummary = result.Summary;

            // Add history
            foreach (var dna in result.EvolutionHistory)
            {
                EvolutionHistory.Add(new GenerationHistory
                {
                    Generation = dna.Generation,
                    BestFitness = dna.FitnessScore,
                    GeneCount = dna.Genes.Count
                });
            }

            StatusMessage = $"✅ Evolution complete! Best fitness: {BestFitness:F2} ({result.EvolutionTime.TotalSeconds:F1}s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsEvolving = false;
        }
    }
}

public class GenerationHistory
{
    public int Generation { get; set; }
    public double BestFitness { get; set; }
    public int GeneCount { get; set; }
}
