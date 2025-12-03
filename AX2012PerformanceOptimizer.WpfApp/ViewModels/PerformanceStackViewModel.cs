using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;
using AX2012PerformanceOptimizer.Core.Services.PerformanceStack;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

/// <summary>
/// ViewModel for Performance Stack view
/// </summary>
public partial class PerformanceStackViewModel : ObservableObject
{
    private readonly IPerformanceStackService _performanceStackService;
    private readonly ILogger<PerformanceStackViewModel> _logger;

    [ObservableProperty]
    private PerformanceStackData? stackData;

    [ObservableProperty]
    private List<Bottleneck> bottlenecks = new();

    [ObservableProperty]
    private LayerType? selectedLayer;

    [ObservableProperty]
    private TimeRange selectedTimeRange = TimeRange.LastHour;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public PerformanceStackViewModel(
        IPerformanceStackService performanceStackService,
        ILogger<PerformanceStackViewModel> logger)
    {
        _performanceStackService = performanceStackService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading performance stack metrics...";

        try
        {
            StackData = await _performanceStackService.GetStackMetricsAsync(SelectedTimeRange);
            Bottlenecks = StackData.Bottlenecks;
            StatusMessage = $"Loaded metrics for {StackData.Timestamp:g}. Found {Bottlenecks.Count} bottlenecks.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing performance stack metrics");
            StatusMessage = "Error loading metrics. Please check connection.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DrillDownAsync(LayerType layer)
    {
        if (layer == SelectedLayer)
        {
            // If already selected, deselect
            SelectedLayer = null;
            return;
        }

        SelectedLayer = layer;
        IsLoading = true;
        StatusMessage = $"Loading details for {layer} layer...";

        try
        {
            var layerDetails = await _performanceStackService.GetLayerDetailsAsync(layer, SelectedTimeRange);
            StatusMessage = $"Loaded {layer} layer details.";
            
            // TODO: Navigate to detail view or show in side panel
            // This will be implemented in Task 1.1.6
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading layer details for {Layer}", layer);
            StatusMessage = $"Error loading {layer} layer details.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TimeRangeChangedAsync()
    {
        // Refresh data when time range changes
        if (StackData != null)
        {
            await RefreshAsync();
        }
    }

    /// <summary>
    /// Gets the display name for a layer type
    /// </summary>
    public static string GetLayerDisplayName(LayerType layer)
    {
        return layer switch
        {
            LayerType.Database => "Database",
            LayerType.AosServer => "AOS Server",
            LayerType.Network => "Network",
            LayerType.Client => "Client",
            _ => layer.ToString()
        };
    }

    /// <summary>
    /// Gets the severity color for a bottleneck
    /// </summary>
    public static string GetSeverityColor(BottleneckSeverity severity)
    {
        return severity switch
        {
            BottleneckSeverity.Critical => "#FF0000", // Red
            BottleneckSeverity.High => "#FF8800", // Orange
            BottleneckSeverity.Medium => "#FFAA00", // Yellow
            BottleneckSeverity.Low => "#88AA00", // Light green
            _ => "#808080" // Gray
        };
    }
}
