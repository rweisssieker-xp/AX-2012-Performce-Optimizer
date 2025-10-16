using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Implementation of AI Performance Insights Service with real data integration
/// </summary>
public class AiPerformanceInsightsService : IAiPerformanceInsightsService
{
    private readonly ILogger<AiPerformanceInsightsService> _logger;
    private readonly ISqlQueryMonitorService _queryMonitorService;
    private readonly IHistoricalDataService _historicalDataService;
    private readonly IPerformanceCostCalculatorService _costCalculatorService;
    private readonly IQueryAnalyzerService _queryAnalyzerService;
    private readonly IDatabaseStatsService _databaseStatsService;

    public AiPerformanceInsightsService(
        ILogger<AiPerformanceInsightsService> logger,
        ISqlQueryMonitorService queryMonitorService,
        IHistoricalDataService historicalDataService,
        IPerformanceCostCalculatorService costCalculatorService,
        IQueryAnalyzerService queryAnalyzerService,
        IDatabaseStatsService databaseStatsService)
    {
        _logger = logger;
        _queryMonitorService = queryMonitorService;
        _historicalDataService = historicalDataService;
        _costCalculatorService = costCalculatorService;
        _queryAnalyzerService = queryAnalyzerService;
        _databaseStatsService = databaseStatsService;
    }

    public async Task<PerformanceInsightsDashboard> GenerateInsightsDashboardAsync(
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInformation("Generating insights dashboard for {Start} to {End}", startDate, endDate);

        try
        {
            var dashboard = new PerformanceInsightsDashboard
            {
                StartDate = startDate,
                EndDate = endDate,
                Metrics = await GenerateMetricsAsync(),
                TopInsights = await GenerateTopInsightsAsync(),
                Opportunities = await GenerateOpportunitiesAsync(),
                Risks = await GenerateRisksAsync(),
                OverallTrend = await GenerateTrendAsync(startDate, endDate),
                ExecutiveSummary = "Dashboard generiert mit echten Performance-Daten aus Ihrem AX 2012 System.",
                TechnicalSummary = "Basierend auf realen Query-Metriken, historischen Trends und Database-Statistiken."
            };

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights dashboard, using fallback");
            return GetFallbackDashboard(startDate, endDate);
        }
    }

    public async Task<List<PerformanceInsight>> GetTopInsightsAsync(int topCount = 5)
    {
        try
        {
            var insights = await GenerateTopInsightsAsync();
            return insights.Take(topCount).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top insights");
            return new List<PerformanceInsight>();
        }
    }

    public async Task<WeeklyPerformanceSummary> GenerateWeeklySummaryAsync()
    {
        try
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-7);

            var trend = await _historicalDataService.AnalyzeTrendAsync("QueryPerformance", startDate, endDate);
            var queries = await _queryMonitorService.GetTopExpensiveQueriesAsync(100);
            var costParams = new CostParameters();
            var costReport = await _costCalculatorService.GenerateExecutiveSummaryAsync(queries.ToList(), costParams);

            return new WeeklyPerformanceSummary
            {
                WeekStartDate = startDate,
                WeekEndDate = endDate,
                Summary = $"Performance-Trend zeigt {trend.Trend} mit {Math.Abs(trend.ChangePercent):F1}% Änderung.",
                TopFindings = new List<string>
                {
                    $"Durchschn. Query-Zeit: {queries.Average(q => q.AvgElapsedTimeMs):F1}ms",
                    $"{queries.Count()} Queries analysiert",
                    $"Top Query: {queries.FirstOrDefault()?.AvgElapsedTimeMs:F0}ms"
                },
                Improvements = new List<string>
                {
                    trend.ChangePercent > 0 ? $"Performance: +{trend.ChangePercent:F1}% besser" : "Stabilisierung erkennbar",
                    "Monitoring aktiv und funktionsfähig"
                },
                Issues = new List<string>
                {
                    $"Langsame Queries: {queries.Count(q => q.AvgElapsedTimeMs > 1000)}",
                    trend.ChangePercent < 0 ? $"Performance-Verschlechterung: {trend.ChangePercent:F1}%" : "Keine kritischen Probleme"
                },
                Recommendations = new List<string>
                {
                    "Top 10 langsamste Queries analysieren",
                    "Index-Strategie überprüfen",
                    "Query-Optimierung für häufige Queries"
                },
                KeyMetrics = new Dictionary<string, double>
                {
                    { "AvgQueryTime", queries.Any() ? queries.Average(q => q.AvgElapsedTimeMs) : 0 },
                    { "SlowQueries", queries.Count(q => q.AvgElapsedTimeMs > 1000) },
                    { "DailyCost", costReport.TotalDailyCost }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly summary");
            return GetFallbackWeeklySummary();
        }
    }

