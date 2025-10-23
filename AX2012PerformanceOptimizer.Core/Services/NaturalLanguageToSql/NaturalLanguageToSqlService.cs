using AX2012PerformanceOptimizer.Core.Models.NaturalLanguageToSql;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AX2012PerformanceOptimizer.Core.Services.NaturalLanguageToSql;

/// <summary>
/// AI-powered natural language to SQL generation service using OpenAI/Azure OpenAI
/// </summary>
public class NaturalLanguageToSqlService : INaturalLanguageToSqlService
{
    private readonly ILogger<NaturalLanguageToSqlService> _logger;
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private string? _endpoint;
    private string _model = "gpt-4o";
    private bool _isAzure;

    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_endpoint);

    public NaturalLanguageToSqlService(ILogger<NaturalLanguageToSqlService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

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

    public async Task<SqlGenerationResult> GenerateSqlAsync(SqlGenerationRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new SqlGenerationResult
            {
                Success = false,
                ErrorMessage = "AI service is not configured. Please set API key in Settings."
            };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var prompt = BuildSqlGenerationPrompt(request);
            var response = await CallAiApiAsync(prompt, cancellationToken);

            var result = ParseSqlGenerationResponse(response, request);
            result.GenerationTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL from natural language");
            return new SqlGenerationResult
            {
                Success = false,
                ErrorMessage = $"SQL generation failed: {ex.Message}",
                GenerationTimeMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
    }

    public async Task<SqlValidationResult> ValidateSqlAsync(string sqlQuery, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            // Fallback to simple validation
            return PerformBasicValidation(sqlQuery);
        }

        try
        {
            var prompt = BuildValidationPrompt(sqlQuery);
            var response = await CallAiApiAsync(prompt, cancellationToken);

            return ParseValidationResponse(response, sqlQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating SQL");
            return new SqlValidationResult
            {
                IsValid = false,
                IsSafe = false,
                Message = $"Validation failed: {ex.Message}"
            };
        }
    }

    public async Task<SqlGenerationResult> RefineSqlAsync(string originalSql, string feedback, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return new SqlGenerationResult
            {
                Success = false,
                ErrorMessage = "AI service is not configured."
            };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var prompt = BuildRefinementPrompt(originalSql, feedback);
            var response = await CallAiApiAsync(prompt, cancellationToken);

            var result = ParseSqlGenerationResponse(response, new SqlGenerationRequest { NaturalLanguageQuery = feedback });
            result.GenerationTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refining SQL");
            return new SqlGenerationResult
            {
                Success = false,
                ErrorMessage = $"SQL refinement failed: {ex.Message}",
                GenerationTimeMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
    }

    public async Task<List<string>> GetExampleQueriesAsync(CancellationToken cancellationToken = default)
    {
        // Provide predefined examples that work well with AX 2012
        return new List<string>
        {
            "Show me all customers from Germany",
            "Find orders created in the last 7 days",
            "List the top 10 vendors by purchase amount",
            "Show inventory transactions for item ABC-123",
            "Get all customers with outstanding balance over 10000",
            "Find batch jobs that failed today",
            "Show sales orders with status 'Open'",
            "List products with low inventory levels",
            "Get customer payment history for the last month",
            "Find duplicate customer records"
        };
    }

    private string BuildSqlGenerationPrompt(SqlGenerationRequest request)
    {
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine("You are an expert SQL Server database consultant specializing in Microsoft Dynamics AX 2012 R3 CU13.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"Generate a SQL query for the following request in {request.Language}:");
        promptBuilder.AppendLine($"\"{request.NaturalLanguageQuery}\"");
        promptBuilder.AppendLine();

        if (!string.IsNullOrEmpty(request.DataAreaId))
        {
            promptBuilder.AppendLine($"DataAreaId: {request.DataAreaId}");
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("IMPORTANT AX 2012 GUIDELINES:");
        promptBuilder.AppendLine("1. All table names should be in UPPERCASE (e.g., CUSTTABLE, VENDTABLE)");
        promptBuilder.AppendLine("2. Always include DATAAREAID filter for multi-company filtering");
        promptBuilder.AppendLine("3. Use proper AX field naming conventions (ACCOUNTNUM, ITEMID, etc.)");
        promptBuilder.AppendLine("4. Consider using WITH (NOLOCK) for read-only queries");
        promptBuilder.AppendLine($"5. Generate only {(request.ReadOnlyMode ? "SELECT" : "SELECT/INSERT/UPDATE/DELETE")} queries");
        promptBuilder.AppendLine();

        if (request.TableHints.Any())
        {
            promptBuilder.AppendLine($"Suggested tables to consider: {string.Join(", ", request.TableHints)}");
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("Common AX 2012 Tables:");
        promptBuilder.AppendLine("- CUSTTABLE (customers), VENDTABLE (vendors)");
        promptBuilder.AppendLine("- SALESTABLE/SALESLINE (sales orders)");
        promptBuilder.AppendLine("- PURCHTABLE/PURCHLINE (purchase orders)");
        promptBuilder.AppendLine("- INVENTTABLE/INVENTTRANS (inventory)");
        promptBuilder.AppendLine("- CUSTTRANS/VENDTRANS (transactions)");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("Return a JSON response with this exact structure:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"sql\": \"<the SQL query>\",");
        promptBuilder.AppendLine("  \"explanation\": \"<brief explanation of what the query does>\",");
        promptBuilder.AppendLine("  \"confidence\": <0-100 confidence score>,");
        promptBuilder.AppendLine("  \"tables\": [\"TABLE1\", \"TABLE2\"],");
        promptBuilder.AppendLine("  \"columns\": [\"COLUMN1\", \"COLUMN2\"],");
        promptBuilder.AppendLine("  \"warnings\": [\"warning 1\", \"warning 2\"],");
        promptBuilder.AppendLine("  \"suggestions\": [\"optimization 1\", \"optimization 2\"],");
        promptBuilder.AppendLine("  \"alternatives\": [\"alternative query 1\"]");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Return ONLY valid JSON, no markdown or additional text.");

        return promptBuilder.ToString();
    }

    private string BuildValidationPrompt(string sqlQuery)
    {
        return $@"You are a SQL Server security and performance expert.

Validate this SQL query:
```sql
{sqlQuery}
```

Check for:
1. Syntax validity
2. Security concerns (SQL injection, dangerous operations)
3. Performance concerns (missing WHERE, SELECT *, etc.)
4. Whether it's read-only (SELECT only) or modifies data

Return JSON:
{{
  ""isValid"": true/false,
  ""isSafe"": true/false,
  ""commandType"": ""SELECT|INSERT|UPDATE|DELETE|DDL|DCL"",
  ""errors"": [""error 1""],
  ""warnings"": [""warning 1""],
  ""securityConcerns"": [""concern 1""],
  ""performanceConcerns"": [""concern 1""],
  ""complexityScore"": <0-100>,
  ""message"": ""summary message""
}}

Return ONLY valid JSON.";
    }

    private string BuildRefinementPrompt(string originalSql, string feedback)
    {
        return $@"You are an expert SQL consultant.

Original SQL:
```sql
{originalSql}
```

User feedback: ""{feedback}""

Refine the SQL based on the feedback. Maintain the original intent but address the feedback.

Return JSON with same structure:
{{
  ""sql"": ""<refined SQL>"",
  ""explanation"": ""<what changed>"",
  ""confidence"": <0-100>,
  ""tables"": [],
  ""columns"": [],
  ""warnings"": [],
  ""suggestions"": [],
  ""alternatives"": []
}}

Return ONLY valid JSON.";
    }

    private async Task<string> CallAiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        object requestBody;

        if (_model.StartsWith("o1-", StringComparison.OrdinalIgnoreCase))
        {
            requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };
        }
        else
        {
            requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are an expert SQL Server and Microsoft Dynamics AX 2012 consultant specializing in query generation." },
                    new { role = "user", content = prompt }
                }
            };
        }

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var url = _isAzure
            ? $"{_endpoint}/openai/deployments/{_model}/chat/completions?api-version=2024-02-15-preview"
            : $"{_endpoint}/v1/chat/completions";

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("AI API call failed with status {StatusCode}. Response: {ErrorBody}",
                response.StatusCode, errorBody);
            throw new HttpRequestException($"AI API returned {response.StatusCode}: {errorBody}");
        }

        var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken);
        return responseBody?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response";
    }

    private SqlGenerationResult ParseSqlGenerationResponse(string response, SqlGenerationRequest request)
    {
        try
        {
            // Extract JSON from response (in case AI added markdown)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var aiResponse = JsonSerializer.Deserialize<SqlGenerationAiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (aiResponse != null)
                {
                    // Perform basic validation
                    var validation = PerformBasicValidation(aiResponse.Sql);

                    return new SqlGenerationResult
                    {
                        Success = true,
                        GeneratedSql = aiResponse.Sql,
                        Explanation = aiResponse.Explanation,
                        ConfidenceScore = aiResponse.Confidence,
                        TablesUsed = aiResponse.Tables ?? new List<string>(),
                        ColumnsUsed = aiResponse.Columns ?? new List<string>(),
                        Warnings = aiResponse.Warnings ?? new List<string>(),
                        OptimizationSuggestions = aiResponse.Suggestions ?? new List<string>(),
                        AlternativeQueries = aiResponse.Alternatives ?? new List<string>(),
                        IsSafeToExecute = validation.IsSafe && validation.IsValid
                    };
                }
            }

            // Fallback: return raw response
            return new SqlGenerationResult
            {
                Success = true,
                GeneratedSql = ExtractSqlFromText(response),
                Explanation = "Response received but could not parse JSON format",
                ConfidenceScore = 50,
                IsSafeToExecute = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SQL generation response");
            return new SqlGenerationResult
            {
                Success = false,
                ErrorMessage = $"Failed to parse response: {ex.Message}"
            };
        }
    }

    private SqlValidationResult ParseValidationResponse(string response, string sqlQuery)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                var validationResponse = JsonSerializer.Deserialize<SqlValidationAiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (validationResponse != null)
                {
                    return new SqlValidationResult
                    {
                        IsValid = validationResponse.IsValid,
                        IsSafe = validationResponse.IsSafe,
                        CommandType = validationResponse.CommandType,
                        Errors = validationResponse.Errors ?? new List<string>(),
                        Warnings = validationResponse.Warnings ?? new List<string>(),
                        SecurityConcerns = validationResponse.SecurityConcerns ?? new List<string>(),
                        PerformanceConcerns = validationResponse.PerformanceConcerns ?? new List<string>(),
                        ComplexityScore = validationResponse.ComplexityScore,
                        Message = validationResponse.Message
                    };
                }
            }

            return PerformBasicValidation(sqlQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing validation response");
            return PerformBasicValidation(sqlQuery);
        }
    }

    private SqlValidationResult PerformBasicValidation(string sqlQuery)
    {
        var upper = sqlQuery.ToUpperInvariant().Trim();
        var result = new SqlValidationResult
        {
            IsValid = true,
            ComplexityScore = 50
        };

        // Detect command type
        if (upper.StartsWith("SELECT"))
            result.CommandType = "SELECT";
        else if (upper.StartsWith("INSERT"))
            result.CommandType = "INSERT";
        else if (upper.StartsWith("UPDATE"))
            result.CommandType = "UPDATE";
        else if (upper.StartsWith("DELETE"))
            result.CommandType = "DELETE";
        else if (upper.StartsWith("CREATE") || upper.StartsWith("ALTER") || upper.StartsWith("DROP"))
            result.CommandType = "DDL";
        else
            result.CommandType = "UNKNOWN";

        // Safety check: Only SELECT is safe
        result.IsSafe = result.CommandType == "SELECT";

        // Check for dangerous patterns
        if (Regex.IsMatch(upper, @"\bDROP\b|\bTRUNCATE\b|\bEXEC\b|\bEXECUTE\b"))
        {
            result.IsSafe = false;
            result.SecurityConcerns.Add("Query contains potentially dangerous operations");
        }

        // Performance concerns
        if (upper.Contains("SELECT *"))
        {
            result.PerformanceConcerns.Add("SELECT * may retrieve unnecessary columns");
        }

        if (!upper.Contains("WHERE") && result.CommandType == "SELECT" && upper.Contains("FROM"))
        {
            result.PerformanceConcerns.Add("Missing WHERE clause - may scan entire table");
        }

        if (upper.Contains("NOT IN"))
        {
            result.PerformanceConcerns.Add("NOT IN can be slow - consider NOT EXISTS instead");
        }

        result.Message = result.IsSafe && result.IsValid
            ? "Query appears valid and safe"
            : "Query has validation concerns";

        return result;
    }

    private string ExtractSqlFromText(string text)
    {
        // Try to extract SQL from markdown code blocks
        var sqlMatch = Regex.Match(text, @"```sql\s*\n(.*?)\n```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (sqlMatch.Success)
        {
            return sqlMatch.Groups[1].Value.Trim();
        }

        // Try generic code blocks
        sqlMatch = Regex.Match(text, @"```\s*\n(.*?)\n```", RegexOptions.Singleline);
        if (sqlMatch.Success)
        {
            return sqlMatch.Groups[1].Value.Trim();
        }

        // Return as is
        return text.Trim();
    }

    #region DTOs for AI API

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

    private class SqlGenerationAiResponse
    {
        [JsonPropertyName("sql")]
        public string Sql { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("tables")]
        public List<string>? Tables { get; set; }

        [JsonPropertyName("columns")]
        public List<string>? Columns { get; set; }

        [JsonPropertyName("warnings")]
        public List<string>? Warnings { get; set; }

        [JsonPropertyName("suggestions")]
        public List<string>? Suggestions { get; set; }

        [JsonPropertyName("alternatives")]
        public List<string>? Alternatives { get; set; }
    }

    private class SqlValidationAiResponse
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("isSafe")]
        public bool IsSafe { get; set; }

        [JsonPropertyName("commandType")]
        public string CommandType { get; set; } = string.Empty;

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("warnings")]
        public List<string>? Warnings { get; set; }

        [JsonPropertyName("securityConcerns")]
        public List<string>? SecurityConcerns { get; set; }

        [JsonPropertyName("performanceConcerns")]
        public List<string>? PerformanceConcerns { get; set; }

        [JsonPropertyName("complexityScore")]
        public double ComplexityScore { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
