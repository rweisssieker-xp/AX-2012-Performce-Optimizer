using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;
using AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class ExecutiveDashboardViewModel : ObservableObject
{
    private readonly IExecutiveDashboardGeneratorService _dashboardService;

    [ObservableProperty] private StakeholderType selectedStakeholderType = StakeholderType.CEO;
    [ObservableProperty] private int timeRangeDays = 7;
    [ObservableProperty] private bool isGenerating;
    [ObservableProperty] private bool hasDashboard;
    [ObservableProperty] private string statusMessage = "Ready to generate dashboard...";
    [ObservableProperty] private string dashboardTitle = string.Empty;
    [ObservableProperty] private string dashboardSubtitle = string.Empty;
    [ObservableProperty] private string executiveSummary = string.Empty;
    [ObservableProperty] private double overallHealthScore;
    [ObservableProperty] private string healthTrend = string.Empty;
    [ObservableProperty] private double generationTimeMs;
    [ObservableProperty] private ObservableCollection<DashboardWidget> widgets = new();
    [ObservableProperty] private ObservableCollection<string> keyInsights = new();
    [ObservableProperty] private ObservableCollection<string> actionItems = new();
    [ObservableProperty] private ObservableCollection<StakeholderType> availableStakeholders = new();

    public ExecutiveDashboardViewModel(IExecutiveDashboardGeneratorService dashboardService)
    {
        _dashboardService = dashboardService;
        LoadStakeholders();
    }

    private async void LoadStakeholders()
    {
        var types = await _dashboardService.GetAvailableStakeholderTypesAsync();
        AvailableStakeholders = new ObservableCollection<StakeholderType>(types);
    }

    [RelayCommand]
    private async Task GenerateDashboardAsync()
    {
        IsGenerating = true;
        StatusMessage = "Generating AI-powered dashboard...";
        HasDashboard = false;

        try
        {
            var request = new DashboardGenerationRequest
            {
                StakeholderType = SelectedStakeholderType,
                TimeRangeDays = TimeRangeDays,
                IncludeCostAnalysis = true,
                IncludeTrendAnalysis = true,
                IncludeRecommendations = true
            };

            var result = await _dashboardService.GenerateDashboardAsync(request);

            if (result.Success && result.Dashboard != null)
            {
                DashboardTitle = result.Dashboard.Title;
                DashboardSubtitle = result.Dashboard.Subtitle;
                ExecutiveSummary = result.Dashboard.ExecutiveSummary;
                OverallHealthScore = result.Dashboard.OverallHealthScore;
                HealthTrend = result.Dashboard.HealthTrend;
                GenerationTimeMs = result.GenerationTimeMs;

                Widgets.Clear();
                foreach (var widget in result.Dashboard.Widgets.OrderBy(w => w.GridRow).ThenBy(w => w.GridColumn))
                    Widgets.Add(widget);

                KeyInsights.Clear();
                foreach (var insight in result.Dashboard.KeyInsights)
                    KeyInsights.Add(insight);

                ActionItems.Clear();
                foreach (var action in result.Dashboard.ActionItems)
                    ActionItems.Add(action);

                HasDashboard = true;
                StatusMessage = $"Dashboard generated in {GenerationTimeMs:F0}ms";
            }
            else
            {
                MessageBox.Show($"Failed to generate dashboard:\n\n{result.ErrorMessage}",
                    "Generation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Generation failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task ExportDashboardAsync()
    {
        if (!HasDashboard) return;

        try
        {
            var fileName = $"Dashboard_{SelectedStakeholderType}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            // Create simplified dashboard for export
            var dashboard = new Core.Models.ExecutiveDashboard.ExecutiveDashboard
            {
                Title = DashboardTitle,
                Subtitle = DashboardSubtitle,
                ExecutiveSummary = ExecutiveSummary,
                Widgets = Widgets.ToList()
            };

            var html = await _dashboardService.ExportDashboardToHtmlAsync(dashboard);
            await System.IO.File.WriteAllTextAsync(filePath, html);

            MessageBox.Show($"Dashboard exported!\n\nFile: {filePath}",
                "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = "Dashboard exported";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        HasDashboard = false;
        Widgets.Clear();
        KeyInsights.Clear();
        ActionItems.Clear();
        StatusMessage = "Ready to generate dashboard...";
    }
}
