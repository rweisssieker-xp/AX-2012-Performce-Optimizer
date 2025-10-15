using AX2012PerformanceOptimizer.Core.Models;
using System.Text.RegularExpressions;

namespace AX2012PerformanceOptimizer.Core.Services;

public class QueryAnalyzerService : IQueryAnalyzerService
{
    private readonly IDatabaseStatsService _databaseStats;

    public QueryAnalyzerService(IDatabaseStatsService databaseStats)
    {
        _databaseStats = databaseStats;
    }

    public async Task<List<QueryOptimizationSuggestion>> AnalyzeQueryAsync(SqlQueryMetric query)
    {
        var suggestions = new List<QueryOptimizationSuggestion>();

        // 1. High CPU Time Analysis
        if (query.AvgCpuTimeMs > 100)
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = query.AvgCpuTimeMs > 500 ? SuggestionSeverity.Critical : SuggestionSeverity.Warning,
                Title = "High CPU Time Detected",
                Description = $"Average CPU time is {query.AvgCpuTimeMs:F2}ms. Consider optimizing query logic or adding indexes.",
                RecommendedAction = "Review WHERE clauses, JOIN conditions, and consider adding covering indexes.",
                EstimatedImpact = Math.Min(90, query.AvgCpuTimeMs / 10)
            });
        }

        // 2. High Logical Reads Analysis
        if (query.AvgLogicalReads > 10000)
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.Index,
                Severity = query.AvgLogicalReads > 100000 ? SuggestionSeverity.Critical : SuggestionSeverity.Warning,
                Title = "Excessive Logical Reads",
                Description = $"Query performs {query.AvgLogicalReads:N0} logical reads on average. This indicates missing indexes or table scans.",
                RecommendedAction = "Check for missing indexes on frequently filtered columns. Consider using Database Engine Tuning Advisor.",
                SqlCode = "-- Check for missing indexes\nSELECT * FROM sys.dm_db_missing_index_details WHERE database_id = DB_ID()",
                EstimatedImpact = 85
            });
        }

        // 3. High Physical Reads Analysis
        if (query.AvgPhysicalReads > 1000)
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.Caching,
                Severity = SuggestionSeverity.Warning,
                Title = "High Physical Reads - Data Not Cached",
                Description = $"Query performs {query.AvgPhysicalReads:N0} physical reads, indicating data is not in buffer cache.",
                RecommendedAction = "Consider increasing server memory, optimizing query to access less data, or scheduling query during off-peak hours.",
                EstimatedImpact = 60
            });
        }

        // 4. Frequent Execution Analysis
        if (query.ExecutionCount > 1000 && query.AvgCpuTimeMs > 50)
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.Caching,
                Severity = SuggestionSeverity.Critical,
                Title = "High-Impact Optimization Target",
                Description = $"Query executed {query.ExecutionCount:N0} times with {query.AvgCpuTimeMs:F2}ms average. Total impact: {query.TotalCpuTimeMs:N0}ms",
                RecommendedAction = "Consider result caching in application layer, query optimization, or using indexed views.",
                EstimatedImpact = 95
            });
        }

        // 5. Query Pattern Analysis
        AnalyzeQueryPatterns(query.QueryText, suggestions);

        // 6. Missing Index Recommendations
        await AnalyzeMissingIndexesAsync(query, suggestions);

        return suggestions;
    }

    private void AnalyzeQueryPatterns(string queryText, List<QueryOptimizationSuggestion> suggestions)
    {
        var upperQuery = queryText.ToUpperInvariant();

        // SELECT * detection
        if (Regex.IsMatch(upperQuery, @"SELECT\s+\*\s+FROM"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Warning,
                Title = "SELECT * Detected",
                Description = "Using SELECT * retrieves all columns, including unused ones, increasing I/O.",
                RecommendedAction = "Specify only required columns in SELECT clause to reduce data transfer and improve performance.",
                EstimatedImpact = 40
            });
        }

        // OR in WHERE clause
        if (Regex.IsMatch(upperQuery, @"WHERE.*\sOR\s"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Info,
                Title = "OR Operator in WHERE Clause",
                Description = "OR conditions can prevent index usage. Consider using UNION or IN operator.",
                RecommendedAction = "Rewrite using UNION ALL or IN clause for better index utilization.",
                SqlCode = "-- Example:\n-- Instead of: WHERE col1 = @val1 OR col1 = @val2\n-- Use: WHERE col1 IN (@val1, @val2)",
                EstimatedImpact = 30
            });
        }

        // Functions on columns in WHERE
        if (Regex.IsMatch(upperQuery, @"WHERE.*\w+\s*\([^\)]*\w+\s*\)\s*[=<>]"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Warning,
                Title = "Function on Column in WHERE Clause",
                Description = "Using functions on columns in WHERE prevents index usage (non-SARGable).",
                RecommendedAction = "Rewrite query to avoid functions on indexed columns. Apply functions to compared values instead.",
                SqlCode = "-- Bad: WHERE YEAR(OrderDate) = 2024\n-- Good: WHERE OrderDate >= '2024-01-01' AND OrderDate < '2025-01-01'",
                EstimatedImpact = 70
            });
        }

        // Multiple JOINs without WHERE
        var joinCount = Regex.Matches(upperQuery, @"\sJOIN\s").Count;
        if (joinCount > 3 && !upperQuery.Contains("WHERE"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Warning,
                Title = "Multiple JOINs Without WHERE Clause",
                Description = $"Query has {joinCount} JOINs without WHERE clause, potentially returning large result sets.",
                RecommendedAction = "Add WHERE clause to filter data early and reduce JOIN result set size.",
                EstimatedImpact = 60
            });
        }

        // NOT IN detection
        if (upperQuery.Contains("NOT IN"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Info,
                Title = "NOT IN Operator Detected",
                Description = "NOT IN can be slow with large datasets and doesn't handle NULLs correctly.",
                RecommendedAction = "Consider using LEFT JOIN with IS NULL or NOT EXISTS for better performance.",
                SqlCode = "-- Instead of: WHERE id NOT IN (SELECT ...)\n-- Use: WHERE NOT EXISTS (SELECT 1 FROM ... WHERE ...)",
                EstimatedImpact = 45
            });
        }

        // DISTINCT usage
        if (upperQuery.Contains("DISTINCT"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Info,
                Title = "DISTINCT Usage Detected",
                Description = "DISTINCT requires sorting/grouping which can be expensive. May indicate data model issues.",
                RecommendedAction = "Verify if DISTINCT is necessary. Consider fixing underlying data duplication or using GROUP BY.",
                EstimatedImpact = 35
            });
        }

        // LIKE with leading wildcard
        if (Regex.IsMatch(upperQuery, @"LIKE\s+['\""]%"))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.Index,
                Severity = SuggestionSeverity.Warning,
                Title = "Leading Wildcard in LIKE",
                Description = "LIKE with leading wildcard (%value) prevents index usage, forcing table scan.",
                RecommendedAction = "If possible, remove leading wildcard or consider full-text search for text searching.",
                EstimatedImpact = 55
            });
        }

        // Implicit conversions (common AX issue)
        if (Regex.IsMatch(upperQuery, @"WHERE\s+\w+\s*=\s*N'") || Regex.IsMatch(upperQuery, @"=\s*CAST\s*\("))
        {
            suggestions.Add(new QueryOptimizationSuggestion
            {
                Category = SuggestionCategory.QueryRewrite,
                Severity = SuggestionSeverity.Warning,
                Title = "Potential Implicit Conversion",
                Description = "Data type mismatch in WHERE clause can cause implicit conversions, preventing index usage.",
                RecommendedAction = "Ensure parameter data types match column types. Check for NVARCHAR vs VARCHAR mismatches.",
                EstimatedImpact = 65
            });
        }
    }

    private async Task AnalyzeMissingIndexesAsync(SqlQueryMetric query, List<QueryOptimizationSuggestion> suggestions)
    {
        try
        {
            // Get missing indexes from SQL Server DMV
            var missingIndexes = await _databaseStats.GetMissingIndexesAsync();

            if (missingIndexes.Any())
            {
                var topMissingIndex = missingIndexes.OrderByDescending(i => i.ImpactScore).FirstOrDefault();

                if (topMissingIndex != null)
                {
                    var createIndexScript = GenerateCreateIndexScript(topMissingIndex);

                    suggestions.Add(new QueryOptimizationSuggestion
                    {
                        Category = SuggestionCategory.Index,
                        Severity = SuggestionSeverity.Critical,
                        Title = "Missing Index Detected",
                        Description = $"SQL Server recommends an index on {topMissingIndex.TableName}. Impact score: {topMissingIndex.ImpactScore:N0}",
                        RecommendedAction = "Create the recommended index to improve query performance.",
                        SqlCode = createIndexScript,
                        EstimatedImpact = Math.Min(95, topMissingIndex.ImpactScore / 1000)
                    });
                }
            }
        }
        catch
        {
            // Silently handle errors
        }
    }

    private string GenerateCreateIndexScript(MissingIndex missingIndex)
    {
        var indexName = $"IX_{missingIndex.TableName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        var script = $"CREATE NONCLUSTERED INDEX [{indexName}] ON {missingIndex.TableName}\n(\n";

        // Equality columns
        if (!string.IsNullOrEmpty(missingIndex.EqualityColumns))
        {
            script += $"    {missingIndex.EqualityColumns}\n";
        }

        // Inequality columns
        if (!string.IsNullOrEmpty(missingIndex.InequalityColumns))
        {
            if (!string.IsNullOrEmpty(missingIndex.EqualityColumns))
                script += $"    ,{missingIndex.InequalityColumns}\n";
            else
                script += $"    {missingIndex.InequalityColumns}\n";
        }

        script += ")\n";

        // Included columns
        if (!string.IsNullOrEmpty(missingIndex.IncludedColumns))
        {
            script += $"INCLUDE ({missingIndex.IncludedColumns})\n";
        }

        script += "WITH (ONLINE = ON, FILLFACTOR = 90)";

        return script;
    }

    public async Task<List<MissingIndex>> DetectMissingIndexesForQueryAsync(string queryText)
    {
        // This would require parsing the query and matching with missing index DMV
        // For now, return all missing indexes
        return await _databaseStats.GetMissingIndexesAsync();
    }

    public async Task<PerformancePrediction> PredictPerformanceAsync(SqlQueryMetric query)
    {
        var prediction = new PerformancePrediction
        {
            PredictedCpuTimeMs = query.AvgCpuTimeMs,
            PredictedLogicalReads = query.AvgLogicalReads,
            PredictedPhysicalReads = query.AvgPhysicalReads,
            PredictedDurationMs = query.AvgElapsedTimeMs,
            ConfidenceScore = 0.75, // Base confidence
            PredictionModel = "Statistical Baseline"
        };

        // Analyze contributing factors
        AnalyzePerformanceFactors(query, prediction);

        return await Task.FromResult(prediction);
    }

    public async Task<PerformancePrediction> PredictPerformanceWithOptimizationsAsync(
        SqlQueryMetric query,
        List<QueryOptimizationSuggestion> suggestions)
    {
        var baseline = await PredictPerformanceAsync(query);

        // Calculate optimization impact
        var totalImpactPercent = suggestions.Sum(s => s.EstimatedImpact) / suggestions.Count;
        var reductionFactor = Math.Min(totalImpactPercent / 100.0, 0.95); // Max 95% improvement

        var prediction = new PerformancePrediction
        {
            PredictedCpuTimeMs = baseline.PredictedCpuTimeMs * (1 - reductionFactor),
            PredictedLogicalReads = (long)(baseline.PredictedLogicalReads * (1 - reductionFactor)),
            PredictedPhysicalReads = (long)(baseline.PredictedPhysicalReads * (1 - reductionFactor * 0.8)),
            PredictedDurationMs = baseline.PredictedDurationMs * (1 - reductionFactor),
            ConfidenceScore = CalculateConfidenceScore(suggestions),
            PredictionModel = "Optimization Impact Model",
            ContributingFactors = baseline.ContributingFactors,
            OptimizationImpact = new PerformanceImpact
            {
                CpuTimeReductionPercent = reductionFactor * 100,
                LogicalReadsReductionPercent = reductionFactor * 100,
                DurationReductionPercent = reductionFactor * 100,
                OverallImprovementPercent = reductionFactor * 100,
                Summary = GenerateImpactSummary(reductionFactor, suggestions.Count)
            }
        };

        return prediction;
    }

    private void AnalyzePerformanceFactors(SqlQueryMetric query, PerformancePrediction prediction)
    {
        var factors = new List<PerformanceFactor>();
        var upperQuery = query.QueryText.ToUpperInvariant();

        // Factor 1: Logical Reads Impact
        if (query.AvgLogicalReads > 10000)
        {
            var impact = Math.Min(40, query.AvgLogicalReads / 10000.0);
            factors.Add(new PerformanceFactor
            {
                Factor = "High Logical Reads",
                ImpactPercent = impact,
                Description = $"{query.AvgLogicalReads:N0} logical reads indicate missing indexes or inefficient query patterns"
            });
        }

        // Factor 2: CPU Time Impact
        if (query.AvgCpuTimeMs > 100)
        {
            var impact = Math.Min(35, query.AvgCpuTimeMs / 50.0);
            factors.Add(new PerformanceFactor
            {
                Factor = "High CPU Usage",
                ImpactPercent = impact,
                Description = $"{query.AvgCpuTimeMs:F2}ms CPU time suggests complex operations or inefficient logic"
            });
        }

        // Factor 3: Physical Reads Impact
        if (query.AvgPhysicalReads > 1000)
        {
            var impact = Math.Min(25, query.AvgPhysicalReads / 1000.0);
            factors.Add(new PerformanceFactor
            {
                Factor = "Physical I/O",
                ImpactPercent = impact,
                Description = $"{query.AvgPhysicalReads:N0} physical reads indicate buffer cache misses"
            });
        }

        // Factor 4: Execution Frequency Impact
        if (query.ExecutionCount > 1000)
        {
            var impact = Math.Min(20, query.ExecutionCount / 1000.0);
            factors.Add(new PerformanceFactor
            {
                Factor = "High Execution Frequency",
                ImpactPercent = impact,
                Description = $"{query.ExecutionCount:N0} executions - optimization will have significant cumulative impact"
            });
        }

        // Factor 5: Query Complexity
        var joinCount = Regex.Matches(upperQuery, @"\sJOIN\s").Count;
        var subqueryCount = Regex.Matches(upperQuery, @"\(\s*SELECT\s").Count;

        if (joinCount > 3 || subqueryCount > 1)
        {
            var complexity = (joinCount * 5) + (subqueryCount * 10);
            var impact = Math.Min(30, complexity);
            factors.Add(new PerformanceFactor
            {
                Factor = "Query Complexity",
                ImpactPercent = impact,
                Description = $"{joinCount} JOINs, {subqueryCount} subqueries - complex execution plan"
            });
        }

        // Factor 6: Anti-patterns
        var antiPatterns = 0;
        if (Regex.IsMatch(upperQuery, @"SELECT\s+\*\s+FROM")) antiPatterns++;
        if (Regex.IsMatch(upperQuery, @"WHERE.*\sOR\s")) antiPatterns++;
        if (Regex.IsMatch(upperQuery, @"WHERE.*\w+\s*\([^\)]*\w+\s*\)\s*[=<>]")) antiPatterns++;
        if (upperQuery.Contains("NOT IN")) antiPatterns++;
        if (Regex.IsMatch(upperQuery, @"LIKE\s+['\""]%")) antiPatterns++;

        if (antiPatterns > 0)
        {
            var impact = antiPatterns * 8;
            factors.Add(new PerformanceFactor
            {
                Factor = "Query Anti-patterns",
                ImpactPercent = impact,
                Description = $"{antiPatterns} performance anti-patterns detected (SELECT *, OR, functions in WHERE, etc.)"
            });
        }

        // Normalize factors so they add up to 100%
        var totalImpact = factors.Sum(f => f.ImpactPercent);
        if (totalImpact > 0)
        {
            foreach (var factor in factors)
            {
                factor.ImpactPercent = (factor.ImpactPercent / totalImpact) * 100;
            }
        }

        prediction.ContributingFactors = factors.OrderByDescending(f => f.ImpactPercent).ToList();

        // Adjust confidence based on data quality
        if (query.ExecutionCount > 100)
            prediction.ConfidenceScore = Math.Min(0.95, prediction.ConfidenceScore + 0.15);
        if (query.ExecutionCount < 10)
            prediction.ConfidenceScore = Math.Max(0.40, prediction.ConfidenceScore - 0.25);
    }

    private double CalculateConfidenceScore(List<QueryOptimizationSuggestion> suggestions)
    {
        // Base confidence
        double confidence = 0.70;

        // More suggestions = more evidence = higher confidence
        var suggestionCount = suggestions.Count;
        if (suggestionCount >= 5)
            confidence += 0.15;
        else if (suggestionCount >= 3)
            confidence += 0.10;
        else if (suggestionCount >= 1)
            confidence += 0.05;

        // High-impact suggestions increase confidence
        var highImpactCount = suggestions.Count(s => s.EstimatedImpact > 70);
        if (highImpactCount > 0)
            confidence += highImpactCount * 0.05;

        // Index recommendations are very reliable
        var indexSuggestions = suggestions.Count(s => s.Category == SuggestionCategory.Index);
        if (indexSuggestions > 0)
            confidence += 0.10;

        return Math.Min(0.98, confidence);
    }

    private string GenerateImpactSummary(double reductionFactor, int suggestionCount)
    {
        var improvementPercent = reductionFactor * 100;

        if (improvementPercent >= 80)
            return $"Exceptional improvement expected: {improvementPercent:F0}% faster with {suggestionCount} optimizations";
        else if (improvementPercent >= 60)
            return $"Significant improvement expected: {improvementPercent:F0}% faster with {suggestionCount} optimizations";
        else if (improvementPercent >= 40)
            return $"Good improvement expected: {improvementPercent:F0}% faster with {suggestionCount} optimizations";
        else if (improvementPercent >= 20)
            return $"Moderate improvement expected: {improvementPercent:F0}% faster with {suggestionCount} optimizations";
        else
            return $"Minor improvement expected: {improvementPercent:F0}% faster with {suggestionCount} optimizations";
    }
}
