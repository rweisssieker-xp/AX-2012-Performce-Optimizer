using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for forecasting query performance trends using ML-based analysis
/// </summary>
public class QueryPerformanceForecastingService : IQueryPerformanceForecastingService
{
    private readonly ILogger<QueryPerformanceForecastingService> _logger;

    public QueryPerformanceForecastingService(ILogger<QueryPerformanceForecastingService> logger)
    {
        _logger = logger;
    }

    public async Task<PerformanceForecast> ForecastPerformanceAsync(
        SqlQueryMetric currentQuery,
        List<HistoricalQuerySnapshot> history,
        int forecastDays = 30)
    {
        _logger.LogInformation("Forecasting performance for query {Hash} over {Days} days",
            currentQuery.QueryHash, forecastDays);

        await Task.CompletedTask;

        var forecast = new PerformanceForecast
        {
            QueryHash = currentQuery.QueryHash,
            ForecastDate = DateTime.Now,
            ForecastDays = forecastDays,
            CurrentAvgElapsedTimeMs = currentQuery.AvgElapsedTimeMs,
            CurrentExecutionCount = currentQuery.ExecutionCount
        };

        if (history == null || history.Count < 3)
        {
            // Not enough data for forecast
            forecast.TrendDirection = "Unknown";
            forecast.ConfidenceScore = 0.0;
            forecast.Summary = "Insufficient historical data for forecast (minimum 3 data points required)";
            return forecast;
        }

        // Sort by timestamp
        var sortedHistory = history.OrderBy(h => h.Timestamp).ToList();

        // Calculate trend using linear regression
        var (slope, intercept) = CalculateLinearRegression(sortedHistory);
        forecast.TrendSlope = slope;

        // Determine trend direction
        if (Math.Abs(slope) < 0.001)
        {
            forecast.TrendDirection = "Stable";
        }
        else if (slope > 0)
        {
            forecast.TrendDirection = "Degrading";
        }
        else
        {
            forecast.TrendDirection = "Improving";
        }

        // Calculate confidence score
        forecast.ConfidenceScore = CalculateConfidenceScore(sortedHistory, slope, intercept);

        // Generate forecast points
        var lastTimestamp = sortedHistory.Last().Timestamp;
        var lastValue = sortedHistory.Last().AvgElapsedTimeMs;

        for (int day = 1; day <= forecastDays; day++)
        {
            var predictedValue = lastValue + (slope * day);
            var stdDev = CalculateStandardDeviation(sortedHistory);

            var point = new ForecastDataPoint
            {
                Date = lastTimestamp.AddDays(day),
                PredictedValue = Math.Max(0, predictedValue),
                LowerBound = Math.Max(0, predictedValue - (2 * stdDev)),
                UpperBound = predictedValue + (2 * stdDev),
                IsAnomaly = false
            };

            forecast.ForecastPoints.Add(point);
        }

        // Predict final values
        forecast.PredictedAvgElapsedTimeMs = forecast.ForecastPoints.Last().PredictedValue;

        // Predict execution count (assume linear growth)
        var executionGrowth = CalculateExecutionGrowthRate(sortedHistory);
        forecast.PredictedExecutionCount = (long)(currentQuery.ExecutionCount * (1 + executionGrowth * forecastDays / 30.0));

        // Predict other metrics
        forecast.PredictedCpuTimeMs = currentQuery.AvgCpuTimeMs * (forecast.PredictedAvgElapsedTimeMs / currentQuery.AvgElapsedTimeMs);
        forecast.PredictedLogicalReads = currentQuery.AvgLogicalReads;

        // Check if query will become an issue
        var thresholds = new PerformanceThresholds();
        forecast.WillBecomeIssue = forecast.PredictedAvgElapsedTimeMs > thresholds.MaxAvgElapsedTimeMs;

        if (forecast.WillBecomeIssue)
        {
            // Find when it will cross threshold
            var daysUntilIssue = CalculateDaysUntilThreshold(lastValue, slope, thresholds.MaxAvgElapsedTimeMs);
            forecast.EstimatedIssueDate = lastTimestamp.AddDays(daysUntilIssue);

            forecast.Severity = forecast.PredictedAvgElapsedTimeMs > thresholds.MaxAvgElapsedTimeMs * 2 ? "Critical" : "High";
            forecast.AlertMessage = $"⚠️ Query wird in {daysUntilIssue:F0} Tagen zum Performance-Problem!";
        }
        else
        {
            forecast.Severity = "None";
            forecast.AlertMessage = "✅ Keine Performance-Probleme vorhergesagt";
        }

        // Generate summary
        var sb = new StringBuilder();
        sb.AppendLine($"📊 Forecast für die nächsten {forecastDays} Tage:");
        sb.AppendLine($"Trend: {forecast.TrendDirection} ({forecast.TrendSlope:+0.00;-0.00;0}ms/Tag)");
        sb.AppendLine($"Aktuell: {forecast.CurrentAvgElapsedTimeMs:F0}ms");
        sb.AppendLine($"Vorhergesagt: {forecast.PredictedAvgElapsedTimeMs:F0}ms ({CalculateChangePercent(forecast.CurrentAvgElapsedTimeMs, forecast.PredictedAvgElapsedTimeMs):+0.0;-0.0;0}%)");
        sb.AppendLine($"Konfidenz: {forecast.ConfidenceScore:P0}");
        sb.AppendLine();
        sb.AppendLine(forecast.AlertMessage);

        forecast.Summary = sb.ToString();

        return forecast;
    }

