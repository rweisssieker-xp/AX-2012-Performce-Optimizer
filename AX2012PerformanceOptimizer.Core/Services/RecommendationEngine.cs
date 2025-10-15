using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class RecommendationEngine : IRecommendationEngine
{
    private readonly ISqlQueryMonitorService _sqlMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IBatchJobMonitorService _batchJobMonitor;
    private readonly ILogger<RecommendationEngine> _logger;
    private readonly List<Recommendation> _recommendations = new();

    public RecommendationEngine(
        ISqlQueryMonitorService sqlMonitor,
        IDatabaseStatsService databaseStats,
        IBatchJobMonitorService batchJobMonitor,
        ILogger<RecommendationEngine> logger)
    {
        _sqlMonitor = sqlMonitor;
        _databaseStats = databaseStats;
        _batchJobMonitor = batchJobMonitor;
        _logger = logger;
    }

    public async Task<List<Recommendation>> GenerateRecommendationsAsync()
    {
        _recommendations.Clear();

        try
        {
            // Analyze SQL queries
            await AnalyzeSqlQueriesAsync();

            // Analyze index fragmentation
            await AnalyzeIndexFragmentationAsync();

            // Analyze missing indexes
            await AnalyzeMissingIndexesAsync();

            // Analyze batch jobs
            await AnalyzeBatchJobsAsync();

            // Analyze database size
            await AnalyzeDatabaseSizeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
        }

        return _recommendations;
    }

    private async Task AnalyzeSqlQueriesAsync()
    {
        var queries = await _sqlMonitor.GetTopExpensiveQueriesAsync(10);
        
        foreach (var query in queries.Where(q => q.AvgCpuTimeMs > 1000))
        {
            _recommendations.Add(new Recommendation
            {
                Title = "High CPU Query Detected",
                Description = $"Query with average CPU time of {query.AvgCpuTimeMs:F2}ms needs optimization.",
                Category = RecommendationCategory.SqlQueryOptimization,
                Priority = query.AvgCpuTimeMs > 5000 ? RecommendationPriority.Critical : RecommendationPriority.High,
                ImpactAnalysis = $"This query has been executed {query.ExecutionCount} times with total CPU time of {query.TotalCpuTimeMs:F2}ms.",
                ActionScript = $"-- Review and optimize this query:\n{query.QueryText.Substring(0, Math.Min(500, query.QueryText.Length))}...",
                RelatedObjects = new List<string> { query.QueryHash }
            });
        }
    }

    private async Task AnalyzeIndexFragmentationAsync()
    {
        var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);
        
        foreach (var index in fragmented.Take(10))
        {
            var action = index.FragmentationPercent > 50 ? "REBUILD" : "REORGANIZE";
            
            _recommendations.Add(new Recommendation
            {
                Title = $"Index Fragmentation: {index.TableName}.{index.IndexName}",
                Description = $"Index is {index.FragmentationPercent:F2}% fragmented and should be {action.ToLower()}d.",
                Category = RecommendationCategory.IndexManagement,
                Priority = index.FragmentationPercent > 70 ? RecommendationPriority.High : RecommendationPriority.Medium,
                ImpactAnalysis = $"Fragmentation affects query performance. Pages: {index.PageCount}",
                ActionScript = $"ALTER INDEX [{index.IndexName}] ON [{index.SchemaName}].[{index.TableName}] {action};",
                RelatedObjects = new List<string> { $"{index.SchemaName}.{index.TableName}.{index.IndexName}" }
            });
        }
    }

    private async Task AnalyzeMissingIndexesAsync()
    {
        var missing = await _databaseStats.GetMissingIndexesAsync();
        
        foreach (var index in missing.Take(10).Where(i => i.ImpactScore > 10000))
        {
            var indexColumns = new List<string>();
            if (!string.IsNullOrEmpty(index.EqualityColumns))
                indexColumns.Add(index.EqualityColumns);
            if (!string.IsNullOrEmpty(index.InequalityColumns))
                indexColumns.Add(index.InequalityColumns);

            var script = $"CREATE NONCLUSTERED INDEX [IX_{index.TableName}_Missing_{Guid.NewGuid().ToString("N").Substring(0, 8)}]\n" +
                        $"ON [{index.TableName}] ({string.Join(", ", indexColumns)})";
            
            if (!string.IsNullOrEmpty(index.IncludedColumns))
                script += $"\nINCLUDE ({index.IncludedColumns})";
            
            script += ";";

            _recommendations.Add(new Recommendation
            {
                Title = $"Missing Index on {index.TableName}",
                Description = $"Creating this index could significantly improve query performance.",
                Category = RecommendationCategory.IndexManagement,
                Priority = index.ImpactScore > 100000 ? RecommendationPriority.Critical : RecommendationPriority.High,
                ImpactAnalysis = $"Impact Score: {index.ImpactScore:F0}, Seeks: {index.UserSeeks}, Scans: {index.UserScans}",
                ActionScript = script,
                RelatedObjects = new List<string> { index.TableName }
            });
        }
    }

    private async Task AnalyzeBatchJobsAsync()
    {
        var failedJobs = await _batchJobMonitor.GetFailedBatchJobsAsync();
        
        if (failedJobs.Count > 5)
        {
            _recommendations.Add(new Recommendation
            {
                Title = "Multiple Failed Batch Jobs",
                Description = $"{failedJobs.Count} batch jobs have failed recently.",
                Category = RecommendationCategory.BatchJobScheduling,
                Priority = RecommendationPriority.High,
                ImpactAnalysis = "Failed batch jobs can lead to data inconsistencies and operational issues.",
                ActionScript = "-- Review failed batch jobs in the Batch Jobs view and investigate error logs.",
                RelatedObjects = failedJobs.Select(j => j.JobId).ToList()
            });
        }
    }

    private async Task AnalyzeDatabaseSizeAsync()
    {
        var dbMetric = await _databaseStats.GetDatabaseMetricsAsync();
        
        if (dbMetric.TotalSizeMB > 100000) // > 100 GB
        {
            _recommendations.Add(new Recommendation
            {
                Title = "Large Database Size",
                Description = $"Database size is {dbMetric.TotalSizeMB:N0} MB. Consider archiving old data.",
                Category = RecommendationCategory.StorageOptimization,
                Priority = RecommendationPriority.Medium,
                ImpactAnalysis = "Large database sizes can impact backup times and overall performance.",
                ActionScript = "-- Review table sizes and consider implementing data archival strategy.",
                RelatedObjects = new List<string> { dbMetric.DatabaseName }
            });
        }
    }

    public Task<List<Recommendation>> GetAllRecommendationsAsync()
    {
        return Task.FromResult(_recommendations);
    }

    public Task<List<Recommendation>> GetRecommendationsByCategoryAsync(RecommendationCategory category)
    {
        return Task.FromResult(_recommendations.Where(r => r.Category == category).ToList());
    }

    public Task<List<Recommendation>> GetRecommendationsByPriorityAsync(RecommendationPriority priority)
    {
        return Task.FromResult(_recommendations.Where(r => r.Priority == priority).ToList());
    }

    public Task MarkAsImplementedAsync(string recommendationId)
    {
        var recommendation = _recommendations.FirstOrDefault(r => r.Id == recommendationId);
        if (recommendation != null)
        {
            recommendation.IsImplemented = true;
            recommendation.ImplementedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public async Task RefreshRecommendationsAsync()
    {
        await GenerateRecommendationsAsync();
    }
}

