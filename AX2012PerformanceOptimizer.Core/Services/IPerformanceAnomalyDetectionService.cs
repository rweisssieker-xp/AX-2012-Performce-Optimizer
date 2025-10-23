using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IPerformanceAnomalyDetectionService
{
    /// <summary>
    /// Detects anomalies in current system performance
    /// </summary>
    Task<AnomalyDetectionResult> DetectAnomaliesAsync();

    /// <summary>
    /// Calculates baseline profile for a metric
    /// </summary>
    Task<BaselineProfile> CalculateBaselineAsync(string metricName, string resourceType, string resourceId, int days = 30);

    /// <summary>
    /// Performs root cause analysis for an anomaly
    /// </summary>
    Task<string> AnalyzeRootCauseAsync(PerformanceAnomaly anomaly);

    /// <summary>
    /// Gets all active anomalies
    /// </summary>
    Task<List<PerformanceAnomaly>> GetActiveAnomaliesAsync();

    /// <summary>
    /// Gets anomaly history
    /// </summary>
    Task<List<PerformanceAnomaly>> GetAnomalyHistoryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Acknowledges an anomaly
    /// </summary>
    Task AcknowledgeAnomalyAsync(string anomalyId, string acknowledgedBy);

    /// <summary>
    /// Resolves an anomaly
    /// </summary>
    Task ResolveAnomalyAsync(string anomalyId, string resolutionNotes);

    /// <summary>
    /// Generates incident report
    /// </summary>
    Task<IncidentReport> GenerateIncidentReportAsync(List<string> anomalyIds);

    /// <summary>
    /// Predicts potential anomalies in next 24 hours
    /// </summary>
    Task<List<PerformanceAnomaly>> PredictUpcomingAnomaliesAsync();
}
