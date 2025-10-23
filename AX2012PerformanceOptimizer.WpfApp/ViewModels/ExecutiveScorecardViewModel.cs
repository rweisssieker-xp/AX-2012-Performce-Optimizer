using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;
using AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class ExecutiveScorecardViewModel : ObservableObject
{
    private readonly IExecutiveScorecardService _scorecardService;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string statusMessage = "Ready to generate scorecard...";
    [ObservableProperty] private ExecutiveScorecard? currentScorecard;
    [ObservableProperty] private MonthlyBusinessReview? currentReview;
    [ObservableProperty] private bool hasScorecard;
    [ObservableProperty] private bool hasReview;

    // Scorecard Display Properties
    [ObservableProperty] private int overallScore;
    [ObservableProperty] private string gradeDisplay = string.Empty;
    [ObservableProperty] private string trendDisplay = string.Empty;
    [ObservableProperty] private string trendIcon = string.Empty;
    [ObservableProperty] private int trendChangePoints;

    // Category Scores
    [ObservableProperty] private ObservableCollection<CategoryScore> categoryScores = new();

    // Business Metrics
    [ObservableProperty] private string monthlySavings = string.Empty;
    [ObservableProperty] private string annualProjected = string.Empty;
    [ObservableProperty] private int optimizationsCount;
    [ObservableProperty] private int issuesPreventedCount;
    [ObservableProperty] private string avgResponseTime = string.Empty;
    [ObservableProperty] private string slaCompliance = string.Empty;

    // Issues and Achievements
    [ObservableProperty] private ObservableCollection<string> criticalIssues = new();
    [ObservableProperty] private ObservableCollection<string> topAchievements = new();
    [ObservableProperty] private ObservableCollection<KeyPerformanceIndicator> kpis = new();

    // Review Mode
    [ObservableProperty] private bool showMonthlyReview;
    [ObservableProperty] private string reviewMonth = DateTime.Now.ToString("MMMM yyyy");
    [ObservableProperty] private string executiveSummary = string.Empty;
    [ObservableProperty] private ObservableCollection<string> keyHighlights = new();
    [ObservableProperty] private ObservableCollection<string> actionItems = new();

    public ExecutiveScorecardViewModel(IExecutiveScorecardService scorecardService)
    {
        _scorecardService = scorecardService;
    }

    [RelayCommand]
    private async Task GenerateScorecardAsync()
    {
        IsLoading = true;
        StatusMessage = "Generating executive scorecard...";
        HasReview = false;
        ShowMonthlyReview = false;

        try
        {
            CurrentScorecard = await _scorecardService.GenerateScorecardAsync();

            if (CurrentScorecard != null)
            {
                DisplayScorecard(CurrentScorecard);
                HasScorecard = true;
                StatusMessage = "Scorecard generated successfully";
            }
            else
            {
                MessageBox.Show("Failed to generate scorecard. Please check logs.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Generation failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating scorecard:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GenerateMonthlyReviewAsync()
    {
        IsLoading = true;
        StatusMessage = "Generating monthly business review...";
        ShowMonthlyReview = false;

        try
        {
            CurrentReview = await _scorecardService.GenerateMonthlyReviewAsync(DateTime.Now);

            if (CurrentReview != null)
            {
                DisplayScorecard(CurrentReview.Scorecard);
                DisplayReview(CurrentReview);
                HasReview = true;
                ShowMonthlyReview = true;
                StatusMessage = "Monthly review generated successfully";
            }
            else
            {
                MessageBox.Show("Failed to generate monthly review. Please check logs.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Generation failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating monthly review:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        if (CurrentScorecard == null) return;

        IsLoading = true;
        StatusMessage = "Exporting to PDF...";

        try
        {
            var pdfBytes = await _scorecardService.ExportToPdfAsync(CurrentScorecard);

            if (pdfBytes != null && pdfBytes.Length > 0)
            {
                var fileName = $"ExecutiveScorecard_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filePath = System.IO.Path.Combine(desktopPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                MessageBox.Show($"Scorecard exported to PDF!\n\nFile: {filePath}",
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "Exported to PDF";
            }
            else
            {
                MessageBox.Show("PDF export is not yet implemented.",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Export failed";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportToPowerPointAsync()
    {
        if (CurrentScorecard == null) return;

        IsLoading = true;
        StatusMessage = "Exporting to PowerPoint...";

        try
        {
            var pptxBytes = await _scorecardService.ExportToPowerPointAsync(CurrentScorecard);

            if (pptxBytes != null && pptxBytes.Length > 0)
            {
                var fileName = $"ExecutiveScorecard_{DateTime.Now:yyyyMMdd_HHmmss}.pptx";
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filePath = System.IO.Path.Combine(desktopPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, pptxBytes);

                MessageBox.Show($"Scorecard exported to PowerPoint!\n\nFile: {filePath}",
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "Exported to PowerPoint";
            }
            else
            {
                MessageBox.Show("PowerPoint export is not yet implemented.",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Export failed";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        HasScorecard = false;
        HasReview = false;
        ShowMonthlyReview = false;
        CurrentScorecard = null;
        CurrentReview = null;
        CategoryScores.Clear();
        CriticalIssues.Clear();
        TopAchievements.Clear();
        Kpis.Clear();
        KeyHighlights.Clear();
        ActionItems.Clear();
        StatusMessage = "Ready to generate scorecard...";
    }

    private void DisplayScorecard(ExecutiveScorecard scorecard)
    {
        OverallScore = scorecard.OverallScore;
        GradeDisplay = scorecard.Grade.ToString().Replace("Plus", "+");
        TrendChangePoints = scorecard.TrendChangePoints;

        // Trend display
        TrendDisplay = scorecard.Trend switch
        {
            TrendDirection.Improving => "Improving",
            TrendDirection.Degrading => "Degrading",
            _ => "Stable"
        };

        TrendIcon = scorecard.Trend switch
        {
            TrendDirection.Improving => "↗️",
            TrendDirection.Degrading => "↘️",
            _ => "→"
        };

        // Category scores
        CategoryScores.Clear();
        CategoryScores.Add(new CategoryScore { Name = "Query Performance", Score = scorecard.QueryPerformanceScore });
        CategoryScores.Add(new CategoryScore { Name = "Database Health", Score = scorecard.DatabaseHealthScore });
        CategoryScores.Add(new CategoryScore { Name = "AOS Performance", Score = scorecard.AosPerformanceScore });
        CategoryScores.Add(new CategoryScore { Name = "Batch Job Success", Score = scorecard.BatchJobSuccessScore });
        CategoryScores.Add(new CategoryScore { Name = "Resource Utilization", Score = scorecard.ResourceUtilizationScore });
        CategoryScores.Add(new CategoryScore { Name = "Index Maintenance", Score = scorecard.IndexMaintenanceScore });
        CategoryScores.Add(new CategoryScore { Name = "Statistics Freshness", Score = scorecard.StatisticsFreshnessScore });
        CategoryScores.Add(new CategoryScore { Name = "Backup & Recovery", Score = scorecard.BackupRecoveryScore });

        // Business metrics
        MonthlySavings = $"€{scorecard.MonthlyCostSavings:N0}";
        AnnualProjected = $"€{scorecard.AnnualProjectedSavings:N0}";
        OptimizationsCount = scorecard.OptimizationsImplemented;
        IssuesPreventedCount = scorecard.IssuesPrevented;
        AvgResponseTime = $"{scorecard.AverageResponseTime:F0}ms";
        SlaCompliance = $"{scorecard.SlaCompliance:F1}%";

        // Issues and achievements
        CriticalIssues.Clear();
        foreach (var issue in scorecard.CriticalIssues)
            CriticalIssues.Add(issue);

        TopAchievements.Clear();
        foreach (var achievement in scorecard.TopAchievements)
            TopAchievements.Add(achievement);

        // KPIs
        Kpis.Clear();
        foreach (var kpi in scorecard.KPIs)
            Kpis.Add(kpi);
    }

    private void DisplayReview(MonthlyBusinessReview review)
    {
        ReviewMonth = review.ReportMonth.ToString("MMMM yyyy");
        ExecutiveSummary = review.ExecutiveSummary;

        KeyHighlights.Clear();
        foreach (var highlight in review.KeyHighlights)
            KeyHighlights.Add(highlight);

        ActionItems.Clear();
        foreach (var action in review.ActionItems)
            ActionItems.Add(action);
    }
}

public class CategoryScore
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Status => Score >= 85 ? "✅" : Score >= 70 ? "⚠️" : "❌";
    public string StatusColor => Score >= 85 ? "Green" : Score >= 70 ? "Orange" : "Red";
}
