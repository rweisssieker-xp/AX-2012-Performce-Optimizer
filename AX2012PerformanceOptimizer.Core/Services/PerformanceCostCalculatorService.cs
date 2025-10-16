using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Implementation of Performance Cost Calculator
/// </summary>
public class PerformanceCostCalculatorService : IPerformanceCostCalculatorService
{
    private readonly ILogger<PerformanceCostCalculatorService> _logger;

    public PerformanceCostCalculatorService(ILogger<PerformanceCostCalculatorService> logger)
    {
        _logger = logger;
    }

    public async Task<QueryCostAnalysis> CalculateQueryCostAsync(
        SqlQueryMetric query,
        CostParameters parameters)
    {
        return await Task.Run(() =>
        {
            var analysis = new QueryCostAnalysis
            {
                QueryHash = query.QueryHash,
                QueryTextSample = query.QueryText.Length > 100
                    ? query.QueryText.Substring(0, 100) + "..."
                    : query.QueryText
            };

            // Calculate executions
            analysis.DailyExecutions = (long)(query.ExecutionCount * (parameters.QueriesPerUserPerDay / 500.0));
            analysis.YearlyExecutions = analysis.DailyExecutions * parameters.WorkingDaysPerYear;

            // Calculate wait time (user waiting for query to complete)
            double avgWaitTimeSeconds = query.AvgElapsedTimeMs / 1000.0;
            analysis.TotalWaitTimeHoursPerDay = (analysis.DailyExecutions * avgWaitTimeSeconds) / 3600.0;
            analysis.TotalWaitTimeHoursPerYear = analysis.TotalWaitTimeHoursPerDay * parameters.WorkingDaysPerYear;

            // User productivity cost (people waiting = money lost)
            analysis.DailyUserProductivityCost = analysis.TotalWaitTimeHoursPerDay * parameters.AverageHourlySalary;
            analysis.MonthlyUserProductivityCost = analysis.DailyUserProductivityCost * (parameters.WorkingDaysPerYear / 12.0);
            analysis.YearlyUserProductivityCost = analysis.DailyUserProductivityCost * parameters.WorkingDaysPerYear;

            // Infrastructure cost (CPU time = server cost)
            double totalCpuSecondsPerDay = (analysis.DailyExecutions * query.AvgCpuTimeMs) / 1000.0;
            analysis.DailyInfrastructureCost = totalCpuSecondsPerDay * parameters.CostPerCpuSecond;
            analysis.MonthlyInfrastructureCost = analysis.DailyInfrastructureCost * (parameters.WorkingDaysPerYear / 12.0);
            analysis.YearlyInfrastructureCost = analysis.DailyInfrastructureCost * parameters.WorkingDaysPerYear;

            // Total costs
            analysis.DailyTotalCost = analysis.DailyUserProductivityCost + analysis.DailyInfrastructureCost;
            analysis.MonthlyTotalCost = analysis.MonthlyUserProductivityCost + analysis.MonthlyInfrastructureCost;
            analysis.YearlyTotalCost = analysis.YearlyUserProductivityCost + analysis.YearlyInfrastructureCost;

            // Affected users (estimate based on execution frequency)
            analysis.AffectedUsers = Math.Min(parameters.ActiveUsers, (int)(analysis.DailyExecutions / 10));

            // Cost factors breakdown
            analysis.CostFactors = new List<CostFactor>
            {
                new CostFactor
                {
                    Name = "User Productivity Loss",
                    Amount = analysis.YearlyUserProductivityCost,
                    Percentage = (analysis.YearlyUserProductivityCost / analysis.YearlyTotalCost) * 100,
                    Description = $"{analysis.TotalWaitTimeHoursPerYear:F0} hours/year of user wait time"
                },
                new CostFactor
                {
                    Name = "Infrastructure & CPU",
                    Amount = analysis.YearlyInfrastructureCost,
                    Percentage = (analysis.YearlyInfrastructureCost / analysis.YearlyTotalCost) * 100,
                    Description = $"{totalCpuSecondsPerDay * parameters.WorkingDaysPerYear:F0} CPU-seconds/year"
                }
            };

            // Cost breakdown text
            analysis.CostBreakdown = GenerateCostBreakdownText(analysis, parameters);

            return analysis;
        });
    }

