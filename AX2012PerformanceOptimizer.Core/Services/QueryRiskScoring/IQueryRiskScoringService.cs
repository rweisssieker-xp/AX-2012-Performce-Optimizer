using AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;

namespace AX2012PerformanceOptimizer.Core.Services.QueryRiskScoring;

/// <summary>
/// Service for ML-based query risk scoring and assessment
/// </summary>
public interface IQueryRiskScoringService
{
    /// <summary>
    /// Analyze a SQL query and calculate its risk score
    /// </summary>
    Task<QueryRiskAssessment> AssessQueryRiskAsync(
        string sqlQuery,
        string? executionPlanXml = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch assess multiple queries
    /// </summary>
    Task<List<QueryRiskAssessment>> AssessMultipleQueriesAsync(
        List<string> sqlQueries,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get risk score for a query based on historical data
    /// </summary>
    Task<QueryRiskScore> GetHistoricalRiskScoreAsync(
        string sqlQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Train the ML model with new data
    /// </summary>
    Task TrainModelAsync(
        List<QueryTrainingData> trainingData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queries that match a specific risk level
    /// </summary>
    Task<List<QueryRiskScore>> GetQueriesByRiskLevelAsync(
        QueryRiskLevel riskLevel,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compare risk between two queries
    /// </summary>
    Task<QueryRiskComparison> CompareQueryRisksAsync(
        string query1,
        string query2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get risk trends over time
    /// </summary>
    Task<List<RiskTrendPoint>> GetRiskTrendsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Training data for the ML model
/// </summary>
public class QueryTrainingData
{
    public string SqlQuery { get; set; } = string.Empty;
    public double ActualExecutionTimeMs { get; set; }
    public double ActualCpuUsagePercent { get; set; }
    public double ActualMemoryUsageMb { get; set; }
    public bool CausedIssue { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Comparison result between two queries
/// </summary>
public class QueryRiskComparison
{
    public QueryRiskScore Query1Risk { get; set; } = new();
    public QueryRiskScore Query2Risk { get; set; } = new();
    public double RiskDifference { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public string SaferQuery { get; set; } = string.Empty;
}

/// <summary>
/// Risk trend data point
/// </summary>
public class RiskTrendPoint
{
    public DateTime Timestamp { get; set; }
    public double AverageRiskScore { get; set; }
    public int QueryCount { get; set; }
    public int HighRiskCount { get; set; }
    public int CriticalRiskCount { get; set; }
}
