using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class AiInsightsDashboardViewModel : ObservableObject
{
    private readonly IAiPerformanceInsightsService _insightsService;

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

    public AiInsightsDashboardViewModel(IAiPerformanceInsightsService insightsService)
    {
        _insightsService = insightsService;
        LoadDashboardAsync();
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
            MessageBox.Show($"Fehler beim Laden des Dashboards: {ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

            var message = new StringBuilder();
            message.AppendLine("📅 Weekly Performance Summary\n");
            message.AppendLine($"Zeitraum: {summary.WeekStartDate:dd.MM.yyyy} - {summary.WeekEndDate:dd.MM.yyyy}\n");
            message.AppendLine($"{summary.Summary}\n");
            message.AppendLine("🔍 Top Findings:");
            foreach (var finding in summary.TopFindings)
            {
                message.AppendLine($"  • {finding}");
            }
            message.AppendLine();
            message.AppendLine("✅ Verbesserungen:");
            foreach (var improvement in summary.Improvements.Take(3))
            {
                message.AppendLine($"  • {improvement}");
            }
            message.AppendLine();
            message.AppendLine("⚠️ Probleme:");
            foreach (var issue in summary.Issues.Take(3))
            {
                message.AppendLine($"  • {issue}");
            }
            message.AppendLine();
            message.AppendLine("💡 Empfehlungen:");
            foreach (var rec in summary.Recommendations.Take(3))
            {
                message.AppendLine($"  • {rec}");
            }

            MessageBox.Show(message.ToString(), "Weekly Performance Summary",
                MessageBoxButton.OK, MessageBoxImage.Information);

            StatusMessage = "Weekly Summary angezeigt";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

            var message = new StringBuilder();
            message.AppendLine("📊 Executive Performance Summary\n");
            message.AppendLine($"{summary.ExecutiveOverview}\n");
            message.AppendLine("📈 Key Numbers:");
            foreach (var kvp in summary.KeyNumbers)
            {
                message.AppendLine($"  • {kvp.Key}: {kvp.Value}");
            }
            message.AppendLine();
            message.AppendLine("✅ Positive Highlights:");
            foreach (var highlight in summary.PositiveHighlights)
            {
                message.AppendLine($"  • {highlight}");
            }
            message.AppendLine();
            message.AppendLine("⚠️ Concern Areas:");
            foreach (var concern in summary.ConcernAreas)
            {
                message.AppendLine($"  • {concern}");
            }
            message.AppendLine();
            message.AppendLine($"💰 Business Impact:\n{summary.BusinessImpact}\n");
            message.AppendLine("📋 Executive Recommendations:");
            foreach (var rec in summary.ExecutiveRecommendations)
            {
                message.AppendLine($"  • {rec}");
            }

            MessageBox.Show(message.ToString(), "Executive Summary",
                MessageBoxButton.OK, MessageBoxImage.Information);

            StatusMessage = "Executive Summary angezeigt";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Fehler";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ViewInsightDetails(PerformanceInsight? insight)
    {
        if (insight == null) return;

        var message = new StringBuilder();
        message.AppendLine($"💡 {insight.Title}\n");
        message.AppendLine($"Severity: {insight.Severity}");
        message.AppendLine($"Category: {insight.Category}");
        message.AppendLine($"Impact Score: {insight.ImpactScore:F0}/100");
        message.AppendLine($"Confidence: {insight.ConfidenceScore:F0}%");
        message.AppendLine($"Potential Improvement: +{insight.PotentialImprovement:F0}%\n");
        message.AppendLine($"Description:\n{insight.Description}\n");
        message.AppendLine("Recommended Actions:");
        foreach (var action in insight.RecommendedActions)
        {
            message.AppendLine($"  • {action}");
        }

        MessageBox.Show(message.ToString(), "Insight Details",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ViewOpportunityDetails(OptimizationOpportunity? opportunity)
    {
        if (opportunity == null) return;

        var message = new StringBuilder();
        message.AppendLine($"💡 {opportunity.Title}\n");
        message.AppendLine($"Type: {opportunity.OpportunityType}");
        message.AppendLine($"Priority: {opportunity.PriorityLevel} ({opportunity.PriorityScore}/100)");
        message.AppendLine($"Effort: {opportunity.EffortLevel} ({opportunity.EstimatedImplementationTime:F1}h)\n");
        message.AppendLine($"Description:\n{opportunity.Description}\n");
        message.AppendLine($"💰 Impact:");
        message.AppendLine($"  • Cost Savings: €{opportunity.EstimatedCostSavings:F2}/month");
        message.AppendLine($"  • Time Savings: {opportunity.EstimatedTimeSavings:F0}ms");
        message.AppendLine($"  • Affected Queries: {opportunity.AffectedQueries}\n");
        message.AppendLine($"📊 ROI Analysis:");
        message.AppendLine($"  • ROI: {opportunity.ROI:F1}x");
        message.AppendLine($"  • Payback Period: {opportunity.PaybackPeriod:F1} days\n");
        message.AppendLine("🔧 Implementation Steps:");
        foreach (var step in opportunity.ImplementationSteps)
        {
            message.AppendLine($"  {step}");
        }
        message.AppendLine();
        message.AppendLine($"Automation Available: {opportunity.AutomationAvailable}");

        MessageBox.Show(message.ToString(), "Optimization Opportunity",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task ExportDashboardAsync()
    {
        if (Dashboard == null)
        {
            MessageBox.Show("Kein Dashboard geladen.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            MessageBox.Show($"Dashboard exportiert!\n\nDatei: {filePath}",
                "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);

            StatusMessage = "Dashboard exportiert";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export fehlgeschlagen: {ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
