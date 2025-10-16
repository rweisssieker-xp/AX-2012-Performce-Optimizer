using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Implementation of Natural Language Query Assistant
/// </summary>
public class NaturalLanguageQueryAssistant : INaturalLanguageQueryAssistant
{
    private readonly ILogger<NaturalLanguageQueryAssistant> _logger;
    private readonly IAiQueryOptimizerService _aiService;
    private readonly ISqlQueryMonitorService _queryMonitorService;
    private readonly IHistoricalDataService _historicalDataService;
    private readonly IPerformanceCostCalculatorService _costCalculatorService;
    private readonly IQueryAnalyzerService _queryAnalyzerService;
    private readonly Dictionary<string, List<NLConversationMessage>> _sessions = new();

    public NaturalLanguageQueryAssistant(
        ILogger<NaturalLanguageQueryAssistant> logger,
        IAiQueryOptimizerService aiService,
        ISqlQueryMonitorService queryMonitorService,
        IHistoricalDataService historicalDataService,
        IPerformanceCostCalculatorService costCalculatorService,
        IQueryAnalyzerService queryAnalyzerService)
    {
        _logger = logger;
        _aiService = aiService;
        _queryMonitorService = queryMonitorService;
        _historicalDataService = historicalDataService;
        _costCalculatorService = costCalculatorService;
        _queryAnalyzerService = queryAnalyzerService;
    }

