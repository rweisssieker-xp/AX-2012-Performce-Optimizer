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

        // Call AI service directly with the prompt
        string aiResponse;
        try
        {
            // Check if AI service is available before calling
            if (_aiService.IsAvailable)
            {
                aiResponse = await _aiService.SendPromptAsync(prompt);

                // Check if response indicates configuration issue
                if (aiResponse.Contains("AI service is not configured") ||
                    aiResponse.StartsWith("Error:"))
                {
                    _logger.LogWarning("AI service returned configuration error, using fallback");
                    aiResponse = GenerateFallbackResponse(naturalLanguageQuery, intent);
                }
            }
            else
            {
                _logger.LogInformation("AI service not available, using fallback response");
                aiResponse = GenerateFallbackResponse(naturalLanguageQuery, intent);
            }
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

        // Generate sample data based on intent (with timeout protection)
        response = await EnrichResponseWithDataAsync(response, intent, context);

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
        var prefix = "💡 **Intelligente Analyse** (Demo-Modus)\n\n";

        var response = intent switch
        {
            "PerformanceIssue" => @"Ich analysiere Performance-Probleme in Ihrem System.

📊 **Typische Performance-Bottlenecks:**
• **Langsame Queries**: Überprüfen Sie die TOP 10 langsamsten Queries im SQL Performance Tab
• **Fehlende Indexes**: Nutzen Sie die Index-Empfehlungen für häufig genutzte Tabellen
• **Hohe CPU-Last**: Analysieren Sie Queries mit hoher CPU-Zeit (>1000ms)
• **I/O-Probleme**: Prüfen Sie Physical Reads - sollten minimal sein

🔍 **Nächste Schritte:**
1. Gehen Sie zum 'SQL Performance' Tab
2. Sortieren Sie nach Total Elapsed Time
3. Analysieren Sie die Top 5 Queries
4. Nutzen Sie die AI-Analyse für detaillierte Empfehlungen",

            "CostAnalysis" => @"💰 **Performance-Kosten Analyse**

Für eine detaillierte Kosten-Analyse:

📈 **Performance Cost Calculator nutzen:**
• Monetäre Auswirkung langsamer Queries berechnen
• ROI von Optimierungen kalkulieren
• Tages-, Monats- und Jahreskosten ermitteln

📊 **Typische Kostenverursacher:**
• Queries mit >1000ms durchschnittlicher Laufzeit
• Hohe Execution Counts (>10.000/Tag)
• Queries mit vielen Physical Reads

💡 **Tipp:** Eine Query-Optimierung von 2s auf 200ms spart bei 1000 Executions/Tag ca. 30 Minuten CPU-Zeit täglich!",

            "Recommendation" => @"🎯 **Top Performance-Empfehlungen**

**Sofort umsetzbar (Quick Wins):**
1. **Index-Optimierung**
   • Fehlende Indexes erstellen (siehe SQL Performance Tab)
   • Fragmentierte Indexes neu aufbauen
   • Ungenutzte Indexes entfernen

2. **Query-Optimierung**
   • SELECT * vermeiden
   • WHERE-Klauseln mit Indexes versehen
   • Joins optimieren (INNER statt OUTER wo möglich)

3. **Batch-Job Scheduling**
   • CPU-intensive Jobs außerhalb der Geschäftszeiten
   • Batch-Größen optimieren
   • Parallele Verarbeitung nutzen

4. **Monitoring & Trending**
   • Performance-Trends beobachten (Historical Trending Tab)
   • Alerts für kritische Queries einrichten
   • Regelmäßige Performance-Reviews",

            "BatchJob" => @"⏱️ **Batch-Job Performance-Optimierung**

**Analyse-Empfehlungen:**
• Nutzen Sie den **Smart Batching Advisor** für intelligente Batch-Größen-Empfehlungen
• Prüfen Sie Batch-Job Queries im SQL Performance Tab
• Analysieren Sie Batch-Job Laufzeiten (Batch Jobs Tab)

**Typische Optimierungen:**
1. **Scheduling**: CPU-intensive Jobs nachts/am Wochenende
2. **Batch-Größe**: Optimal zwischen 50-200 Datensätzen
3. **Parallelisierung**: Mehrere Batch-Threads für große Jobs
4. **Commit-Strategie**: Transaktionsgröße anpassen

**Anti-Patterns vermeiden:**
❌ Zu kleine Batches (<10) → zu viel Overhead
❌ Zu große Batches (>500) → lange Locks
❌ Keine Error-Handling → Komplette Rollbacks",

            "QueryAnalysis" => @"🔎 **Query-Analyse Tools**

**Verfügbare Analyse-Features:**

1. **Query Correlation Engine**
   • Versteckte Beziehungen zwischen Queries finden
   • Gemeinsame Muster erkennen
   • Bulk-Optimierungen identifizieren

2. **Query Clustering**
   • Ähnliche Queries gruppieren
   • Gemeinsame Optimierungen anwenden
   • Duplicate Queries eliminieren

3. **Execution Plan Analyzer**
   • Execution Plans visualisieren
   • Bottlenecks identifizieren
   • Index-Hints generieren

4. **Real-Time Monitoring**
   • Laufende Queries beobachten
   • Performance-Anomalien erkennen
   • Alert-System nutzen",

            "TimeBasedQuery" => @"📅 **Zeitbasierte Performance-Analyse**

Ich analysiere Performance-Daten für Ihren gewählten Zeitraum.

**Verfügbare Zeiträume:**
• Heute (letzte 24 Stunden)
• Gestern
• Letzte 7 Tage
• Letzte 30 Tage
• Benutzerdefiniert

**Analysierte Metriken:**
• Query Execution Counts
• Durchschnittliche Laufzeiten
• CPU-Auslastung
• I/O-Statistiken
• Performance-Trends

💡 **Tipp:** Nutzen Sie den 'Historical Trending' Tab für detaillierte Zeitreihen-Analysen!",

            "Trend" => @"📈 **Performance-Trend Analyse**

**Trend-Analysen verfügbar:**

1. **Query Performance Trends**
   • Laufzeit-Entwicklung über Zeit
   • CPU-Auslastung Trends
   • I/O-Performance

2. **System Health Trends**
   • Datenbank-Größe
   • Connection Counts
   • Lock-Statistiken

3. **Predictive Analytics**
   • Performance-Vorhersagen (Crystal Ball)
   • Capacity Planning
   • Bottleneck-Prognosen

🔮 **Crystal Ball Feature nutzen:**
Gehen Sie zum 'Crystal Ball' Tab für What-If-Szenarien und Performance-Vorhersagen!",

            _ => $@"❓ **Ihre Frage:** ""{query}""

Ich habe Ihre Frage analysiert. Für bessere Ergebnisse, formulieren Sie Ihre Frage spezifischer:

**Beispiel-Fragen:**
• ""Zeig mir die langsamsten Queries heute""
• ""Was kostet mich die schlechte Performance?""
• ""Welche Indexes sollte ich erstellen?""
• ""Wie optimiere ich Batch Jobs?""
• ""Zeig mir Performance-Trends der letzten Woche""

**Verfügbare Features:**
• SQL Performance Analysis
• Batch Job Monitoring
• Cost Calculator
• AI Insights Dashboard
• Performance DNA & Crystal Ball

💡 **Hinweis:** Für AI-gestützte Antworten konfigurieren Sie bitte einen OpenAI API Key im Settings Tab!"
        };

        return prefix + response;
    }

    private double CalculateConfidence(string intent, List<string> entities)
    {
        var baseConfidence = intent != "General" ? 70.0 : 40.0;
        var entityBonus = Math.Min(entities.Count * 5.0, 30.0);
        return Math.Min(baseConfidence + entityBonus, 95.0);
    }

    private async Task<NLQueryResponse> EnrichResponseWithDataAsync(NLQueryResponse response, string intent, NLQueryContext context)
    {
        // Enrich response with real data based on intent - with timeout protection
        try
        {
            // Use Task.WhenAny for timeout protection without CancellationToken
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

            switch (intent)
            {
                case "PerformanceIssue":
                case "QueryAnalysis":
                case "TimeBasedQuery":
                    // Get real query data from database with timeout
                    var queriesTask = _queryMonitorService.GetTopExpensiveQueriesAsync(10);
                    if (await Task.WhenAny(queriesTask, timeoutTask) == queriesTask)
                    {
                        var queries = await queriesTask;
                        response.QueryResults = queries.ToList();
                        response.VisualizationType = "Table";
                    }
                    else
                    {
                        throw new TimeoutException("Query data retrieval timed out");
                    }
                    break;

                case "CostAnalysis":
                    // Calculate real costs using cost calculator
                    var allQueriesTask = _queryMonitorService.GetTopExpensiveQueriesAsync(100);
                    if (await Task.WhenAny(allQueriesTask, timeoutTask) == allQueriesTask)
                    {
                        var allQueries = await allQueriesTask;
                        var costParams = new CostParameters();
                        var costReport = await _costCalculatorService.GenerateExecutiveSummaryAsync(allQueries.ToList(), costParams);

                        response.AdditionalData = new Dictionary<string, object>
                        {
                            { "DailyCost", costReport.TotalDailyCost },
                            { "MonthlyCost", costReport.TotalMonthlyCost },
                            { "YearlyCost", costReport.TotalYearlyCost },
                            { "Currency", "EUR" },
                            { "TotalQueries", costReport.TotalQueriesAnalyzed }
                        };
                        response.VisualizationType = "Chart";
                    }
                    else
                    {
                        throw new TimeoutException("Cost analysis timed out");
                    }
                    break;

                case "Trend":
                    // Get real trend data from historical service
                    var trendTask = _historicalDataService.AnalyzeTrendAsync(
                        "QueryPerformance",
                        context.StartDate,
                        context.EndDate);

                    if (await Task.WhenAny(trendTask, timeoutTask) == trendTask)
                    {
                        var trendData = await trendTask;
                        response.AdditionalData = new Dictionary<string, object>
                        {
                            { "TrendDirection", trendData.Trend.ToString() },
                            { "PerformanceChange", trendData.ChangePercent },
                            { "CurrentValue", trendData.CurrentValue },
                            { "AverageValue", trendData.AverageValue }
                        };
                        response.VisualizationType = "Timeline";
                    }
                    else
                    {
                        throw new TimeoutException("Trend analysis timed out");
                    }
                    break;

                case "Recommendation":
                    // Get real optimization suggestions from analyzer
                    var topQueriesTask = _queryMonitorService.GetTopExpensiveQueriesAsync(5);
                    if (await Task.WhenAny(topQueriesTask, timeoutTask) == topQueriesTask)
                    {
                        var topQueries = await topQueriesTask;
                        var suggestions = new List<PerformanceInsight>();

                        foreach (var query in topQueries.Take(3)) // Limit to 3 for performance
                        {
                            var suggestionList = await _queryAnalyzerService.AnalyzeQueryAsync(query);
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
                    }
                    else
                    {
                        throw new TimeoutException("Recommendations retrieval timed out");
                    }
                    break;

                case "BatchJob":
                    // Get batch job related queries
                    var batchQueriesTask = _queryMonitorService.GetTopExpensiveQueriesAsync(20);
                    if (await Task.WhenAny(batchQueriesTask, timeoutTask) == batchQueriesTask)
                    {
                        var batchQueries = await batchQueriesTask;
                        response.QueryResults = batchQueries.Where(q =>
                            q.QueryText.ToUpperInvariant().Contains("BATCH") ||
                            q.QueryText.ToUpperInvariant().Contains("JOB")).ToList();
                        response.VisualizationType = "Table";
                    }
                    else
                    {
                        throw new TimeoutException("Batch queries retrieval timed out");
                    }
                    break;

                default:
                    // For general queries, show top expensive queries
                    var defaultQueriesTask = _queryMonitorService.GetTopExpensiveQueriesAsync(5);
                    if (await Task.WhenAny(defaultQueriesTask, timeoutTask) == defaultQueriesTask)
                    {
                        var defaultQueries = await defaultQueriesTask;
                        response.QueryResults = defaultQueries.ToList();
                        response.VisualizationType = "Table";
                    }
                    else
                    {
                        throw new TimeoutException("Default queries retrieval timed out");
                    }
                    break;
            }
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Data enrichment timed out (5s), using sample data");
            // Fallback to sample data if timeout
            response.QueryResults = GenerateSampleQueries();
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
                QueryText = "SELECT IT.ITEMID, IT.QTY, IT.DATEPHYSICAL FROM INVENTTRANS IT WHERE IT.ITEMID = @P1 AND IT.DATAAREAID = 'DAT'",
                ExecutionCount = 1250,
                AvgElapsedTimeMs = 2450,
                TotalElapsedTimeMs = 3062500,
                AvgCpuTimeMs = 2100,
                TotalCpuTimeMs = 2625000,
                AvgPhysicalReads = 1200,
                AvgLogicalReads = 4500
            },
            new SqlQueryMetric
            {
                QueryHash = "E5F6G7H8",
                QueryText = "SELECT ACCOUNTNUM, NAME, BLOCKED FROM CUSTTABLE WHERE DATAAREAID = 'DAT' ORDER BY ACCOUNTNUM",
                ExecutionCount = 890,
                AvgElapsedTimeMs = 1820,
                TotalElapsedTimeMs = 1619800,
                AvgCpuTimeMs = 1650,
                TotalCpuTimeMs = 1468500,
                AvgPhysicalReads = 850,
                AvgLogicalReads = 3200
            },
            new SqlQueryMetric
            {
                QueryHash = "I9J0K1L2",
                QueryText = "SELECT ST.SALESID, ST.CUSTACCOUNT, ST.SALESSTATUS, ST.CREATEDDATE FROM SALESTABLE ST WHERE ST.SALESSTATUS = 3 AND ST.DATAAREAID = 'DAT'",
                ExecutionCount = 650,
                AvgElapsedTimeMs = 1560,
                TotalElapsedTimeMs = 1014000,
                AvgCpuTimeMs = 1320,
                TotalCpuTimeMs = 858000,
                AvgPhysicalReads = 680,
                AvgLogicalReads = 2800
            },
            new SqlQueryMetric
            {
                QueryHash = "M3N4O5P6",
                QueryText = "SELECT PL.ITEMID, PL.QTYORDERED, PL.PURCHPRICE FROM PURCHLINE PL INNER JOIN PURCHTABLE PT ON PL.PURCHID = PT.PURCHID WHERE PT.PURCHSTATUS = 2",
                ExecutionCount = 420,
                AvgElapsedTimeMs = 3200,
                TotalElapsedTimeMs = 1344000,
                AvgCpuTimeMs = 2850,
                TotalCpuTimeMs = 1197000,
                AvgPhysicalReads = 1500,
                AvgLogicalReads = 5800
            },
            new SqlQueryMetric
            {
                QueryHash = "Q7R8S9T0",
                QueryText = "SELECT BATCHJOBID, CAPTION, STATUS, COMPANY FROM BATCH WHERE STATUS IN (1,2,3) ORDER BY CREATEDDATETIME DESC",
                ExecutionCount = 180,
                AvgElapsedTimeMs = 890,
                TotalElapsedTimeMs = 160200,
                AvgCpuTimeMs = 750,
                TotalCpuTimeMs = 135000,
                AvgPhysicalReads = 320,
                AvgLogicalReads = 1200
            }
        };
    }

    private List<PerformanceInsight> GenerateSampleInsights()
    {
        return new List<PerformanceInsight>
        {
            new PerformanceInsight
            {
                Title = "INVENTTRANS Index Fragmentation erkannt",
                Description = "Index IX_INVENTTRANS_ITEMID ist zu 72% fragmentiert und verursacht langsame Queries. Durchschnittliche Query-Zeit: 2,4 Sekunden (sollte < 500ms sein).",
                Severity = "High",
                ImpactArea = "Queries",
                ImpactScore = 85.0,
                RecommendedActions = new List<string>
                {
                    "INDEX REBUILD auf INVENTTRANS.IX_INVENTTRANS_ITEMID ausführen",
                    "Automatisches Maintenance-Window für Wochenenden konfigurieren",
                    "Query-Hints mit NOLOCK testen für Read-Operations",
                    "Statistiken mit UPDATE STATISTICS aktualisieren"
                },
                PotentialImprovement = 65.0,
                ConfidenceScore = 92.0,
                Category = "Performance"
            },
            new PerformanceInsight
            {
                Title = "Duplicate Queries bei CUSTTABLE erkannt",
                Description = "23% (205 von 890) der CUSTTABLE Queries sind Duplikate. Geschätzte Einsparung: 1,5 Sekunden CPU-Zeit pro Minute.",
                Severity = "Medium",
                ImpactArea = "Queries",
                ImpactScore = 55.0,
                RecommendedActions = new List<string>
                {
                    "Query Clustering für CUSTTABLE-Zugriffe nutzen",
                    "Stored Procedures für häufige Abfragen erstellen",
                    "Parameterized Queries konsequent einsetzen",
                    "Query Result Caching implementieren"
                },
                PotentialImprovement = 35.0,
                ConfidenceScore = 87.0,
                Category = "Cost"
            },
            new PerformanceInsight
            {
                Title = "PURCHLINE JOIN Performance-Problem",
                Description = "PURCHLINE-PURCHTABLE JOIN ohne geeigneten Index. 420 Executions mit durchschnittlich 3,2 Sekunden - 80% zu langsam!",
                Severity = "High",
                ImpactArea = "Queries",
                ImpactScore = 78.0,
                RecommendedActions = new List<string>
                {
                    "Composite Index auf (PURCHID, PURCHSTATUS) erstellen",
                    "Query umschreiben: EXISTS statt JOIN prüfen",
                    "Covering Index mit INCLUDE(ITEMID, QTYORDERED) erwägen"
                },
                PotentialImprovement = 70.0,
                ConfidenceScore = 88.0,
                Category = "Performance"
            },
            new PerformanceInsight
            {
                Title = "Batch-Job Query-Optimierung möglich",
                Description = "BATCH-Tabelle Queries können durch Indexierung um 45% beschleunigt werden. Aktuell 180 Executions/Stunde.",
                Severity = "Medium",
                ImpactArea = "BatchJobs",
                ImpactScore = 62.0,
                RecommendedActions = new List<string>
                {
                    "Index auf (STATUS, CREATEDDATETIME) erstellen",
                    "Query Filtering vor ORDER BY optimieren",
                    "Batch Status Caching implementieren"
                },
                PotentialImprovement = 45.0,
                ConfidenceScore = 85.0,
                Category = "Performance"
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
