using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// AI-powered query explainer that provides natural language explanations for query performance
/// </summary>
public interface IAiQueryExplainerService
{
    /// <summary>
    /// Generate a comprehensive, easy-to-understand explanation of why a query is slow
    /// </summary>
    Task<QueryExplanation> ExplainQueryPerformanceAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Answer a specific question about the query in natural language
    /// </summary>
    Task<ConversationResponse> AskQuestionAsync(string question, SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get suggested questions that users might want to ask about this query
    /// </summary>
    Task<List<string>> GetSuggestedQuestionsAsync(SqlQueryMetric query);

    /// <summary>
    /// Get a quick summary (1-2 sentences) of the query's performance issue
    /// </summary>
    Task<string> GetQuickSummaryAsync(SqlQueryMetric query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if AI service is configured and available
    /// </summary>
    bool IsAvailable { get; }
}

/// <summary>
/// Comprehensive explanation of a query's performance
/// </summary>
public class QueryExplanation
{
    /// <summary>
    /// Quick 1-2 sentence summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Performance rating: Excellent, Good, Fair, Poor, Critical
    /// </summary>
    public string PerformanceRating { get; set; } = string.Empty;

    /// <summary>
    /// Overall performance score (0-100)
    /// </summary>
    public int PerformanceScore { get; set; }

    /// <summary>
    /// List of identified problems with the query
    /// </summary>
    public List<PerformanceProblem> Problems { get; set; } = new();

    /// <summary>
    /// List of things the query is doing well
    /// </summary>
    public List<string> PositiveAspects { get; set; } = new();

    /// <summary>
    /// Prioritized recommendations for improvement
    /// </summary>
    public List<ExplainerRecommendation> Recommendations { get; set; } = new();

    /// <summary>
    /// Estimated total improvement if all recommendations are implemented
    /// </summary>
    public int EstimatedTotalImprovement { get; set; }

    /// <summary>
    /// ROI summary for implementing fixes
    /// </summary>
    public string ROISummary { get; set; } = string.Empty;

    /// <summary>
    /// Technical details for advanced users
    /// </summary>
    public string TechnicalDetails { get; set; } = string.Empty;

    /// <summary>
    /// When this explanation was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// A specific performance problem identified in the query
/// </summary>
public class PerformanceProblem
{
    /// <summary>
    /// Severity: Critical, High, Medium, Low
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Emoji icon for visual representation
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Short title of the problem
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed explanation in simple terms
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// Impact on performance (percentage or description)
    /// </summary>
    public string Impact { get; set; } = string.Empty;

    /// <summary>
    /// Why this is a problem
    /// </summary>
    public string Why { get; set; } = string.Empty;

    /// <summary>
    /// Example or analogy to make it understandable
    /// </summary>
    public string Analogy { get; set; } = string.Empty;
}

/// <summary>
/// A recommendation from the explainer
/// </summary>
public class ExplainerRecommendation
{
    /// <summary>
    /// Priority: Critical, High, Medium, Low
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Emoji icon
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Short title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// What to do (action-oriented)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Why this will help
    /// </summary>
    public string Benefit { get; set; } = string.Empty;

    /// <summary>
    /// Estimated performance improvement (%)
    /// </summary>
    public int EstimatedImprovement { get; set; }

    /// <summary>
    /// Estimated time to implement (minutes)
    /// </summary>
    public int EstimatedTimeMinutes { get; set; }

    /// <summary>
    /// Risk level: Safe, Low, Medium, High
    /// </summary>
    public string RiskLevel { get; set; } = "Safe";

    /// <summary>
    /// SQL script to execute (if applicable)
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Step-by-step implementation guide
    /// </summary>
    public List<string> Steps { get; set; } = new();
}

/// <summary>
/// Response to a user's question about the query
/// </summary>
public class ConversationResponse
{
    /// <summary>
    /// The user's question
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// The AI's answer
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Related suggestions or follow-up questions
    /// </summary>
    public List<string> FollowUpSuggestions { get; set; } = new();

    /// <summary>
    /// Code snippets or scripts related to the answer
    /// </summary>
    public List<CodeSnippet> CodeSnippets { get; set; } = new();

    /// <summary>
    /// When this response was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Code snippet included in a response
/// </summary>
public class CodeSnippet
{
    /// <summary>
    /// Description of what this code does
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The code (SQL, etc.)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Language (SQL, TSQL, etc.)
    /// </summary>
    public string Language { get; set; } = "SQL";
}
