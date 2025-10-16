using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for calculating monetary costs of query performance
/// </summary>
public interface IPerformanceCostCalculatorService
{
    /// <summary>
    /// Calculate total cost impact of a query
    /// </summary>
    Task<QueryCostAnalysis> CalculateQueryCostAsync(SqlQueryMetric query, CostParameters parameters);

    /// <summary>
    /// Calculate ROI of optimizing a query
    /// </summary>
    Task<OptimizationROI> CalculateOptimizationROIAsync(
        SqlQueryMetric query,
        double estimatedImprovementPercent,
        CostParameters parameters);

    /// <summary>
    /// Generate executive summary report
    /// </summary>
    Task<ExecutiveSummary> GenerateExecutiveSummaryAsync(
        List<SqlQueryMetric> queries,
        CostParameters parameters);

    /// <summary>
    /// Calculate TCO (Total Cost of Ownership)
    /// </summary>
    Task<TCOAnalysis> CalculateTCOAsync(
        List<SqlQueryMetric> queries,
        CostParameters parameters,
        int projectionMonths = 12);
}

/// <summary>
/// Cost calculation parameters
/// </summary>
public class CostParameters
{
    /// <summary>
    /// Average hourly salary (€/$/£ per hour)
    /// </summary>
    public double AverageHourlySalary { get; set; } = 50.0;

    /// <summary>
    /// Number of active users
    /// </summary>
    public int ActiveUsers { get; set; } = 100;

    /// <summary>
    /// Average queries per user per day
    /// </summary>
    public int QueriesPerUserPerDay { get; set; } = 500;

    /// <summary>
    /// Monthly hardware cost (server, licenses)
    /// </summary>
    public double MonthlyHardwareCost { get; set; } = 5000.0;

    /// <summary>
    /// Currency symbol
    /// </summary>
    public string Currency { get; set; } = "€";

    /// <summary>
    /// Business hours per day
    /// </summary>
    public double BusinessHoursPerDay { get; set; } = 8.0;

    /// <summary>
    /// Working days per year
    /// </summary>
    public int WorkingDaysPerYear { get; set; } = 250;

    /// <summary>
    /// Cost per CPU second (cloud/infrastructure)
    /// </summary>
    public double CostPerCpuSecond { get; set; } = 0.0001;
}

/// <summary>
/// Cost analysis for a single query
/// </summary>
public class QueryCostAnalysis
{
    public string QueryHash { get; set; } = string.Empty;
    public string QueryTextSample { get; set; } = string.Empty;

    // Time-based costs
    public double DailyUserProductivityCost { get; set; }
    public double MonthlyUserProductivityCost { get; set; }
    public double YearlyUserProductivityCost { get; set; }

    // Infrastructure costs
    public double DailyInfrastructureCost { get; set; }
    public double MonthlyInfrastructureCost { get; set; }
    public double YearlyInfrastructureCost { get; set; }

    // Total costs
    public double DailyTotalCost { get; set; }
    public double MonthlyTotalCost { get; set; }
    public double YearlyTotalCost { get; set; }

    // Metrics
    public double TotalWaitTimeHoursPerDay { get; set; }
    public double TotalWaitTimeHoursPerYear { get; set; }
    public int AffectedUsers { get; set; }
    public long DailyExecutions { get; set; }
    public long YearlyExecutions { get; set; }

    // Details
    public string CostBreakdown { get; set; } = string.Empty;
    public List<CostFactor> CostFactors { get; set; } = new();
}

/// <summary>
/// Individual cost contributing factor
/// </summary>
public class CostFactor
{
    public string Name { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// ROI calculation for query optimization
/// </summary>
public class OptimizationROI
{
    public string QueryHash { get; set; } = string.Empty;

    // Current costs
    public QueryCostAnalysis CurrentCost { get; set; } = new();

    // Projected costs after optimization
    public double EstimatedImprovementPercent { get; set; }
    public QueryCostAnalysis ProjectedCost { get; set; } = new();

    // Savings
    public double DailySavings { get; set; }
    public double MonthlySavings { get; set; }
    public double YearlySavings { get; set; }

    // ROI metrics
    public double InvestmentHours { get; set; } = 2.0; // Hours to implement fix
    public double InvestmentCost { get; set; }
    public double PaybackDays { get; set; }
    public double ROIPercent { get; set; }

    // Break-even analysis
    public DateTime BreakEvenDate { get; set; }
    public string ROISummary { get; set; } = string.Empty;
}

/// <summary>
/// Executive summary report
/// </summary>
public class ExecutiveSummary
{
    public DateTime GeneratedDate { get; set; }
    public int TotalQueriesAnalyzed { get; set; }

    // Aggregate costs
    public double TotalDailyCost { get; set; }
    public double TotalMonthlyCost { get; set; }
    public double TotalYearlyCost { get; set; }

    // Optimization potential
    public double EstimatedSavingsPotential { get; set; }
    public double EstimatedSavingsPercent { get; set; }

    // Top issues
    public List<QueryCostAnalysis> TopCostlyQueries { get; set; } = new();
    public List<OptimizationROI> TopROIOpportunities { get; set; } = new();

    // Key metrics
    public double TotalProductivityHoursLostPerYear { get; set; }
    public double TotalInfrastructureCostPerYear { get; set; }
    public int TotalAffectedUsers { get; set; }

    // Recommendations
    public List<ExecutiveRecommendation> Recommendations { get; set; } = new();
}

/// <summary>
/// Executive recommendation
/// </summary>
public class ExecutiveRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialSavings { get; set; }
    public string Priority { get; set; } = string.Empty; // Critical, High, Medium
    public int ImplementationEffortDays { get; set; }
}

/// <summary>
/// Total Cost of Ownership analysis
/// </summary>
public class TCOAnalysis
{
    public int ProjectionMonths { get; set; }

    // Current state
    public double CurrentMonthlyCost { get; set; }
    public double CurrentProjectedCost { get; set; }

    // Optimized state
    public double OptimizedMonthlyCost { get; set; }
    public double OptimizedProjectedCost { get; set; }

    // Savings
    public double TotalSavings { get; set; }
    public double SavingsPercent { get; set; }

    // Month-by-month breakdown
    public List<MonthlyTCO> MonthlyBreakdown { get; set; } = new();

    // Categories
    public double UserProductivityCost { get; set; }
    public double InfrastructureCost { get; set; }
    public double LicensingCost { get; set; }
    public double ManpowerCost { get; set; }

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Monthly TCO data point
/// </summary>
public class MonthlyTCO
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public double CurrentCost { get; set; }
    public double OptimizedCost { get; set; }
    public double Savings { get; set; }
    public double CumulativeSavings { get; set; }
}
