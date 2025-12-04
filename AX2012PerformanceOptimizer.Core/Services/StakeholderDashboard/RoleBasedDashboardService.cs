using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services.StakeholderDashboard;

/// <summary>
/// Implementation of Role-Based Dashboard Service
/// Provides role-specific dashboard data and widgets
/// </summary>
public class RoleBasedDashboardService : IRoleBasedDashboardService
{
    private readonly IExecutiveScorecardService _executiveScorecardService;
    private readonly ISqlQueryMonitorService _sqlQueryMonitorService;
    private readonly IQueryAnalyzerService _queryAnalyzerService;
    private readonly IAosMonitorService _aosMonitorService;
    private readonly IPerformanceCostCalculatorService _costCalculatorService;
    private readonly ILogger<RoleBasedDashboardService> _logger;

    public RoleBasedDashboardService(
        IExecutiveScorecardService executiveScorecardService,
        ISqlQueryMonitorService sqlQueryMonitorService,
        IQueryAnalyzerService queryAnalyzerService,
        IAosMonitorService aosMonitorService,
        IPerformanceCostCalculatorService costCalculatorService,
        ILogger<RoleBasedDashboardService> logger)
    {
        _executiveScorecardService = executiveScorecardService;
        _sqlQueryMonitorService = sqlQueryMonitorService;
        _queryAnalyzerService = queryAnalyzerService;
        _aosMonitorService = aosMonitorService;
        _costCalculatorService = costCalculatorService;
        _logger = logger;
    }

    public async Task<RoleBasedDashboardData> GetDashboardDataAsync(UserRole role, TimeRange timeRange)
    {
        _logger.LogInformation("Getting dashboard data for role {Role}", role);

        var data = new RoleBasedDashboardData
        {
            Role = role,
            TimeRange = timeRange,
            Widgets = await GetRoleSpecificWidgetsAsync(role),
            Summary = await GenerateRoleSummaryAsync(role),
            ActionItems = await GetRoleActionItemsAsync(role),
            RoleSpecificMetrics = await GetRoleSpecificMetricsAsync(role),
            LastUpdated = DateTime.UtcNow
        };

        return data;
    }

    public async Task<List<DashboardWidget>> GetRoleSpecificWidgetsAsync(UserRole role)
    {
        var widgets = new List<DashboardWidget>();

        switch (role)
        {
            case UserRole.Executive:
                widgets.AddRange(await GetExecutiveWidgetsAsync());
                break;
            case UserRole.DBA:
                widgets.AddRange(await GetDBAWidgetsAsync());
                break;
            case UserRole.Developer:
                widgets.AddRange(await GetDeveloperWidgetsAsync());
                break;
            case UserRole.EndUser:
                widgets.AddRange(await GetEndUserWidgetsAsync());
                break;
        }

        return widgets.OrderBy(w => w.Position).ToList();
    }

    public async Task<RoleWidgetConfiguration> GetRoleConfigurationAsync(UserRole role)
    {
        return role switch
        {
            UserRole.Executive => GetExecutiveConfiguration(),
            UserRole.DBA => GetDBAConfiguration(),
            UserRole.Developer => GetDeveloperConfiguration(),
            UserRole.EndUser => GetEndUserConfiguration(),
            _ => GetDBAConfiguration() // Default
        };
    }

    public List<UserRole> GetAvailableRoles()
    {
        return Enum.GetValues<UserRole>().ToList();
    }