    public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(
        SqlQueryMetric query,
        List<HistoricalQuerySnapshot> history)
    {
        _logger.LogInformation("Detecting anomalies for query {Hash}", query.QueryHash);

        await Task.CompletedTask;

        var result = new AnomalyDetectionResult
        {
            QueryHash = query.QueryHash
        };

        if (history == null || history.Count < 10)
        {
            result.Summary = "Nicht genügend Daten für Anomalie-Erkennung (mindestens 10 Datenpunkte benötigt)";
            return result;
        }

        // Calculate statistical measures
        var values = history.Select(h => h.AvgElapsedTimeMs).ToList();
        result.Mean = values.Average();
        result.StandardDeviation = CalculateStandardDeviation(history);

        // Use 3-sigma rule for anomaly detection
        result.UpperThreshold = result.Mean + (3 * result.StandardDeviation);
        result.LowerThreshold = Math.Max(0, result.Mean - (3 * result.StandardDeviation));

        // Detect anomalies
        foreach (var snapshot in history.OrderBy(h => h.Timestamp))
        {
            if (snapshot.AvgElapsedTimeMs > result.UpperThreshold)
            {
                var anomaly = new PerformanceAnomaly
                {
                    DetectedAt = snapshot.Timestamp,
                    Type = "Spike",
                    Value = snapshot.AvgElapsedTimeMs,
                    ExpectedValue = result.Mean,
                    Deviation = (snapshot.AvgElapsedTimeMs - result.Mean) / result.StandardDeviation,
                    Severity = snapshot.AvgElapsedTimeMs > result.UpperThreshold * 1.5 ? "Critical" : "High",
                    Description = $"Performance-Spike: {snapshot.AvgElapsedTimeMs:F0}ms (Erwartet: {result.Mean:F0}ms)",
                    PossibleCause = DeterminePossibleCause(snapshot, history.Average(h => h.AvgElapsedTimeMs))
                };
                result.Anomalies.Add(anomaly);
            }
            else if (snapshot.AvgElapsedTimeMs < result.LowerThreshold && result.LowerThreshold > 0)
            {
                var anomaly = new PerformanceAnomaly
                {
                    DetectedAt = snapshot.Timestamp,
                    Type = "Drop",
                    Value = snapshot.AvgElapsedTimeMs,
                    ExpectedValue = result.Mean,
                    Deviation = (result.Mean - snapshot.AvgElapsedTimeMs) / result.StandardDeviation,
                    Severity = "Low",
                    Description = $"Unerwartete Verbesserung: {snapshot.AvgElapsedTimeMs:F0}ms (Erwartet: {result.Mean:F0}ms)",
                    PossibleCause = "Index hinzugefügt, Cache warming, oder reduzierte Last"
                };
                result.Anomalies.Add(anomaly);
            }
        }

        // Detect drift (gradual performance degradation)
        var recentAvg = history.OrderByDescending(h => h.Timestamp).Take(5).Average(h => h.AvgElapsedTimeMs);
        var oldAvg = history.OrderBy(h => h.Timestamp).Take(5).Average(h => h.AvgElapsedTimeMs);

        if (recentAvg > oldAvg * 1.5)
        {
            var anomaly = new PerformanceAnomaly
            {
                DetectedAt = DateTime.Now,
                Type = "Drift",
                Value = recentAvg,
                ExpectedValue = oldAvg,
                Deviation = (recentAvg - oldAvg) / oldAvg,
                Severity = "Medium",
                Description = $"Performance-Drift erkannt: {((recentAvg - oldAvg) / oldAvg * 100):F0}% Verschlechterung über Zeit",
                PossibleCause = "Daten-Wachstum, fehlende Index-Wartung, oder zunehmende Fragmentierung"
            };
            result.Anomalies.Add(anomaly);
        }

        result.HasAnomalies = result.Anomalies.Count > 0;

        // Generate summary
        if (result.HasAnomalies)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"⚠️ {result.Anomalies.Count} Anomalie(n) erkannt:");
            sb.AppendLine();

