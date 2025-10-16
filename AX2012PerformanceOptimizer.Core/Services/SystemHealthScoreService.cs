using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Implementation of System Health Score calculation service
/// </summary>
public class SystemHealthScoreService : ISystemHealthScoreService
{
    private readonly ILogger<SystemHealthScoreService> _logger;
    private readonly ISqlQueryMonitorService _queryMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly List<HealthScoreHistory> _history = new();
    private SystemHealthScore? _lastScore;

    public SystemHealthScoreService(
        ILogger<SystemHealthScoreService> logger,
        ISqlQueryMonitorService queryMonitor,
        IDatabaseStatsService databaseStats,
        IBatchJobMonitorService batchJobMonitor)
    {
        _logger = logger;
        _queryMonitor = queryMonitor;
        _databaseStats = databaseStats;
        _batchJobMonitor = batchJobMonitor;
    }

    public async Task<SystemHealthScore> CalculateHealthScoreAsync()
    {
        _logger.LogInformation("Calculating system health score...");

        try
        {
            // Calculate category scores in parallel
            var sqlTask = CalculateSqlPerformanceScoreAsync();
            var indexTask = CalculateIndexHealthScoreAsync();
            var batchTask = CalculateBatchJobsScoreAsync();
            var dbSizeTask = CalculateDatabaseSizeScoreAsync();

            await Task.WhenAll(sqlTask, indexTask, batchTask, dbSizeTask);

            var sqlScore = await sqlTask;
            var indexScore = await indexTask;
            var batchScore = await batchTask;
            var dbSizeScore = await dbSizeTask;

            // Calculate weighted overall score
            var overallScore = (int)Math.Round(
                (sqlScore.Score * sqlScore.Weight / 100.0) +
                (indexScore.Score * indexScore.Weight / 100.0) +
                (batchScore.Score * batchScore.Weight / 100.0) +
                (dbSizeScore.Score * dbSizeScore.Weight / 100.0)
            );

            var healthScore = new SystemHealthScore
            {
                OverallScore = overallScore,
                PreviousScore = _lastScore?.OverallScore ?? overallScore,
                Timestamp = DateTime.Now,
                SqlPerformance = sqlScore,
                IndexHealth = indexScore,
                BatchJobs = batchScore,
                DatabaseSize = dbSizeScore
            };

            // Get recommended actions
            healthScore.RecommendedActions = await GetPrioritizedActionsAsync();
            healthScore.TopImpactAction = healthScore.RecommendedActions.FirstOrDefault();

            _lastScore = healthScore;

            _logger.LogInformation("Health score calculated: {Score}/100 ({Status})",
                overallScore, healthScore.Status);

            return healthScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating health score");

            // Return a default score on error
            return new SystemHealthScore
            {
                OverallScore = 50,
                PreviousScore = 50,
                SqlPerformance = new HealthCategory { Name = "SQL Performance", Score = 50 },
                IndexHealth = new HealthCategory { Name = "Index Health", Score = 50 },
                BatchJobs = new HealthCategory { Name = "Batch Jobs", Score = 50 },
                DatabaseSize = new HealthCategory { Name = "Database Size", Score = 50 }
            };
        }
    }