    public async Task<OptimizationROI> CalculateOptimizationROIAsync(
        SqlQueryMetric query,
        double estimatedImprovementPercent,
        CostParameters parameters)
    {
        var currentCost = await CalculateQueryCostAsync(query, parameters);

        // Create optimized version
        var optimizedQuery = CloneQueryWithImprovement(query, estimatedImprovementPercent);
        var projectedCost = await CalculateQueryCostAsync(optimizedQuery, parameters);

        var roi = new OptimizationROI
        {
            QueryHash = query.QueryHash,
            CurrentCost = currentCost,
            EstimatedImprovementPercent = estimatedImprovementPercent,
            ProjectedCost = projectedCost
        };

        // Calculate savings
        roi.DailySavings = currentCost.DailyTotalCost - projectedCost.DailyTotalCost;
        roi.MonthlySavings = currentCost.MonthlyTotalCost - projectedCost.MonthlyTotalCost;
        roi.YearlySavings = currentCost.YearlyTotalCost - projectedCost.YearlyTotalCost;

        // Calculate investment cost
        roi.InvestmentHours = 2.0; // Estimated 2 hours to implement fix
        roi.InvestmentCost = roi.InvestmentHours * parameters.AverageHourlySalary;

        // Calculate payback period
        if (roi.DailySavings > 0)
        {
            roi.PaybackDays = roi.InvestmentCost / roi.DailySavings;
            roi.BreakEvenDate = DateTime.Now.AddDays(roi.PaybackDays);
        }

        // Calculate ROI percentage
        if (roi.InvestmentCost > 0)
        {
            roi.ROIPercent = ((roi.YearlySavings - roi.InvestmentCost) / roi.InvestmentCost) * 100;
        }

        // Generate summary
        roi.ROISummary = GenerateROISummary(roi, parameters);

        return roi;
    }

    public async Task<ExecutiveSummary> GenerateExecutiveSummaryAsync(
        List<SqlQueryMetric> queries,
        CostParameters parameters)
    {
        var summary = new ExecutiveSummary
        {
            GeneratedDate = DateTime.Now,
            TotalQueriesAnalyzed = queries.Count
        };

        // Analyze all queries
        var analyses = new List<QueryCostAnalysis>();
        foreach (var query in queries)
        {
            var analysis = await CalculateQueryCostAsync(query, parameters);
            analyses.Add(analysis);
        }

        // Aggregate costs
        summary.TotalDailyCost = analyses.Sum(a => a.DailyTotalCost);
        summary.TotalMonthlyCost = analyses.Sum(a => a.MonthlyTotalCost);
        summary.TotalYearlyCost = analyses.Sum(a => a.YearlyTotalCost);

        // Calculate optimization potential (assume 40% average improvement)
        summary.EstimatedSavingsPotential = summary.TotalYearlyCost * 0.40;
        summary.EstimatedSavingsPercent = 40.0;

        // Top costly queries
        summary.TopCostlyQueries = analyses
            .OrderByDescending(a => a.YearlyTotalCost)
            .Take(10)
            .ToList();

        // Calculate ROI for top queries
        summary.TopROIOpportunities = new List<OptimizationROI>();
        foreach (var topQuery in queries.Take(10))
        {
            var roi = await CalculateOptimizationROIAsync(topQuery, 40.0, parameters);
            summary.TopROIOpportunities.Add(roi);
        }

        summary.TopROIOpportunities = summary.TopROIOpportunities
            .OrderByDescending(r => r.YearlySavings)
            .Take(5)
            .ToList();

        // Key metrics
        summary.TotalProductivityHoursLostPerYear = analyses.Sum(a => a.TotalWaitTimeHoursPerYear);
        summary.TotalInfrastructureCostPerYear = analyses.Sum(a => a.YearlyInfrastructureCost);
        summary.TotalAffectedUsers = parameters.ActiveUsers;

        // Generate recommendations
        summary.Recommendations = GenerateRecommendations(summary, analyses, parameters);

        return summary;
    }

