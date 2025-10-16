using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for forecasting query performance trends and predicting future issues
/// </summary>
public interface IQueryPerformanceForecastingService
{
    /// <summary>
    /// Predict future performance based on historical trends
    /// </summary>
    Task<PerformanceForecast> ForecastPerformanceAsync(
        SqlQueryMetric currentQuery,
        List<HistoricalQuerySnapshot> history,
        int forecastDays = 30);

    /// <summary>
    /// Detect performance anomalies and outliers
    /// </summary>
    Task<AnomalyDetectionResult> DetectAnomaliesAsync(
        SqlQueryMetric query,
        List<HistoricalQuerySnapshot> history);

    /// <summary>
    /// Predict when a query will become a performance problem
    /// </summary>
    Task<PerformanceIssuePrediction> PredictPerformanceIssueAsync(
        SqlQueryMetric query,
        List<HistoricalQuerySnapshot> history,
        PerformanceThresholds thresholds);

    /// <summary>
    /// Generate historical trend analysis
    /// </summary>
    Task<HistoricalTrendAnalysis> AnalyzeTrendAsync(
        SqlQueryMetric query,
        List<HistoricalQuerySnapshot> history);

    /// <summary>
    /// What-if simulator for load changes
    /// </summary>
    Task<WhatIfAnalysis> SimulateLoadChangeAsync(
        SqlQueryMetric query,
        double loadMultiplier,
        string scenario);
}

/// <summary>
/// Historical snapshot of query performance
/// </summary>
public class HistoricalQuerySnapshot
{
    public DateTime Timestamp { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public long ExecutionCount { get; set; }
    public double AvgElapsedTimeMs { get; set; }
    public double AvgCpuTimeMs { get; set; }
    public long AvgLogicalReads { get; set; }
    public long AvgPhysicalReads { get; set; }
    public double AvgWaitTimeMs { get; set; }
    public int ActiveUsers { get; set; }
}

/// <summary>
/// Performance forecast result
/// </summary>
public class PerformanceForecast
{
    public string QueryHash { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; }
    public int ForecastDays { get; set; }

    // Current metrics
    public double CurrentAvgElapsedTimeMs { get; set; }
    public long CurrentExecutionCount { get; set; }

    // Predicted metrics
    public double PredictedAvgElapsedTimeMs { get; set; }
    public long PredictedExecutionCount { get; set; }
    public double PredictedCpuTimeMs { get; set; }
    public long PredictedLogicalReads { get; set; }

    // Trend information
    public string TrendDirection { get; set; } = string.Empty; // Improving, Stable, Degrading
    public double TrendSlope { get; set; }
    public double ConfidenceScore { get; set; }

    // Alerts
    public bool WillBecomeIssue { get; set; }
    public DateTime? EstimatedIssueDate { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // None, Low, Medium, High, Critical

    // Visual data
    public List<ForecastDataPoint> ForecastPoints { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Forecast data point for charting
/// </summary>
public class ForecastDataPoint
{
    public DateTime Date { get; set; }
    public double PredictedValue { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public bool IsAnomaly { get; set; }
}

/// <summary>
/// Anomaly detection result
/// </summary>
public class AnomalyDetectionResult
{
    public string QueryHash { get; set; } = string.Empty;
    public bool HasAnomalies { get; set; }
    public List<PerformanceAnomaly> Anomalies { get; set; } = new();

    // Statistical info
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double UpperThreshold { get; set; }
    public double LowerThreshold { get; set; }

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual performance anomaly
/// </summary>
public class PerformanceAnomaly
{
    public DateTime DetectedAt { get; set; }
    public string Type { get; set; } = string.Empty; // Spike, Drop, Drift
    public double Value { get; set; }
    public double ExpectedValue { get; set; }
    public double Deviation { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PossibleCause { get; set; } = string.Empty;
}

/// <summary>
/// Performance prediction for issue detection
/// </summary>
public class PerformanceIssuePrediction
{
    public string QueryHash { get; set; } = string.Empty;
    public bool WillBecomeIssue { get; set; }
    public DateTime? PredictedIssueDate { get; set; }
    public int DaysUntilIssue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    // Threshold violations
    public List<ThresholdViolation> ViolatedThresholds { get; set; } = new();

    // Recommendations
    public List<string> PreventiveActions { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Threshold violation
/// </summary>
public class ThresholdViolation
{
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PredictedValue { get; set; }
    public double Threshold { get; set; }
    public DateTime ViolationDate { get; set; }
}

/// <summary>
/// Performance thresholds for issue detection
/// </summary>
public class PerformanceThresholds
{
    public double MaxAvgElapsedTimeMs { get; set; } = 5000.0; // 5 seconds
    public long MaxExecutionsPerHour { get; set; } = 100000;
    public double MaxCpuTimeMs { get; set; } = 3000.0;
    public long MaxLogicalReads { get; set; } = 1000000;
    public double MaxWaitTimeMs { get; set; } = 2000.0;
}

/// <summary>
/// Trend analysis result
/// </summary>
public class HistoricalTrendAnalysis
{
    public string QueryHash { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public int DataPointsAnalyzed { get; set; }

    // 7-day trend
    public TrendMetrics Last7Days { get; set; } = new();

    // 30-day trend
    public TrendMetrics Last30Days { get; set; } = new();

    // 90-day trend
    public TrendMetrics Last90Days { get; set; } = new();

    // Overall trend
    public string OverallTrend { get; set; } = string.Empty; // Improving, Stable, Degrading
    public double TrendStrength { get; set; } // 0-100%
    public string Interpretation { get; set; } = string.Empty;

    // Seasonality detection
    public bool HasSeasonality { get; set; }
    public string SeasonalityPattern { get; set; } = string.Empty; // Daily, Weekly, Monthly

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Trend metrics for a time period
/// </summary>
public class TrendMetrics
{
    public double AverageElapsedTime { get; set; }
    public double MinElapsedTime { get; set; }
    public double MaxElapsedTime { get; set; }
    public double TrendSlope { get; set; }
    public string Direction { get; set; } = string.Empty; // Up, Down, Flat
    public double ChangePercent { get; set; }
    public int Samples { get; set; }
}

/// <summary>
/// What-if analysis result
/// </summary>
public class WhatIfAnalysis
{
    public string QueryHash { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public double LoadMultiplier { get; set; }

    // Current state
    public double CurrentAvgElapsedTimeMs { get; set; }
    public long CurrentExecutionCount { get; set; }
    public double CurrentCpuTimeMs { get; set; }

    // Predicted state
    public double PredictedAvgElapsedTimeMs { get; set; }
    public long PredictedExecutionCount { get; set; }
    public double PredictedCpuTimeMs { get; set; }
    public double PredictedQueueTime { get; set; }

    // Impact analysis
    public string ImpactLevel { get; set; } = string.Empty; // Minimal, Moderate, Significant, Severe
    public List<string> Concerns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();

    // Capacity
    public bool WillExceedCapacity { get; set; }
    public double CapacityUtilization { get; set; }

    public string Summary { get; set; } = string.Empty;
}
