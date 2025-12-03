using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;
using AX2012PerformanceOptimizer.Core.Services.PerformanceStack;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

/// <summary>
/// ViewModel for Layer Detail view (drill-down)
/// </summary>
public partial class LayerDetailViewModel : ObservableObject
{
    private readonly IPerformanceStackService _performanceStackService;
    private readonly ILogger<LayerDetailViewModel> _logger;

    [ObservableProperty]
    private LayerType layerType;

    [ObservableProperty]
    private object? layerDetails;

    [ObservableProperty]
    private TimeRange timeRange;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public LayerDetailViewModel(
        IPerformanceStackService performanceStackService,
        ILogger<LayerDetailViewModel> logger)
    {
        _performanceStackService = performanceStackService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadDetailsAsync()
    {
        IsLoading = true;
        StatusMessage = $"Loading details for {GetLayerDisplayName(LayerType)}...";

        try
        {
            LayerDetails = await _performanceStackService.GetLayerDetailsAsync(LayerType, TimeRange);
            StatusMessage = $"Details loaded successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading layer details for {Layer}", LayerType);
            StatusMessage = "Error loading details.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        // Navigation back will be handled by parent view
        // This command can be bound to a back button
    }

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
}
