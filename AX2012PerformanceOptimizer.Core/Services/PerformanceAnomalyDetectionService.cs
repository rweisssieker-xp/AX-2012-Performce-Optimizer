using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class PerformanceAnomalyDetectionService : IPerformanceAnomalyDetectionService
{
    private readonly ISqlQueryMonitorService _queryMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IAosMonitorService _aosMonitor;
    private readonly ILogger<PerformanceAnomalyDetectionService> _logger;
    private readonly Dictionary<string, BaselineProfile> _baselineCache = new();

    public PerformanceAnomalyDetectionService(
        ISqlQueryMonitorService queryMonitor,
        IDatabaseStatsService databaseStats,
        IAosMonitorService aosMonitor,
        ILogger<PerformanceAnomalyDetectionService> logger)
    {
        _queryMonitor = queryMonitor;
        _databaseStats = databaseStats;
        _aosMonitor = aosMonitor;
        _logger = logger;
    }

    public async Task<AnomalyDetectionResult> DetectAnomaliesAsync()
    {
        var startTime = DateTime.UtcNow;
        var result = new AnomalyDetectionResult();

        try
        {
            // Detect query anomalies
            var queryAnomalies = await DetectQueryAnomaliesAsync();
            result.Anomalies.AddRange(queryAnomalies);

            // Detect database anomalies
            var dbAnomalies = await DetectDatabaseAnomaliesAsync();
            result.Anomalies.AddRange(dbAnomalies);

            // Detect AOS anomalies
            var aosAnomalies = await DetectAosAnomaliesAsync();
            result.Anomalies.AddRange(aosAnomalies);

            result.TotalAnomaliesDetected = result.Anomalies.Count;

            _logger.LogInformation("Anomaly detection complete: {Count} anomalies detected", result.TotalAnomaliesDetected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during anomaly detection");
            result.Warnings.Add($"Error: {ex.Message}");
        }

        result.AnalysisDurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        return result;
    }

    public async Task<BaselineProfile> CalculateBaselineAsync(string metricName, string resourceType, string resourceId, int days = 30)
    {
        var cacheKey = $"{metricName}_{resourceType}_{resourceId}";

        if (_baselineCache.TryGetValue(cacheKey, out var cached))
        {
            if ((DateTime.UtcNow - cached.CalculatedAt).TotalHours < 24)
            {
                return cached;
            }
        }

        // Calculate new baseline (simplified for now)
        var baseline = new BaselineProfile
        {
            MetricName = metricName,
            ResourceType = resourceType,
            ResourceId = resourceId,
            CalculatedAt = DateTime.UtcNow,
            DataStartDate = DateTime.UtcNow.AddDays(-days),
            DataEndDate = DateTime.UtcNow,
            SampleSize = days * 24  // Hourly samples
        };

        // TODO: Calculate actual statistics from historical data
        baseline.Mean = 150;
        baseline.StandardDeviation = 30;
        baseline.Min = 50;
        baseline.Max = 500;
        baseline.Percentile95 = 250;
        baseline.Percentile99 = 400;

        _baselineCache[cacheKey] = baseline;

        await Task.CompletedTask;
        return baseline;
    }

    public async Task<string> AnalyzeRootCauseAsync(PerformanceAnomaly anomaly)
    {
        // Simplified root cause analysis
        var analysis = new List<string>();

        if (anomaly.Type == AnomalyType.Spike && anomaly.ResourceType == "Query")
        {
            analysis.Add("Query execution time spike detected");
            analysis.Add("Possible causes:");
            analysis.Add("- Outdated statistics causing poor query plan");
            analysis.Add("- Missing index on frequently queried column");
            analysis.Add("- Increased data volume without index maintenance");
            analysis.Add("- Lock/blocking from concurrent operations");
        }
        else if (anomaly.Type == AnomalyType.Drift)
        {
            analysis.Add("Gradual performance degradation detected");
            analysis.Add("Possible causes:");
            analysis.Add("- Data volume growth exceeding capacity");
            analysis.Add("- Index fragmentation accumulating");
            analysis.Add("- Memory pressure increasing");
        }

        await Task.CompletedTask;
        return string.Join("\n", analysis);
    }

    public async Task<List<PerformanceAnomaly>> GetActiveAnomaliesAsync()
    {
        // Placeholder - would retrieve from database/cache
        await Task.CompletedTask;
        return new List<PerformanceAnomaly>();
    }

    public async Task<List<PerformanceAnomaly>> GetAnomalyHistoryAsync(DateTime startDate, DateTime endDate)
    {
        // Placeholder - would retrieve from database
        await Task.CompletedTask;
        return new List<PerformanceAnomaly>();
    }

    public async Task AcknowledgeAnomalyAsync(string anomalyId, string acknowledgedBy)
    {
        // Placeholder - would update in database
        await Task.CompletedTask;
        _logger.LogInformation("Anomaly {AnomalyId} acknowledged by {User}", anomalyId, acknowledgedBy);
    }

    public async Task ResolveAnomalyAsync(string anomalyId, string resolutionNotes)
    {
        // Placeholder - would update in database
        await Task.CompletedTask;
        _logger.LogInformation("Anomaly {AnomalyId} resolved: {Notes}", anomalyId, resolutionNotes);
    }

    public async Task<IncidentReport> GenerateIncidentReportAsync(List<string> anomalyIds)
    {
        var report = new IncidentReport
        {
            IncidentStart = DateTime.UtcNow.AddHours(-1),
            IncidentEnd = DateTime.UtcNow
        };

        // Placeholder - would analyze anomalies and generate report
        await Task.CompletedTask;
        return report;
    }

    public async Task<List<PerformanceAnomaly>> PredictUpcomingAnomaliesAsync()
    {
        // Placeholder - would use ML models to predict
        await Task.CompletedTask;
        return new List<PerformanceAnomaly>();
    }

    // Private helper methods

    private async Task<List<PerformanceAnomaly>> DetectQueryAnomaliesAsync()
    {
        var anomalies = new List<PerformanceAnomaly>();

        try
        {
            var queries = await _queryMonitor.GetTopExpensiveQueriesAsync(50);

            foreach (var query in queries)
            {
                var baseline = await CalculateBaselineAsync("QueryDuration", "Query", query.QueryHash, 30);

                // Check if query duration is anomalous (3 standard deviations from mean)
                var deviations = Math.Abs(query.AvgElapsedTimeMs - baseline.Mean) / baseline.StandardDeviation;

                if (deviations >= 3)
                {
                    var anomaly = new PerformanceAnomaly
                    {
                        Type = query.AvgElapsedTimeMs > baseline.Mean ? AnomalyType.Spike : AnomalyType.Drop,
                        Severity = deviations >= 5 ? AnomalySeverity.Critical :
                                 deviations >= 4 ? AnomalySeverity.High :
                                 AnomalySeverity.Medium,
                        ResourceType = "Query",
                        ResourceName = query.QueryText.Length > 100 ? query.QueryText.Substring(0, 100) + "..." : query.QueryText,
                        ResourceId = query.QueryHash,
                        MetricName = "Query Duration",
                        BaselineValue = baseline.Mean,
                        AnomalousValue = query.AvgElapsedTimeMs,
                        DeviationPercentage = ((query.AvgElapsedTimeMs - baseline.Mean) / baseline.Mean) * 100,
                        Unit = "ms",
                        ConfidenceScore = Math.Min(95, 60 + (deviations * 7)),
                        StandardDeviations = deviations,
                        RootCauseAnalysis = await AnalyzeRootCauseAsync(new PerformanceAnomaly
                        {
                            Type = AnomalyType.Spike,
                            ResourceType = "Query"
                        })
                    };

                    anomaly.RecommendedActions.Add("Review query execution plan");
                    anomaly.RecommendedActions.Add("Check for missing indexes");
                    anomaly.RecommendedActions.Add("Update statistics on involved tables");

                    anomalies.Add(anomaly);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting query anomalies");
        }

        return anomalies;
    }

    private async Task<List<PerformanceAnomaly>> DetectDatabaseAnomaliesAsync()
    {
        var anomalies = new List<PerformanceAnomaly>();

        try
        {
            var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);

            // Check for excessive fragmentation
            if (fragmented.Count > 20)
            {
                anomalies.Add(new PerformanceAnomaly
                {
                    Type = AnomalyType.Drift,
                    Severity = fragmented.Count > 50 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    ResourceType = "Database",
                    ResourceName = "Index Fragmentation",
                    MetricName = "Fragmented Indexes Count",
                    BaselineValue = 10,
                    AnomalousValue = fragmented.Count,
                    DeviationPercentage = ((fragmented.Count - 10) / 10.0) * 100,
                    Unit = "indexes",
                    ConfidenceScore = 90,
                    RootCauseAnalysis = "Index fragmentation has exceeded acceptable threshold. Regular index maintenance may not be running as scheduled."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting database anomalies");
        }

        return anomalies;
    }

    private async Task<List<PerformanceAnomaly>> DetectAosAnomaliesAsync()
    {
        var anomalies = new List<PerformanceAnomaly>();

        try
        {
            var aosMetrics = await _aosMonitor.GetAosMetricsAsync();

            // Check if AOS is unhealthy
            if (!aosMetrics.IsHealthy)
            {
                anomalies.Add(new PerformanceAnomaly
                {
                    Type = AnomalyType.ThresholdBreach,
                    Severity = AnomalySeverity.Critical,
                    ResourceType = "AOS",
                    ResourceName = aosMetrics.ServerName,
                    MetricName = "AOS Health",
                    ConfidenceScore = 95,
                    RootCauseAnalysis = "AOS server health check failed. Service may be unavailable or experiencing issues."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting AOS anomalies");
        }

        return anomalies;
    }
}
