using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Filters;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class RecommendationsViewModel : ObservableObject
{
    private readonly IRecommendationEngine _recommendationEngine;
    private List<Recommendation> _allRecommendations = new();

    [ObservableProperty]
    private ObservableCollection<Recommendation> recommendations = new();

    [ObservableProperty]
    private Recommendation? selectedRecommendation;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isSurvivalModeEnabled;

    [ObservableProperty]
    private int filteredCount;

    [ObservableProperty]
    private int totalCount;

    public RecommendationsViewModel(IRecommendationEngine recommendationEngine)
    {
        _recommendationEngine = recommendationEngine;
        
        // Load Survival Mode preference (simplified - would use ISettingsService)
        IsSurvivalModeEnabled = LoadSurvivalModePreference();
    }

    [RelayCommand]
    private async Task LoadRecommendationsAsync()
    {
        IsLoading = true;

        try
        {
            var recs = await _recommendationEngine.GenerateRecommendationsAsync();
            _allRecommendations = recs;
            TotalCount = recs.Count;

            ApplySurvivalModeFilter();
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

    partial void OnIsSurvivalModeEnabledChanged(bool value)
    {
        ApplySurvivalModeFilter();
        SaveSurvivalModePreference(value);
    }

    [RelayCommand]
    private void ToggleSurvivalMode()
    {
        IsSurvivalModeEnabled = !IsSurvivalModeEnabled;
    }

    private void ApplySurvivalModeFilter()
    {
        Recommendations.Clear();

        var recommendationsToShow = IsSurvivalModeEnabled
            ? SurvivalModeFilter.Filter(_allRecommendations)
            : _allRecommendations.OrderBy(r => r.Priority).ToList();

        FilteredCount = recommendationsToShow.Count;
        TotalCount = _allRecommendations.Count;

        foreach (var rec in recommendationsToShow)
        {
            Recommendations.Add(rec);
        }
    }

    private bool LoadSurvivalModePreference()
    {
        // Simplified - would use ISettingsService
        // For now, default to false
        return false;
    }

    private void SaveSurvivalModePreference(bool enabled)
    {
        // Simplified - would use ISettingsService
        // Store in user settings: "SurvivalModeEnabled" = enabled
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