            foreach (var anomaly in result.Anomalies.OrderByDescending(a => a.DetectedAt).Take(5))
            {
                sb.AppendLine($"[{anomaly.DetectedAt:yyyy-MM-dd}] {anomaly.Type} - {anomaly.Severity}");
                sb.AppendLine($"  {anomaly.Description}");
                sb.AppendLine($"  Mögliche Ursache: {anomaly.PossibleCause}");
                sb.AppendLine();
            }

            result.Summary = sb.ToString();
        }
        else
        {
            result.Summary = "✅ Keine Performance-Anomalien erkannt";
        }

        return result;
    }

    public async Task<PerformanceIssuePrediction> PredictPerformanceIssueAsync(
        SqlQueryMetric query,
        List<HistoricalQuerySnapshot> history,
        PerformanceThresholds thresholds)
    {
        _logger.LogInformation("Predicting performance issues for query {Hash}", query.QueryHash);

        await Task.CompletedTask;

        var prediction = new PerformanceIssuePrediction
        {
            QueryHash = query.QueryHash
        };

        if (history == null || history.Count < 5)
        {
            prediction.Summary = "Nicht genügend historische Daten für Vorhersage";
            return prediction;
        }

        var sortedHistory = history.OrderBy(h => h.Timestamp).ToList();
        var (slope, _) = CalculateLinearRegression(sortedHistory);

        // Check if performance is degrading
        if (slope <= 0)
        {
            prediction.WillBecomeIssue = false;
            prediction.Severity = "None";
            prediction.Summary = "✅ Query-Performance verbessert sich oder bleibt stabil";
            return prediction;
        }

        // Calculate when thresholds will be violated
        var currentValue = query.AvgElapsedTimeMs;

        if (currentValue >= thresholds.MaxAvgElapsedTimeMs)
        {
            // Already an issue
            prediction.WillBecomeIssue = true;
            prediction.PredictedIssueDate = DateTime.Now;
            prediction.DaysUntilIssue = 0;
            prediction.Severity = "Critical";
            prediction.Reason = "Query überschreitet bereits Performance-Schwellenwerte";
        }
        else
        {
            var daysUntilIssue = CalculateDaysUntilThreshold(currentValue, slope, thresholds.MaxAvgElapsedTimeMs);

            if (daysUntilIssue < 90)
            {
                prediction.WillBecomeIssue = true;
                prediction.PredictedIssueDate = DateTime.Now.AddDays(daysUntilIssue);
                prediction.DaysUntilIssue = (int)daysUntilIssue;

                prediction.Severity = daysUntilIssue switch
                {
                    < 7 => "Critical",
                    < 14 => "High",
                    < 30 => "Medium",
                    _ => "Low"
                };

                prediction.Reason = $"Query wird in {daysUntilIssue:F0} Tagen den Schwellenwert von {thresholds.MaxAvgElapsedTimeMs:F0}ms überschreiten";

                var violation = new ThresholdViolation
                {
                    MetricName = "AvgElapsedTime",
                    CurrentValue = currentValue,
                    PredictedValue = currentValue + (slope * daysUntilIssue),
                    Threshold = thresholds.MaxAvgElapsedTimeMs,
                    ViolationDate = prediction.PredictedIssueDate.Value
                };
                prediction.ViolatedThresholds.Add(violation);
            }
            else
            {
                prediction.WillBecomeIssue = false;
                prediction.Severity = "None";
                prediction.Reason = "Query wird in absehbarer Zukunft (90 Tage) kein Performance-Problem";
            }
        }

        // Generate preventive actions
        if (prediction.WillBecomeIssue)
        {
            prediction.PreventiveActions.Add("🔍 Führen Sie eine Index-Analyse durch");
            prediction.PreventiveActions.Add("📊 Überprüfen Sie Execution Plans auf Scan-Operationen");
            prediction.PreventiveActions.Add("⚙️ Erwägen Sie Query-Optimierung (SELECT *, Joins)");
            prediction.PreventiveActions.Add("💾 Prüfen Sie, ob Daten archiviert werden können");
            prediction.PreventiveActions.Add("📈 Überwachen Sie Daten-Wachstum und Fragmentierung");
        }

        // Generate summary
        var sb = new StringBuilder();
        sb.AppendLine($"🔮 Performance-Vorhersage:");
        sb.AppendLine();

        if (prediction.WillBecomeIssue)
        {
            sb.AppendLine($"⚠️ {prediction.Severity} - {prediction.Reason}");
            sb.AppendLine();
            sb.AppendLine("Präventive Maßnahmen:");
            foreach (var action in prediction.PreventiveActions)
            {
                sb.AppendLine($"  • {action}");
            }
        }
        else
        {
            sb.AppendLine($"✅ {prediction.Reason}");
        }

        prediction.Summary = sb.ToString();

        return prediction;
    }

    public async Task<HistoricalTrendAnalysis> AnalyzeTrendAsync(
        SqlQueryMetric query,
        List<HistoricalQuerySnapshot> history)
    {
        _logger.LogInformation("Analyzing trends for query {Hash}", query.QueryHash);

        await Task.CompletedTask;

        var analysis = new HistoricalTrendAnalysis
        {
            QueryHash = query.QueryHash,
            AnalysisDate = DateTime.Now,
            DataPointsAnalyzed = history?.Count ?? 0
        };

        if (history == null || history.Count < 2)
        {
            analysis.Summary = "Nicht genügend Daten für Trend-Analyse";
            return analysis;
        }

        var sortedHistory = history.OrderBy(h => h.Timestamp).ToList();
        var now = DateTime.Now;

        // Analyze 7-day trend
        var last7Days = sortedHistory.Where(h => h.Timestamp >= now.AddDays(-7)).ToList();
        if (last7Days.Count >= 2)
        {
            analysis.Last7Days = AnalyzePeriod(last7Days);
        }

        // Analyze 30-day trend
        var last30Days = sortedHistory.Where(h => h.Timestamp >= now.AddDays(-30)).ToList();
        if (last30Days.Count >= 2)
        {
            analysis.Last30Days = AnalyzePeriod(last30Days);
        }

        // Analyze 90-day trend
        var last90Days = sortedHistory.Where(h => h.Timestamp >= now.AddDays(-90)).ToList();
        if (last90Days.Count >= 2)
        {
            analysis.Last90Days = AnalyzePeriod(last90Days);
        }

        // Determine overall trend
        var (slope, _) = CalculateLinearRegression(sortedHistory);

        if (Math.Abs(slope) < 0.5)
        {
            analysis.OverallTrend = "Stable";
        }
        else if (slope > 0)
        {
            analysis.OverallTrend = "Degrading";
        }
        else
        {
            analysis.OverallTrend = "Improving";
        }

        // Calculate trend strength (0-100%)
        var avgValue = sortedHistory.Average(h => h.AvgElapsedTimeMs);
        analysis.TrendStrength = Math.Min(100, Math.Abs(slope / avgValue * 30) * 100); // Normalized to 30 days

        // Interpret trend
        analysis.Interpretation = analysis.OverallTrend switch
        {
            "Improving" => "Query-Performance verbessert sich kontinuierlich. Optimierungen zeigen Wirkung.",
            "Stable" => "Query-Performance ist stabil. Keine signifikanten Änderungen erkennbar.",
            "Degrading" => "Query-Performance verschlechtert sich. Untersuchung empfohlen.",
            _ => "Unbekannt"
        };

        // Detect seasonality (simplified)
        analysis.HasSeasonality = DetectSeasonality(sortedHistory);
        if (analysis.HasSeasonality)
        {
            analysis.SeasonalityPattern = "Weekly"; // Simplified - could be more sophisticated
        }

        // Generate summary
        var sb = new StringBuilder();
        sb.AppendLine($"📈 Trend-Analyse:");
        sb.AppendLine($"Overall: {analysis.OverallTrend} (Stärke: {analysis.TrendStrength:F0}%)");
        sb.AppendLine($"{analysis.Interpretation}");
        sb.AppendLine();

        if (analysis.Last7Days.Samples > 0)
        {
            sb.AppendLine($"7 Tage:  {analysis.Last7Days.Direction} ({analysis.Last7Days.ChangePercent:+0.0;-0.0;0}%) - Ø {analysis.Last7Days.AverageElapsedTime:F0}ms");
        }

        if (analysis.Last30Days.Samples > 0)
        {
            sb.AppendLine($"30 Tage: {analysis.Last30Days.Direction} ({analysis.Last30Days.ChangePercent:+0.0;-0.0;0}%) - Ø {analysis.Last30Days.AverageElapsedTime:F0}ms");
        }

        if (analysis.Last90Days.Samples > 0)
        {
            sb.AppendLine($"90 Tage: {analysis.Last90Days.Direction} ({analysis.Last90Days.ChangePercent:+0.0;-0.0;0}%) - Ø {analysis.Last90Days.AverageElapsedTime:F0}ms");
        }

        if (analysis.HasSeasonality)
        {
            sb.AppendLine();
            sb.AppendLine($"📊 Saisonalität erkannt: {analysis.SeasonalityPattern}");
        }

        analysis.Summary = sb.ToString();

        return analysis;
    }

    public async Task<WhatIfAnalysis> SimulateLoadChangeAsync(
        SqlQueryMetric query,
        double loadMultiplier,
        string scenario)
    {
        _logger.LogInformation("Simulating load change for query {Hash}: {Scenario} (x{Multiplier})",
            query.QueryHash, scenario, loadMultiplier);

        await Task.CompletedTask;

        var analysis = new WhatIfAnalysis
        {
            QueryHash = query.QueryHash,
            Scenario = scenario,
            LoadMultiplier = loadMultiplier,
            CurrentAvgElapsedTimeMs = query.AvgElapsedTimeMs,
            CurrentExecutionCount = query.ExecutionCount,
            CurrentCpuTimeMs = query.AvgCpuTimeMs
        };

        // Simple scaling model (can be made more sophisticated)
        // Execution count scales linearly
        analysis.PredictedExecutionCount = (long)(query.ExecutionCount * loadMultiplier);

        // Elapsed time increases non-linearly due to contention
        // Using formula: T_new = T_old * (1 + (multiplier - 1) * 1.3)
        var contentionFactor = 1 + ((loadMultiplier - 1) * 1.3);
        analysis.PredictedAvgElapsedTimeMs = query.AvgElapsedTimeMs * contentionFactor;

        // CPU time scales roughly linearly but with overhead
        analysis.PredictedCpuTimeMs = query.AvgCpuTimeMs * loadMultiplier * 1.1;

        // Queue time increases exponentially with load
        if (loadMultiplier > 1.5)
        {
            analysis.PredictedQueueTime = (loadMultiplier - 1) * 100; // Simplified model
        }

        // Determine impact level
        var performanceDegradation = (analysis.PredictedAvgElapsedTimeMs - analysis.CurrentAvgElapsedTimeMs) / analysis.CurrentAvgElapsedTimeMs;

        analysis.ImpactLevel = performanceDegradation switch
        {
            < 0.1 => "Minimal",
            < 0.3 => "Moderate",
            < 0.7 => "Significant",
            _ => "Severe"
        };

        // Check capacity
        var thresholds = new PerformanceThresholds();
        analysis.WillExceedCapacity = analysis.PredictedAvgElapsedTimeMs > thresholds.MaxAvgElapsedTimeMs;
        analysis.CapacityUtilization = (analysis.PredictedAvgElapsedTimeMs / thresholds.MaxAvgElapsedTimeMs) * 100;

        // Generate concerns
        if (performanceDegradation > 0.2)
        {
            analysis.Concerns.Add($"⚠️ Performance-Degradierung: {performanceDegradation * 100:F0}%");
        }

        if (analysis.WillExceedCapacity)
        {
            analysis.Concerns.Add($"❌ Kapazitätsgrenze überschritten ({analysis.CapacityUtilization:F0}% Auslastung)");
        }

        if (analysis.PredictedQueueTime > 50)
        {
            analysis.Concerns.Add($"⏱️ Signifikante Queue-Time erwartet: {analysis.PredictedQueueTime:F0}ms");
        }

        if (loadMultiplier > 2.0)
        {
            analysis.Concerns.Add("🔥 Hohe Last-Steigerung - Lock-Contention wahrscheinlich");
        }

        // Generate recommendations
        if (analysis.Concerns.Count > 0)
        {
            analysis.Recommendations.Add("🔍 Führen Sie Performance-Tests unter Last durch");
            analysis.Recommendations.Add("📊 Erwägen Sie Query-Optimierung vor Skalierung");
            analysis.Recommendations.Add("💾 Prüfen Sie Index-Strategie für höhere Last");
            analysis.Recommendations.Add("⚙️ Erwägen Sie Read-Replicas oder Caching");
        }

        if (analysis.WillExceedCapacity)
        {
            analysis.Recommendations.Add("🚨 Hardware-Upgrade oder Architektur-Änderung erforderlich");
        }

        // Generate summary
        var sb = new StringBuilder();
        sb.AppendLine($"🔮 What-If Analyse: {scenario}");
        sb.AppendLine($"Load-Multiplikator: {loadMultiplier}x");
        sb.AppendLine();
        sb.AppendLine("Vorhergesagte Auswirkungen:");
        sb.AppendLine($"  Elapsed Time: {analysis.CurrentAvgElapsedTimeMs:F0}ms → {analysis.PredictedAvgElapsedTimeMs:F0}ms ({performanceDegradation * 100:+F0;-F0;0}%)");
        sb.AppendLine($"  Executions: {analysis.CurrentExecutionCount:N0} → {analysis.PredictedExecutionCount:N0}");
        sb.AppendLine($"  Impact Level: {analysis.ImpactLevel}");
        sb.AppendLine();

        if (analysis.Concerns.Count > 0)
        {
            sb.AppendLine("Bedenken:");
            foreach (var concern in analysis.Concerns)
            {
                sb.AppendLine($"  {concern}");
            }
            sb.AppendLine();
        }

        if (analysis.Recommendations.Count > 0)
        {
            sb.AppendLine("Empfehlungen:");
            foreach (var rec in analysis.Recommendations)
            {
                sb.AppendLine($"  • {rec}");
            }
        }

        analysis.Summary = sb.ToString();

        return analysis;
    }

    #region Helper Methods

    private (double slope, double intercept) CalculateLinearRegression(List<HistoricalQuerySnapshot> history)
    {
        var n = history.Count;
        if (n < 2) return (0, 0);

        var xValues = Enumerable.Range(0, n).Select(i => (double)i).ToList();
        var yValues = history.Select(h => h.AvgElapsedTimeMs).ToList();

        var xMean = xValues.Average();
        var yMean = yValues.Average();

        var numerator = xValues.Zip(yValues, (x, y) => (x - xMean) * (y - yMean)).Sum();
        var denominator = xValues.Sum(x => Math.Pow(x - xMean, 2));

        if (denominator == 0) return (0, yMean);

        var slope = numerator / denominator;
        var intercept = yMean - (slope * xMean);

        return (slope, intercept);
    }

    private double CalculateStandardDeviation(List<HistoricalQuerySnapshot> history)
    {
        var values = history.Select(h => h.AvgElapsedTimeMs).ToList();
        if (values.Count < 2) return 0;

        var avg = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquares / (values.Count - 1));
    }

    private double CalculateConfidenceScore(List<HistoricalQuerySnapshot> history, double slope, double intercept)
    {
        // Calculate R-squared (coefficient of determination)
        var n = history.Count;
        var yValues = history.Select(h => h.AvgElapsedTimeMs).ToList();
        var yMean = yValues.Average();

        var ssTotal = yValues.Sum(y => Math.Pow(y - yMean, 2));
        var ssResidual = 0.0;

        for (int i = 0; i < n; i++)
        {
            var predicted = intercept + (slope * i);
            ssResidual += Math.Pow(yValues[i] - predicted, 2);
        }

        if (ssTotal == 0) return 0;

        var rSquared = 1 - (ssResidual / ssTotal);
        return Math.Max(0, Math.Min(1, rSquared));
    }

    private double CalculateExecutionGrowthRate(List<HistoricalQuerySnapshot> history)
    {
        if (history.Count < 2) return 0;

        var oldExec = history.OrderBy(h => h.Timestamp).Take(history.Count / 2).Average(h => h.ExecutionCount);
        var newExec = history.OrderByDescending(h => h.Timestamp).Take(history.Count / 2).Average(h => h.ExecutionCount);

        if (oldExec == 0) return 0;

        return (newExec - oldExec) / oldExec;
    }

    private double CalculateDaysUntilThreshold(double currentValue, double slope, double threshold)
    {
        if (slope <= 0) return double.MaxValue;
        if (currentValue >= threshold) return 0;

        return (threshold - currentValue) / slope;
    }

    private double CalculateChangePercent(double oldValue, double newValue)
    {
        if (oldValue == 0) return 0;
        return ((newValue - oldValue) / oldValue) * 100;
    }

    private string DeterminePossibleCause(HistoricalQuerySnapshot spike, double avgValue)
    {
        var causes = new List<string>();

        if (spike.AvgPhysicalReads > spike.AvgLogicalReads * 0.5)
        {
            causes.Add("Hohe Physical Reads (kalter Cache)");
        }

        if (spike.AvgWaitTimeMs > spike.AvgElapsedTimeMs * 0.3)
        {
            causes.Add("Hohe Wait-Time (Locking/Blocking)");
        }

        if (spike.ActiveUsers > 100)
        {
            causes.Add("Hohe Benutzer-Last");
        }

        if (causes.Count == 0)
        {
            causes.Add("Index-Fragmentierung, Statistik-Probleme, oder Daten-Wachstum");
        }

        return string.Join(", ", causes);
    }

    private TrendMetrics AnalyzePeriod(List<HistoricalQuerySnapshot> periodData)
    {
        if (periodData.Count < 2)
        {
            return new TrendMetrics { Samples = periodData.Count };
        }

        var values = periodData.Select(h => h.AvgElapsedTimeMs).ToList();
        var first = periodData.First().AvgElapsedTimeMs;
        var last = periodData.Last().AvgElapsedTimeMs;

        var (slope, _) = CalculateLinearRegression(periodData);

        var metrics = new TrendMetrics
        {
            AverageElapsedTime = values.Average(),
            MinElapsedTime = values.Min(),
            MaxElapsedTime = values.Max(),
            TrendSlope = slope,
            Samples = periodData.Count
        };

        if (Math.Abs(slope) < 0.1)
        {
            metrics.Direction = "Flat";
        }
        else if (slope > 0)
        {
            metrics.Direction = "Up";
        }
        else
        {
            metrics.Direction = "Down";
        }

        if (first != 0)
        {
            metrics.ChangePercent = ((last - first) / first) * 100;
        }

        return metrics;
    }

    private bool DetectSeasonality(List<HistoricalQuerySnapshot> history)
    {
        // Simplified seasonality detection
        // Check if there are recurring patterns (e.g., weekly)
        if (history.Count < 14) return false;

        var sortedHistory = history.OrderBy(h => h.Timestamp).ToList();

        // Check for weekly pattern
        var weeklyGroups = sortedHistory
            .GroupBy(h => h.Timestamp.DayOfWeek)
            .Select(g => new { Day = g.Key, Avg = g.Average(h => h.AvgElapsedTimeMs) })
            .ToList();

        if (weeklyGroups.Count < 7) return false;

        var maxAvg = weeklyGroups.Max(g => g.Avg);
        var minAvg = weeklyGroups.Min(g => g.Avg);

        // If difference is > 30%, likely seasonal
        return (maxAvg - minAvg) / maxAvg > 0.3;
    }

    #endregion
}
