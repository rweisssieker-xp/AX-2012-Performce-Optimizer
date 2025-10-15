using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// AI-powered query optimization service using OpenAI/Azure OpenAI
/// </summary>
public class AiQueryOptimizerService : IAiQueryOptimizerService
{
    private readonly ILogger<AiQueryOptimizerService> _logger;
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private string? _endpoint;
    private string _model = "gpt-4o";
    private bool _isAzure;

    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_endpoint);

    public AiQueryOptimizerService(ILogger<AiQueryOptimizerService> logger, HttpClient httpClient)
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

    public async Task<AiQueryAnalysisResult> AnalyzeQueryWithAiAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new AiQueryAnalysisResult
            {
                Success = false,
                ErrorMessage = "AI service is not configured. Please set API key in Settings."
            };
        }

        try
        {
            var prompt = BuildAnalysisPrompt(query);
            var response = await CallAiApiAsync(prompt, cancellationToken);

            return ParseAnalysisResponse(response, query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query with AI");
            return new AiQueryAnalysisResult
            {
                Success = false,
                ErrorMessage = $"AI analysis failed: {ex.Message}"
            };
        }
    }

    public async Task<string> GenerateOptimizedQueryAsync(string queryText, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return "AI service not configured.";
        }

        try
        {
            var prompt = $@"You are an expert SQL Server performance consultant specializing in Microsoft Dynamics AX 2012.

Given this SQL query:
```sql
{queryText}
```

Generate an optimized version of this query that:
1. Improves performance
2. Uses proper indexes
3. Follows SQL Server best practices
4. Is compatible with AX 2012 R3 CU13

Return ONLY the optimized SQL query, no explanations.";

            return await CallAiApiAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating optimized query");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> ExplainQueryPerformanceAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return "AI service not configured.";
        }

        try
        {
            var prompt = $@"You are an expert SQL Server DBA explaining performance issues to a system administrator.

Analyze this query and its metrics:

Query:
```sql
{query.QueryText}
```

Performance Metrics:
- Execution Count: {query.ExecutionCount:N0}
- Average CPU Time: {query.AvgCpuTimeMs:F2}ms
- Total CPU Time: {query.TotalCpuTimeMs:N0}ms
- Average Logical Reads: {query.AvgLogicalReads:N0}
- Average Physical Reads: {query.AvgPhysicalReads:N0}
- Average Elapsed Time: {query.AvgElapsedTimeMs:F2}ms

Explain in simple terms (2-3 sentences):
1. Why is this query slow?
2. What is the main performance bottleneck?
3. What should be done to fix it?";

            return await CallAiApiAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining query performance");
            return $"Error: {ex.Message}";
        }
    }

    private string BuildAnalysisPrompt(SqlQueryMetric query)
    {
        return $@"You are an expert SQL Server performance consultant specializing in Microsoft Dynamics AX 2012 R3 CU13 and SQL Server 2016.

Analyze this SQL query and its performance metrics:

Query:
```sql
{query.QueryText}
```

Performance Metrics:
- Execution Count: {query.ExecutionCount:N0}
- Average CPU Time: {query.AvgCpuTimeMs:F2}ms
- Total CPU Time: {query.TotalCpuTimeMs:N0}ms
- Average Logical Reads: {query.AvgLogicalReads:N0}
- Average Physical Reads: {query.AvgPhysicalReads:N0}
- Average Elapsed Time: {query.AvgElapsedTimeMs:F2}ms
- Last Execution: {query.LastExecutionTime}

Provide a JSON response with this structure:
{{
  ""performanceScore"": <0-100, where 100 is excellent>,
  ""explanation"": ""<brief explanation of performance issues>"",
  ""estimatedImprovement"": <percentage improvement possible>,
  ""suggestions"": [
    {{
      ""category"": ""<Index|QueryRewrite|Statistics|Caching|Configuration|TableDesign>"",
      ""severity"": ""<Info|Warning|Critical>"",
      ""title"": ""<short title>"",
      ""explanation"": ""<detailed explanation>"",
      ""codeExample"": ""<SQL code or null>"",
      ""reasoning"": ""<why this helps>"",
      ""estimatedImpact"": <0-100>,
      ""difficulty"": ""<Easy|Medium|Hard|RequiresArchitectureChange>""
    }}
  ],
  ""optimizedQuery"": ""<optimized SQL or null>""
}}

Focus on AX 2012 specific issues like:
- AX table naming conventions (dbo.TABLENAME)
- Common AX performance patterns
- AX Business Logic considerations
- Batch framework queries
- AIF/Data Import Export Framework queries

Return ONLY valid JSON, no markdown or additional text.";
    }

    private async Task<string> CallAiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are an expert SQL Server and Microsoft Dynamics AX 2012 performance consultant." },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 2000
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

    private AiQueryAnalysisResult ParseAnalysisResponse(string response, SqlQueryMetric query)
    {
        try
        {
            // Try to extract JSON from the response (in case AI added markdown)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var aiResponse = JsonSerializer.Deserialize<AiAnalysisResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

                if (aiResponse != null)
                {
                    return new AiQueryAnalysisResult
                    {
                        Success = true,
                        PerformanceScore = aiResponse.PerformanceScore,
                        Explanation = aiResponse.Explanation,
                        EstimatedImprovement = aiResponse.EstimatedImprovement,
                        OptimizedQuery = aiResponse.OptimizedQuery,
                        Suggestions = aiResponse.Suggestions?.Select(s => new AiOptimizationSuggestion
                        {
                            Category = s.Category,
                            Severity = s.Severity,
                            Title = s.Title,
                            Explanation = s.Explanation,
                            CodeExample = s.CodeExample,
                            Reasoning = s.Reasoning,
                            EstimatedImpact = s.EstimatedImpact,
                            Difficulty = s.Difficulty
                        }).ToList() ?? new()
                    };
                }
            }

            // Fallback: return raw response as explanation
            return new AiQueryAnalysisResult
            {
                Success = true,
                Explanation = response,
                PerformanceScore = 50,
                EstimatedImprovement = 20,
                Suggestions = new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing AI response");
            return new AiQueryAnalysisResult
            {
                Success = false,
                ErrorMessage = $"Failed to parse AI response: {ex.Message}"
            };
        }
    }

    #region DTOs for OpenAI API

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

    private class AiAnalysisResponse
    {
        [JsonPropertyName("performanceScore")]
        public int PerformanceScore { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonPropertyName("estimatedImprovement")]
        public double EstimatedImprovement { get; set; }

        [JsonPropertyName("suggestions")]
        public List<AiSuggestion>? Suggestions { get; set; }

        [JsonPropertyName("optimizedQuery")]
        public string? OptimizedQuery { get; set; }
    }

    private class AiSuggestion
    {
        [JsonPropertyName("category")]
        public SuggestionCategory Category { get; set; }

        [JsonPropertyName("severity")]
        public SuggestionSeverity Severity { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonPropertyName("codeExample")]
        public string? CodeExample { get; set; }

        [JsonPropertyName("reasoning")]
        public string? Reasoning { get; set; }

        [JsonPropertyName("estimatedImpact")]
        public double EstimatedImpact { get; set; }

        [JsonPropertyName("difficulty")]
        public ImplementationDifficulty Difficulty { get; set; }
    }

    #endregion

    public async Task<List<AiQueryAnalysisResult>> BatchAnalyzeQueriesAsync(List<SqlQueryMetric> queries, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return queries.Select(q => new AiQueryAnalysisResult
            {
                Success = false,
                ErrorMessage = "AI service is not configured."
            }).ToList();
        }

        try
        {
            var batchPrompt = BuildBatchAnalysisPrompt(queries);
            var response = await CallAiApiAsync(batchPrompt, cancellationToken);

            // For now, return individual analysis for each query
            // TODO: Parse batch response properly
            var results = new List<AiQueryAnalysisResult>();
            foreach (var query in queries)
            {
                results.Add(await AnalyzeQueryWithAiAsync(query, cancellationToken));
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch analysis");
            return queries.Select(q => new AiQueryAnalysisResult
            {
                Success = false,
                ErrorMessage = $"Batch analysis failed: {ex.Message}"
            }).ToList();
        }
    }

    public async Task<int> CalculateComplexityScoreAsync(string queryText, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            // Fallback to simple heuristic
            return CalculateComplexityHeuristic(queryText);
        }

        try
        {
            var prompt = $@"You are an expert SQL query analyzer.

Analyze the complexity of this SQL query and return a score from 0-100:
- 0-30: Simple (basic SELECT, single table, simple WHERE)
- 31-60: Medium (multiple JOINs, aggregations, GROUP BY)
- 61-85: Complex (subqueries, CTEs, multiple aggregations)
- 86-100: Very Complex (recursive CTEs, window functions, complex nested logic)

Query:
```sql
{queryText}
```

Return ONLY a number between 0 and 100, nothing else.";

            var response = await CallAiApiAsync(prompt, cancellationToken);
            if (int.TryParse(response.Trim(), out var score))
            {
                return Math.Clamp(score, 0, 100);
            }

            return CalculateComplexityHeuristic(queryText);
        }
        catch
        {
            return CalculateComplexityHeuristic(queryText);
        }
    }

    public async Task<List<IndexRecommendation>> GenerateIndexRecommendationsAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new List<IndexRecommendation>();
        }

        try
        {
            var prompt = $@"You are an expert SQL Server index design consultant.

Analyze this query and generate index recommendations:

Query:
```sql
{query.QueryText}
```

Performance Metrics:
- Average Logical Reads: {query.AvgLogicalReads:N0}
- Execution Count: {query.ExecutionCount:N0}

Return a JSON array of index recommendations with this structure:
[
  {{
    ""tableName"": ""dbo.TABLENAME"",
    ""indexName"": ""IX_TABLENAME_COLUMNS"",
    ""keyColumns"": [""COLUMN1"", ""COLUMN2""],
    ""includeColumns"": [""COLUMN3"", ""COLUMN4""],
    ""indexType"": ""Nonclustered"",
    ""reasoning"": ""Why this index helps"",
    ""createScript"": ""CREATE NONCLUSTERED INDEX..."",
    ""estimatedImpact"": 85
  }}
]

Return ONLY valid JSON array.";

            var response = await CallAiApiAsync(prompt, cancellationToken);
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var recommendations = JsonSerializer.Deserialize<List<IndexRecommendation>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return recommendations ?? new List<IndexRecommendation>();
            }

            return new List<IndexRecommendation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating index recommendations");
            return new List<IndexRecommendation>();
        }
    }

    public async Task<QueryCostEstimate> EstimateQueryCostAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new QueryCostEstimate
            {
                CpuCostMs = query.AvgCpuTimeMs,
                IoCost = query.AvgLogicalReads,
                EstimatedMonthlyCost = 0,
                Currency = "USD"
            };
        }

        try
        {
            var prompt = $@"You are a cloud cost analyst specializing in SQL Server.

Estimate the monthly cost for running this query given:

Query:
```sql
{query.QueryText}
```

Metrics:
- Executions per day: {query.ExecutionCount / 30} (estimated)
- Average CPU Time: {query.AvgCpuTimeMs:F2}ms
- Average Logical Reads: {query.AvgLogicalReads:N0}
- Average Physical Reads: {query.AvgPhysicalReads:N0}

Assume Azure SQL Database (S3 tier: $150/month base).

Return JSON:
{{
  ""cpuCostMs"": {query.AvgCpuTimeMs},
  ""ioCost"": {query.AvgLogicalReads},
  ""memoryCostMb"": <estimated MB>,
  ""estimatedDailyCost"": <USD>,
  ""estimatedMonthlyCost"": <USD>,
  ""currency"": ""USD"",
  ""breakdown"": ""<brief explanation>""
}}

Return ONLY valid JSON.";

            var response = await CallAiApiAsync(prompt, cancellationToken);
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var estimate = JsonSerializer.Deserialize<QueryCostEstimate>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return estimate ?? new QueryCostEstimate();
            }

            return new QueryCostEstimate();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating query cost");
            return new QueryCostEstimate
            {
                CpuCostMs = query.AvgCpuTimeMs,
                IoCost = query.AvgLogicalReads
            };
        }
    }

    public async Task<QueryComparisonResult> CompareQueriesAsync(string originalQuery, string optimizedQuery, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new QueryComparisonResult
            {
                OriginalQuery = originalQuery,
                OptimizedQuery = optimizedQuery,
                KeyDifferences = new List<string>(),
                EstimatedSpeedup = 1.0
            };
        }

        try
        {
            var prompt = $@"You are an expert SQL performance analyst.

Compare these two queries and identify performance differences:

Original Query:
```sql
{originalQuery}
```

Optimized Query:
```sql
{optimizedQuery}
```

Return JSON:
{{
  ""keyDifferences"": [""difference 1"", ""difference 2""],
  ""estimatedSpeedup"": 2.5,
  ""improvementAreas"": [""JOIN optimization"", ""Index usage""],
  ""summary"": ""Brief summary of improvements""
}}

Return ONLY valid JSON.";

            var response = await CallAiApiAsync(prompt, cancellationToken);
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var comparison = JsonSerializer.Deserialize<QueryComparisonResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (comparison != null)
                {
                    comparison.OriginalQuery = originalQuery;
                    comparison.OptimizedQuery = optimizedQuery;
                    return comparison;
                }
            }

            return new QueryComparisonResult
            {
                OriginalQuery = originalQuery,
                OptimizedQuery = optimizedQuery
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing queries");
            return new QueryComparisonResult
            {
                OriginalQuery = originalQuery,
                OptimizedQuery = optimizedQuery
            };
        }
    }

    public async Task<List<string>> GetAxSpecificInsightsAsync(SqlQueryMetric query, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new List<string>();
        }

        try
        {
            var prompt = $@"You are a Microsoft Dynamics AX 2012 R3 CU13 expert.

Analyze this query for AX-specific optimization opportunities:

Query:
```sql
{query.QueryText}
```

Focus on:
- AX table naming patterns (CUSTTABLE, VENDTABLE, etc.)
- AX index naming conventions
- DATAAREAID filtering
- AX business logic considerations
- Batch framework patterns
- Common AX performance anti-patterns

Return a JSON array of AX-specific insights:
[""insight 1"", ""insight 2"", ""insight 3""]

Return ONLY a valid JSON array of strings.";

            var response = await CallAiApiAsync(prompt, cancellationToken);
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var insights = JsonSerializer.Deserialize<List<string>>(json);
                return insights ?? new List<string>();
            }

            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AX-specific insights");
            return new List<string>();
        }
    }

    private int CalculateComplexityHeuristic(string queryText)
    {
        var upper = queryText.ToUpperInvariant();
        int score = 10; // Base score

        // Count various complexity factors
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bJOIN\b").Count * 8;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bLEFT\s+JOIN\b").Count * 5;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bWHERE\b").Count * 3;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bGROUP\s+BY\b").Count * 6;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bORDER\s+BY\b").Count * 4;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bHAVING\b").Count * 7;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bUNION\b").Count * 10;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bCASE\s+WHEN\b").Count * 5;
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\(\s*SELECT\b").Count * 12; // Subqueries
        score += System.Text.RegularExpressions.Regex.Matches(upper, @"\bWITH\b.*\bAS\b").Count * 15; // CTEs

        // Anti-patterns add complexity
        if (upper.Contains("SELECT *")) score += 5;
        if (upper.Contains("NOT IN")) score += 8;
        if (System.Text.RegularExpressions.Regex.IsMatch(upper, @"WHERE.*\bOR\b")) score += 6;

        return Math.Clamp(score, 0, 100);
    }

    private string BuildBatchAnalysisPrompt(List<SqlQueryMetric> queries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an expert SQL Server performance consultant.");
        sb.AppendLine();
        sb.AppendLine($"Analyze these {queries.Count} queries and prioritize by impact:");
        sb.AppendLine();

        for (int i = 0; i < queries.Take(10).Count(); i++)
        {
            var q = queries[i];
            sb.AppendLine($"Query {i + 1}:");
            sb.AppendLine($"```sql");
            sb.AppendLine(q.QueryText.Length > 500 ? q.QueryText.Substring(0, 500) + "..." : q.QueryText);
            sb.AppendLine($"```");
            sb.AppendLine($"CPU: {q.AvgCpuTimeMs:F2}ms, Reads: {q.AvgLogicalReads:N0}, Executions: {q.ExecutionCount:N0}");
            sb.AppendLine();
        }

        sb.AppendLine("Return a prioritized list of the most impactful optimizations.");
        return sb.ToString();
    }
}