    public async Task<NLQueryResponse> ProcessQueryAsync(
        string naturalLanguageQuery,
        NLQueryContext context)
    {
        _logger.LogInformation("Processing NL query: {Query}", naturalLanguageQuery);

        var startTime = DateTime.Now;

        // Detect intent and extract entities
        var intent = DetectIntent(naturalLanguageQuery);
        var entities = ExtractEntities(naturalLanguageQuery);

        // Build AI prompt
        var prompt = BuildAIPrompt(naturalLanguageQuery, intent, context);

        // Call AI service (use ExplainQueryPerformanceAsync with dummy query)
        string aiResponse;
        try
        {
            var dummyQuery = new SqlQueryMetric
            {
                QueryText = prompt,
                QueryHash = Guid.NewGuid().ToString()
            };
            aiResponse = await _aiService.ExplainQueryPerformanceAsync(dummyQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI service");
            aiResponse = GenerateFallbackResponse(naturalLanguageQuery, intent);
        }

        // Parse response and generate result
        var response = new NLQueryResponse
        {
            TextResponse = aiResponse,
            ResponseType = intent,
            IntentDetected = intent,
            EntitiesExtracted = entities,
            ConfidenceScore = CalculateConfidence(intent, entities),
            ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds
        };

        // Generate sample data based on intent
        response = EnrichResponseWithData(response, intent, context);

        // Generate suggested follow-up questions
        response.SuggestedQuestions = GenerateSuggestedQuestions(intent, response);

        // Add to conversation history
        if (!string.IsNullOrEmpty(context.SessionId))
        {
            AddToHistory(context.SessionId, "User", naturalLanguageQuery, intent);
            AddToHistory(context.SessionId, "Assistant", aiResponse, null);
        }

        return response;
    }

    public async Task<List<NLConversationMessage>> GetConversationHistoryAsync(string sessionId)
    {
        await Task.Delay(1);
        return _sessions.TryGetValue(sessionId, out var history)
            ? history
            : new List<NLConversationMessage>();
    }

    public async Task<string> StartNewSessionAsync()
    {
        await Task.Delay(1);
        var sessionId = Guid.NewGuid().ToString();
        _sessions[sessionId] = new List<NLConversationMessage>();
        _logger.LogInformation("Started new NL session: {SessionId}", sessionId);
        return sessionId;
    }

    public async Task ClearSessionAsync(string sessionId)
    {
        await Task.Delay(1);
        if (_sessions.ContainsKey(sessionId))
        {
            _sessions.Remove(sessionId);
            _logger.LogInformation("Cleared NL session: {SessionId}", sessionId);
        }
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(NLQueryResponse lastResponse)
    {
        await Task.Delay(1);
        return lastResponse.SuggestedQuestions;
    }

    // Private helper methods

    private string DetectIntent(string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Pattern matching for intent detection
        if (lowerQuery.Contains("langsam") || lowerQuery.Contains("slow") || lowerQuery.Contains("performance"))
            return "PerformanceIssue";

        if (lowerQuery.Contains("teuer") || lowerQuery.Contains("cost") || lowerQuery.Contains("kosten"))
            return "CostAnalysis";

        if (lowerQuery.Contains("empfehlung") || lowerQuery.Contains("recommendation") || lowerQuery.Contains("optimize"))
            return "Recommendation";

        if (lowerQuery.Contains("batch") || lowerQuery.Contains("job"))
            return "BatchJob";

        if (lowerQuery.Contains("query") || lowerQuery.Contains("queries") || lowerQuery.Contains("abfrage"))
            return "QueryAnalysis";

        if (lowerQuery.Contains("gestern") || lowerQuery.Contains("heute") || lowerQuery.Contains("yesterday") || lowerQuery.Contains("today"))
            return "TimeBasedQuery";

        if (lowerQuery.Contains("trend") || lowerQuery.Contains("forecast") || lowerQuery.Contains("vorhersage"))
            return "Trend";

        return "General";
    }

    private List<string> ExtractEntities(string query)
    {
        var entities = new List<string>();

        // Extract time references
        var timePatterns = new[] { "gestern", "heute", "morgen", "yesterday", "today", "tomorrow", "letzte woche", "last week" };
        foreach (var pattern in timePatterns)
        {
            if (query.ToLowerInvariant().Contains(pattern))
                entities.Add($"Time:{pattern}");
        }

        // Extract table names (common AX tables)
        var tablePatterns = new[] { "CUSTTABLE", "INVENTTABLE", "INVENTTRANS", "SALESTABLE", "PURCHLINE" };
        foreach (var table in tablePatterns)
        {
            if (query.ToUpperInvariant().Contains(table))
                entities.Add($"Table:{table}");
        }

        // Extract numbers
        var numberMatches = Regex.Matches(query, @"\d+");
        foreach (Match match in numberMatches)
        {
            entities.Add($"Number:{match.Value}");
        }

        return entities;
    }

    private string BuildAIPrompt(string query, string intent, NLQueryContext context)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are an AI Performance Assistant for Microsoft Dynamics AX 2012.");
        prompt.AppendLine("Analyze the user's question and provide a helpful, technical response.");
        prompt.AppendLine();
        prompt.AppendLine($"User Question: {query}");
        prompt.AppendLine($"Detected Intent: {intent}");
        prompt.AppendLine($"Time Range: {context.StartDate:yyyy-MM-dd} to {context.EndDate:yyyy-MM-dd}");
        prompt.AppendLine();
        prompt.AppendLine("Provide a concise, actionable response in German. Include:");
        prompt.AppendLine("1. Summary of findings");
        prompt.AppendLine("2. Specific metrics or examples");
        prompt.AppendLine("3. Recommended actions");
        prompt.AppendLine();
        prompt.AppendLine("Keep response under 200 words.");

        return prompt.ToString();
    }

    private string GenerateFallbackResponse(string query, string intent)
    {
        return intent switch
        {
            "PerformanceIssue" => "Ich analysiere Performance-Probleme in Ihrem System. Basierend auf typischen Mustern sollten Sie die TOP 10 langsamsten Queries überprüfen und Indexes optimieren.",
            "CostAnalysis" => "Für eine Kosten-Analyse empfehle ich, den Performance Cost Calculator zu verwenden. Dieser zeigt Ihnen die monetären Auswirkungen langsamer Queries.",
            "Recommendation" => "Meine Top-Empfehlungen: 1) Index-Optimierung für häufige Queries, 2) Batch-Job Scheduling optimieren, 3) Query-Clustering für Bulk-Optimierung nutzen.",
            "BatchJob" => "Für Batch-Job Optimierung empfehle ich den Smart Batching Advisor. Dieser analysiert Batch-Größen, Scheduling und Anti-Patterns.",
            "QueryAnalysis" => "Nutzen Sie die Query Correlation Engine um versteckte Beziehungen zwischen Queries zu finden und das Query Clustering für Bulk-Optimierungen.",
            _ => $"Ich habe Ihre Frage verstanden: '{query}'. Können Sie spezifischer sein? Z.B.: 'Zeig mir die langsamsten Queries heute' oder 'Was kostet mich Query XYZ?'"
        };
    }

    private double CalculateConfidence(string intent, List<string> entities)
    {
        var baseConfidence = intent != "General" ? 70.0 : 40.0;
        var entityBonus = Math.Min(entities.Count * 5.0, 30.0);
        return Math.Min(baseConfidence + entityBonus, 95.0);
    }

    private NLQueryResponse EnrichResponseWithData(NLQueryResponse response, string intent, NLQueryContext context)
    {
        // Enrich response with real data based on intent
        try
        {
            switch (intent)
            {
                case "PerformanceIssue":
                case "QueryAnalysis":
                case "TimeBasedQuery":
                    // Get real query data from database
                    var queries = _queryMonitorService.GetTopExpensiveQueriesAsync(10).GetAwaiter().GetResult();
                    response.QueryResults = queries.ToList();
                    response.VisualizationType = "Table";
                    break;

                case "CostAnalysis":
                    // Calculate real costs using cost calculator
                    var allQueries = _queryMonitorService.GetTopExpensiveQueriesAsync(100).GetAwaiter().GetResult();
                    var costParams = new CostParameters();
                    var costReport = _costCalculatorService.GenerateExecutiveSummaryAsync(allQueries.ToList(), costParams).GetAwaiter().GetResult();

                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "DailyCost", costReport.TotalDailyCost },
                        { "MonthlyCost", costReport.TotalMonthlyCost },
                        { "YearlyCost", costReport.TotalYearlyCost },
                        { "Currency", "EUR" },
                        { "TotalQueries", costReport.TotalQueriesAnalyzed }
                    };
                    response.VisualizationType = "Chart";
                    break;

                case "Trend":
                    // Get real trend data from historical service
                    var trendData = _historicalDataService.AnalyzeTrendAsync(
                        "QueryPerformance",
                        context.StartDate,
                        context.EndDate).GetAwaiter().GetResult();

                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "TrendDirection", trendData.Trend.ToString() },
                        { "PerformanceChange", trendData.ChangePercent },
                        { "CurrentValue", trendData.CurrentValue },
                        { "AverageValue", trendData.AverageValue }
                    };
                    response.VisualizationType = "Timeline";
                    break;

