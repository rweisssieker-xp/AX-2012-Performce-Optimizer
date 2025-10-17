using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.WpfApp.Services;
using AX2012PerformanceOptimizer.Data.SqlServer;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class AiInsightsDashboardViewModel : ObservableObject
{
    private readonly IAiPerformanceInsightsService _insightsService;
    private readonly IDialogService _dialogService;
    private readonly ISqlConnectionManager _connectionManager;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Bereit";

    // Dashboard Data
    [ObservableProperty]
    private PerformanceInsightsDashboard? dashboard;

    [ObservableProperty]
    private bool hasDashboard;

    // Top Insights
    [ObservableProperty]
    private ObservableCollection<PerformanceInsight> topInsights = new();

    // Opportunities
    [ObservableProperty]
    private ObservableCollection<OptimizationOpportunity> opportunities = new();

    // Risks
    [ObservableProperty]
    private ObservableCollection<RiskAlert> risks = new();

    // Selected items
    [ObservableProperty]
    private PerformanceInsight? selectedInsight;

    [ObservableProperty]
    private OptimizationOpportunity? selectedOpportunity;

    // Metrics
    [ObservableProperty]
    private double overallPerformanceScore;

    [ObservableProperty]
    private string performanceGrade = "-";

    [ObservableProperty]
    private double estimatedDailyCost;

    [ObservableProperty]
    private double estimatedMonthlyCost;

    [ObservableProperty]
    private string trendDirection = "-";

    [ObservableProperty]
    private double performanceChange;

    public AiInsightsDashboardViewModel(
        IAiPerformanceInsightsService insightsService,
        IDialogService dialogService,
        ISqlConnectionManager connectionManager)
    {
        _insightsService = insightsService;
        _dialogService = dialogService;
        _connectionManager = connectionManager;
        _connectionManager.ConnectionChanged += OnConnectionChanged;

        // Only load dashboard if already connected
        if (_connectionManager.IsConnected)
        {
            LoadDashboardAsync();
        }
    }

    private async void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        if (e.IsConnected)
        {
            // Connection established - load dashboard with real data
            StatusMessage = "Verbindung hergestellt - Lade Daten...";
            await LoadDashboardAsync();
        }
        else
        {
            // Connection lost - clear dashboard
            HasDashboard = false;
            Dashboard = null;
            TopInsights.Clear();
            Opportunities.Clear();
            Risks.Clear();
            StatusMessage = "Keine Datenbankverbindung - Bitte verbinden Sie sich in Settings";
        }
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        StatusMessage = "Lade AI Performance Insights...";

        try
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-7);

            Dashboard = await _insightsService.GenerateInsightsDashboardAsync(startDate, endDate);

            // Update metrics
            if (Dashboard != null)
            {
                OverallPerformanceScore = Dashboard.Metrics.OverallPerformanceScore;
                PerformanceGrade = Dashboard.Metrics.PerformanceGrade;
                EstimatedDailyCost = Dashboard.Metrics.EstimatedDailyCost;
                EstimatedMonthlyCost = Dashboard.Metrics.EstimatedMonthlyCost;
                TrendDirection = Dashboard.Metrics.TrendDirection;
                PerformanceChange = Dashboard.Metrics.PerformanceChange;

                // Load top insights
                TopInsights.Clear();
                foreach (var insight in Dashboard.TopInsights)
                {
                    TopInsights.Add(insight);
                }

                // Load opportunities
                Opportunities.Clear();
                foreach (var opp in Dashboard.Opportunities)
                {
                    Opportunities.Add(opp);
                }

                // Load risks
                Risks.Clear();
                foreach (var risk in Dashboard.Risks)
                {
                    Risks.Add(risk);
                }

                HasDashboard = true;
                StatusMessage = "Dashboard erfolgreich geladen";
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Fehler beim Laden",
                $"Fehler beim Laden des Dashboards:\n\n{ex.Message}\n\nBitte überprüfen Sie die Datenbankverbindung in den Settings.",
                ex);
            StatusMessage = "Fehler beim Laden";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowWeeklySummaryAsync()
    {
        IsLoading = true;
        StatusMessage = "Generiere Weekly Summary...";

        try
        {
            var summary = await _insightsService.GenerateWeeklySummaryAsync();

            var sections = new Dictionary<string, string>
            {
                { "📅 Zeitraum", $"{summary.WeekStartDate:dd.MM.yyyy} - {summary.WeekEndDate:dd.MM.yyyy}" },
                { "📊 Summary", summary.Summary },
                { "🔍 Top Findings", string.Join("\n", summary.TopFindings.Select(f => $"• {f}")) },
                { "✅ Verbesserungen", string.Join("\n", summary.Improvements.Take(3).Select(i => $"• {i}")) },
                { "⚠️ Probleme", string.Join("\n", summary.Issues.Take(3).Select(i => $"• {i}")) },
                { "💡 Empfehlungen", string.Join("\n", summary.Recommendations.Take(3).Select(r => $"• {r}")) }
            };

            await _dialogService.ShowSummaryAsync("Weekly Performance Summary", sections);

            StatusMessage = "Weekly Summary angezeigt";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Fehler", $"Fehler beim Generieren der Weekly Summary:\n\n{ex.Message}", ex);
            StatusMessage = "Fehler";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowExecutiveSummaryAsync()
    {
        IsLoading = true;
        StatusMessage = "Generiere Executive Summary...";

        try
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var summary = await _insightsService.GenerateExecutiveSummaryAsync(startDate, endDate);

            var sections = new Dictionary<string, string>
            {
                { "📅 Zeitraum", $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}" },
                { "📊 Executive Overview", summary.ExecutiveOverview },
                { "📈 Key Numbers", string.Join("\n", summary.KeyNumbers.Select(kvp => $"• {kvp.Key}: {kvp.Value}")) },
                { "✅ Positive Highlights", string.Join("\n", summary.PositiveHighlights.Select(h => $"• {h}")) },
                { "⚠️ Concern Areas", string.Join("\n", summary.ConcernAreas.Select(c => $"• {c}")) },
                { "💰 Business Impact", summary.BusinessImpact },
                { "📋 Executive Recommendations", string.Join("\n", summary.ExecutiveRecommendations.Select(r => $"• {r}")) }
            };

            await _dialogService.ShowSummaryAsync("Executive Performance Summary", sections);

            StatusMessage = "Executive Summary angezeigt";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Fehler", $"Fehler beim Generieren der Executive Summary:\n\n{ex.Message}", ex);
            StatusMessage = "Fehler";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ViewInsightDetails(PerformanceInsight? insight)
    {
        if (insight == null) return;

        var content = $"Severity: {insight.Severity}\n" +
                     $"Category: {insight.Category}\n" +
                     $"Impact Score: {insight.ImpactScore:F0}/100\n" +
                     $"Confidence: {insight.ConfidenceScore:F0}%\n" +
                     $"Potential Improvement: +{insight.PotentialImprovement:F0}%\n\n" +
                     $"Description:\n{insight.Description}\n\n" +
                     $"Recommended Actions:\n" +
                     string.Join("\n", insight.RecommendedActions.Select(a => $"• {a}"));

        await _dialogService.ShowDetailsAsync(
            $"💡 {insight.Title}",
            content,
            null,
            "💡");
    }

    [RelayCommand]
    private async Task ViewOpportunityDetails(OptimizationOpportunity? opportunity)
    {
        if (opportunity == null) return;

        var content = $"Type: {opportunity.OpportunityType}\n" +
                     $"Priority: {opportunity.PriorityLevel} ({opportunity.PriorityScore}/100)\n" +
                     $"Effort: {opportunity.EffortLevel} ({opportunity.EstimatedImplementationTime:F1}h)\n\n" +
                     $"Description:\n{opportunity.Description}\n\n" +
                     $"💰 Impact:\n" +
                     $"• Cost Savings: €{opportunity.EstimatedCostSavings:F2}/month\n" +
                     $"• Time Savings: {opportunity.EstimatedTimeSavings:F0}ms\n" +
                     $"• Affected Queries: {opportunity.AffectedQueries}\n\n" +
                     $"📊 ROI Analysis:\n" +
                     $"• ROI: {opportunity.ROI:F1}x\n" +
                     $"• Payback Period: {opportunity.PaybackPeriod:F1} days\n\n" +
                     $"🔧 Implementation Steps:\n" +
                     string.Join("\n", opportunity.ImplementationSteps) + "\n\n" +
                     $"Automation Available: {opportunity.AutomationAvailable}";

        await _dialogService.ShowDetailsAsync(
            $"💡 {opportunity.Title}",
            content,
            null,
            "💡");
    }

    [RelayCommand]
    private async Task ExportDashboardAsync()
    {
        if (Dashboard == null)
        {
            await _dialogService.ShowWarningAsync("Export", "Kein Dashboard geladen.\n\nBitte laden Sie zuerst ein Dashboard.");
            return;
        }

        try
        {
            var content = new StringBuilder();
            content.AppendLine("==============================================");
            content.AppendLine("AI Performance Insights Dashboard");
            content.AppendLine($"Generated: {Dashboard.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            content.AppendLine($"Period: {Dashboard.StartDate:yyyy-MM-dd} to {Dashboard.EndDate:yyyy-MM-dd}");
            content.AppendLine("==============================================\n");

            content.AppendLine("METRICS:");
            content.AppendLine($"  Performance Score: {Dashboard.Metrics.PerformanceGrade} ({Dashboard.Metrics.OverallPerformanceScore:F0}/100)");
            content.AppendLine($"  Trend: {Dashboard.Metrics.TrendDirection} ({Dashboard.Metrics.PerformanceChange:+0.0;-0.0}%)");
            content.AppendLine($"  Daily Cost: €{Dashboard.Metrics.EstimatedDailyCost:F2}");
            content.AppendLine($"  Monthly Cost: €{Dashboard.Metrics.EstimatedMonthlyCost:F2}\n");

            content.AppendLine("EXECUTIVE SUMMARY:");
            content.AppendLine(Dashboard.ExecutiveSummary);
            content.AppendLine();

            content.AppendLine("TOP INSIGHTS:");
            foreach (var insight in Dashboard.TopInsights)
            {
                content.AppendLine($"  [{insight.Severity}] {insight.Title}");
                content.AppendLine($"      {insight.Description}");
            }
            content.AppendLine();

            content.AppendLine("OPTIMIZATION OPPORTUNITIES:");
            foreach (var opp in Dashboard.Opportunities)
            {
                content.AppendLine($"  [{opp.PriorityLevel}] {opp.Title}");
                content.AppendLine($"      Savings: €{opp.EstimatedCostSavings:F2}/mo, ROI: {opp.ROI:F1}x");
            }
            content.AppendLine();

            content.AppendLine("RISK ALERTS:");
            foreach (var risk in Dashboard.Risks)
            {
                content.AppendLine($"  [{risk.Severity}] {risk.Title}");
                content.AppendLine($"      {risk.Description}");
            }

            var fileName = $"AI_Performance_Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            await System.IO.File.WriteAllTextAsync(filePath, content.ToString());

            await _dialogService.ShowSuccessAsync(
                "Export erfolgreich",
                "Dashboard erfolgreich exportiert!",
                $"Datei: {filePath}");

            StatusMessage = "Dashboard exportiert";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Export fehlgeschlagen",
                $"Fehler beim Exportieren des Dashboards:\n\n{ex.Message}",
                ex);
        }
    }
}
