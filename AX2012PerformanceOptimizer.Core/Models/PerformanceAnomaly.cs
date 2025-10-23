namespace AX2012PerformanceOptimizer.Core.Models;

public enum AnomalyType
{
    Spike,              // Sudden increase
    Drop,               // Sudden decrease
    Drift,              // Gradual change over time
    Outlier,            // Statistical outlier
    PatternChange,      // Change in behavior pattern
    ThresholdBreach     // Exceeds defined threshold
}

public enum AnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AnomalyStatus
{
    Detected,
    Acknowledged,
    InvestigationInProgress,
    Resolved,
    FalsePositive
}

public class PerformanceAnomaly
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public AnomalyType Type { get; set; }
    public AnomalySeverity Severity { get; set; }
    public AnomalyStatus Status { get; set; } = AnomalyStatus.Detected;

    // Affected Resource
    public string ResourceType { get; set; } = string.Empty;  // "Query", "Table", "AOS", "BatchJob"
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;

    // Metric Information
    public string MetricName { get; set; } = string.Empty;
    public double BaselineValue { get; set; }
    public double AnomalousValue { get; set; }
    public double DeviationPercentage { get; set; }
    public string Unit { get; set; } = string.Empty;

    // Statistical Information
    public double ConfidenceScore { get; set; }  // 0-100
    public double StandardDeviations { get; set; }  // How many σ from mean
    public double PValue { get; set; }  // Statistical significance

    // Root Cause Analysis
    public string RootCauseAnalysis { get; set; } = string.Empty;
    public List<string> ContributingFactors { get; set; } = new();
    public List<string> AffectedComponents { get; set; } = new();
    public int EstimatedImpactedUsers { get; set; }

    // Business Impact
    public decimal EstimatedCostPerHour { get; set; }
    public string BusinessImpactDescription { get; set; } = string.Empty;

    // Recommended Actions
    public List<string> RecommendedActions { get; set; } = new();
    public List<string> AutoActionsTaken { get; set; } = new();

    // Prediction
    public DateTime? PredictedResolutionTime { get; set; }
    public string PredictedImpact { get; set; } = string.Empty;

    // Historical Context
    public bool IsSimilarToPreviousIncident { get; set; }
    public string SimilarIncidentId { get; set; } = string.Empty;
    public DateTime? SimilarIncidentDate { get; set; }

    // Resolution
    public DateTime? ResolvedAt { get; set; }
    public string ResolutionNotes { get; set; } = string.Empty;
    public List<string> PreventiveMeasures { get; set; } = new();
}

public class AnomalyDetectionResult
{
    public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
    public int TotalAnomaliesDetected { get; set; }
    public List<PerformanceAnomaly> Anomalies { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public double AnalysisDurationMs { get; set; }
}

public class BaselineProfile
{
    public string MetricName { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;

    // Statistical Baseline
    public double Mean { get; set; }
    public double Median { get; set; }
    public double StandardDeviation { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Percentile95 { get; set; }
    public double Percentile99 { get; set; }

    // Time-based patterns
    public Dictionary<int, double> HourlyPattern { get; set; } = new();  // Hour -> Expected Value
    public Dictionary<DayOfWeek, double> DailyPattern { get; set; } = new();

    // Baseline calculation
    public DateTime CalculatedAt { get; set; }
    public int SampleSize { get; set; }
    public DateTime DataStartDate { get; set; }
    public DateTime DataEndDate { get; set; }
}

public class AnomalyAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public PerformanceAnomaly Anomaly { get; set; } = new();
    public DateTime AlertTime { get; set; } = DateTime.UtcNow;
    public bool WasNotificationSent { get; set; }
    public List<string> NotificationChannels { get; set; } = new();  // "Email", "SMS", "PagerDuty", "Teams"
    public bool RequiresAcknowledgment { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string AcknowledgedBy { get; set; } = string.Empty;
}

public class IncidentReport
{
    public string IncidentId { get; set; } = Guid.NewGuid().ToString();
    public DateTime IncidentStart { get; set; }
    public DateTime? IncidentEnd { get; set; }
    public TimeSpan Duration => IncidentEnd.HasValue ? IncidentEnd.Value - IncidentStart : TimeSpan.Zero;

    public List<PerformanceAnomaly> RelatedAnomalies { get; set; } = new();
    public string PrimaryCause { get; set; } = string.Empty;
    public List<string> SecondaryContributors { get; set; } = new();

    // System State Snapshot
    public SystemSnapshot PreIncidentSnapshot { get; set; } = new();
    public SystemSnapshot DuringIncidentSnapshot { get; set; } = new();
    public SystemSnapshot PostIncidentSnapshot { get; set; } = new();

    // Impact Analysis
    public int AffectedUsers { get; set; }
    public List<string> AffectedModules { get; set; } = new();
    public decimal EstimatedBusinessCost { get; set; }

    // Resolution
    public List<string> ActionsTaken { get; set; } = new();
    public string ResolutionSummary { get; set; } = string.Empty;
    public List<string> PreventiveMeasuresImplemented { get; set; } = new();

    // Lessons Learned
    public string WhatWentWrong { get; set; } = string.Empty;
    public string WhatWorkedWell { get; set; } = string.Empty;
    public string WhatCouldHaveHelped { get; set; } = string.Empty;
}

public class SystemSnapshot
{
    public DateTime SnapshotTime { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public int ActiveConnections { get; set; }
    public int RunningQueries { get; set; }
    public double AverageQueryTimeMs { get; set; }
    public int BlockingSessions { get; set; }
    public List<string> TopSlowQueries { get; set; } = new();
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}