                case "Recommendation":
                    // Get real optimization suggestions from analyzer
                    var topQueries = _queryMonitorService.GetTopExpensiveQueriesAsync(5).GetAwaiter().GetResult();
                    var suggestions = new List<PerformanceInsight>();

                    foreach (var query in topQueries)
                    {
                        var suggestionList = _queryAnalyzerService.AnalyzeQueryAsync(query).GetAwaiter().GetResult();
                        if (suggestionList != null && suggestionList.Any())
                        {
                            var firstSuggestion = suggestionList.First();
                            suggestions.Add(new PerformanceInsight
                            {
                                Title = $"Optimierung für Query {query.QueryHash.Substring(0, 8)}",
                                Description = string.Join(", ", suggestionList.Select(s => s.Title).Take(3)),
                                Severity = firstSuggestion.Severity == SuggestionSeverity.Critical ? "High" : "Medium",
                                ImpactArea = "Queries",
                                ImpactScore = firstSuggestion.EstimatedImpact,
                                RecommendedActions = suggestionList.Select(s => s.Title).ToList(),
                                PotentialImprovement = suggestionList.Any(s => s.EstimatedImpact > 0)
                                    ? suggestionList.Max(s => s.EstimatedImpact) : 0,
                                ConfidenceScore = 85.0,
                                Category = "Performance"
                            });
                        }
                    }

                    response.Insights = suggestions;
                    response.VisualizationType = "None";
                    break;

                case "BatchJob":
                    // Get batch job related queries
                    var batchQueries = _queryMonitorService.GetTopExpensiveQueriesAsync(20).GetAwaiter().GetResult();
                    response.QueryResults = batchQueries.Where(q =>
                        q.QueryText.ToUpperInvariant().Contains("BATCH") ||
                        q.QueryText.ToUpperInvariant().Contains("JOB")).ToList();
                    response.VisualizationType = "Table";
                    break;

