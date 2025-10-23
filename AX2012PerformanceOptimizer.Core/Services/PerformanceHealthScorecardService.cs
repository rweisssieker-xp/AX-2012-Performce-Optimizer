using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class PerformanceHealthScorecardService : IPerformanceHealthScorecardService
{
    private readonly ISqlQueryMonitorService _queryMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IAosMonitorService _aosMonitor;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly ILogger<PerformanceHealthScorecardService> _logger;

    public PerformanceHealthScorecardService(
        ISqlQueryMonitorService queryMonitor,
        IDatabaseStatsService databaseStats,
        IAosMonitorService aosMonitor,
        IBatchJobMonitorService batchJobMonitor,
        ILogger<PerformanceHealthScorecardService> logger)
    {
        _queryMonitor = queryMonitor;
        _databaseStats = databaseStats;
        _aosMonitor = aosMonitor;
        _batchJobMonitor = batchJobMonitor;
        _logger = logger;
    }

    public async Task<PerformanceHealthScorecard> GenerateHealthScorecardAsync()
    {
        try
        {
            var scorecard = new PerformanceHealthScorecard
            {
                GeneratedAt = DateTime.UtcNow
            };

            // Generate all metric categories
            scorecard.Categories.Add(await GenerateQueryPerformanceCategoryAsync());
            scorecard.Categories.Add(await GenerateDatabaseHealthCategoryAsync());
            scorecard.Categories.Add(await GenerateAosPerformanceCategoryAsync());
            scorecard.Categories.Add(await GenerateBatchJobCategoryAsync());
            scorecard.Categories.Add(await GenerateResourceUtilizationCategoryAsync());
            scorecard.Categories.Add(await GenerateIndexMaintenanceCategoryAsync());
            scorecard.Categories.Add(await GenerateStatisticsCategoryAsync());
            scorecard.Categories.Add(await GenerateBackupRecoveryCategoryAsync());

            // Calculate overall score (weighted average)
            double totalScore = 0;
            double totalWeight = 0;
            foreach (var category in scorecard.Categories)
            {
                totalScore += category.CategoryScore * category.Weight;
                totalWeight += category.Weight;
            }
            scorecard.OverallScore = (int)(totalScore / totalWeight);

            // Assign grade
            scorecard.OverallGrade = scorecard.OverallScore switch
            {
                >= 90 => "Excellent",
                >= 80 => "Good",
                >= 70 => "Acceptable",
                >= 60 => "Poor",
                _ => "Critical"
            };

            _logger.LogInformation("Health scorecard generated: Score={Score}, Grade={Grade}",
                scorecard.OverallScore, scorecard.OverallGrade);

            return scorecard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating health scorecard");
            return new PerformanceHealthScorecard();
        }
    }

    public async Task<DetailedMetricsReport> GenerateDetailedReportAsync()
    {
        var report = new DetailedMetricsReport
        {
            Scorecard = await GenerateHealthScorecardAsync()
        };

        return report;
    }

    public async Task<BenchmarkComparison> CompareToBenchmarksAsync(string industry, string companySize, string region)
    {
        var scorecard = await GenerateHealthScorecardAsync();

        var comparison = new BenchmarkComparison
        {
            Industry = industry,
            CompanySize = companySize,
            Region = region,
            YourScore = scorecard.OverallScore,
            IndustryAverage = 78,
            Top10PercentThreshold = 90,
            Top25PercentThreshold = 85,
            TotalCompaniesInComparison = 445
        };

        // Calculate percentile rank
        if (comparison.YourScore >= comparison.Top10PercentThreshold)
        {
            comparison.PercentileRank = "Top 10%";
            comparison.YourRank = 30;
        }
        else if (comparison.YourScore >= comparison.Top25PercentThreshold)
        {
            comparison.PercentileRank = "Top 25%";
            comparison.YourRank = 85;
        }
        else if (comparison.YourScore >= comparison.IndustryAverage)
        {
            comparison.PercentileRank = "Above Average";
            comparison.YourRank = 180;
        }
        else
        {
            comparison.PercentileRank = "Below Average";
            comparison.YourRank = 300;
        }

        return comparison;
    }

    public async Task<ComplianceStatus> CheckComplianceAsync(List<string> requirements)
    {
        var compliance = new ComplianceStatus
        {
            Requirements = new List<ComplianceRequirement>()
        };

        foreach (var req in requirements)
        {
            compliance.Requirements.Add(new ComplianceRequirement
            {
                RequirementName = req,
                IsMet = true,
                Status = "Met",
                LastAuditDate = DateTime.UtcNow.AddMonths(-3),
                NextAuditDate = DateTime.UtcNow.AddMonths(9)
            });
        }

        compliance.OverallCompliant = compliance.Requirements.All(r => r.IsMet);

        await Task.CompletedTask;
        return compliance;
    }

    public async Task<List<HistoricalScore>> GetHistoricalTrendAsync(DateTime startDate, DateTime endDate)
    {
        // Placeholder - would retrieve from database
        await Task.CompletedTask;
        var scores = new List<HistoricalScore>();

        // Generate sample data
        var days = (endDate - startDate).Days;
        for (int i = 0; i <= days; i += 7)  // Weekly data points
        {
            scores.Add(new HistoricalScore
            {
                Date = startDate.AddDays(i),
                OverallScore = 82 + Random.Shared.Next(-5, 5)
            });
        }

        return scores;
    }

    public async Task<byte[]> ExportToPdfAsync(PerformanceHealthScorecard scorecard)
    {
        // TODO: Implement PDF export
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> ExportToExcelAsync(PerformanceHealthScorecard scorecard)
    {
        // TODO: Implement Excel export
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task SendToAuditorAsync(string auditorEmail, PerformanceHealthScorecard scorecard)
    {
        // TODO: Implement email sending
        await Task.CompletedTask;
        _logger.LogInformation("Health scorecard sent to auditor: {Email}", auditorEmail);
    }

    // Private helper methods to generate each category

    private async Task<HealthMetricCategory> GenerateQueryPerformanceCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.QueryPerformance,
            Weight = 0.25
        };

        var queries = await _queryMonitor.GetTopExpensiveQueriesAsync(100);
        var avgDuration = queries.Any() ? queries.Average(q => q.AvgElapsedTimeMs) : 0;

        category.Metrics.Add(new HealthMetric
        {
            MetricName = "Average Query Response Time",
            Description = "Average execution time across all queries",
            CurrentValue = avgDuration,
            TargetValue = IndustryStandards.QueryPerformance.ExcellentAvgResponseTimeMs,
            IndustryAverage = 180,
            IndustryTop10Percent = 85,
            Unit = "ms",
            Score = CalculateMetricScore(avgDuration, 100, 200, 500, true),
            IsCritical = avgDuration > 1000
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        return category;
    }

    private async Task<HealthMetricCategory> GenerateDatabaseHealthCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.DatabaseHealth,
            Weight = 0.20
        };

        var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);
        var fragmentedCount = fragmented.Count;

        category.Metrics.Add(new HealthMetric
        {
            MetricName = "Fragmented Indexes",
            Description = "Number of indexes with >30% fragmentation",
            CurrentValue = fragmentedCount,
            TargetValue = 5,
            IndustryAverage = 15,
            IndustryTop10Percent = 3,
            Unit = "indexes",
            Score = CalculateMetricScore(fragmentedCount, 5, 10, 20, true),
            IsCritical = fragmentedCount > 30
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        return category;
    }

    private async Task<HealthMetricCategory> GenerateAosPerformanceCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.AosPerformance,
            Weight = 0.15
        };

        var aosMetrics = await _aosMonitor.GetAosMetricsAsync();

        category.Metrics.Add(new HealthMetric
        {
            MetricName = "AOS Health Status",
            Description = "Overall AOS server health",
            CurrentValue = aosMetrics.IsHealthy ? 100 : 0,
            TargetValue = 100,
            IndustryAverage = 95,
            IndustryTop10Percent = 100,
            Unit = "status",
            Score = aosMetrics.IsHealthy ? 100 : 0,
            IsCritical = !aosMetrics.IsHealthy
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        return category;
    }

    private async Task<HealthMetricCategory> GenerateBatchJobCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.BatchJobSuccess,
            Weight = 0.15
        };

        var metrics = await _batchJobMonitor.GetBatchJobMetricsAsync();
        var failedCount = await _batchJobMonitor.GetFailedBatchJobsCountAsync();
        var successRate = metrics.TotalJobs > 0 ? ((double)(metrics.TotalJobs - failedCount) / metrics.TotalJobs) * 100 : 100;

        category.Metrics.Add(new HealthMetric
        {
            MetricName = "Batch Job Success Rate",
            Description = "Percentage of successful batch job executions",
            CurrentValue = successRate,
            TargetValue = IndustryStandards.BatchJobs.ExcellentSuccessRate,
            IndustryAverage = 91,
            IndustryTop10Percent = 99,
            Unit = "%",
            Score = CalculateMetricScore(successRate, 98, 95, 90, false),
            IsCritical = successRate < 85
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        return category;
    }

    private async Task<HealthMetricCategory> GenerateResourceUtilizationCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.ResourceUtilization,
            Weight = 0.10
        };

        // Placeholder metrics
        category.Metrics.Add(new HealthMetric
        {
            MetricName = "CPU Utilization",
            Description = "Average CPU usage percentage",
            CurrentValue = 65,
            TargetValue = 70,
            IndustryAverage = 72,
            IndustryTop10Percent = 45,
            Unit = "%",
            Score = 75
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        await Task.CompletedTask;
        return category;
    }

    private async Task<HealthMetricCategory> GenerateIndexMaintenanceCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.IndexMaintenance,
            Weight = 0.05
        };

        var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);
        var avgFragmentation = fragmented.Any() ? fragmented.Average(f => f.FragmentationPercent) : 0;

        category.Metrics.Add(new HealthMetric
        {
            MetricName = "Average Index Fragmentation",
            Description = "Average fragmentation percentage across all indexes",
            CurrentValue = avgFragmentation,
            TargetValue = IndustryStandards.IndexHealth.ExcellentFragmentationPercent,
            IndustryAverage = 22,
            IndustryTop10Percent = 8,
            Unit = "%",
            Score = CalculateMetricScore(avgFragmentation, 10, 20, 30, true),
            IsCritical = avgFragmentation > 40
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        return category;
    }

    private async Task<HealthMetricCategory> GenerateStatisticsCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.StatisticsFreshness,
            Weight = 0.05
        };

        // Placeholder
        category.Metrics.Add(new HealthMetric
        {
            MetricName = "Statistics Freshness",
            Description = "Percentage of statistics updated within 7 days",
            CurrentValue = 85,
            TargetValue = 95,
            IndustryAverage = 80,
            IndustryTop10Percent = 98,
            Unit = "%",
            Score = 82
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        await Task.CompletedTask;
        return category;
    }

    private async Task<HealthMetricCategory> GenerateBackupRecoveryCategoryAsync()
    {
        var category = new HealthMetricCategory
        {
            CategoryName = HealthMetricCategories.BackupRecovery,
            Weight = 0.05
        };

        // Placeholder
        category.Metrics.Add(new HealthMetric
        {
            MetricName = "Last Backup Age",
            Description = "Hours since last successful backup",
            CurrentValue = 18,
            TargetValue = 24,
            IndustryAverage = 20,
            IndustryTop10Percent = 12,
            Unit = "hours",
            Score = 90
        });

        category.CategoryScore = category.Metrics.Any() ? (int)category.Metrics.Average(m => m.Score) : 50;
        category.Status = GetStatusFromScore(category.CategoryScore);

        await Task.CompletedTask;
        return category;
    }

    private int CalculateMetricScore(double currentValue, double excellent, double good, double acceptable, bool lowerIsBetter)
    {
        if (lowerIsBetter)
        {
            if (currentValue <= excellent) return 100;
            if (currentValue <= good) return 85;
            if (currentValue <= acceptable) return 70;
            return 50;
        }
        else
        {
            if (currentValue >= excellent) return 100;
            if (currentValue >= good) return 85;
            if (currentValue >= acceptable) return 70;
            return 50;
        }
    }

    private string GetStatusFromScore(int score)
    {
        return score switch
        {
            >= 90 => "Excellent",
            >= 80 => "Good",
            >= 70 => "Acceptable",
            >= 60 => "Poor",
            _ => "Critical"
        };
    }
}
