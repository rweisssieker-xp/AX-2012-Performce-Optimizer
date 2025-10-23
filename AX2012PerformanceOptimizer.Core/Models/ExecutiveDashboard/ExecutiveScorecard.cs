namespace AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

public enum PerformanceGrade
{
    A,      // 90-100: Excellent
    BPlus,  // 85-89: Very Good
    B,      // 80-84: Good
    C,      // 70-79: Acceptable
    D,      // 60-69: Poor
    F       // 0-59: Critical
}

public enum TrendDirection
{
    Improving,
    Stable,
    Degrading
}

public class ExecutiveScorecard
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int OverallScore { get; set; }  // 0-100
    public PerformanceGrade Grade { get; set; }
    public TrendDirection Trend { get; set; }
    public int TrendChangePoints { get; set; }  // +/- points vs previous period

    // Category Scores
    public int QueryPerformanceScore { get; set; }
    public int DatabaseHealthScore { get; set; }
    public int AosPerformanceScore { get; set; }
    public int BatchJobSuccessScore { get; set; }
    public int ResourceUtilizationScore { get; set; }
    public int IndexMaintenanceScore { get; set; }
    public int StatisticsFreshnessScore { get; set; }
    public int BackupRecoveryScore { get; set; }

    // Business Metrics
    public decimal MonthlyCostSavings { get; set; }  // € saved from optimizations
    public decimal AnnualProjectedSavings { get; set; }
    public int OptimizationsImplemented { get; set; }
    public int IssuesPrevented { get; set; }
    public double AverageResponseTime { get; set; }  // ms
    public double SlaCompliance { get; set; }  // percentage

    // Top Issues
    public List<string> CriticalIssues { get; set; } = new();
    public List<string> TopAchievements { get; set; } = new();
    public List<KeyPerformanceIndicator> KPIs { get; set; } = new();
}

public class KeyPerformanceIndicator
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double TargetValue { get; set; }
    public double PreviousValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public TrendDirection Trend { get; set; }
    public string Status { get; set; } = string.Empty;  // "On Track", "At Risk", "Critical"
    public string Description { get; set; } = string.Empty;
}

public class MonthlyBusinessReview
{
    public DateTime ReportMonth { get; set; }
    public ExecutiveScorecard Scorecard { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<string> KeyHighlights { get; set; } = new();
    public List<string> ActionItems { get; set; } = new();
    public List<PerformanceTrend> Trends { get; set; } = new();
    public BudgetImpact BudgetImpact { get; set; } = new();
}

public class PerformanceTrend
{
    public string MetricName { get; set; } = string.Empty;
    public List<DataPoint> DataPoints { get; set; } = new();
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
}

public class DataPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
}

public class BudgetImpact
{
    public decimal CurrentMonthCost { get; set; }
    public decimal CurrentMonthSavings { get; set; }
    public decimal YearToDateCost { get; set; }
    public decimal YearToDateSavings { get; set; }
    public decimal ProjectedAnnualCost { get; set; }
    public decimal ProjectedAnnualSavings { get; set; }
    public decimal BudgetVariance { get; set; }  // vs. planned budget
    public string BudgetStatus { get; set; } = "On Track";  // "On Track", "Over Budget", "Under Budget"
}