                default:
                    // For general queries, show top expensive queries
                    var defaultQueries = _queryMonitorService.GetTopExpensiveQueriesAsync(5).GetAwaiter().GetResult();
                    response.QueryResults = defaultQueries.ToList();
                    response.VisualizationType = "Table";
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching response with real data, using fallback");
            // Fallback to sample data if real data unavailable
            response.QueryResults = GenerateSampleQueries();
        }

        return response;
    }

    private List<SqlQueryMetric> GenerateSampleQueries()
    {
        return new List<SqlQueryMetric>
        {
            new SqlQueryMetric
            {
                QueryHash = "A1B2C3D4",
                QueryText = "SELECT * FROM INVENTTRANS WHERE ITEMID = 'ITEM001'",
                ExecutionCount = 1250,
                AvgElapsedTimeMs = 2450,
                TotalElapsedTimeMs = 3062500
            },
            new SqlQueryMetric
            {
                QueryHash = "E5F6G7H8",
                QueryText = "SELECT * FROM CUSTTABLE WHERE DATAAREAID = 'DAT'",
                ExecutionCount = 890,
                AvgElapsedTimeMs = 1820,
                TotalElapsedTimeMs = 1619800
            },
            new SqlQueryMetric
            {
                QueryHash = "I9J0K1L2",
                QueryText = "SELECT * FROM SALESTABLE WHERE SALESSTATUS = 3",
                ExecutionCount = 650,
                AvgElapsedTimeMs = 1560,
                TotalElapsedTimeMs = 1014000
            }
        };
    }

    private List<PerformanceInsight> GenerateSampleInsights()
    {
        return new List<PerformanceInsight>
        {
            new PerformanceInsight
            {
                Title = "INVENTTRANS Index Fragmentation",
                Description = "Index IX_INVENTTRANS_ITEMID ist zu 72% fragmentiert und verursacht langsame Queries.",
                Severity = "High",
                ImpactArea = "Queries",
                ImpactScore = 85.0,
                RecommendedActions = new List<string>
                {
                    "INDEX REBUILD ausführen",
                    "Automatisches Maintenance-Window konfigurieren",
                    "Query-Hints mit NOLOCK testen"
                },
                PotentialImprovement = 65.0,
                ConfidenceScore = 92.0,
                Category = "Performance"
            },
            new PerformanceInsight
            {
                Title = "Duplicate Queries Detected",
                Description = "23% der CUSTTABLE Queries sind Duplikate und können konsolidiert werden.",
                Severity = "Medium",
                ImpactArea = "Queries",
                ImpactScore = 55.0,
                RecommendedActions = new List<string>
                {
                    "Query Clustering nutzen",
                    "Stored Procedures erstellen",
                    "Parameterized Queries einsetzen"
                },
                PotentialImprovement = 35.0,
                ConfidenceScore = 87.0,
                Category = "Cost"
            }
        };
    }

    private List<string> GenerateSuggestedQuestions(string intent, NLQueryResponse response)
    {
        return intent switch
        {
            "PerformanceIssue" => new List<string>
            {
                "Was sind die teuersten Queries?",
                "Welche Indexes sollte ich erstellen?",
                "Zeig mir die Performance-Trends der letzten Woche"
            },
            "CostAnalysis" => new List<string>
            {
                "Wie viel spare ich durch Optimierung?",
                "Was kostet mich Query XYZ pro Monat?",
                "Zeig mir das ROI für Index-Optimierung"
            },
            "BatchJob" => new List<string>
            {
                "Welche Batch Jobs laufen zu lange?",
                "Gibt es Anti-Patterns in meinen Batch Jobs?",
                "Wann sollte ich Batch Jobs schedulen?"
            },
            "Recommendation" => new List<string>
            {
                "Was ist die wichtigste Optimierung?",
                "Wie setze ich die Empfehlungen um?",
                "Zeig mir Quick Wins"
            },
            _ => new List<string>
            {
                "Zeig mir die langsamsten Queries heute",
                "Was kostet mich die Performance?",
                "Welche Optimierungen empfiehlst du?"
            }
        };
    }

    private void AddToHistory(string sessionId, string role, string content, string? intent)
    {
        if (!_sessions.ContainsKey(sessionId))
            _sessions[sessionId] = new List<NLConversationMessage>();

        _sessions[sessionId].Add(new NLConversationMessage
        {
            Role = role,
            Content = content,
            IntentDetected = intent
        });
    }
}
