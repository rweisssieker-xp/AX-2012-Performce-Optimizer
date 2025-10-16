using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// AI Performance Insights Dashboard - Automatic insight generation
/// UNIQUE FEATURE #17: Zero-config intelligence with proactive insights
/// </summary>
public interface IAiPerformanceInsightsService
{
    /// <summary>
    /// Generate comprehensive performance insights for a time period
    /// </summary>
    Task<PerformanceInsightsDashboard> GenerateInsightsDashboardAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get top insights (most important)
    /// </summary>
    Task<List<PerformanceInsight>> GetTopInsightsAsync(
        int topCount = 5);

    /// <summary>
    /// Generate weekly performance summary
    /// </summary>
    Task<WeeklyPerformanceSummary> GenerateWeeklySummaryAsync();

    /// <summary>
    /// Detect hidden opportunities for optimization
    /// </summary>
    Task<List<OptimizationOpportunity>> FindOptimizationOpportunitiesAsync();

    /// <summary>
    /// Generate executive summary (for management)
    /// </summary>
    Task<AiExecutiveSummary> GenerateExecutiveSummaryAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Detect risks and alerts
    /// </summary>
    Task<List<RiskAlert>> DetectRisksAsync();
}

/// <summary>
/// Performance Insights Dashboard
/// </summary>
public class PerformanceInsightsDashboard
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Key metrics
    public DashboardMetrics Metrics { get; set; } = new();

    // Insights by category
    public List<PerformanceInsight> TopInsights { get; set; } = new();
    public List<PerformanceInsight> PerformanceInsights { get; set; } = new();
    public List<PerformanceInsight> CostInsights { get; set; } = new();
    public List<PerformanceInsight> ReliabilityInsights { get; set; } = new();

    // Opportunities
    public List<OptimizationOpportunity> Opportunities { get; set; } = new();

    // Risks & Alerts
    public List<RiskAlert> Risks { get; set; } = new();

    // Trends
    public PerformanceTrend OverallTrend { get; set; } = new();

    // Natural language summary
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string TechnicalSummary { get; set; } = string.Empty;
}

/// <summary>
/// Dashboard metrics
/// </summary>
public class DashboardMetrics
{
    // Query metrics
    public int TotalQueries { get; set; }
    public double AvgQueryTime { get; set; }
    public double SlowQueryPercentage { get; set; }

    // Performance metrics
    public double OverallPerformanceScore { get; set; } // 0-100
    public string PerformanceGrade { get; set; } = string.Empty; // A, B, C, D, F

    // Cost metrics
    public double EstimatedDailyCost { get; set; }
    public double EstimatedMonthlyCost { get; set; }

    // Reliability metrics
    public int FailedQueries { get; set; }
    public double SuccessRate { get; set; }

    // Improvement metrics
    public double PerformanceChange { get; set; } // Percentage vs last period
    public string TrendDirection { get; set; } = string.Empty; // Improving, Degrading, Stable
}

/// <summary>
/// Weekly Performance Summary
/// </summary>
public class WeeklyPerformanceSummary
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    // Summary text
    public string Summary { get; set; } = string.Empty;

    // Key findings
    public List<string> TopFindings { get; set; } = new();

    // Improvements
    public List<string> Improvements { get; set; } = new();

    // Issues
    public List<string> Issues { get; set; } = new();

    // Recommendations
    public List<string> Recommendations { get; set; } = new();

    // Metrics
    public Dictionary<string, double> KeyMetrics { get; set; } = new();

    // Comparison with last week
    public Dictionary<string, double> ChangeVsLastWeek { get; set; } = new();
}

/// <summary>
/// Optimization Opportunity
/// </summary>
public class OptimizationOpportunity
{
    public string OpportunityId { get; set; } = Guid.NewGuid().ToString();
    public DateTime DiscoveredAt { get; set; } = DateTime.Now;

    // Opportunity details
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = string.Empty; // Performance, Cost, Reliability

    // Impact assessment
    public double EstimatedTimeSavings { get; set; } // milliseconds
    public double EstimatedCostSavings { get; set; } // currency per month
    public int AffectedQueries { get; set; }

    // Effort assessment
    public string EffortLevel { get; set; } = string.Empty; // Low, Medium, High
    public double EstimatedImplementationTime { get; set; } // hours

    // Priority
    public int PriorityScore { get; set; } // 0-100
    public string PriorityLevel { get; set; } = string.Empty; // Critical, High, Medium, Low

    // Implementation
    public List<string> ImplementationSteps { get; set; } = new();
    public string AutomationAvailable { get; set; } = string.Empty; // Yes, Partial, No

    // ROI
    public double ROI { get; set; } // Return on Investment ratio
    public double PaybackPeriod { get; set; } // days
}

/// <summary>
/// AI Executive Summary
/// </summary>
public class AiExecutiveSummary
{
    public DateTime ReportDate { get; set; } = DateTime.Now;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // High-level summary
    public string ExecutiveOverview { get; set; } = string.Empty;

    // Key numbers
    public Dictionary<string, string> KeyNumbers { get; set; } = new();

    // Highlights
    public List<string> PositiveHighlights { get; set; } = new();
    public List<string> ConcernAreas { get; set; } = new();

    // Business impact
    public string BusinessImpact { get; set; } = string.Empty;
    public double EstimatedCostImpact { get; set; }

    // Recommendations for leadership
    public List<string> ExecutiveRecommendations { get; set; } = new();

    // Visual data
    public Dictionary<string, object> ChartData { get; set; } = new();
}

/// <summary>
/// Risk Alert
/// </summary>
public class RiskAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public DateTime DetectedAt { get; set; } = DateTime.Now;

    // Alert details
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low

    // Risk assessment
    public string RiskType { get; set; } = string.Empty; // Performance, Availability, Security, Capacity
    public double RiskScore { get; set; } // 0-100
    public double Probability { get; set; } // 0-100 (likelihood of occurrence)
    public double Impact { get; set; } // 0-100 (impact if occurs)

    // Timeline
    public string TimeToImpact { get; set; } = string.Empty; // Immediate, Hours, Days, Weeks
    public DateTime? EstimatedImpactDate { get; set; }

    // Mitigation
    public List<string> MitigationActions { get; set; } = new();
    public bool AutoMitigationAvailable { get; set; }

    // Supporting data
    public Dictionary<string, object> SupportingData { get; set; } = new();
}

/// <summary>
/// Performance Trend
/// </summary>
public class PerformanceTrend
{
    public string TrendDirection { get; set; } = string.Empty; // Improving, Degrading, Stable
    public double TrendPercentage { get; set; }
    public string TrendDescription { get; set; } = string.Empty;

    // Time series data
    public List<TrendDataPoint> DataPoints { get; set; } = new();

    // Prediction
    public double? Predicted7Days { get; set; }
    public double? Predicted30Days { get; set; }
    public double? ConfidenceScore { get; set; }
}

/// <summary>
/// Trend Data Point
/// </summary>
public class TrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}