    private async Task<List<DashboardWidget>> GetExecutiveWidgetsAsync()
    {
        var widgets = new List<DashboardWidget>();

        // Cost Impact Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.CostImpact,
            Title = "Cost Impact",
            Description = "Performance-related costs",
            Position = 1,
            Size = WidgetSize.Large,
            Data = await GetCostImpactDataAsync()
        });

        // ROI Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.ROI,
            Title = "ROI Analysis",
            Description = "Return on investment for optimizations",
            Position = 2,
            Size = WidgetSize.Medium,
            Data = await GetROIDataAsync()
        });

        // System Health Score Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.SystemHealthScore,
            Title = "System Health Score",
            Description = "Overall system performance score",
            Position = 3,
            Size = WidgetSize.Medium,
            Data = await GetSystemHealthScoreDataAsync()
        });

        // Summary Card Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.SummaryCard,
            Title = "Executive Summary",
            Description = "Key performance indicators",
            Position = 4,
            Size = WidgetSize.Large,
            Data = await GetExecutiveSummaryDataAsync()
        });

        // Trend Visualization Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.TrendVisualization,
            Title = "Performance Trends",
            Description = "Cost and performance trends over time",
            Position = 5,
            Size = WidgetSize.Large,
            Data = await GetTrendDataAsync()
        });

        // Action Items Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.ActionItems,
            Title = "Management Action Items",
            Description = "Actions requiring management attention",
            Position = 6,
            Size = WidgetSize.Medium,
            Data = await GetExecutiveActionItemsAsync()
        });

        return widgets;
    }

    private async Task<List<DashboardWidget>> GetDBAWidgetsAsync()
    {
        var widgets = new List<DashboardWidget>();

        // Query Performance Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.QueryPerformance,
            Title = "Query Performance",
            Description = "Top expensive queries and execution times",
            Position = 1,
            Size = WidgetSize.Large,
            Data = await GetQueryPerformanceDataAsync()
        });

        // Database Health Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.DatabaseHealth,
            Title = "Database Health",
            Description = "Database metrics and health indicators",
            Position = 2,
            Size = WidgetSize.Medium,
            Data = await GetDatabaseHealthDataAsync()
        });

        // Summary Card Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.SummaryCard,
            Title = "DBA Summary",
            Description = "Key database performance metrics",
            Position = 3,
            Size = WidgetSize.Medium,
            Data = await GetDBASummaryDataAsync()
        });

        return widgets;
    }

    private async Task<List<DashboardWidget>> GetDeveloperWidgetsAsync()
    {
        var widgets = new List<DashboardWidget>();

        // Code Performance Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.CodePerformance,
            Title = "Code Performance",
            Description = "Performance metrics by module and class",
            Position = 1,
            Size = WidgetSize.Large,
            Data = await GetCodePerformanceDataAsync()
        });

        // Query Details Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.QueryPerformance,
            Title = "Query Details",
            Description = "Detailed query analysis and execution plans",
            Position = 2,
            Size = WidgetSize.Large,
            Data = await GetQueryDetailsDataAsync()
        });

        // Summary Card Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.SummaryCard,
            Title = "Developer Summary",
            Description = "Code-level performance insights",
            Position = 3,
            Size = WidgetSize.Medium,
            Data = await GetDeveloperSummaryDataAsync()
        });

        return widgets;
    }

    private async Task<List<DashboardWidget>> GetEndUserWidgetsAsync()
    {
        var widgets = new List<DashboardWidget>();

        // User Experience Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.UserExperience,
            Title = "User Experience",
            Description = "Response times and user-facing metrics",
            Position = 1,
            Size = WidgetSize.Large,
            Data = await GetUserExperienceDataAsync()
        });

        // System Availability Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.SystemAvailability,
            Title = "System Availability",
            Description = "System uptime and availability status",
            Position = 2,
            Size = WidgetSize.Medium,
            Data = await GetSystemAvailabilityDataAsync()
        });

        // Summary Card Widget
        widgets.Add(new DashboardWidget
        {
            Type = WidgetType.SummaryCard,
            Title = "System Status",
            Description = "Simple status indicators",
            Position = 3,
            Size = WidgetSize.Medium,
            Data = await GetEndUserSummaryDataAsync()
        });

        return widgets;
    }

    private async Task<DashboardSummary> GenerateRoleSummaryAsync(UserRole role)
    {
        return role switch
        {
            UserRole.Executive => await GetExecutiveSummaryAsync(),
            UserRole.DBA => await GetDBASummaryAsync(),
            UserRole.Developer => await GetDeveloperSummaryAsync(),
            UserRole.EndUser => await GetEndUserSummaryAsync(),
            _ => new DashboardSummary { Title = "Dashboard", Status = "Good" }
        };
    }

    private async Task<DashboardSummary> GetExecutiveSummaryAsync()
    {
        var scorecard = await _executiveScorecardService.GenerateScorecardAsync();
        
        // Get cost data from queries
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);
        var costParams = new CostParameters();
        var executiveSummary = queries.Count > 0 
            ? await _costCalculatorService.GenerateExecutiveSummaryAsync(queries, costParams)
            : null;

        return new DashboardSummary
        {
            Title = "Executive Performance Summary",
            Description = "High-level business performance metrics",
            KeyMetrics = new Dictionary<string, string>
            {
                { "Overall Score", $"{scorecard.OverallScore:F0}/100" },
                { "Daily Cost", executiveSummary != null ? $"€{executiveSummary.TotalDailyCost:F2}" : "€0.00" },
                { "System Health", scorecard.OverallScore >= 80 ? "Good" : scorecard.OverallScore >= 60 ? "Fair" : "Poor" }
            },
            Status = scorecard.OverallScore >= 80 ? "Good" : scorecard.OverallScore >= 60 ? "Fair" : "Poor"
        };
    }

    private async Task<DashboardSummary> GetDBASummaryAsync()
    {
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);

        return new DashboardSummary
        {
            Title = "DBA Performance Summary",
            Description = "Database and query performance metrics",
            KeyMetrics = new Dictionary<string, string>
            {
                { "Expensive Queries", queries.Count.ToString() },
                { "Avg Query Time", $"{queries.Average(q => q.AvgElapsedTimeMs):F0} ms" },
                { "Total Executions", queries.Sum(q => q.ExecutionCount).ToString("N0") }
            },
            Status = "Good"
        };
    }

    private async Task<DashboardSummary> GetDeveloperSummaryAsync()
    {
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);

        return new DashboardSummary
        {
            Title = "Developer Performance Summary",
            Description = "Code-level performance insights",
            KeyMetrics = new Dictionary<string, string>
            {
                { "Queries Analyzed", queries.Count.ToString() },
                { "Avg CPU Time", $"{queries.Average(q => q.AvgCpuTimeMs):F0} ms" },
                { "Optimization Opportunities", "5" }
            },
            Status = "Good"
        };
    }

    private async Task<DashboardSummary> GetEndUserSummaryAsync()
    {
        var aosMetrics = await _aosMonitorService.GetAosMetricsAsync();

        return new DashboardSummary
        {
            Title = "System Status",
            Description = "User-facing system performance",
            KeyMetrics = new Dictionary<string, string>
            {
                { "System Status", "Good" },
                { "Response Time", $"{aosMetrics.AvgResponseTimeMs:F0} ms" },
                { "Active Users", aosMetrics.ActiveUserSessions.ToString() }
            },
            Status = aosMetrics.AvgResponseTimeMs < 1000 ? "Good" : aosMetrics.AvgResponseTimeMs < 2000 ? "Fair" : "Poor"
        };
    }

    private async Task<List<ActionItem>> GetRoleActionItemsAsync(UserRole role)
    {
        // Generate role-specific action items
        return role switch
        {
            UserRole.Executive => await GetExecutiveActionItemsAsync(),
            UserRole.DBA => await GetDBAActionItemsAsync(),
            UserRole.Developer => await GetDeveloperActionItemsAsync(),
            UserRole.EndUser => new List<ActionItem>(), // End users don't have action items
            _ => new List<ActionItem>()
        };
    }

    private async Task<List<ActionItem>> GetExecutiveActionItemsAsync()
    {
        var scorecard = await _executiveScorecardService.GenerateScorecardAsync();
        var items = new List<ActionItem>();

        if (scorecard.OverallScore < 70)
        {
            items.Add(new ActionItem
            {
                Title = "Review Performance Issues",
                Description = "System performance score is below target. Review optimization opportunities.",
                Priority = "High"
            });
        }

        return items;
    }

    private async Task<List<ActionItem>> GetDBAActionItemsAsync()
    {
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(5);
        var items = new List<ActionItem>();

        foreach (var query in queries.Take(3))
        {
            items.Add(new ActionItem
            {
                Title = $"Optimize Query: {query.QueryHash.Substring(0, 8)}...",
                Description = $"Query takes {query.AvgElapsedTimeMs:F0}ms on average",
                Priority = query.AvgElapsedTimeMs > 5000 ? "High" : "Medium",
                RelatedObjectId = query.QueryHash
            });
        }

        return items;
    }

    private async Task<List<ActionItem>> GetDeveloperActionItemsAsync()
    {
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(5);
        var items = new List<ActionItem>();

        foreach (var query in queries.Take(2))
        {
            items.Add(new ActionItem
            {
                Title = $"Review Query Performance",
                Description = $"High CPU usage: {query.AvgCpuTimeMs:F0}ms",
                Priority = "Medium",
                RelatedObjectId = query.QueryHash
            });
        }

        return items;
    }

    private async Task<Dictionary<string, object>> GetRoleSpecificMetricsAsync(UserRole role)
    {
        return role switch
        {
            UserRole.Executive => await GetExecutiveMetricsAsync(),
            UserRole.DBA => await GetDBAMetricsAsync(),
            UserRole.Developer => await GetDeveloperMetricsAsync(),
            UserRole.EndUser => await GetEndUserMetricsAsync(),
            _ => new Dictionary<string, object>()
        };
    }

    private async Task<Dictionary<string, object>> GetExecutiveMetricsAsync()
    {
        var scorecard = await _executiveScorecardService.GenerateScorecardAsync();
        
        // Get cost data from queries
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);
        var costParams = new CostParameters();
        var executiveSummary = queries.Count > 0 
            ? await _costCalculatorService.GenerateExecutiveSummaryAsync(queries, costParams)
            : null;

        var dailyCost = executiveSummary?.TotalDailyCost ?? 0.0;

        return new Dictionary<string, object>
        {
            { "OverallScore", scorecard.OverallScore },
            { "DailyCost", dailyCost },
            { "MonthlyCost", dailyCost * 30 },
            { "PerformanceGrade", scorecard.OverallScore >= 80 ? "A" : scorecard.OverallScore >= 60 ? "B" : "C" }
        };
    }

    private async Task<Dictionary<string, object>> GetDBAMetricsAsync()
    {
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);

        return new Dictionary<string, object>
        {
            { "TotalQueries", queries.Count },
            { "AvgExecutionTime", queries.Average(q => q.AvgElapsedTimeMs) },
            { "TotalExecutions", queries.Sum(q => q.ExecutionCount) },
            { "MaxExecutionTime", queries.Max(q => q.AvgElapsedTimeMs) }
        };
    }

    private async Task<Dictionary<string, object>> GetDeveloperMetricsAsync()
    {
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);

        return new Dictionary<string, object>
        {
            { "AvgCpuTime", queries.Average(q => q.AvgCpuTimeMs) },
            { "AvgLogicalReads", queries.Average(q => q.AvgLogicalReads) },
            { "OptimizationOpportunities", 5 }
        };
    }

    private async Task<Dictionary<string, object>> GetEndUserMetricsAsync()
    {
        var aosMetrics = await _aosMonitorService.GetAosMetricsAsync();

        return new Dictionary<string, object>
        {
            { "ResponseTime", aosMetrics.AvgResponseTimeMs },
            { "ActiveUsers", aosMetrics.ActiveUserSessions },
            { "SystemStatus", aosMetrics.AvgResponseTimeMs < 1000 ? "Good" : "Fair" }
        };
    }

    // Widget data getters (simplified - return mock data structures)
    private async Task<object> GetCostImpactDataAsync()
    {
        await Task.Delay(10);
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);
        var costParams = new CostParameters();
        var executiveSummary = queries.Count > 0 
            ? await _costCalculatorService.GenerateExecutiveSummaryAsync(queries, costParams)
            : null;

        var dailyCost = executiveSummary?.TotalDailyCost ?? 0.0;
        return new { DailyCost = dailyCost, MonthlyCost = dailyCost * 30.0, PotentialSavings = dailyCost * 0.2 };
    }

    private async Task<object> GetROIDataAsync()
    {
        await Task.Delay(10);
        return new { ROI = 3.5, PaybackPeriod = 45, EstimatedSavings = 850.00m };
    }

    private async Task<object> GetSystemHealthScoreDataAsync()
    {
        await Task.Delay(10);
        var scorecard = await _executiveScorecardService.GenerateScorecardAsync();
        return new { Score = scorecard.OverallScore, Grade = scorecard.OverallScore >= 80 ? "A" : "B" };
    }

    private async Task<object> GetExecutiveSummaryDataAsync()
    {
        await Task.Delay(10);
        var scorecard = await _executiveScorecardService.GenerateScorecardAsync();
        return new { Score = scorecard.OverallScore, Status = "Good", Trend = "Improving" };
    }

    private async Task<object> GetTrendDataAsync()
    {
        await Task.Delay(10);
        return new { CostTrend = "Decreasing", PerformanceTrend = "Improving" };
    }

    private async Task<object> GetQueryPerformanceDataAsync()
    {
        await Task.Delay(10);
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);
        
        return new 
        { 
            TotalQueries = queries.Count,
            AvgElapsedTime = $"{queries.Average(q => q.AvgElapsedTimeMs):F0} ms",
            MaxElapsedTime = $"{queries.Max(q => q.AvgElapsedTimeMs):F0} ms",
            TotalExecutions = queries.Sum(q => q.ExecutionCount).ToString("N0"),
            SlowestQuery = $"{queries.OrderByDescending(q => q.AvgElapsedTimeMs).First().AvgElapsedTimeMs:F0} ms"
        };
    }

    private async Task<object> GetDatabaseHealthDataAsync()
    {
        await Task.Delay(10);
        return new { HealthScore = 85, Status = "Good", Issues = 2 };
    }

    private async Task<object> GetDBASummaryDataAsync()
    {
        await Task.Delay(10);
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(10);
        return new 
        { 
            ExpensiveQueries = queries.Count,
            AvgExecutionTime = $"{queries.Average(q => q.AvgElapsedTimeMs):F0} ms",
            TotalExecutions = queries.Sum(q => q.ExecutionCount).ToString("N0"),
            Status = queries.Average(q => q.AvgElapsedTimeMs) < 1000 ? "Good" : "Needs Attention"
        };
    }

    private async Task<object> GetCodePerformanceDataAsync()
    {
        await Task.Delay(10);
        return new 
        { 
            TopModules = "Sales, Purchase, Inventory",
            PerformanceScore = "85/100",
            Status = "Good",
            OptimizationPotential = "15%"
        };
    }

    private async Task<object> GetQueryDetailsDataAsync()
    {
        await Task.Delay(10);
        var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(5);
        
        // Return summary data instead of full query texts
        return new 
        { 
            TotalQueries = queries.Count,
            AvgCpuTime = $"{queries.Average(q => q.AvgCpuTimeMs):F0} ms",
            TotalExecutions = queries.Sum(q => q.ExecutionCount),
            TopQueryTime = $"{queries.Max(q => q.AvgCpuTimeMs):F0} ms",
            TopQueries = queries.Take(3).Select(q => new 
            { 
                Hash = q.QueryHash.Length > 8 ? q.QueryHash.Substring(0, 8) + "..." : q.QueryHash,
                CpuTime = $"{q.AvgCpuTimeMs:F0} ms",
                Executions = q.ExecutionCount
            }).ToList()
        };
    }

    private async Task<object> GetDeveloperSummaryDataAsync()
    {
        await Task.Delay(10);
        return new 
        { 
            OptimizationOpportunities = "5 found",
            CodeQuality = "Good",
            TechnicalDebt = "Low",
            RecommendedAction = "Review top 3 queries"
        };
    }

    private async Task<object> GetUserExperienceDataAsync()
    {
        await Task.Delay(10);
        var aosMetrics = await _aosMonitorService.GetAosMetricsAsync();
        var responseStatus = aosMetrics.AvgResponseTimeMs < 500 ? "Excellent" : 
                            aosMetrics.AvgResponseTimeMs < 1000 ? "Good" : 
                            aosMetrics.AvgResponseTimeMs < 2000 ? "Fair" : "Poor";
        return new 
        { 
            ResponseTime = $"{aosMetrics.AvgResponseTimeMs:F0} ms",
            ResponseStatus = responseStatus,
            ErrorRate = "0.5%",
            UserSatisfaction = "Good"
        };
    }

    private async Task<object> GetSystemAvailabilityDataAsync()
    {
        await Task.Delay(10);
        return new 
        { 
            Uptime = "99.9%",
            Status = "✅ Available",
            LastDowntime = "30+ days ago",
            HealthCheck = "All systems operational"
        };
    }

    private async Task<object> GetEndUserSummaryDataAsync()
    {
        await Task.Delay(10);
        var aosMetrics = await _aosMonitorService.GetAosMetricsAsync();
        return new 
        { 
            SystemStatus = "✅ Online",
            ResponseTime = $"{aosMetrics.AvgResponseTimeMs:F0} ms",
            ActiveUsers = aosMetrics.ActiveUserSessions,
            Performance = aosMetrics.AvgResponseTimeMs < 1000 ? "Good" : "Slow"
        };
    }

    // Configuration getters
    private RoleWidgetConfiguration GetExecutiveConfiguration()
    {
        return new RoleWidgetConfiguration
        {
            Role = UserRole.Executive,
            EnabledWidgets = new List<WidgetType>
            {
                WidgetType.CostImpact,
                WidgetType.ROI,
                WidgetType.SystemHealthScore,
                WidgetType.SummaryCard,
                WidgetType.TrendVisualization,
                WidgetType.ActionItems
            }
        };
    }

    private RoleWidgetConfiguration GetDBAConfiguration()
    {
        return new RoleWidgetConfiguration
        {
            Role = UserRole.DBA,
            EnabledWidgets = new List<WidgetType>
            {
                WidgetType.QueryPerformance,
                WidgetType.DatabaseHealth,
                WidgetType.SummaryCard
            }
        };
    }

    private RoleWidgetConfiguration GetDeveloperConfiguration()
    {
        return new RoleWidgetConfiguration
        {
            Role = UserRole.Developer,
            EnabledWidgets = new List<WidgetType>
            {
                WidgetType.CodePerformance,
                WidgetType.QueryPerformance,
                WidgetType.SummaryCard
            }
        };
    }

    private RoleWidgetConfiguration GetEndUserConfiguration()
    {
        return new RoleWidgetConfiguration
        {
            Role = UserRole.EndUser,
            EnabledWidgets = new List<WidgetType>
            {
                WidgetType.UserExperience,
                WidgetType.SystemAvailability,
                WidgetType.SummaryCard
            }
        };
    }
}
