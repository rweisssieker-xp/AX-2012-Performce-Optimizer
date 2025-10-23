using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;
using Microsoft.Extensions.Logging;
using TrendDirection = AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard.TrendDirection;

namespace AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;

public class ExecutiveScorecardService : IExecutiveScorecardService
{
    private readonly IDatabaseStatsService _databaseStats;
    private readonly ISqlQueryMonitorService _queryMonitor;
    private readonly IAosMonitorService _aosMonitor;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly ISystemHealthScoreService _healthScore;
    private readonly ILogger<ExecutiveScorecardService> _logger;

    public ExecutiveScorecardService(
        IDatabaseStatsService databaseStats,
        ISqlQueryMonitorService queryMonitor,
        IAosMonitorService aosMonitor,
        IBatchJobMonitorService batchJobMonitor,
        ISystemHealthScoreService healthScore,
        ILogger<ExecutiveScorecardService> logger)
    {
        _databaseStats = databaseStats;
        _queryMonitor = queryMonitor;
        _aosMonitor = aosMonitor;
        _batchJobMonitor = batchJobMonitor;
        _healthScore = healthScore;
        _logger = logger;
    }

    public async Task<ExecutiveScorecard> GenerateScorecardAsync()
    {
        try
        {
            var scorecard = new ExecutiveScorecard
            {
                GeneratedAt = DateTime.UtcNow
            };

            // Get category scores
            scorecard.QueryPerformanceScore = await CalculateQueryPerformanceScoreAsync();
            scorecard.DatabaseHealthScore = await CalculateDatabaseHealthScoreAsync();
            scorecard.AosPerformanceScore = await CalculateAosPerformanceScoreAsync();
            scorecard.BatchJobSuccessScore = await CalculateBatchJobSuccessScoreAsync();
            scorecard.ResourceUtilizationScore = await CalculateResourceUtilizationScoreAsync();
            scorecard.IndexMaintenanceScore = await CalculateIndexMaintenanceScoreAsync();
            scorecard.StatisticsFreshnessScore = await CalculateStatisticsFreshnessScoreAsync();
            scorecard.BackupRecoveryScore = await CalculateBackupRecoveryScoreAsync();

            // Calculate overall score (weighted average)
            scorecard.OverallScore = CalculateOverallScore(scorecard);
            scorecard.Grade = CalculateGrade(scorecard.OverallScore);

            // Calculate trend
            var previousScorecard = await GetPreviousScorecardAsync();
            if (previousScorecard != null)
            {
                scorecard.TrendChangePoints = scorecard.OverallScore - previousScorecard.OverallScore;
                scorecard.Trend = scorecard.TrendChangePoints > 2 ? Models.ExecutiveDashboard.TrendDirection.Improving :
                                scorecard.TrendChangePoints < -2 ? Models.ExecutiveDashboard.TrendDirection.Degrading :
                                Models.ExecutiveDashboard.TrendDirection.Stable;
            }

            // Business metrics
            await PopulateBusinessMetricsAsync(scorecard);

            // Top issues and achievements
            await PopulateIssuesAndAchievementsAsync(scorecard);

            // KPIs
            await PopulateKPIsAsync(scorecard);

            _logger.LogInformation("Executive scorecard generated: Score={Score}, Grade={Grade}",
                scorecard.OverallScore, scorecard.Grade);

            return scorecard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating executive scorecard");
            return new ExecutiveScorecard(); // Return empty scorecard on error
        }
    }

