using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// AI-powered query optimization service using Large Language Models
/// </summary>
public interface IAiQueryOptimizerService
{
    /// <summary>
    /// Analyzes a SQL query using AI and provides intelligent optimization suggestions
    /// </summary>
    Task<AiQueryAnalysisResult> AnalyzeQueryWithAiAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an optimized version of the SQL query
    /// </summary>
    Task<string> GenerateOptimizedQueryAsync(string queryText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explains why a query is slow in natural language
    /// </summary>
    Task<string> ExplainQueryPerformanceAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch analyze multiple queries at once
    /// </summary>
    Task<List<AiQueryAnalysisResult>> BatchAnalyzeQueriesAsync(List<SqlQueryMetric> queries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate query complexity score (0-100)
    /// </summary>
    Task<int> CalculateComplexityScoreAsync(string queryText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate detailed index recommendations
    /// </summary>
    Task<List<IndexRecommendation>> GenerateIndexRecommendationsAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimate query execution cost in monetary terms
    /// </summary>
    Task<QueryCostEstimate> EstimateQueryCostAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compare two queries and show differences
    /// </summary>
    Task<QueryComparisonResult> CompareQueriesAsync(string originalQuery, string optimizedQuery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get AX 2012 specific optimization insights
    /// </summary>
    Task<List<string>> GetAxSpecificInsightsAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if AI service is configured and available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Sends a direct prompt to the AI service without query-specific formatting
    /// </summary>
    Task<string> SendPromptAsync(string prompt, CancellationToken cancellationToken = default);
}