    public async Task<TCOAnalysis> CalculateTCOAsync(
        List<SqlQueryMetric> queries,
        CostParameters parameters,
        int projectionMonths = 12)
    {
        var tco = new TCOAnalysis
        {
            ProjectionMonths = projectionMonths
        };

        // Calculate current costs
        var analyses = new List<QueryCostAnalysis>();
        foreach (var query in queries)
        {
            var analysis = await CalculateQueryCostAsync(query, parameters);
            analyses.Add(analysis);
        }

        tco.CurrentMonthlyCost = analyses.Sum(a => a.MonthlyTotalCost);
        tco.CurrentProjectedCost = tco.CurrentMonthlyCost * projectionMonths;

        // Calculate optimized costs (assume 40% improvement)
        tco.OptimizedMonthlyCost = tco.CurrentMonthlyCost * 0.60;
        tco.OptimizedProjectedCost = tco.OptimizedMonthlyCost * projectionMonths;

        // Savings
        tco.TotalSavings = tco.CurrentProjectedCost - tco.OptimizedProjectedCost;
        tco.SavingsPercent = (tco.TotalSavings / tco.CurrentProjectedCost) * 100;

        // Category breakdown
        tco.UserProductivityCost = analyses.Sum(a => a.MonthlyUserProductivityCost) * projectionMonths;
        tco.InfrastructureCost = analyses.Sum(a => a.MonthlyInfrastructureCost) * projectionMonths;
        tco.LicensingCost = parameters.MonthlyHardwareCost * projectionMonths * 0.3; // 30% for licenses
        tco.ManpowerCost = parameters.AverageHourlySalary * 40 * projectionMonths; // 40 hours/month DBA time

        // Month-by-month breakdown
        double cumulativeSavings = 0;
        for (int month = 1; month <= projectionMonths; month++)
        {
            double monthlySavings = tco.CurrentMonthlyCost - tco.OptimizedMonthlyCost;
            cumulativeSavings += monthlySavings;

            tco.MonthlyBreakdown.Add(new MonthlyTCO
            {
                Month = month,
                MonthName = DateTime.Now.AddMonths(month - 1).ToString("MMM yyyy"),
                CurrentCost = tco.CurrentMonthlyCost,
                OptimizedCost = tco.OptimizedMonthlyCost,
                Savings = monthlySavings,
                CumulativeSavings = cumulativeSavings
            });
        }

        tco.Summary = GenerateTCOSummary(tco, parameters);

        return tco;
    }

    // Helper methods
    private SqlQueryMetric CloneQueryWithImprovement(SqlQueryMetric query, double improvementPercent)
    {
        double factor = 1.0 - (improvementPercent / 100.0);
        return new SqlQueryMetric
        {
            QueryHash = query.QueryHash,
            QueryText = query.QueryText,
            ExecutionCount = query.ExecutionCount,
            AvgCpuTimeMs = query.AvgCpuTimeMs * factor,
            AvgElapsedTimeMs = query.AvgElapsedTimeMs * factor,
            AvgLogicalReads = (long)(query.AvgLogicalReads * factor),
            AvgPhysicalReads = (long)(query.AvgPhysicalReads * factor),
            TotalCpuTimeMs = query.TotalCpuTimeMs * factor,
            TotalElapsedTimeMs = query.TotalElapsedTimeMs * factor,
            TotalLogicalReads = (long)(query.TotalLogicalReads * factor),
            TotalPhysicalReads = (long)(query.TotalPhysicalReads * factor),
            LastExecutionTime = query.LastExecutionTime
        };
    }

