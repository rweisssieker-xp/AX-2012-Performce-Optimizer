using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// AI-powered query explainer that provides natural language explanations
/// </summary>
public class AiQueryExplainerService : IAiQueryExplainerService
{
    private readonly ILogger<AiQueryExplainerService> _logger;
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private string? _endpoint;
    private string _model = "gpt-4o";
    private bool _isAzure;

    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_endpoint);

    public AiQueryExplainerService(ILogger<AiQueryExplainerService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    /// <summary>
    /// Configure the AI service with API credentials
    /// </summary>
    public void Configure(string apiKey, string endpoint, string model = "gpt-4o", bool isAzure = false)
    {
        _apiKey = apiKey;
        _endpoint = endpoint;
        _model = model;
        _isAzure = isAzure;

        _httpClient.DefaultRequestHeaders.Clear();
        if (isAzure)
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
    }

    public async Task<QueryExplanation> ExplainQueryPerformanceAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return CreateFallbackExplanation(query);
        }

        try
        {
            var prompt = BuildExplanationPrompt(query);
            var response = await CallAiApiAsync(prompt, cancellationToken);
            return ParseExplanationResponse(response, query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query explanation");
            return CreateFallbackExplanation(query);
        }
    }

    public async Task<ConversationResponse> AskQuestionAsync(string question, SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new ConversationResponse
            {
                Question = question,
                Answer = "AI service is not configured. Please set API key in Settings.",
                FollowUpSuggestions = new()
            };
        }

        try
        {
            var prompt = BuildConversationPrompt(question, query);
            var response = await CallAiApiAsync(prompt, cancellationToken);

            return new ConversationResponse
            {
                Question = question,
                Answer = response,
                FollowUpSuggestions = await GenerateFollowUpQuestionsAsync(question, response, query),
                Timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in conversation");
            return new ConversationResponse
            {
                Question = question,
                Answer = $"Error: {ex.Message}",
                FollowUpSuggestions = new()
            };
        }
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(SqlQueryMetric query)
    {
        var questions = new List<string>
        {
            "Why is this query slow?",
            "What's the main performance bottleneck?",
            "How can I optimize this?",
            "What indexes should I add?",
            "Is this query well-written?",
            "What's the estimated improvement?",
            "Will this query scale?",
            "Are there any anti-patterns?"
        };

        // Add context-specific questions
        if (query.AvgLogicalReads > 100000)
        {
            questions.Insert(2, "Why so many logical reads?");
        }

        if (query.ExecutionCount > 10000)
        {
            questions.Insert(2, "Should this be cached?");
        }

        if (query.AvgElapsedTimeMs > 1000)
        {
            questions.Insert(2, "Why does it take so long?");
        }

        return await Task.FromResult(questions.Take(8).ToList());
    }

    public async Task<string> GetQuickSummaryAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return GenerateFallbackSummary(query);
        }

        try
        {
            var prompt = $@"You are an expert SQL performance analyst. Provide a ONE SENTENCE summary of this query's performance issue.

Query Metrics:
- Execution Count: {query.ExecutionCount:N0}
- Avg Elapsed Time: {query.AvgElapsedTimeMs:F2}ms
- Avg CPU Time: {query.AvgCpuTimeMs:F2}ms
- Avg Logical Reads: {query.AvgLogicalReads:N0}
- Avg Physical Reads: {query.AvgPhysicalReads:N0}

Query:
{query.QueryText.Substring(0, Math.Min(500, query.QueryText.Length))}

Return ONLY one clear, concise sentence explaining the main issue.";

            return await CallAiApiAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quick summary");
            return GenerateFallbackSummary(query);
        }
    }

    private string BuildExplanationPrompt(SqlQueryMetric query)
    {
        return $@"You are a friendly SQL performance expert explaining to a system administrator why their query is slow.

Analyze this query and its metrics:

Query:
```sql
{query.QueryText}
```

Performance Metrics:
- Execution Count: {query.ExecutionCount:N0}
- Average Elapsed Time: {query.AvgElapsedTimeMs:F2}ms
- Average CPU Time: {query.AvgCpuTimeMs:F2}ms
- Average Logical Reads: {query.AvgLogicalReads:N0}
- Average Physical Reads: {query.AvgPhysicalReads:N0}
- Last Execution: {query.LastExecutionTime}

Provide a comprehensive explanation in JSON format:
{{
  ""summary"": ""One clear sentence summarizing the main issue"",
  ""performanceRating"": ""Excellent|Good|Fair|Poor|Critical"",
  ""performanceScore"": <0-100>,
  ""problems"": [
    {{
      ""severity"": ""Critical|High|Medium|Low"",
      ""icon"": ""🔴|🟡|🟢"",
      ""title"": ""Short problem title"",
      ""explanation"": ""Clear explanation in simple terms"",
      ""impact"": ""How much this affects performance"",
      ""why"": ""Why this is a problem"",
      ""analogy"": ""Real-world analogy to help understand""
    }}
  ],
  ""positiveAspects"": [""Things the query does well""],
  ""recommendations"": [
    {{
      ""priority"": ""Critical|High|Medium|Low"",
      ""icon"": ""💡"",
      ""title"": ""What to do"",
      ""action"": ""Specific action to take"",
      ""benefit"": ""Why this will help"",
      ""estimatedImprovement"": <percentage 0-100>,
      ""estimatedTimeMinutes"": <time to implement>,
      ""riskLevel"": ""Safe|Low|Medium|High"",
      ""script"": ""SQL script or null"",
      ""steps"": [""Step 1"", ""Step 2""]
    }}
  ],
  ""estimatedTotalImprovement"": <total percentage improvement if all recommendations applied>,
  ""roiSummary"": ""Brief ROI summary"",
  ""technicalDetails"": ""Advanced technical details for DBAs""
}}

Focus on:
1. Use simple, non-technical language
2. Include helpful analogies
3. Provide actionable recommendations
4. Prioritize by impact
5. Consider AX 2012 specific patterns

Return ONLY valid JSON, no markdown.";
    }

    private string BuildConversationPrompt(string question, SqlQueryMetric query)
    {
        return $@"You are a friendly SQL performance expert helping a system administrator understand their query.

User's question: {question}

Context - Query metrics:
- Execution Count: {query.ExecutionCount:N0}
- Avg Elapsed Time: {query.AvgElapsedTimeMs:F2}ms
- Avg CPU Time: {query.AvgCpuTimeMs:F2}ms
- Avg Logical Reads: {query.AvgLogicalReads:N0}

Query (first 500 chars):
{query.QueryText.Substring(0, Math.Min(500, query.QueryText.Length))}

Answer the user's question clearly and concisely:
1. Use simple, non-technical language
2. Be specific and actionable
3. Include examples if helpful
4. Keep it friendly and encouraging

Return ONLY the answer text, no JSON or markdown formatting.";
    }

    private QueryExplanation ParseExplanationResponse(string response, SqlQueryMetric query)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var explanation = JsonSerializer.Deserialize<QueryExplanation>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

                if (explanation != null)
                {
                    explanation.GeneratedAt = DateTime.Now;
                    return explanation;
                }
            }

            // Fallback if parsing fails
            return CreateFallbackExplanation(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing explanation response");
            return CreateFallbackExplanation(query);
        }
    }

    private async Task<List<string>> GenerateFollowUpQuestionsAsync(string originalQuestion, string answer, SqlQueryMetric query)
    {
        // Generate context-aware follow-up questions
        var followUps = new List<string>();

        if (answer.Contains("index", StringComparison.OrdinalIgnoreCase))
        {
            followUps.Add("What's the SQL script to create the index?");
            followUps.Add("How long will the index take to build?");
        }

        if (answer.Contains("join", StringComparison.OrdinalIgnoreCase))
        {
            followUps.Add("Can I rewrite the JOINs?");
            followUps.Add("What's the optimal JOIN order?");
        }

        if (answer.Contains("improvement", StringComparison.OrdinalIgnoreCase) || answer.Contains("faster", StringComparison.OrdinalIgnoreCase))
        {
            followUps.Add("What's the ROI of this optimization?");
            followUps.Add("How can I test the improvement?");
        }

        // Default follow-ups
        if (followUps.Count < 3)
        {
            followUps.AddRange(new[]
            {
                "Can you show me an example?",
                "What are the risks?",
                "How do I test this safely?"
            });
        }

        return await Task.FromResult(followUps.Take(4).ToList());
    }

    private QueryExplanation CreateFallbackExplanation(SqlQueryMetric query)
    {
        var explanation = new QueryExplanation
        {
            Summary = GenerateFallbackSummary(query),
            PerformanceRating = GetPerformanceRating(query),
            PerformanceScore = CalculatePerformanceScore(query),
            GeneratedAt = DateTime.Now
        };

        // Analyze problems heuristically
        if (query.AvgLogicalReads > 100000)
        {
            explanation.Problems.Add(new PerformanceProblem
            {
                Severity = "High",
                Icon = "🔴",
                Title = "Excessive Logical Reads",
                Explanation = $"This query reads {query.AvgLogicalReads:N0} pages on average, which is very high.",
                Impact = "Causes high I/O and slow performance",
                Why = "The query likely scans large tables without proper indexes",
                Analogy = "It's like reading an entire book to find one sentence instead of using the index"
            });
        }

        if (query.AvgElapsedTimeMs > 1000)
        {
            explanation.Problems.Add(new PerformanceProblem
            {
                Severity = "High",
                Icon = "🔴",
                Title = "Slow Query Execution",
                Explanation = $"Average execution time is {query.AvgElapsedTimeMs:F0}ms, which is quite slow.",
                Impact = "Users experience delays",
                Why = "Query takes too long to retrieve data",
                Analogy = "Like waiting in a long queue at a store"
            });
        }

        if (query.ExecutionCount > 10000)
        {
            explanation.Problems.Add(new PerformanceProblem
            {
                Severity = "Medium",
                Icon = "🟡",
                Title = "High Execution Frequency",
                Explanation = $"This query runs {query.ExecutionCount:N0} times, making it a hot spot.",
                Impact = "Even small improvements have big impact",
                Why = "Frequent execution multiplies any performance issue",
                Analogy = "A small leak that drips thousands of times becomes a flood"
            });
        }

        // Add recommendations
        if (query.AvgLogicalReads > 100000)
        {
            explanation.Recommendations.Add(new ExplainerRecommendation
            {
                Priority = "High",
                Icon = "💡",
                Title = "Add Missing Indexes",
                Action = "Create indexes on frequently filtered columns",
                Benefit = "Reduces table scans and speeds up data retrieval",
                EstimatedImprovement = 70,
                EstimatedTimeMinutes = 5,
                RiskLevel = "Low",
                Steps = new List<string>
                {
                    "Identify columns used in WHERE clauses",
                    "Check for existing indexes",
                    "Create non-clustered indexes on filter columns",
                    "Monitor query performance after creation"
                }
            });
        }

        explanation.EstimatedTotalImprovement = explanation.Recommendations.Sum(r => r.EstimatedImprovement);
        explanation.ROISummary = $"Implementing these fixes could improve performance by ~{explanation.EstimatedTotalImprovement}%";

        return explanation;
    }

    private string GenerateFallbackSummary(SqlQueryMetric query)
    {
        if (query.AvgElapsedTimeMs > 5000)
            return "This query is very slow and needs immediate optimization.";

        if (query.AvgLogicalReads > 100000)
            return "High logical reads indicate missing or ineffective indexes.";

        if (query.ExecutionCount > 10000 && query.AvgElapsedTimeMs > 100)
            return "Frequent execution combined with slow performance creates a bottleneck.";

        if (query.AvgElapsedTimeMs < 50)
            return "Query performance is good, no critical issues detected.";

        return "This query could benefit from optimization.";
    }

    private string GetPerformanceRating(SqlQueryMetric query)
    {
        var score = CalculatePerformanceScore(query);

        return score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Poor",
            _ => "Critical"
        };
    }

    private int CalculatePerformanceScore(SqlQueryMetric query)
    {
        int score = 100;

        // Penalize slow queries
        if (query.AvgElapsedTimeMs > 5000) score -= 40;
        else if (query.AvgElapsedTimeMs > 1000) score -= 25;
        else if (query.AvgElapsedTimeMs > 500) score -= 15;
        else if (query.AvgElapsedTimeMs > 100) score -= 5;

        // Penalize high I/O
        if (query.AvgLogicalReads > 1000000) score -= 30;
        else if (query.AvgLogicalReads > 100000) score -= 20;
        else if (query.AvgLogicalReads > 10000) score -= 10;

        // Penalize physical reads (disk I/O)
        if (query.AvgPhysicalReads > 10000) score -= 15;
        else if (query.AvgPhysicalReads > 1000) score -= 10;

        return Math.Max(0, score);
    }

    private async Task<string> CallAiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a friendly SQL performance expert who explains complex database concepts in simple terms." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7, // Slightly higher for more natural explanations
            max_tokens = 3000
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var url = _isAzure
            ? $"{_endpoint}/openai/deployments/{_model}/chat/completions?api-version=2024-02-15-preview"
            : $"{_endpoint}/v1/chat/completions";

        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken);
        return responseBody?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response";
    }

    #region DTOs

    private class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    #endregion
}
