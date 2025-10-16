using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Natural Language Query Assistant - AI-powered conversational interface
/// UNIQUE FEATURE #11: Natural language interface for performance analysis
/// </summary>
public interface INaturalLanguageQueryAssistant
{
    /// <summary>
    /// Process natural language query and return AI response
    /// </summary>
    Task<NLQueryResponse> ProcessQueryAsync(
        string naturalLanguageQuery,
        NLQueryContext context);

    /// <summary>
    /// Get conversation history
    /// </summary>
    Task<List<NLConversationMessage>> GetConversationHistoryAsync(
        string sessionId);

    /// <summary>
    /// Start new conversation session
    /// </summary>
    Task<string> StartNewSessionAsync();

    /// <summary>
    /// Clear conversation history
    /// </summary>
    Task ClearSessionAsync(string sessionId);

    /// <summary>
    /// Get suggested follow-up questions
    /// </summary>
    Task<List<string>> GetSuggestedQuestionsAsync(
        NLQueryResponse lastResponse);
}

/// <summary>
/// Natural Language Query Response
/// </summary>
public class NLQueryResponse
{
    public string ResponseId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Response content
    public string TextResponse { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty; // Query, Insight, Recommendation, Error

    // Data results
    public List<SqlQueryMetric>? QueryResults { get; set; }
    public List<PerformanceInsight>? Insights { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }

    // Visualization hints
    public string VisualizationType { get; set; } = string.Empty; // Chart, Table, Timeline, None
    public object? VisualizationData { get; set; }

    // AI metadata
    public double ConfidenceScore { get; set; } // 0-100
    public string IntentDetected { get; set; } = string.Empty;
    public List<string> EntitiesExtracted { get; set; } = new();

    // Follow-up suggestions
    public List<string> SuggestedQuestions { get; set; } = new();

    // Performance
    public double ProcessingTimeMs { get; set; }
}

/// <summary>
/// Context for natural language query processing
/// </summary>
public class NLQueryContext
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    // Available data scope
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-7);
    public DateTime EndDate { get; set; } = DateTime.Now;

    // Previous context
    public List<NLConversationMessage> ConversationHistory { get; set; } = new();

    // User preferences
    public string PreferredLanguage { get; set; } = "de-DE";
    public string PreferredTimeZone { get; set; } = "Central European Standard Time";

    // Performance data cache
    public Dictionary<string, object> CachedData { get; set; } = new();
}

/// <summary>
/// Conversation message
/// </summary>
public class NLConversationMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string Role { get; set; } = string.Empty; // User, Assistant
    public string Content { get; set; } = string.Empty;

    // Metadata
    public string? IntentDetected { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Performance Insight
/// </summary>
public class PerformanceInsight
{
    public string InsightId { get; set; } = Guid.NewGuid().ToString();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    // Insight content
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Info, Warning, Critical

    // Impact
    public string ImpactArea { get; set; } = string.Empty; // Queries, Batch, AOS, Database
    public double ImpactScore { get; set; } // 0-100

    // Recommendations
    public List<string> RecommendedActions { get; set; } = new();
    public double PotentialImprovement { get; set; } // Percentage

    // Supporting data
    public Dictionary<string, object> SupportingData { get; set; } = new();

    // AI metadata
    public double ConfidenceScore { get; set; } // 0-100
    public string Category { get; set; } = string.Empty; // Performance, Cost, Reliability, etc.
}