    public async Task<List<OptimizationOpportunity>> FindOptimizationOpportunitiesAsync()
    {
        return await GenerateOpportunitiesAsync();
    }

    public async Task<AiExecutiveSummary> GenerateExecutiveSummaryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var queries = await _queryMonitorService.GetTopExpensiveQueriesAsync(100);
            var costParams = new CostParameters();
            var costReport = await _costCalculatorService.GenerateExecutiveSummaryAsync(queries.ToList(), costParams);
            var trend = await _historicalDataService.AnalyzeTrendAsync("QueryPerformance", startDate, endDate);

            var slowQueries = queries.Count(q => q.AvgElapsedTimeMs > 1000);
            var avgQueryTime = queries.Any() ? queries.Average(q => q.AvgElapsedTimeMs) : 0;

            return new AiExecutiveSummary
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                ExecutiveOverview = $"Performance-Monitoring zeigt {queries.Count()} analysierte Queries mit durchschnittlich {avgQueryTime:F1}ms Ausführungszeit.",
                KeyNumbers = new Dictionary<string, string>
                {
                    { "Analysierte Queries", queries.Count().ToString() },
                    { "Durchschn. Query-Zeit", $"{avgQueryTime:F1}ms" },
                    { "Langsame Queries", slowQueries.ToString() },
                    { "Geschätzte Kosten/Monat", $"€{costReport.TotalMonthlyCost:F2}" }
                },
                PositiveHighlights = new List<string>
                {
                    "Performance-Monitoring aktiv",
                    trend.Trend == TrendDirection.Increasing ? "Performance-Verbesserung erkennbar" : "Baseline etabliert",
                    $"{queries.Count()} Queries erfasst und analysiert"
                },
                ConcernAreas = new List<string>
                {
                    $"{slowQueries} langsame Queries (>1000ms)",
                    trend.Trend == TrendDirection.Decreasing ? "Performance-Verschlechterung" : "Monitoring erforderlich"
                },
                BusinessImpact = $"Aktuelle Performance-Kosten: €{costReport.TotalMonthlyCost:F2}/Monat. Optimierungspotential vorhanden.",
                EstimatedCostImpact = costReport.TotalMonthlyCost * 0.3, // 30% Einsparungspotential
                ExecutiveRecommendations = new List<string>
                {
                    "Top 10 langsamste Queries priorisiert optimieren",
                    "Index-Strategie für häufige Queries überprüfen",
                    "Batch-Job Scheduling optimieren"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating executive summary");
            return GetFallbackExecutiveSummary(startDate, endDate);
        }
    }

    public async Task<List<RiskAlert>> DetectRisksAsync()
    {
        return await GenerateRisksAsync();
    }

    // Private helper methods - Real data implementation

    private async Task<DashboardMetrics> GenerateMetricsAsync()
    {
        try
        {
            var queries = await _queryMonitorService.GetTopExpensiveQueriesAsync(100);
            var costParams = new CostParameters();
            var costReport = await _costCalculatorService.GenerateExecutiveSummaryAsync(queries.ToList(), costParams);
            var dbStats = await _databaseStatsService.GetDatabaseMetricsAsync();

            var avgQueryTime = queries.Any() ? queries.Average(q => q.AvgElapsedTimeMs) : 0;
            var slowQueries = queries.Count(q => q.AvgElapsedTimeMs > 1000);
            var slowPercentage = queries.Any() ? (slowQueries * 100.0 / queries.Count()) : 0;

            // Calculate performance score (0-100, higher is better)
            var performanceScore = Math.Max(0, Math.Min(100, 100 - (avgQueryTime / 10)));

            var grade = performanceScore switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };

            return new DashboardMetrics
            {
                TotalQueries = queries.Count(),
                AvgQueryTime = avgQueryTime,
                SlowQueryPercentage = slowPercentage,
                OverallPerformanceScore = performanceScore,
                PerformanceGrade = grade,
                EstimatedDailyCost = costReport.TotalDailyCost,
                EstimatedMonthlyCost = costReport.TotalMonthlyCost,
                FailedQueries = 0, // Not tracked yet
                SuccessRate = 100.0,
                PerformanceChange = 0.0, // Requires historical comparison
                TrendDirection = "Stable"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating metrics");
            return GetFallbackMetrics();
        }
    }

    private async Task<List<PerformanceInsight>> GenerateTopInsightsAsync()
    {
        var insights = new List<PerformanceInsight>();

        try
        {
            var queries = await _queryMonitorService.GetTopExpensiveQueriesAsync(10);

            foreach (var query in queries.Take(5))
            {
                var suggestionList = await _queryAnalyzerService.AnalyzeQueryAsync(query);

                if (suggestionList != null && suggestionList.Any())
                {
                    var firstSuggestion = suggestionList.First();
                    var avgImpact = suggestionList.Average(s => s.EstimatedImpact);

                    insights.Add(new PerformanceInsight
                    {
                        Title = $"Query {query.QueryHash.Substring(0, 8)} - {suggestionList.Count} Optimierungen",
                        Description = $"Query mit {query.AvgElapsedTimeMs:F0}ms durchschn. Ausführungszeit. {suggestionList.Count} Optimierungen identifiziert.",
                        Severity = firstSuggestion.Severity == SuggestionSeverity.Critical ? "Critical" : firstSuggestion.Severity == SuggestionSeverity.Warning ? "High" : "Medium",
                        ImpactArea = "Queries",
                        ImpactScore = avgImpact,
                        RecommendedActions = suggestionList.Select(s => s.Title).ToList(),
                        PotentialImprovement = avgImpact,
                        ConfidenceScore = 80.0,
                        Category = "Performance"
                    });
                }
            }

            // If no real insights, add a general one
            if (!insights.Any())
            {
                insights.Add(new PerformanceInsight
                {
                    Title = "Performance-Monitoring aktiv",
                    Description = "System wird überwacht. Weitere Insights werden generiert sobald Performance-Muster erkannt werden.",
                    Severity = "Info",
                    ImpactArea = "System",
                    ImpactScore = 0,
                    RecommendedActions = new List<string> { "Monitoring fortsetzen", "Mehr Daten sammeln" },
                    PotentialImprovement = 0,
                    ConfidenceScore = 100,
                    Category = "Info"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
        }

        return insights;
    }

    private async Task<List<OptimizationOpportunity>> GenerateOpportunitiesAsync()
    {
        var opportunities = new List<OptimizationOpportunity>();

        try
        {
            var queries = await _queryMonitorService.GetTopExpensiveQueriesAsync(20);
            var slowQueries = queries.Where(q => q.AvgElapsedTimeMs > 1000).ToList();

            if (slowQueries.Any())
            {
                var avgTimeSavings = slowQueries.Average(q => q.AvgElapsedTimeMs) * 0.5; // 50% improvement estimate
                var costParams = new CostParameters();
                var costReport = await _costCalculatorService.GenerateExecutiveSummaryAsync(slowQueries, costParams);

                opportunities.Add(new OptimizationOpportunity
                {
                    Title = $"Optimierung von {slowQueries.Count} langsamen Queries",
                    Description = $"{slowQueries.Count} Queries mit >1000ms Ausführungszeit optimieren.",
                    OpportunityType = "Performance",
                    EstimatedTimeSavings = avgTimeSavings,
                    EstimatedCostSavings = costReport.TotalMonthlyCost * 0.5,
                    AffectedQueries = slowQueries.Count,
                    EffortLevel = "Medium",
                    EstimatedImplementationTime = slowQueries.Count * 0.5, // 30min per query
                    PriorityScore = 85,
                    PriorityLevel = "High",
                    ImplementationSteps = new List<string>
                    {
                        "1. Query-Analysen für Top 10 durchführen",
                        "2. Index-Strategie überprüfen",
                        "3. Query-Rewrite durchführen",
                        "4. Tests und Deployment"
                    },
                    AutomationAvailable = "Partial",
                    ROI = 3.0,
                    PaybackPeriod = 2.0
                });
            }

            // General optimization opportunity
            var allQueries = await _queryMonitorService.GetTopExpensiveQueriesAsync(100);
            if (allQueries.Any())
            {
                opportunities.Add(new OptimizationOpportunity
                {
                    Title = "Index-Strategie Überprüfung",
                    Description = "Systematische Überprüfung der Index-Strategie für häufige Queries.",
                    OpportunityType = "Performance",
                    EstimatedTimeSavings = 500.0,
                    EstimatedCostSavings = 1000.00,
                    AffectedQueries = allQueries.Count(),
                    EffortLevel = "Low",
                    EstimatedImplementationTime = 4.0,
                    PriorityScore = 70,
                    PriorityLevel = "Medium",
                    ImplementationSteps = new List<string>
                    {
                        "1. Missing Index Report ausführen",
                        "2. Index Usage Statistics analysieren",
                        "3. Neue Indexes erstellen",
                        "4. Monitoring fortsetzen"
                    },
                    AutomationAvailable = "Yes",
                    ROI = 2.5,
                    PaybackPeriod = 3.0
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating opportunities");
        }

        return opportunities;
    }

    private async Task<List<RiskAlert>> GenerateRisksAsync()
    {
        var risks = new List<RiskAlert>();

        try
        {
            var queries = await _queryMonitorService.GetTopExpensiveQueriesAsync(100);
            var criticalQueries = queries.Where(q => q.AvgElapsedTimeMs > 5000).ToList();

            if (criticalQueries.Any())
            {
                risks.Add(new RiskAlert
                {
                    Title = $"{criticalQueries.Count} kritisch langsame Queries erkannt",
                    Description = $"{criticalQueries.Count} Queries mit >5000ms Ausführungszeit können zu Timeouts führen.",
                    Severity = "High",
                    RiskType = "Performance",
                    RiskScore = 80.0,
                    Probability = 90.0,
                    Impact = 70.0,
                    TimeToImpact = "Days",
                    EstimatedImpactDate = DateTime.Now.AddDays(7),
                    MitigationActions = new List<string>
                    {
                        "Kritische Queries sofort analysieren",
                        "Timeout-Limits überprüfen",
                        "Query-Optimierung priorisieren"
                    },
                    AutoMitigationAvailable = false
                });
            }

            // Database growth monitoring (if stats available)
            try
            {
                var dbStats = await _databaseStatsService.GetDatabaseMetricsAsync();
                // Add database-related risks if needed
            }
            catch
            {
                // Database stats not available
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating risks");
        }

        return risks;
    }

    private async Task<PerformanceTrend> GenerateTrendAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var trendAnalysis = await _historicalDataService.AnalyzeTrendAsync("QueryPerformance", startDate, endDate);

            // Convert TrendAnalysis to PerformanceTrend
            return new PerformanceTrend
            {
                TrendDirection = trendAnalysis.Trend.ToString(),
                TrendPercentage = trendAnalysis.ChangePercent,
                TrendDescription = $"{trendAnalysis.MetricName}: {trendAnalysis.Trend}",
                DataPoints = new List<TrendDataPoint>(),
                Predicted7Days = trendAnalysis.CurrentValue,
                Predicted30Days = trendAnalysis.CurrentValue,
                ConfidenceScore = 70.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trend");
            return GetFallbackTrend();
        }
    }

    // Fallback methods for when real data is unavailable

    private PerformanceInsightsDashboard GetFallbackDashboard(DateTime startDate, DateTime endDate)
    {
        return new PerformanceInsightsDashboard
        {
            StartDate = startDate,
            EndDate = endDate,
            Metrics = GetFallbackMetrics(),
            TopInsights = new List<PerformanceInsight>(),
            Opportunities = new List<OptimizationOpportunity>(),
            Risks = new List<RiskAlert>(),
            OverallTrend = GetFallbackTrend(),
            ExecutiveSummary = "Dashboard-Daten nicht verfügbar. Bitte Datenbankverbindung konfigurieren.",
            TechnicalSummary = "Keine Performance-Daten verfügbar."
        };
    }

    private DashboardMetrics GetFallbackMetrics()
    {
        return new DashboardMetrics
        {
            TotalQueries = 0,
            AvgQueryTime = 0,
            SlowQueryPercentage = 0,
            OverallPerformanceScore = 0,
            PerformanceGrade = "-",
            EstimatedDailyCost = 0,
            EstimatedMonthlyCost = 0,
            FailedQueries = 0,
            SuccessRate = 0,
            PerformanceChange = 0,
            TrendDirection = "Unknown"
        };
    }

    private WeeklyPerformanceSummary GetFallbackWeeklySummary()
    {
        return new WeeklyPerformanceSummary
        {
            WeekStartDate = DateTime.Now.AddDays(-7),
            WeekEndDate = DateTime.Now,
            Summary = "Keine Daten verfügbar. Bitte Datenbankverbindung konfigurieren.",
            TopFindings = new List<string> { "Keine Daten verfügbar" },
            Improvements = new List<string>(),
            Issues = new List<string> { "Datenbankverbindung erforderlich" },
            Recommendations = new List<string> { "Settings → Datenbankverbindung konfigurieren" },
            KeyMetrics = new Dictionary<string, double>()
        };
    }

    private AiExecutiveSummary GetFallbackExecutiveSummary(DateTime startDate, DateTime endDate)
    {
        return new AiExecutiveSummary
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            ExecutiveOverview = "Keine Performance-Daten verfügbar.",
            KeyNumbers = new Dictionary<string, string>(),
            PositiveHighlights = new List<string>(),
            ConcernAreas = new List<string> { "Datenbankverbindung nicht konfiguriert" },
            BusinessImpact = "Keine Daten verfügbar.",
            EstimatedCostImpact = 0,
            ExecutiveRecommendations = new List<string> { "Datenbankverbindung in Settings konfigurieren" }
        };
    }

    private PerformanceTrend GetFallbackTrend()
    {
        return new PerformanceTrend
        {
            TrendDirection = "Unknown",
            TrendPercentage = 0,
            TrendDescription = "Keine Trend-Daten verfügbar.",
            DataPoints = new List<TrendDataPoint>(),
            Predicted7Days = 0,
            Predicted30Days = 0,
            ConfidenceScore = 0
        };
    }
}