    private string GenerateCostBreakdownText(QueryCostAnalysis analysis, CostParameters parameters)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"💰 Cost Breakdown for Query");
        sb.AppendLine();
        sb.AppendLine($"Daily Costs:");
        sb.AppendLine($"  User Productivity: {parameters.Currency}{analysis.DailyUserProductivityCost:F2}");
        sb.AppendLine($"  Infrastructure: {parameters.Currency}{analysis.DailyInfrastructureCost:F2}");
        sb.AppendLine($"  Total: {parameters.Currency}{analysis.DailyTotalCost:F2}");
        sb.AppendLine();
        sb.AppendLine($"Monthly Costs:");
        sb.AppendLine($"  User Productivity: {parameters.Currency}{analysis.MonthlyUserProductivityCost:F2}");
        sb.AppendLine($"  Infrastructure: {parameters.Currency}{analysis.MonthlyInfrastructureCost:F2}");
        sb.AppendLine($"  Total: {parameters.Currency}{analysis.MonthlyTotalCost:F2}");
        sb.AppendLine();
        sb.AppendLine($"Yearly Costs:");
        sb.AppendLine($"  User Productivity: {parameters.Currency}{analysis.YearlyUserProductivityCost:F2}");
        sb.AppendLine($"  Infrastructure: {parameters.Currency}{analysis.YearlyInfrastructureCost:F2}");
        sb.AppendLine($"  Total: {parameters.Currency}{analysis.YearlyTotalCost:F2}");
        sb.AppendLine();
        sb.AppendLine($"Impact:");
        sb.AppendLine($"  Affected Users: {analysis.AffectedUsers}");
        sb.AppendLine($"  Wait Time: {analysis.TotalWaitTimeHoursPerYear:F0} hours/year");
        sb.AppendLine($"  Executions: {analysis.YearlyExecutions:N0}/year");

        return sb.ToString();
    }

    private string GenerateROISummary(OptimizationROI roi, CostParameters parameters)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📊 ROI Analysis");
        sb.AppendLine();
        sb.AppendLine($"Current Annual Cost: {parameters.Currency}{roi.CurrentCost.YearlyTotalCost:F2}");
        sb.AppendLine($"Projected Cost (after {roi.EstimatedImprovementPercent}% improvement): {parameters.Currency}{roi.ProjectedCost.YearlyTotalCost:F2}");
        sb.AppendLine();
        sb.AppendLine($"Annual Savings: {parameters.Currency}{roi.YearlySavings:F2}");
        sb.AppendLine($"Investment Required: {parameters.Currency}{roi.InvestmentCost:F2} ({roi.InvestmentHours:F1} hours)");
        sb.AppendLine();
        sb.AppendLine($"Payback Period: {roi.PaybackDays:F1} days");
        sb.AppendLine($"Break-Even Date: {roi.BreakEvenDate:yyyy-MM-dd}");
        sb.AppendLine($"ROI: {roi.ROIPercent:F0}%");
        sb.AppendLine();
        if (roi.PaybackDays < 7)
        {
            sb.AppendLine("✅ EXCELLENT ROI - Implement immediately!");
        }
        else if (roi.PaybackDays < 30)
        {
            sb.AppendLine("✅ GOOD ROI - High priority optimization");
        }
        else if (roi.PaybackDays < 90)
        {
            sb.AppendLine("⚠️ MODERATE ROI - Consider for next sprint");
        }
        else
        {
            sb.AppendLine("⚠️ LOW ROI - Low priority, consider later");
        }

        return sb.ToString();
    }

    private string GenerateTCOSummary(TCOAnalysis tco, CostParameters parameters)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"💼 Total Cost of Ownership ({tco.ProjectionMonths} months)");
        sb.AppendLine();
        sb.AppendLine($"Current Trajectory: {parameters.Currency}{tco.CurrentProjectedCost:N2}");
        sb.AppendLine($"Optimized Trajectory: {parameters.Currency}{tco.OptimizedProjectedCost:N2}");
        sb.AppendLine();
        sb.AppendLine($"Total Savings: {parameters.Currency}{tco.TotalSavings:N2} ({tco.SavingsPercent:F1}%)");
        sb.AppendLine();
        sb.AppendLine($"Cost Breakdown:");
        sb.AppendLine($"  User Productivity: {parameters.Currency}{tco.UserProductivityCost:N2}");
        sb.AppendLine($"  Infrastructure: {parameters.Currency}{tco.InfrastructureCost:N2}");
        sb.AppendLine($"  Licensing: {parameters.Currency}{tco.LicensingCost:N2}");
        sb.AppendLine($"  Manpower (DBA): {parameters.Currency}{tco.ManpowerCost:N2}");

        return sb.ToString();
    }

    private List<ExecutiveRecommendation> GenerateRecommendations(
        ExecutiveSummary summary,
        List<QueryCostAnalysis> analyses,
        CostParameters parameters)
    {
        var recommendations = new List<ExecutiveRecommendation>();

        // Recommendation 1: Optimize top queries
        if (summary.TopCostlyQueries.Any())
        {
            var topCost = summary.TopCostlyQueries.First();
            recommendations.Add(new ExecutiveRecommendation
            {
                Title = "Optimize Top 10 Costly Queries",
                Description = $"The top 10 queries cost {parameters.Currency}{summary.TopCostlyQueries.Sum(q => q.YearlyTotalCost):N2}/year. " +
                             $"Optimizing these could save {parameters.Currency}{summary.TopCostlyQueries.Sum(q => q.YearlyTotalCost) * 0.4:N2}/year.",
                PotentialSavings = summary.TopCostlyQueries.Sum(q => q.YearlyTotalCost) * 0.4,
                Priority = "Critical",
                ImplementationEffortDays = 5
            });
        }

        // Recommendation 2: Index optimization
        recommendations.Add(new ExecutiveRecommendation
        {
            Title = "Implement Missing Indexes",
            Description = $"Missing indexes contribute to {parameters.Currency}{summary.TotalInfrastructureCostPerYear * 0.3:N2}/year in infrastructure costs. " +
                         "Strategic index implementation could reduce I/O by 50%.",
            PotentialSavings = summary.TotalInfrastructureCostPerYear * 0.3 * 0.5,
            Priority = "High",
            ImplementationEffortDays = 3
        });

        // Recommendation 3: User productivity
        recommendations.Add(new ExecutiveRecommendation
        {
            Title = "Reduce User Wait Time",
            Description = $"Users spend {summary.TotalProductivityHoursLostPerYear:N0} hours/year waiting for queries. " +
                         $"This costs {parameters.Currency}{summary.TotalProductivityHoursLostPerYear * parameters.AverageHourlySalary:N2}/year in lost productivity.",
            PotentialSavings = summary.TotalProductivityHoursLostPerYear * parameters.AverageHourlySalary * 0.3,
            Priority = "High",
            ImplementationEffortDays = 10
        });

        return recommendations.OrderByDescending(r => r.PotentialSavings).ToList();
    }
}
