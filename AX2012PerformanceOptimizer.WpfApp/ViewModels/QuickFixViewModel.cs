using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models.QuickFix;
using AX2012PerformanceOptimizer.Core.Services.QuickFix;
using AX2012PerformanceOptimizer.WpfApp.Services;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

/// <summary>
/// ViewModel for Quick-Fix Mode view
/// </summary>
public partial class QuickFixViewModel : ObservableObject
{
    private readonly IQuickFixService _quickFixService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<QuickFixViewModel> _logger;

    [ObservableProperty]
    private QuickFixAnalysisResult? analysisResult;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private string? selectedFixId;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private double analysisProgress;

    public QuickFixViewModel(
        IQuickFixService quickFixService,
        IDialogService dialogService,
        ILogger<QuickFixViewModel> logger)
    {
        _quickFixService = quickFixService;
        _dialogService = dialogService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        IsAnalyzing = true;
        StatusMessage = "Analyzing system for quick fixes...";
        AnalysisProgress = 0;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            // Simulate progress updates
            var progressTask = UpdateProgressAsync(cts.Token);
            var analysisTask = _quickFixService.AnalyzeQuickFixesAsync(cts.Token);

            await Task.WhenAny(analysisTask, progressTask);
            var result = await analysisTask;

            AnalysisResult = result;
            AnalysisProgress = 100;

            if (result.IsSuccess)
            {
                StatusMessage = $"Analysis complete in {result.AnalysisDuration.TotalSeconds:F1}s. Found {result.QuickFixes.Count} quick fixes.";
            }
            else
            {
                StatusMessage = $"Analysis failed: {result.ErrorMessage}";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Analysis timed out after 30 seconds.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quick fix analysis");
            StatusMessage = "Error during analysis.";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFixAsync(string? fixId)
    {
        if (string.IsNullOrEmpty(fixId))
        {
            StatusMessage = "Please select a fix to apply.";
            return;
        }

        SelectedFixId = fixId;

        var fix = AnalysisResult?.QuickFixes.FirstOrDefault(f => f.Id == fixId);
        if (fix == null)
        {
            StatusMessage = "Fix not found.";
            return;
        }

        // Check if confirmation is needed
        var canApplyDirectly = await _quickFixService.CanApplyDirectlyAsync(fixId);
        
        if (!canApplyDirectly)
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Confirm Quick Fix",
                $"Apply fix: {fix.Title}?\n\n{fix.Description}\n\nImpact: {fix.Impact:F0}% | Effort: {fix.Effort:F0}%");

            if (!confirmed)
            {
                StatusMessage = "Fix application cancelled.";
                return;
            }
        }

        StatusMessage = $"Applying fix: {fix.Title}...";

        try
        {
            var result = await _quickFixService.ApplyQuickFixAsync(fixId);

            if (result.IsSuccess)
            {
                StatusMessage = $"Fix applied successfully: {result.Message}";
                
                // Refresh analysis after applying fix
                await AnalyzeAsync();
            }
            else
            {
                StatusMessage = $"Failed to apply fix: {result.ErrorMessage}";
                await _dialogService.ShowErrorAsync("Apply Failed", result.ErrorMessage ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying quick fix {FixId}", fixId);
            StatusMessage = "Error applying fix.";
            await _dialogService.ShowErrorAsync("Error", ex.Message);
        }
    }

    private async Task UpdateProgressAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsAnalyzing)
        {
            await Task.Delay(500, cancellationToken);
            AnalysisProgress = Math.Min(95, AnalysisProgress + 5);
        }
    }

    /// <summary>
    /// Gets the display name for quick fix type
    /// </summary>
    public static string GetFixTypeDisplayName(QuickFixType type)
    {
        return type switch
        {
            QuickFixType.CreateIndex => "Create Index",
            QuickFixType.UpdateStatistics => "Update Statistics",
            QuickFixType.RebuildIndex => "Rebuild Index",
            QuickFixType.ClearCache => "Clear Cache",
            QuickFixType.KillBlockingQuery => "Kill Blocking Query",
            QuickFixType.OptimizeQuery => "Optimize Query",
            QuickFixType.AdjustConfiguration => "Adjust Configuration",
            _ => type.ToString()
        };
    }

    /// <summary>
    /// Gets the color for priority
    /// </summary>
    public static string GetPriorityColor(QuickFixPriority priority)
    {
        return priority switch
        {
            QuickFixPriority.Critical => "#F44336", // Red
            QuickFixPriority.High => "#FF9800", // Orange
            QuickFixPriority.Medium => "#FFC107", // Yellow
            QuickFixPriority.Low => "#4CAF50", // Green
            _ => "#757575" // Gray
        };
    }
}
