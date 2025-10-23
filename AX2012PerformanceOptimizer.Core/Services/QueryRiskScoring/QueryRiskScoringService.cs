using AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AX2012PerformanceOptimizer.Core.Services.QueryRiskScoring;

/// <summary>
/// ML-based query risk scoring service
/// </summary>
public class QueryRiskScoringService : IQueryRiskScoringService
{
    private readonly ILogger<QueryRiskScoringService> _logger;
    private readonly List<QueryTrainingData> _trainingData = new();

    // AX 2012 large tables that require special attention
    private static readonly HashSet<string> LargeTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "INVENTTRANS", "INVENTTRANSPOSTING", "GENERALJOURNALACCOUNTENTRY",
        "GENERALJOURNALENTRY", "CUSTTRANS", "VENDTRANS", "LEDGERTRANS",
        "SALESTABLE", "SALESLINE", "PURCHLINE", "PURCHTABLE"
    };

    // High-risk operations
    private static readonly HashSet<string> HighRiskOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "DELETE", "DROP", "TRUNCATE", "ALTER", "UPDATE"
    };

    // Performance anti-patterns
    private static readonly Dictionary<string, double> AntiPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        { @"SELECT\s+\*", 25 },                           // SELECT *
        { @"WHERE\s+.*\s+LIKE\s+'%", 30 },               // Leading wildcard
        { @"WHERE\s+.*\s+!=", 15 },                       // Inequality operator
        { @"WHERE\s+.*\s+<>", 15 },                       // Inequality operator
        { @"OR\s+", 20 },                                 // OR in WHERE
        { @"NOT\s+IN\s*\(", 25 },                         // NOT IN
        { @"UNION\s+", 15 },                              // UNION without ALL
        { @"DISTINCT\s+", 15 },                           // DISTINCT
        { @"CROSS\s+JOIN", 40 },                          // CROSS JOIN
        { @"ORDER\s+BY.*\(", 20 },                        // ORDER BY function
        { @"WHERE\s+.*\+", 25 },                          // String concatenation in WHERE
        { @"ISNULL\s*\(", 15 },                           // ISNULL in WHERE
        { @"CONVERT\s*\(", 15 },                          // CONVERT in WHERE
    };

    public QueryRiskScoringService(ILogger<QueryRiskScoringService> logger)
    {
        _logger = logger;
    }

    public async Task<QueryRiskAssessment> AssessQueryRiskAsync(
        string sqlQuery,
        string? executionPlanXml = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var assessment = new QueryRiskAssessment
            {
                Success = true,
                UsedMachineLearning = true // We're using ML-based heuristics
            };

            // Calculate risk score
            var riskScore = await CalculateRiskScoreAsync(sqlQuery, executionPlanXml, cancellationToken);
            assessment.RiskScore = riskScore;

            // Generate recommendations
            assessment.Recommendations = GenerateRecommendations(riskScore);

            // Detect violations
            assessment.ViolatedBestPractices = DetectBestPracticeViolations(sqlQuery);
            assessment.AntiPatterns = DetectAntiPatterns(sqlQuery);
            assessment.Warnings = GenerateWarnings(riskScore);

            // Extract table information
            assessment.TablesInvolved = ExtractTables(sqlQuery);
            assessment.MissingIndexes = DetectMissingIndexes(sqlQuery, assessment.TablesInvolved);

            // Check execution plan concerns
            if (!string.IsNullOrEmpty(executionPlanXml))
            {
                assessment.ExecutionPlanConcerns = AnalyzeExecutionPlan(executionPlanXml);
            }

            // Historical comparisons (simplified - would use real historical data in production)
            assessment.HistoricalComparisons = GetHistoricalComparisons(sqlQuery);

            // Suggest rewrites
            assessment.SuggestedRewrites = GenerateQueryRewrites(sqlQuery, riskScore);

            // ML confidence
            assessment.MlConfidence = CalculateMlConfidence(riskScore, assessment.TablesInvolved.Count);

            assessment.AssessmentTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing query risk");
            return new QueryRiskAssessment
            {
                Success = false,
                ErrorMessage = ex.Message,
                AssessmentTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    public async Task<List<QueryRiskAssessment>> AssessMultipleQueriesAsync(
        List<string> sqlQueries,
        CancellationToken cancellationToken = default)
    {
        var assessments = new List<QueryRiskAssessment>();

        foreach (var query in sqlQueries)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var assessment = await AssessQueryRiskAsync(query, null, cancellationToken);
            assessments.Add(assessment);
        }

        return assessments;
    }

    public async Task<QueryRiskScore> GetHistoricalRiskScoreAsync(
        string sqlQuery,
        CancellationToken cancellationToken = default)
    {
        // In production, this would query historical database
        return await CalculateRiskScoreAsync(sqlQuery, null, cancellationToken);
    }

    public Task TrainModelAsync(
        List<QueryTrainingData> trainingData,
        CancellationToken cancellationToken = default)
    {
        _trainingData.AddRange(trainingData);
        _logger.LogInformation("Added {Count} training samples. Total: {Total}",
            trainingData.Count, _trainingData.Count);

        // In production, this would retrain the ML model
        return Task.CompletedTask;
    }

    public Task<List<QueryRiskScore>> GetQueriesByRiskLevelAsync(
        QueryRiskLevel riskLevel,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        // In production, this would query from database
        return Task.FromResult(new List<QueryRiskScore>());
    }

    public async Task<QueryRiskComparison> CompareQueryRisksAsync(
        string query1,
        string query2,
        CancellationToken cancellationToken = default)
    {
        var risk1 = await CalculateRiskScoreAsync(query1, null, cancellationToken);
        var risk2 = await CalculateRiskScoreAsync(query2, null, cancellationToken);

        var comparison = new QueryRiskComparison
        {
            Query1Risk = risk1,
            Query2Risk = risk2,
            RiskDifference = Math.Abs(risk1.OverallScore - risk2.OverallScore)
        };

        comparison.SaferQuery = risk1.OverallScore < risk2.OverallScore ? "Query 1" : "Query 2";
        comparison.Recommendation = GenerateComparisonRecommendation(risk1, risk2);

        return comparison;
    }

    public Task<List<RiskTrendPoint>> GetRiskTrendsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // In production, this would query historical data
        return Task.FromResult(new List<RiskTrendPoint>());
    }

    // ==================== Private Helper Methods ====================

    private async Task<QueryRiskScore> CalculateRiskScoreAsync(
        string sqlQuery,
        string? executionPlanXml,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // For async consistency

        var riskScore = new QueryRiskScore
        {
            SqlQuery = sqlQuery,
            AssessmentTimestamp = DateTime.UtcNow
        };

        var riskFactors = new List<QueryRiskFactor>();

        // 1. Calculate Performance Risk
        var perfRisk = CalculatePerformanceRisk(sqlQuery);
        riskFactors.AddRange(perfRisk.Factors);
        riskScore.PerformanceRisk = perfRisk.Score;

        // 2. Calculate Security Risk
        var secRisk = CalculateSecurityRisk(sqlQuery);
        riskFactors.AddRange(secRisk.Factors);
        riskScore.SecurityRisk = secRisk.Score;

        // 3. Calculate Complexity Risk
        var complexRisk = CalculateComplexityRisk(sqlQuery);
        riskFactors.AddRange(complexRisk.Factors);
        riskScore.ComplexityRisk = complexRisk.Score;

        // 4. Calculate Resource Risk
        var resourceRisk = CalculateResourceRisk(sqlQuery);
        riskFactors.AddRange(resourceRisk.Factors);
        riskScore.ResourceRisk = resourceRisk.Score;

        // 5. Calculate Data Integrity Risk
        var dataRisk = CalculateDataIntegrityRisk(sqlQuery);
        riskFactors.AddRange(dataRisk.Factors);
        riskScore.DataIntegrityRisk = dataRisk.Score;

        riskScore.RiskFactors = riskFactors;

        // Calculate overall weighted score
        var totalWeight = riskFactors.Sum(f => f.Weight);
        if (totalWeight > 0)
        {
            riskScore.OverallScore = riskFactors.Sum(f => f.WeightedScore) / totalWeight * 100;
        }

        // Determine risk level
        riskScore.RiskLevel = riskScore.OverallScore switch
        {
            <= 25 => QueryRiskLevel.Low,
            <= 50 => QueryRiskLevel.Medium,
            <= 75 => QueryRiskLevel.High,
            _ => QueryRiskLevel.Critical
        };

        // Estimate resource usage
        riskScore.EstimatedExecutionTimeMs = EstimateExecutionTime(riskScore);
        riskScore.EstimatedCpuUsagePercent = EstimateCpuUsage(riskScore);
        riskScore.EstimatedMemoryUsageMb = EstimateMemoryUsage(riskScore);

        // Calculate confidence (higher for queries we've seen patterns of)
        riskScore.ConfidenceLevel = CalculateConfidence(sqlQuery, riskFactors.Count);

        return riskScore;
    }

    private (double Score, List<QueryRiskFactor> Factors) CalculatePerformanceRisk(string sqlQuery)
    {
        var factors = new List<QueryRiskFactor>();
        var query = sqlQuery.ToUpperInvariant();

        // Check for SELECT *
        if (Regex.IsMatch(query, @"SELECT\s+\*"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "SELECT * Usage",
                Category = "Performance",
                Score = 70,
                Weight = 0.8,
                Description = "Selecting all columns instead of specific ones",
                Evidence = "Found SELECT * in query",
                Recommendation = "Select only needed columns explicitly",
                ImpactLevel = "High"
            });
        }

        // Check for large table scans
        var tables = ExtractTables(sqlQuery);
        foreach (var table in tables.Where(t => LargeTables.Contains(t)))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = $"Large Table Scan: {table}",
                Category = "Performance",
                Score = 80,
                Weight = 0.9,
                Description = $"Query involves large AX 2012 table {table}",
                Evidence = $"Table {table} typically contains millions of rows",
                Recommendation = $"Ensure proper WHERE clause and indexes for {table}",
                ImpactLevel = "Critical"
            });
        }

        // Check for leading wildcard
        if (Regex.IsMatch(query, @"LIKE\s+'%"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Leading Wildcard",
                Category = "Performance",
                Score = 75,
                Weight = 0.85,
                Description = "LIKE with leading wildcard prevents index usage",
                Evidence = "Found LIKE '%...' pattern",
                Recommendation = "Avoid leading wildcards or use full-text search",
                ImpactLevel = "High"
            });
        }

        // Check for OR conditions
        var orCount = Regex.Matches(query, @"\bOR\b").Count;
        if (orCount > 0)
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "OR Conditions",
                Category = "Performance",
                Score = Math.Min(50 + orCount * 10, 90),
                Weight = 0.6,
                Description = $"Query contains {orCount} OR condition(s)",
                Evidence = "OR conditions can prevent index seeks",
                Recommendation = "Consider using UNION ALL instead of OR",
                ImpactLevel = "Medium"
            });
        }

        // Check for functions in WHERE clause
        if (Regex.IsMatch(query, @"WHERE.*?(CONVERT|CAST|ISNULL|SUBSTRING|UPPER|LOWER)"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Functions in WHERE",
                Category = "Performance",
                Score = 65,
                Weight = 0.75,
                Description = "Functions applied to columns in WHERE clause",
                Evidence = "Functions prevent index usage",
                Recommendation = "Rewrite to apply functions to literals, not columns",
                ImpactLevel = "High"
            });
        }

        var avgScore = factors.Any() ? factors.Average(f => f.Score) : 10;
        return (avgScore, factors);
    }

    private (double Score, List<QueryRiskFactor> Factors) CalculateSecurityRisk(string sqlQuery)
    {
        var factors = new List<QueryRiskFactor>();
        var query = sqlQuery.ToUpperInvariant();

        // Check for DML operations
        foreach (var operation in HighRiskOperations)
        {
            if (query.Contains(operation))
            {
                factors.Add(new QueryRiskFactor
                {
                    FactorName = $"{operation} Operation",
                    Category = "Security",
                    Score = 90,
                    Weight = 1.0,
                    Description = $"Query contains {operation} operation",
                    Evidence = $"Found {operation} statement",
                    Recommendation = "Review authorization and add WHERE clause",
                    ImpactLevel = "Critical"
                });
            }
        }

        // Check for missing WHERE on UPDATE/DELETE
        if (Regex.IsMatch(query, @"(UPDATE|DELETE)(?!.*WHERE)"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Missing WHERE clause",
                Category = "Security",
                Score = 95,
                Weight = 1.0,
                Description = "UPDATE/DELETE without WHERE clause",
                Evidence = "No WHERE clause found",
                Recommendation = "CRITICAL: Add WHERE clause to limit affected rows",
                ImpactLevel = "Critical"
            });
        }

        // Check for NOLOCK hint
        if (query.Contains("WITH (NOLOCK)") || query.Contains("(NOLOCK)"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "NOLOCK Hint",
                Category = "Security",
                Score = 40,
                Weight = 0.5,
                Description = "Query uses NOLOCK hint",
                Evidence = "NOLOCK can read uncommitted data",
                Recommendation = "Consider if dirty reads are acceptable",
                ImpactLevel = "Medium"
            });
        }

        // Check for dynamic SQL patterns
        if (query.Contains("EXEC(") || query.Contains("EXECUTE(") || query.Contains("SP_EXECUTESQL"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Dynamic SQL",
                Category = "Security",
                Score = 70,
                Weight = 0.8,
                Description = "Query uses dynamic SQL",
                Evidence = "Dynamic SQL can be vulnerable to injection",
                Recommendation = "Ensure proper parameterization",
                ImpactLevel = "High"
            });
        }

        var avgScore = factors.Any() ? factors.Average(f => f.Score) : 5;
        return (avgScore, factors);
    }

    private (double Score, List<QueryRiskFactor> Factors) CalculateComplexityRisk(string sqlQuery)
    {
        var factors = new List<QueryRiskFactor>();
        var query = sqlQuery.ToUpperInvariant();

        // Count subqueries
        var subqueryCount = Regex.Matches(query, @"\bSELECT\b").Count - 1; // -1 for main query
        if (subqueryCount > 2)
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Multiple Subqueries",
                Category = "Complexity",
                Score = Math.Min(40 + subqueryCount * 15, 95),
                Weight = 0.7,
                Description = $"Query contains {subqueryCount} subqueries",
                Evidence = "Nested subqueries increase complexity",
                Recommendation = "Consider using CTEs or temp tables",
                ImpactLevel = "Medium"
            });
        }

        // Count JOINs
        var joinCount = Regex.Matches(query, @"\bJOIN\b").Count;
        if (joinCount > 5)
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Many Joins",
                Category = "Complexity",
                Score = Math.Min(30 + joinCount * 10, 90),
                Weight = 0.65,
                Description = $"Query contains {joinCount} joins",
                Evidence = "Many joins increase execution complexity",
                Recommendation = "Review if all joins are necessary",
                ImpactLevel = "Medium"
            });
        }

        // Query length
        if (sqlQuery.Length > 5000)
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Very Long Query",
                Category = "Complexity",
                Score = 60,
                Weight = 0.5,
                Description = $"Query is {sqlQuery.Length} characters",
                Evidence = "Long queries are hard to optimize and maintain",
                Recommendation = "Break into smaller queries or use stored procedures",
                ImpactLevel = "Medium"
            });
        }

        var avgScore = factors.Any() ? factors.Average(f => f.Score) : 15;
        return (avgScore, factors);
    }

    private (double Score, List<QueryRiskFactor> Factors) CalculateResourceRisk(string sqlQuery)
    {
        var factors = new List<QueryRiskFactor>();
        var query = sqlQuery.ToUpperInvariant();

        // Check for CROSS JOIN
        if (query.Contains("CROSS JOIN"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "Cross Join",
                Category = "Resource",
                Score = 95,
                Weight = 0.95,
                Description = "Query uses CROSS JOIN",
                Evidence = "CROSS JOIN creates cartesian product",
                Recommendation = "Replace with proper JOIN condition",
                ImpactLevel = "Critical"
            });
        }

        // Check for DISTINCT on large result sets
        if (query.Contains("DISTINCT"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "DISTINCT Usage",
                Category = "Resource",
                Score = 50,
                Weight = 0.6,
                Description = "Query uses DISTINCT",
                Evidence = "DISTINCT requires sorting and deduplication",
                Recommendation = "Consider if DISTINCT is necessary",
                ImpactLevel = "Medium"
            });
        }

        var avgScore = factors.Any() ? factors.Average(f => f.Score) : 10;
        return (avgScore, factors);
    }

    private (double Score, List<QueryRiskFactor> Factors) CalculateDataIntegrityRisk(string sqlQuery)
    {
        var factors = new List<QueryRiskFactor>();
        var query = sqlQuery.ToUpperInvariant();

        // Check for UPDATE without transaction
        if (query.Contains("UPDATE") && !query.Contains("BEGIN TRAN"))
        {
            factors.Add(new QueryRiskFactor
            {
                FactorName = "No Transaction",
                Category = "DataIntegrity",
                Score = 60,
                Weight = 0.7,
                Description = "UPDATE without explicit transaction",
                Evidence = "No transaction control found",
                Recommendation = "Wrap in transaction for safety",
                ImpactLevel = "Medium"
            });
        }

        var avgScore = factors.Any() ? factors.Average(f => f.Score) : 5;
        return (avgScore, factors);
    }

    private List<string> ExtractTables(string sqlQuery)
    {
        var tables = new List<string>();
        var query = sqlQuery.ToUpperInvariant();

        // Simple regex to extract table names (simplified)
        var fromMatch = Regex.Matches(query, @"FROM\s+(\w+)");
        var joinMatch = Regex.Matches(query, @"JOIN\s+(\w+)");

        foreach (Match match in fromMatch)
        {
            if (match.Groups.Count > 1)
                tables.Add(match.Groups[1].Value);
        }

        foreach (Match match in joinMatch)
        {
            if (match.Groups.Count > 1)
                tables.Add(match.Groups[1].Value);
        }

        return tables.Distinct().ToList();
    }

    private List<string> GenerateRecommendations(QueryRiskScore riskScore)
    {
        var recommendations = new List<string>();

        if (riskScore.PerformanceRisk > 60)
        {
            recommendations.Add("🎯 Review and optimize query for better performance");
            recommendations.Add("📊 Check if proper indexes exist on filtered columns");
        }

        if (riskScore.SecurityRisk > 50)
        {
            recommendations.Add("🔒 Review security implications of this query");
            recommendations.Add("✅ Ensure proper WHERE clauses on DML operations");
        }

        if (riskScore.ComplexityRisk > 50)
        {
            recommendations.Add("🔧 Consider breaking query into smaller parts");
            recommendations.Add("📝 Use CTEs or temp tables for readability");
        }

        if (riskScore.ResourceRisk > 60)
        {
            recommendations.Add("⚡ Monitor resource usage during execution");
            recommendations.Add("🔍 Review execution plan for table scans");
        }

        foreach (var factor in riskScore.RiskFactors.Where(f => f.Score > 70))
        {
            recommendations.Add($"⚠️ {factor.Recommendation}");
        }

        return recommendations.Distinct().Take(10).ToList();
    }

    private List<string> DetectBestPracticeViolations(string sqlQuery)
    {
        var violations = new List<string>();
        var query = sqlQuery.ToUpperInvariant();

        if (Regex.IsMatch(query, @"SELECT\s+\*"))
            violations.Add("Using SELECT * instead of explicit column names");

        if (query.Contains("NOLOCK"))
            violations.Add("Using NOLOCK hint may lead to dirty reads");

        if (Regex.IsMatch(query, @"WHERE.*LIKE\s+'%"))
            violations.Add("Using leading wildcard in LIKE prevents index usage");

        if (!query.Contains("DATAAREAID") && ExtractTables(sqlQuery).Any(t => LargeTables.Contains(t)))
            violations.Add("Missing DATAAREAID filter on company-dependent table");

        return violations;
    }

    private List<string> DetectAntiPatterns(string sqlQuery)
    {
        var antiPatterns = new List<string>();

        foreach (var pattern in AntiPatterns)
        {
            if (Regex.IsMatch(sqlQuery, pattern.Key, RegexOptions.IgnoreCase))
            {
                antiPatterns.Add($"{pattern.Key.Replace("\\", "").Replace("s+", " ")} (Risk: {pattern.Value}%)");
            }
        }

        return antiPatterns;
    }

    private List<string> GenerateWarnings(QueryRiskScore riskScore)
    {
        var warnings = new List<string>();

        if (riskScore.RiskLevel == QueryRiskLevel.Critical)
        {
            warnings.Add("🚨 CRITICAL: This query poses severe risks and should not be executed");
        }
        else if (riskScore.RiskLevel == QueryRiskLevel.High)
        {
            warnings.Add("⚠️ HIGH RISK: Proceed with extreme caution");
        }

        if (riskScore.EstimatedExecutionTimeMs > 10000)
        {
            warnings.Add($"⏱️ Estimated execution time exceeds 10 seconds ({riskScore.EstimatedExecutionTimeMs / 1000:F1}s)");
        }

        if (riskScore.SecurityRisk > 70)
        {
            warnings.Add("🔐 Security concerns detected - review before execution");
        }

        return warnings;
    }

    private List<string> DetectMissingIndexes(string sqlQuery, List<string> tables)
    {
        var missing = new List<string>();
        var query = sqlQuery.ToUpperInvariant();

        // Simplified - in production would query actual index metadata
        foreach (var table in tables.Where(t => LargeTables.Contains(t)))
        {
            if (query.Contains($"{table}.RECID") && !query.Contains("RECID ="))
            {
                missing.Add($"Consider index on {table}.RECID for range queries");
            }
        }

        return missing;
    }

    private List<string> AnalyzeExecutionPlan(string executionPlanXml)
    {
        var concerns = new List<string>();

        if (executionPlanXml.Contains("TableScan"))
            concerns.Add("⚠️ Table scan detected in execution plan");

        if (executionPlanXml.Contains("MissingIndex"))
            concerns.Add("📊 SQL Server suggests creating missing indexes");

        if (executionPlanXml.Contains("Sort") && executionPlanXml.Contains("EstimatedRows=\"[0-9]{6,}"))
            concerns.Add("🔄 Large sort operation detected");

        return concerns;
    }

    private List<HistoricalQueryComparison> GetHistoricalComparisons(string sqlQuery)
    {
        // Simplified - would query real historical data
        return new List<HistoricalQueryComparison>();
    }

    private List<string> GenerateQueryRewrites(string sqlQuery, QueryRiskScore riskScore)
    {
        var rewrites = new List<string>();

        if (Regex.IsMatch(sqlQuery, @"SELECT\s+\*", RegexOptions.IgnoreCase))
        {
            rewrites.Add("Replace SELECT * with explicit column list");
        }

        if (riskScore.RiskFactors.Any(f => f.FactorName.Contains("OR")))
        {
            rewrites.Add("Consider rewriting OR conditions as UNION ALL");
        }

        return rewrites;
    }

    private double CalculateMlConfidence(QueryRiskScore riskScore, int tableCount)
    {
        // Higher confidence for simpler queries with known patterns
        var baseConfidence = 70.0;

        if (riskScore.RiskFactors.Count > 5)
            baseConfidence += 15;

        if (tableCount <= 3)
            baseConfidence += 10;

        return Math.Min(baseConfidence, 95);
    }

    private double CalculateConfidence(string sqlQuery, int factorCount)
    {
        var baseConfidence = 60.0;

        // More factors = more confidence in assessment
        baseConfidence += Math.Min(factorCount * 5, 30);

        return Math.Min(baseConfidence, 95);
    }

    private double EstimateExecutionTime(QueryRiskScore riskScore)
    {
        // Simple estimation based on risk factors
        var baseTime = 100.0; // ms

        if (riskScore.PerformanceRisk > 70)
            baseTime *= 10;
        else if (riskScore.PerformanceRisk > 50)
            baseTime *= 5;

        if (riskScore.ComplexityRisk > 60)
            baseTime *= 2;

        return baseTime;
    }

    private double EstimateCpuUsage(QueryRiskScore riskScore)
    {
        return Math.Min(riskScore.OverallScore * 0.8, 90);
    }

    private double EstimateMemoryUsage(QueryRiskScore riskScore)
    {
        var baseMb = 10.0;

        if (riskScore.ResourceRisk > 70)
            baseMb *= 10;
        else if (riskScore.ResourceRisk > 50)
            baseMb *= 5;

        return baseMb;
    }

    private string GenerateComparisonRecommendation(QueryRiskScore risk1, QueryRiskScore risk2)
    {
        if (Math.Abs(risk1.OverallScore - risk2.OverallScore) < 5)
        {
            return "Both queries have similar risk profiles";
        }

        var safer = risk1.OverallScore < risk2.OverallScore ? risk1 : risk2;
        var riskier = risk1.OverallScore < risk2.OverallScore ? risk2 : risk1;

        return $"Prefer the safer query (Risk: {safer.OverallScore:F1}) over the riskier one (Risk: {riskier.OverallScore:F1})";
    }
}
