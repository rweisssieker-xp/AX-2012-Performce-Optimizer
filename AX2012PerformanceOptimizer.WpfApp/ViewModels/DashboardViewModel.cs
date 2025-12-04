using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.StakeholderDashboard;
using AX2012PerformanceOptimizer.Data.SqlServer;
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
    private readonly IRoleBasedDashboardService _roleDashboardService;
    private readonly ISqlConnectionManager _connectionManager;

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
    
    [ObservableProperty]
    private decimal dailyCost;
    
    [ObservableProperty]
    private decimal monthlyCost;
    
    [ObservableProperty]
    private decimal potentialSavings;

    [ObservableProperty]
    private UserRole selectedRole = UserRole.DBA;

    [ObservableProperty]
    private RoleBasedDashboardData? roleData;

    [ObservableProperty]
    private List<UserRole> availableRoles = new();

    public DashboardViewModel(
        ISqlQueryMonitorService sqlMonitor,
        IAosMonitorService aosMonitor,
        IBatchJobMonitorService batchJobMonitor,
        IDatabaseStatsService databaseStats,
        IRoleBasedDashboardService roleDashboardService,
        ISqlConnectionManager connectionManager,
        IAiPerformanceInsightsService? insightsService = null)
    {
        _sqlMonitor = sqlMonitor;
        _aosMonitor = aosMonitor;
        _batchJobMonitor = batchJobMonitor;
        _databaseStats = databaseStats;
        _roleDashboardService = roleDashboardService;
        _connectionManager = connectionManager;
        _insightsService = insightsService;

        // Listen to connection changes
        _connectionManager.ConnectionChanged += OnConnectionChanged;

        // Initialize available roles
        AvailableRoles = _roleDashboardService.GetAvailableRoles().ToList();

        // Load saved role preference (simplified)
        SelectedRole = LoadRolePreference();

        // Initialize with demo data if not connected
        if (!_connectionManager.IsConnected)
        {
            LoadDemoData();
        }
        else
        {
            // Already connected - load role data
            LoadRoleDataAsync().ConfigureAwait(false);
        }
    }

    private async void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        if (e.IsConnected)
        {
            // Connection established - load role-specific data for current role
            StatusMessage = $"Connected - Loading {SelectedRole} dashboard...";
            await LoadRoleDataAsync();
        }
        else
        {
            // Connection lost - clear role data and show demo data
            RoleData = null;
            LoadDemoData();
            StatusMessage = "Connection lost - Showing demo data";
        }
    }

    partial void OnSelectedRoleChanged(UserRole value)
    {
        LoadRoleDataAsync().ConfigureAwait(false);
        SaveRolePreference(value);
    }

    [RelayCommand]
    private async Task LoadRoleDataAsync()
    {
        // Only load role data if connected, otherwise show demo/default dashboard
        if (!_connectionManager.IsConnected)
        {
            RoleData = null;
            LoadDemoData();
            StatusMessage = "Not connected - Showing demo data. Connect to database to see role-specific dashboard.";
            return;
        }

        IsLoading = true;
        StatusMessage = $"Loading {SelectedRole} dashboard...";

        try
        {
            var timeRange = new TimeRange
            {
                StartTime = DateTime.UtcNow.AddHours(-24),
                EndTime = DateTime.UtcNow
            };

            RoleData = await _roleDashboardService.GetDashboardDataAsync(SelectedRole, timeRange);
            
            // Also update the default dashboard metrics with real data
            await LoadDataAsync();
            
            // Update cost data from role-specific metrics if available
            if (RoleData?.RoleSpecificMetrics != null)
            {
                if (RoleData.RoleSpecificMetrics.TryGetValue("DailyCost", out var dailyCostObj) && dailyCostObj is double dailyCost)
                {
                    DailyCost = (decimal)dailyCost;
                }
                if (RoleData.RoleSpecificMetrics.TryGetValue("MonthlyCost", out var monthlyCostObj) && monthlyCostObj is double monthlyCost)
                {
                    MonthlyCost = (decimal)monthlyCost;
                }
            }
            
            StatusMessage = $"{SelectedRole} dashboard loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading {SelectedRole} dashboard: {ex.Message}";
            RoleData = null;
            LoadDemoData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ChangeRole(UserRole role)
    {
        SelectedRole = role;
    }

    private UserRole LoadRolePreference()
    {
        // Simplified - would use ISettingsService
        return UserRole.DBA; // Default
    }

    private void SaveRolePreference(UserRole role)
    {
        // Simplified - would use ISettingsService
        // Store preference: "SelectedDashboardRole" = role
    }

    private void LoadDemoData()
    {
        // Show demo data when no connection is available
        ActiveUsers = 42;
        RunningBatchJobs = 7;
        DatabaseSizeMB = 15360; // 15 GB
        ExpensiveQueries = 23;
        DailyCost = 125.50m;
        MonthlyCost = 3765.00m;
        PotentialSavings = 850.00m;
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

            // Calculate cost based on expensive queries (simplified calculation)
            // Assuming each expensive query costs ~€5-15/day based on CPU time
            if (queries.Count > 0)
            {
                var totalCpuMs = queries.Sum(q => q.AvgCpuTimeMs * q.ExecutionCount / 1000.0);
                var estimatedDailyCost = (decimal)(totalCpuMs * 0.001); // €0.001 per CPU second
                if (estimatedDailyCost < 10) estimatedDailyCost = queries.Count * 12.5m; // Minimum cost estimate
                
                DailyCost = Math.Round(estimatedDailyCost, 2);
                MonthlyCost = Math.Round(DailyCost * 30, 2);
                PotentialSavings = Math.Round(DailyCost * 30 * 0.2m, 2); // 20% potential savings
            }

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

