using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using System.Text;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ISqlQueryMonitorService _sqlMonitor;
    private readonly IAosMonitorService _aosMonitor;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IAiPerformanceInsightsService? _insightsService;

    [ObservableProperty]
    private int activeUsers;

    [ObservableProperty]
    private int runningBatchJobs;

    [ObservableProperty]
    private long databaseSizeMB;

    [ObservableProperty]
    private int expensiveQueries;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public DashboardViewModel(
        ISqlQueryMonitorService sqlMonitor,
        IAosMonitorService aosMonitor,
        IBatchJobMonitorService batchJobMonitor,
        IDatabaseStatsService databaseStats,
        IAiPerformanceInsightsService? insightsService = null)
    {
        _sqlMonitor = sqlMonitor;
        _aosMonitor = aosMonitor;
        _batchJobMonitor = batchJobMonitor;
        _databaseStats = databaseStats;
        _insightsService = insightsService;

        // Initialize with demo data
        LoadDemoData();
    }

    private void LoadDemoData()
    {
        // Show demo data when no connection is available
        ActiveUsers = 42;
        RunningBatchJobs = 7;
        DatabaseSizeMB = 15360; // 15 GB
        ExpensiveQueries = 23;
        StatusMessage = "Demo Mode - Connect to database for live data";
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading dashboard data...";

        try
        {
            var aosMetrics = await _aosMonitor.GetAosMetricsAsync();
            ActiveUsers = aosMetrics.ActiveUserSessions;

            var batchJobs = await _batchJobMonitor.GetRunningBatchJobsAsync();
            RunningBatchJobs = batchJobs.Count;

            var dbMetrics = await _databaseStats.GetDatabaseMetricsAsync();
            DatabaseSizeMB = dbMetrics.TotalSizeMB;

            var queries = await _sqlMonitor.GetTopExpensiveQueriesAsync(10);
            ExpensiveQueries = queries.Count;

            StatusMessage = "Data loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection not configured - Showing demo data";
            // Keep demo data on connection error
            LoadDemoData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    // ===== AI Performance Insights Commands (Phase 1 Features) =====

    [RelayCommand]
    private async Task GenerateInsightsDashboardAsync()
    {
        if (_insightsService == null)
        {
            MessageBox.Show("AI Performance Insights Service is not available.",
                "AI Insights", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generating AI Performance Insights...";

            var dashboard = await _insightsService.GenerateInsightsDashboardAsync(
                DateTime.Now.AddDays(-7),
                DateTime.Now);

            var message = new StringBuilder();
            message.AppendLine("📊 AI Performance Insights Dashboard");
            message.AppendLine();
            message.AppendLine($"Performance Score: {dashboard.Metrics.PerformanceGrade} ({dashboard.Metrics.OverallPerformanceScore:F0}/100)");
            message.AppendLine($"Trend: {dashboard.Metrics.TrendDirection} ({dashboard.Metrics.PerformanceChange:+0.0;-0.0}%)");
            message.AppendLine($"Total Queries: {dashboard.Metrics.TotalQueries:N0}");
            message.AppendLine($"Slow Queries: {dashboard.Metrics.SlowQueryPercentage:F1}%");
            message.AppendLine();
            message.AppendLine($"💰 Estimated Cost: €{dashboard.Metrics.EstimatedDailyCost:F2}/day");
            message.AppendLine();
            message.AppendLine("🔍 Top Insights:");
            foreach (var insight in dashboard.TopInsights.Take(3))
            {
                message.AppendLine($"  • [{insight.Severity}] {insight.Title}");
            }
            message.AppendLine();
            message.AppendLine($"Summary: {dashboard.ExecutiveSummary}");

            MessageBox.Show(message.ToString(), "AI Performance Insights",
                MessageBoxButton.OK, MessageBoxImage.Information);

            StatusMessage = "AI Insights generated successfully";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating insights: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error generating insights";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowWeeklySummaryAsync()
    {
        if (_insightsService == null)
        {
            MessageBox.Show("AI Performance Insights Service is not available.",
                "Weekly Summary", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generating Weekly Performance Summary...";

            var summary = await _insightsService.GenerateWeeklySummaryAsync();

            var message = new StringBuilder();
            message.AppendLine("📅 Weekly Performance Summary");
            message.AppendLine($"Period: {summary.WeekStartDate:yyyy-MM-dd} to {summary.WeekEndDate:yyyy-MM-dd}");
            message.AppendLine();
            message.AppendLine(summary.Summary);
            message.AppendLine();
            message.AppendLine("🔍 Top Findings:");
            foreach (var finding in summary.TopFindings)
            {
                message.AppendLine($"  • {finding}");
            }
            message.AppendLine();
            message.AppendLine("✅ Improvements:");
            foreach (var improvement in summary.Improvements.Take(3))
            {
                message.AppendLine($"  • {improvement}");
            }
            message.AppendLine();
            message.AppendLine("⚠️ Issues:");
            foreach (var issue in summary.Issues.Take(3))
            {
                message.AppendLine($"  • {issue}");
            }
            message.AppendLine();
            message.AppendLine("💡 Recommendations:");
            foreach (var rec in summary.Recommendations.Take(3))
            {
                message.AppendLine($"  • {rec}");
            }

            MessageBox.Show(message.ToString(), "Weekly Performance Summary",
                MessageBoxButton.OK, MessageBoxImage.Information);

            StatusMessage = "Weekly summary generated";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating summary: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error generating summary";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowOptimizationOpportunitiesAsync()
    {
        if (_insightsService == null)
        {
            MessageBox.Show("AI Performance Insights Service is not available.",
                "Optimization Opportunities", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Finding optimization opportunities...";

            var opportunities = await _insightsService.FindOptimizationOpportunitiesAsync();

            var message = new StringBuilder();
            message.AppendLine($"💡 Optimization Opportunities ({opportunities.Count} found)");
            message.AppendLine();

            foreach (var opp in opportunities.Take(5))
            {
                message.AppendLine($"[{opp.PriorityLevel}] {opp.Title}");
                message.AppendLine($"  Type: {opp.OpportunityType}");
                message.AppendLine($"  Impact: €{opp.EstimatedCostSavings:F2}/month, {opp.AffectedQueries} queries");
                message.AppendLine($"  Effort: {opp.EffortLevel} ({opp.EstimatedImplementationTime:F1}h)");
                message.AppendLine($"  ROI: {opp.ROI:F1}x, Payback: {opp.PaybackPeriod:F1} days");
                message.AppendLine();
            }

            MessageBox.Show(message.ToString(), "Optimization Opportunities",
                MessageBoxButton.OK, MessageBoxImage.Information);

            StatusMessage = "Opportunities identified";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error finding opportunities: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error finding opportunities";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