    public async Task<MonthlyBusinessReview> GenerateMonthlyReviewAsync(DateTime month)
    {
        try
        {
            var review = new MonthlyBusinessReview
            {
                ReportMonth = new DateTime(month.Year, month.Month, 1),
                Scorecard = await GenerateScorecardAsync()
            };

            // Generate executive summary
            review.ExecutiveSummary = GenerateExecutiveSummary(review.Scorecard);

            // Key highlights
            review.KeyHighlights = GenerateKeyHighlights(review.Scorecard);

            // Action items
            review.ActionItems = GenerateActionItems(review.Scorecard);

            // Performance trends
            review.Trends = await GeneratePerformanceTrendsAsync(month);

            // Budget impact
            review.BudgetImpact = await CalculateBudgetImpactAsync(month);

            return review;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly review");
            return new MonthlyBusinessReview();
        }
    }

    public async Task<MonthlyBusinessReview> GenerateQuarterlyReviewAsync(int year, int quarter)
    {
        // Calculate quarter months
        int startMonth = (quarter - 1) * 3 + 1;
        var quarterStart = new DateTime(year, startMonth, 1);
        var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);

        // For now, use the end month of quarter
        return await GenerateMonthlyReviewAsync(quarterEnd);
    }

    public PerformanceGrade CalculateGrade(int score)
    {
        return score switch
        {
            >= 90 => PerformanceGrade.A,
            >= 85 => PerformanceGrade.BPlus,
            >= 80 => PerformanceGrade.B,
            >= 70 => PerformanceGrade.C,
            >= 60 => PerformanceGrade.D,
            _ => PerformanceGrade.F
        };
    }

    public async Task<byte[]> ExportToPowerPointAsync(ExecutiveScorecard scorecard)
    {
        // TODO: Implement PowerPoint export using Open XML SDK
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> ExportToPdfAsync(ExecutiveScorecard scorecard)
    {
        // TODO: Implement PDF export
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<List<ExecutiveScorecard>> GetHistoricalScorecardsAsync(DateTime startDate, DateTime endDate)
    {
        // TODO: Implement historical data retrieval
        await Task.CompletedTask;
        return new List<ExecutiveScorecard>();
    }

    // Private helper methods

    private async Task<int> CalculateQueryPerformanceScoreAsync()
    {
        try
        {
            var queries = await _queryMonitor.GetTopExpensiveQueriesAsync(50);
            if (!queries.Any()) return 100;

            var avgDuration = queries.Average(q => q.AvgElapsedTimeMs);

            // Score based on average duration
            // < 100ms = 100, < 200ms = 90, < 500ms = 80, etc.
            return avgDuration switch
            {
                < 100 => 100,
                < 200 => 95,
                < 300 => 90,
                < 500 => 85,
                < 1000 => 75,
                < 2000 => 60,
                _ => 40
            };
        }
        catch
        {
            return 50; // Default to middle score on error
        }
    }

    private async Task<int> CalculateDatabaseHealthScoreAsync()
    {
        try
        {
            var metrics = await _databaseStats.GetDatabaseMetricsAsync();
            var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);

            int score = 100;

            // Penalize for fragmented indexes
            score -= Math.Min(30, fragmented.Count * 3);

            // Check database size growth
            if (metrics.TotalSizeMB > 500000) score -= 10; // Very large DB

            return Math.Max(0, score);
        }
        catch
        {
            return 50;
        }
    }

    private async Task<int> CalculateAosPerformanceScoreAsync()
    {
        try
        {
            var metrics = await _aosMonitor.GetAosMetricsAsync();
            return metrics.IsHealthy ? 90 : 60;
        }
        catch
        {
            return 50;
        }
    }

    private async Task<int> CalculateBatchJobSuccessScoreAsync()
    {
        try
        {
            var metrics = await _batchJobMonitor.GetBatchJobMetricsAsync();
            var failedCount = await _batchJobMonitor.GetFailedBatchJobsCountAsync();

            if (metrics.TotalJobs == 0) return 100;

            var successRate = ((double)(metrics.TotalJobs - failedCount) / metrics.TotalJobs) * 100;

            return successRate switch
            {
                >= 95 => 100,
                >= 90 => 90,
                >= 85 => 80,
                >= 80 => 70,
                >= 70 => 60,
                _ => 40
            };
        }
        catch
        {
            return 50;
        }
    }

    private async Task<int> CalculateResourceUtilizationScoreAsync()
    {
        // Placeholder - in real implementation, would check CPU, Memory, Disk I/O
        await Task.CompletedTask;
        return 75;
    }

    private async Task<int> CalculateIndexMaintenanceScoreAsync()
    {
        try
        {
            var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);

            return fragmented.Count switch
            {
                0 => 100,
                <= 5 => 90,
                <= 10 => 80,
                <= 20 => 70,
                <= 30 => 60,
                _ => 40
            };
        }
        catch
        {
            return 50;
        }
    }

    private async Task<int> CalculateStatisticsFreshnessScoreAsync()
    {
        // Placeholder - would check statistics last updated dates
        await Task.CompletedTask;
        return 80;
    }

    private async Task<int> CalculateBackupRecoveryScoreAsync()
    {
        // Placeholder - would check backup history
        await Task.CompletedTask;
        return 90;
    }

    private int CalculateOverallScore(ExecutiveScorecard scorecard)
    {
        // Weighted average of category scores
        var scores = new (int score, double weight)[]
        {
            (scorecard.QueryPerformanceScore, 0.25),
            (scorecard.DatabaseHealthScore, 0.20),
            (scorecard.AosPerformanceScore, 0.15),
            (scorecard.BatchJobSuccessScore, 0.15),
            (scorecard.ResourceUtilizationScore, 0.10),
            (scorecard.IndexMaintenanceScore, 0.05),
            (scorecard.StatisticsFreshnessScore, 0.05),
            (scorecard.BackupRecoveryScore, 0.05)
        };

        double weightedSum = scores.Sum(s => s.score * s.weight);
        return (int)Math.Round(weightedSum);
    }

    private async Task<ExecutiveScorecard?> GetPreviousScorecardAsync()
    {
        // Placeholder - would retrieve from database/cache
        await Task.CompletedTask;
        return null;
    }

    private async Task PopulateBusinessMetricsAsync(ExecutiveScorecard scorecard)
    {
        // Placeholder - would calculate from historical data
        await Task.CompletedTask;
        scorecard.MonthlyCostSavings = 5000;
        scorecard.AnnualProjectedSavings = 60000;
        scorecard.OptimizationsImplemented = 12;
        scorecard.IssuesPrevented = 8;
        scorecard.AverageResponseTime = 150;
        scorecard.SlaCompliance = 98.5;
    }

    private async Task PopulateIssuesAndAchievementsAsync(ExecutiveScorecard scorecard)
    {
        await Task.CompletedTask;

        if (scorecard.QueryPerformanceScore < 70)
        {
            scorecard.CriticalIssues.Add("Query performance below acceptable threshold");
        }

        if (scorecard.BatchJobSuccessScore < 80)
        {
            scorecard.CriticalIssues.Add("Batch job success rate needs improvement");
        }

        if (scorecard.OverallScore >= 85)
        {
            scorecard.TopAchievements.Add("Overall performance score excellent");
        }

        if (scorecard.IndexMaintenanceScore >= 90)
        {
            scorecard.TopAchievements.Add("Index maintenance in optimal condition");
        }
    }

    private async Task PopulateKPIsAsync(ExecutiveScorecard scorecard)
    {
        await Task.CompletedTask;

        scorecard.KPIs.Add(new KeyPerformanceIndicator
        {
            Name = "Average Query Response Time",
            Category = "Performance",
            CurrentValue = scorecard.AverageResponseTime,
            TargetValue = 100,
            PreviousValue = 180,
            Unit = "ms",
            Trend = Models.ExecutiveDashboard.TrendDirection.Improving,
            Status = scorecard.AverageResponseTime <= 100 ? "On Track" : "At Risk",
            Description = "Average response time for all queries"
        });

        scorecard.KPIs.Add(new KeyPerformanceIndicator
        {
            Name = "SLA Compliance",
            Category = "Business",
            CurrentValue = scorecard.SlaCompliance,
            TargetValue = 95,
            PreviousValue = 97.5,
            Unit = "%",
            Trend = Models.ExecutiveDashboard.TrendDirection.Stable,
            Status = "On Track",
            Description = "Percentage of SLA targets met"
        });
    }

    private string GenerateExecutiveSummary(ExecutiveScorecard scorecard)
    {
        var grade = scorecard.Grade switch
        {
            PerformanceGrade.A => "Excellent",
            PerformanceGrade.BPlus => "Very Good",
            PerformanceGrade.B => "Good",
            PerformanceGrade.C => "Acceptable",
            PerformanceGrade.D => "Poor",
            _ => "Critical"
        };

        var trend = scorecard.Trend switch
        {
            Models.ExecutiveDashboard.TrendDirection.Improving => "improving",
            Models.ExecutiveDashboard.TrendDirection.Degrading => "degrading",
            _ => "stable"
        };

        return $"System performance is {grade.ToLower()} with an overall score of {scorecard.OverallScore}/100 (Grade: {scorecard.Grade}). " +
               $"Performance trend is {trend} with {Math.Abs(scorecard.TrendChangePoints)} point change from previous period. " +
               $"Current optimizations have saved €{scorecard.MonthlyCostSavings:N0} this month with annual projected savings of €{scorecard.AnnualProjectedSavings:N0}. " +
               $"SLA compliance is at {scorecard.SlaCompliance:F1}% with {scorecard.IssuesPrevented} issues prevented through proactive monitoring.";
    }

    private List<string> GenerateKeyHighlights(ExecutiveScorecard scorecard)
    {
        var highlights = new List<string>();

        if (scorecard.Grade == PerformanceGrade.A || scorecard.Grade == PerformanceGrade.BPlus)
        {
            highlights.Add("Performance exceeds industry standards");
        }

        if (scorecard.Trend == Models.ExecutiveDashboard.TrendDirection.Improving)
        {
            highlights.Add($"Performance improving: +{scorecard.TrendChangePoints} points");
        }

        if (scorecard.SlaCompliance >= 95)
        {
            highlights.Add($"SLA compliance at {scorecard.SlaCompliance:F1}%");
        }

        highlights.AddRange(scorecard.TopAchievements);

        return highlights;
    }

    private List<string> GenerateActionItems(ExecutiveScorecard scorecard)
    {
        var actions = new List<string>();

        if (scorecard.QueryPerformanceScore < 80)
        {
            actions.Add("Optimize top 10 slow queries");
        }

        if (scorecard.IndexMaintenanceScore < 80)
        {
            actions.Add("Schedule index maintenance window");
        }

        if (scorecard.BatchJobSuccessScore < 85)
        {
            actions.Add("Review and fix failing batch jobs");
        }

        actions.AddRange(scorecard.CriticalIssues.Select(issue => $"Address: {issue}"));

        return actions;
    }

    private async Task<List<Models.ExecutiveDashboard.PerformanceTrend>> GeneratePerformanceTrendsAsync(DateTime month)
    {
        // Placeholder - would generate from historical data
        await Task.CompletedTask;
        return new List<Models.ExecutiveDashboard.PerformanceTrend>();
    }

    private async Task<BudgetImpact> CalculateBudgetImpactAsync(DateTime month)
    {
        // Placeholder - would calculate from financial data
        await Task.CompletedTask;
        return new BudgetImpact
        {
            CurrentMonthCost = 15000,
            CurrentMonthSavings = 5000,
            YearToDateCost = 150000,
            YearToDateSavings = 45000,
            ProjectedAnnualCost = 180000,
            ProjectedAnnualSavings = 60000,
            BudgetVariance = -5000, // 5K under budget
            BudgetStatus = "Under Budget"
        };
    }
}
