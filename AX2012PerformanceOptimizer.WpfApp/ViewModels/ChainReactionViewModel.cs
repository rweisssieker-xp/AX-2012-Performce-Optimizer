using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.ChainReaction;
using AX2012PerformanceOptimizer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

/// <summary>
/// ViewModel for Chain Reaction view (cascade impact prediction)
/// </summary>
public partial class ChainReactionViewModel : ObservableObject
{
    private readonly IQueryCorrelationEngine _queryCorrelationEngine;
    private readonly ISqlQueryMonitorService _sqlQueryMonitorService;
    private readonly ILogger<ChainReactionViewModel> _logger;

    [ObservableProperty]
    private DependencyGraph? graph;

    [ObservableProperty]
    private CascadeImpactResult? impactResult;

    [ObservableProperty]
    private string? selectedQueryHash;

    [ObservableProperty]
    private List<SqlQueryMetric> availableQueries = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string optimizationType = "general";

    [ObservableProperty]
    private List<string> optimizationTypes = new()
    {
        "general",
        "index",
        "query_rewrite",
        "statistics"
    };

    public ChainReactionViewModel(
        IQueryCorrelationEngine queryCorrelationEngine,
        ISqlQueryMonitorService sqlQueryMonitorService,
        ILogger<ChainReactionViewModel> logger)
    {
        _queryCorrelationEngine = queryCorrelationEngine;
        _sqlQueryMonitorService = sqlQueryMonitorService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadGraphAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading dependency graph...";

        try
        {
            var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(100);
            AvailableQueries = queries;
            Graph = await _queryCorrelationEngine.BuildDependencyGraphAsync(queries);
            StatusMessage = $"Loaded graph with {Graph.TotalNodes} queries and {Graph.TotalEdges} dependencies.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dependency graph");
            StatusMessage = "Error loading dependency graph.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectQueryAsync(string? queryHash)
    {
        if (string.IsNullOrEmpty(queryHash))
        {
            SelectedQueryHash = null;
            ImpactResult = null;
            return;
        }

        SelectedQueryHash = queryHash;
        StatusMessage = $"Selected query {queryHash.Substring(0, Math.Min(8, queryHash.Length))}...";
        
        // Auto-predict impact when query is selected
        await PredictImpactAsync();
    }

    [RelayCommand]
    private async Task PredictImpactAsync()
    {
        if (string.IsNullOrEmpty(SelectedQueryHash))
        {
            StatusMessage = "Please select a query first.";
            return;
        }

        IsLoading = true;
        StatusMessage = $"Predicting cascade impact for query {SelectedQueryHash.Substring(0, Math.Min(8, SelectedQueryHash.Length))}...";

        try
        {
            ImpactResult = await _queryCorrelationEngine.PredictCascadeImpactAsync(
                SelectedQueryHash, 
                OptimizationType);

            StatusMessage = $"Impact prediction complete. {ImpactResult.QueriesAffected} queries affected, " +
                          $"{ImpactResult.TotalTimeSaved:F0}ms total time saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting cascade impact for query {QueryHash}", SelectedQueryHash);
            StatusMessage = "Error predicting cascade impact.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Gets the display name for impact type
    /// </summary>
    public static string GetImpactTypeDisplayName(ImpactType impactType)
    {
        return impactType switch
        {
            ImpactType.Positive => "Improved",
            ImpactType.Neutral => "Neutral",
            ImpactType.Negative => "Degraded",
            _ => impactType.ToString()
        };
    }

    /// <summary>
    /// Gets the color for impact type
    /// </summary>
    public static string GetImpactTypeColor(ImpactType impactType)
    {
        return impactType switch
        {
            ImpactType.Positive => "#4CAF50", // Green
            ImpactType.Neutral => "#FFC107", // Yellow
            ImpactType.Negative => "#F44336", // Red
            _ => "#757575" // Gray
        };
    }

    /// <summary>
    /// Gets confidence level display name
    /// </summary>
    public static string GetConfidenceLevel(double confidence)
    {
        return confidence switch
        {
            >= 80 => "High",
            >= 50 => "Medium",
            _ => "Low"
        };
    }
}
