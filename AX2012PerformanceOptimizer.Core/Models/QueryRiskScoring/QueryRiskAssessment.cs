namespace AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;

/// <summary>
/// Complete risk assessment for a query with recommendations
/// </summary>
public class QueryRiskAssessment
{
    /// <summary>
    /// The risk score details
    /// </summary>
    public QueryRiskScore RiskScore { get; set; } = new();

    /// <summary>
    /// Prioritized list of recommendations to reduce risk
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Warnings about potential issues
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Best practices that are violated
    /// </summary>
    public List<string> ViolatedBestPractices { get; set; } = new();

    /// <summary>
    /// Detected anti-patterns in the query
    /// </summary>
    public List<string> AntiPatterns { get; set; } = new();

    /// <summary>
    /// Similar historical queries with known issues
    /// </summary>
    public List<HistoricalQueryComparison> HistoricalComparisons { get; set; } = new();

    /// <summary>
    /// Suggested query rewrites that reduce risk
    /// </summary>
    public List<string> SuggestedRewrites { get; set; } = new();

    /// <summary>
    /// Execution plan concerns
    /// </summary>
    public List<string> ExecutionPlanConcerns { get; set; } = new();

    /// <summary>
    /// Tables involved in the query
    /// </summary>
    public List<string> TablesInvolved { get; set; } = new();

    /// <summary>
    /// Indexes that should exist but are missing
    /// </summary>
    public List<string> MissingIndexes { get; set; } = new();

    /// <summary>
    /// Whether the assessment was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if assessment failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Time taken to perform the assessment (milliseconds)
    /// </summary>
    public double AssessmentTimeMs { get; set; }

    /// <summary>
    /// Machine learning model confidence in the prediction
    /// </summary>
    public double MlConfidence { get; set; }

    /// <summary>
    /// Whether ML model was used or fallback heuristics
    /// </summary>
    public bool UsedMachineLearning { get; set; }
}

/// <summary>
/// Comparison with a historical query that has similar patterns
/// </summary>
public class HistoricalQueryComparison
{
    /// <summary>
    /// Similarity score to the current query (0-100)
    /// </summary>
    public double SimilarityScore { get; set; }

    /// <summary>
    /// Historical average execution time
    /// </summary>
    public double AverageExecutionTimeMs { get; set; }

    /// <summary>
    /// How often this historical query caused issues
    /// </summary>
    public double IssueFrequencyPercent { get; set; }

    /// <summary>
    /// Description of the historical query pattern
    /// </summary>
    public string PatternDescription { get; set; } = string.Empty;

    /// <summary>
    /// What happened when this pattern was executed
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// Lesson learned from this historical pattern
    /// </summary>
    public string LessonLearned { get; set; } = string.Empty;
}
