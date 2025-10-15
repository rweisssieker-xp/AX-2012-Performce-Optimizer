using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class RecommendationsViewModel : ObservableObject
{
    private readonly IRecommendationEngine _recommendationEngine;

    [ObservableProperty]
    private ObservableCollection<Recommendation> recommendations = new();

    [ObservableProperty]
    private Recommendation? selectedRecommendation;

    [ObservableProperty]
    private bool isLoading;

    public RecommendationsViewModel(IRecommendationEngine recommendationEngine)
    {
        _recommendationEngine = recommendationEngine;
    }

    [RelayCommand]
    private async Task LoadRecommendationsAsync()
    {
        IsLoading = true;

        try
        {
            var recs = await _recommendationEngine.GenerateRecommendationsAsync();
            Recommendations.Clear();
            foreach (var rec in recs.OrderBy(r => r.Priority))
            {
                Recommendations.Add(rec);
            }
        }
        catch
        {
            // Handle error gracefully
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CopyScript()
    {
        if (SelectedRecommendation != null)
        {
            Clipboard.SetText(SelectedRecommendation.ActionScript);
        }
    }

    [RelayCommand]
    private async Task MarkAsImplementedAsync()
    {
        if (SelectedRecommendation != null)
        {
            await _recommendationEngine.MarkAsImplementedAsync(SelectedRecommendation.Id);
            SelectedRecommendation.IsImplemented = true;
            SelectedRecommendation.ImplementedAt = DateTime.UtcNow;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadRecommendationsAsync();
    }
}