    public async Task<HealthCategory> CalculateSqlPerformanceScoreAsync()
    {
        var category = new HealthCategory
        {
            Name = "SQL Performance",
            Weight = 30 // Most important category
        };

        try
        {
            var queries = await _queryMonitor.GetTopExpensiveQueriesAsync(50);
            if (!queries.Any())
            {
                category.Score = 50; // Neutral if no data
                category.Issues.Add("Keine Query-Daten verfügbar");
                return category;
            }

            int score = 100; // Start at perfect

            // Deduct points for slow queries
            var slowQueries = queries.Where(q => q.AvgElapsedTimeMs > 1000).ToList();
            if (slowQueries.Any())
            {
                score -= Math.Min(30, slowQueries.Count * 3);
                category.Issues.Add($"{slowQueries.Count} Queries > 1 Sekunde");
            }

            // Deduct points for high CPU queries
            var highCpuQueries = queries.Where(q => q.AvgCpuTimeMs > 500).ToList();
            if (highCpuQueries.Any())
            {
                score -= Math.Min(20, highCpuQueries.Count * 2);
                category.Issues.Add($"{highCpuQueries.Count} Queries mit hoher CPU-Last");
            }

            // Deduct points for high I/O queries
            var highIoQueries = queries.Where(q => q.AvgPhysicalReads > 1000).ToList();
            if (highIoQueries.Any())
            {
                score -= Math.Min(15, highIoQueries.Count * 2);
                category.Issues.Add($"{highIoQueries.Count} Queries mit hohen I/O-Reads");
            }

            // Bonus for good performance
            var fastQueries = queries.Where(q => q.AvgElapsedTimeMs < 100).Count();
            if (fastQueries > 40)
            {
                category.Improvements.Add($"{fastQueries} schnelle Queries (<100ms)");
            }

            category.Score = Math.Max(0, Math.Min(100, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SQL performance score");
            category.Score = 50;
            category.Issues.Add("Fehler bei Score-Berechnung");
        }

        return category;
    }

    public async Task<HealthCategory> CalculateIndexHealthScoreAsync()
    {
        var category = new HealthCategory
        {
            Name = "Index Health",
            Weight = 25
        };

        try
        {
            var fragmentedIndexes = await _databaseStats.GetFragmentedIndexesAsync();
            var missingIndexes = await _databaseStats.GetMissingIndexesAsync();

            int score = 100;

            // Deduct for fragmented indexes
            var criticalFragmentation = fragmentedIndexes.Where(i => i.FragmentationPercent > 50).ToList();
            var mediumFragmentation = fragmentedIndexes.Where(i => i.FragmentationPercent >= 30 && i.FragmentationPercent <= 50).ToList();

            if (criticalFragmentation.Any())
            {
                score -= Math.Min(40, criticalFragmentation.Count * 8);
                category.Issues.Add($"{criticalFragmentation.Count} Indexes >50% fragmentiert");
            }

            if (mediumFragmentation.Any())
            {
                score -= Math.Min(20, mediumFragmentation.Count * 4);
                category.Issues.Add($"{mediumFragmentation.Count} Indexes 30-50% fragmentiert");
            }

            // Deduct for missing indexes
            var highImpactMissing = missingIndexes.Where(i => i.ImpactScore > 100000).ToList();
            if (highImpactMissing.Any())
            {
                score -= Math.Min(25, highImpactMissing.Count * 5);
                category.Issues.Add($"{highImpactMissing.Count} fehlende High-Impact Indexes");
            }

            // Bonus for good index health
            var healthyIndexes = fragmentedIndexes.Where(i => i.FragmentationPercent < 10).Count();
            if (healthyIndexes > 20)
            {
                category.Improvements.Add($"{healthyIndexes} gesunde Indexes (<10% Fragmentation)");
            }

            category.Score = Math.Max(0, Math.Min(100, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating index health score");
            category.Score = 50;
            category.Issues.Add("Fehler bei Score-Berechnung");
        }

        return category;
    }

    public async Task<HealthCategory> CalculateBatchJobsScoreAsync()
    {
        var category = new HealthCategory
        {
            Name = "Batch Jobs",
            Weight = 25
        };

        try
        {
            var runningJobs = await _batchJobMonitor.GetRunningBatchJobsAsync();
            var failedJobs = await _batchJobMonitor.GetFailedBatchJobsAsync();

            int score = 100;

            // Deduct for failed jobs
            if (failedJobs.Any())
            {
                score -= Math.Min(40, failedJobs.Count * 10);
                category.Issues.Add($"{failedJobs.Count} fehlgeschlagene Batch Jobs");
            }

            // Deduct for stuck jobs (running > 2 hours)
            var stuckJobs = runningJobs.Where(j =>
                j.StartDateTime.HasValue &&
                (DateTime.Now - j.StartDateTime.Value).TotalHours > 2
            ).ToList();

            if (stuckJobs.Any())
            {
                score -= Math.Min(30, stuckJobs.Count * 15);
                category.Issues.Add($"{stuckJobs.Count} Batch Jobs laufen länger als 2 Stunden");
            }

            // Deduct for too many concurrent jobs
            if (runningJobs.Count() > 20)
            {
                score -= 15;
                category.Issues.Add($"{runningJobs.Count()} gleichzeitige Batch Jobs (>20)");
            }

            // Bonus for good batch health
            if (failedJobs.Count() == 0 && runningJobs.Count() < 10)
            {
                category.Improvements.Add("Keine fehlgeschlagenen Jobs, normale Last");
            }

            category.Score = Math.Max(0, Math.Min(100, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating batch jobs score");
            category.Score = 50;
            category.Issues.Add("Fehler bei Score-Berechnung");
        }

        return category;
    }

    public async Task<HealthCategory> CalculateDatabaseSizeScoreAsync()
    {
        var category = new HealthCategory
        {
            Name = "Database Size",
            Weight = 20
        };

        try
        {
            var dbMetrics = await _databaseStats.GetDatabaseMetricsAsync();
            var topTables = await _databaseStats.GetTopTablesBySize(10);

            int score = 100;

            // Deduct for large database (relative to available space)
            if (dbMetrics.TotalSizeMB > 500000) // > 500 GB
            {
                score -= 20;
                category.Issues.Add($"Sehr große Datenbank ({dbMetrics.TotalSizeMB / 1024:F1} GB)");
            }
            else if (dbMetrics.TotalSizeMB > 200000) // > 200 GB
            {
                score -= 10;
                category.Issues.Add($"Große Datenbank ({dbMetrics.TotalSizeMB / 1024:F1} GB)");
            }

            // Deduct for log file size issues
            var logPercentage = (dbMetrics.LogSizeMB / (double)dbMetrics.TotalSizeMB) * 100;
            if (logPercentage > 30)
            {
                score -= 20;
                category.Issues.Add($"Log-File zu groß ({logPercentage:F1}% der Gesamt-DB)");
            }
            else if (logPercentage > 20)
            {
                score -= 10;
                category.Issues.Add($"Log-File groß ({logPercentage:F1}% der Gesamt-DB)");
            }

            // Deduct for very large tables (convert KB to MB)
            var hugeTables = topTables.Where(t => (t.TotalSpaceKB / 1024) > 50000).ToList(); // > 50 GB
            if (hugeTables.Any())
            {
                score -= Math.Min(15, hugeTables.Count * 5);
                category.Issues.Add($"{hugeTables.Count} Tabellen >50 GB");
            }

            // Bonus for healthy size distribution
            if (logPercentage < 10 && dbMetrics.TotalSizeMB < 100000)
            {
                category.Improvements.Add("Gesunde Datenbankgröße und Log-Ratio");
            }

            category.Score = Math.Max(0, Math.Min(100, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating database size score");
            category.Score = 50;
            category.Issues.Add("Fehler bei Score-Berechnung");
        }

        return category;
    }

    public async Task<List<HealthAction>> GetPrioritizedActionsAsync()
    {
        var actions = new List<HealthAction>();

        try
        {
            // Get data for generating actions
            var queries = await _queryMonitor.GetTopExpensiveQueriesAsync(20);
            var fragmentedIndexes = await _databaseStats.GetFragmentedIndexesAsync();
            var missingIndexes = await _databaseStats.GetMissingIndexesAsync();
            var failedJobs = await _batchJobMonitor.GetFailedBatchJobsAsync();

            // Action: Rebuild critical fragmented indexes
            var criticalFragmentation = fragmentedIndexes
                .Where(i => i.FragmentationPercent > 50)
                .OrderByDescending(i => i.FragmentationPercent)
                .Take(3)
                .ToList();

            foreach (var index in criticalFragmentation)
            {
                actions.Add(new HealthAction
                {
                    Title = $"Index Rebuild: {index.IndexName}",
                    Description = $"Index ist zu {index.FragmentationPercent:F1}% fragmentiert und verlangsamt Queries signifikant.",
                    Category = "IndexHealth",
                    ActionType = "Index",
                    EstimatedScoreImpact = (int)(index.FragmentationPercent / 10), // Higher fragmentation = more impact
                    Script = $"ALTER INDEX [{index.IndexName}] ON [{index.TableName}] REBUILD WITH (ONLINE = ON);",
                    EstimatedTimeMinutes = 15,
                    IsAutomatable = true
                });
            }

            // Action: Create high-impact missing indexes
            var topMissingIndexes = missingIndexes
                .OrderByDescending(i => i.ImpactScore)
                .Take(3)
                .ToList();

            foreach (var index in topMissingIndexes)
            {
                var impactPoints = Math.Min(10, (int)(index.ImpactScore / 50000));
                actions.Add(new HealthAction
                {
                    Title = $"Create Missing Index: {index.TableName}",
                    Description = $"Fehlender Index würde {index.ImpactScore:N0} Seeks/Scans/Lookups verbessern.",
                    Category = "IndexHealth",
                    ActionType = "Index",
                    EstimatedScoreImpact = impactPoints,
                    Script = $"-- Impact Score: {index.ImpactScore:N0}\r\n-- Equality Columns: {index.EqualityColumns}\r\n-- Included Columns: {index.IncludedColumns}",
                    EstimatedTimeMinutes = 5,
                    IsAutomatable = false
                });
            }

            // Action: Optimize slowest queries
            var slowestQueries = queries
                .OrderByDescending(q => q.AvgElapsedTimeMs)
                .Take(3)
                .ToList();

            foreach (var query in slowestQueries)
            {
                var impactPoints = Math.Min(12, (int)(query.AvgElapsedTimeMs / 500));
                actions.Add(new HealthAction
                {
                    Title = $"Optimize Query: {query.QueryHash.Substring(0, Math.Min(8, query.QueryHash.Length))}",
                    Description = $"Query hat Ø {query.AvgElapsedTimeMs:F0}ms Laufzeit mit {query.ExecutionCount} Executions.",
                    Category = "SqlPerformance",
                    ActionType = "Query",
                    EstimatedScoreImpact = impactPoints,
                    Script = null, // Would need AI analysis
                    EstimatedTimeMinutes = 30,
                    IsAutomatable = false
                });
            }

            // Action: Investigate failed batch jobs
            if (failedJobs.Any())
            {
                var impactPoints = Math.Min(15, failedJobs.Count() * 3);
                actions.Add(new HealthAction
                {
                    Title = $"Fix {failedJobs.Count()} Failed Batch Jobs",
                    Description = "Mehrere Batch Jobs sind fehlgeschlagen und müssen untersucht werden.",
                    Category = "BatchJobs",
                    ActionType = "Batch",
                    EstimatedScoreImpact = impactPoints,
                    Script = null,
                    EstimatedTimeMinutes = 60,
                    IsAutomatable = false
                });
            }

            // Sort by estimated impact (descending)
            return actions
                .OrderByDescending(a => a.EstimatedScoreImpact)
                .ThenBy(a => a.EstimatedTimeMinutes)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prioritized actions");
            return new List<HealthAction>();
        }
    }

    public async Task<List<HealthScoreHistory>> GetHealthScoreHistoryAsync(int days = 30)
    {
        await Task.Delay(1); // Async placeholder

        // Return recent history
        return _history
            .Where(h => h.Timestamp >= DateTime.Now.AddDays(-days))
            .OrderBy(h => h.Timestamp)
            .ToList();
    }

    public async Task SaveHealthScoreToHistoryAsync(SystemHealthScore score)
    {
        await Task.Delay(1); // Async placeholder

        _history.Add(new HealthScoreHistory
        {
            Timestamp = score.Timestamp,
            OverallScore = score.OverallScore,
            SqlPerformanceScore = score.SqlPerformance.Score,
            IndexHealthScore = score.IndexHealth.Score,
            BatchJobsScore = score.BatchJobs.Score,
            DatabaseSizeScore = score.DatabaseSize.Score
        });

        // Keep only last 90 days
        var cutoff = DateTime.Now.AddDays(-90);
        _history.RemoveAll(h => h.Timestamp < cutoff);

        _logger.LogInformation("Health score saved to history");
    }
}
