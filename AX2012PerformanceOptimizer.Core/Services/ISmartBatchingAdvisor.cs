using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Smart Batching Advisor - Optimizes batch job sizing and scheduling
/// UNIQUE FEATURE: Advanced batch optimization with ML-based recommendations
/// </summary>
public interface ISmartBatchingAdvisor
{
    /// <summary>
    /// Analyze batch job performance and recommend optimal batch sizes
    /// </summary>
    Task<BatchSizingRecommendation> AnalyzeBatchSizingAsync(
        BatchJobMetric batchJob,
        List<HistoricalQuerySnapshot> historicalData);

    /// <summary>
    /// Recommend optimal scheduling time for batch jobs
    /// </summary>
    Task<SchedulingRecommendation> RecommendSchedulingAsync(
        List<BatchJobMetric> batchJobs,
        SystemLoadProfile systemLoad);

    /// <summary>
    /// Detect batch job anti-patterns
    /// </summary>
    Task<List<BatchAntiPattern>> DetectAntiPatternsAsync(
        List<BatchJobMetric> batchJobs);

    /// <summary>
    /// Optimize batch job parallelization
    /// </summary>
    Task<ParallelizationStrategy> OptimizeParallelizationAsync(
        BatchJobMetric batchJob);

    /// <summary>
    /// Calculate optimal chunk size for data processing
    /// </summary>
    Task<ChunkSizeRecommendation> CalculateOptimalChunkSizeAsync(
        long totalRecords,
        double avgProcessingTimePerRecord,
        int availableThreads);

    /// <summary>
    /// Predict batch job completion time
    /// </summary>
    Task<BatchCompletionPrediction> PredictCompletionTimeAsync(
        BatchJobMetric batchJob,
        int recordsToProcess);
}

/// <summary>
/// Batch sizing recommendation
/// </summary>
public class BatchSizingRecommendation
{
    public string BatchJobId { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }

    // Current configuration
    public int CurrentBatchSize { get; set; }
    public double CurrentAvgProcessingTime { get; set; }
    public double CurrentThroughput { get; set; } // records/second

    // Recommended configuration
    public int RecommendedBatchSize { get; set; }
    public double EstimatedProcessingTime { get; set; }
    public double EstimatedThroughput { get; set; }

    // Performance impact
    public double ImprovementPercent { get; set; }
    public double TimeSavings { get; set; }

    // Analysis
    public string OptimizationReason { get; set; } = string.Empty;
    public List<string> Factors { get; set; } = new();
    public string Confidence { get; set; } = string.Empty; // Low, Medium, High
    public double ConfidenceScore { get; set; } // 0-100

    // Constraints
    public int MinBatchSize { get; set; }
    public int MaxBatchSize { get; set; }
    public List<string> Constraints { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Scheduling recommendation
/// </summary>
public class SchedulingRecommendation
{
    public DateTime RecommendationDate { get; set; }
    public int BatchJobsAnalyzed { get; set; }

    // Optimal time windows
    public List<TimeWindow> OptimalWindows { get; set; } = new();
    public List<TimeWindow> AvoidWindows { get; set; } = new();

    // Batch job schedule
    public List<ScheduledBatch> RecommendedSchedule { get; set; } = new();

    // Load distribution
    public Dictionary<int, double> HourlyLoadDistribution { get; set; } = new(); // Hour -> Load %

    // Impact analysis
    public double CurrentPeakLoad { get; set; }
    public double OptimizedPeakLoad { get; set; }
    public double LoadReduction { get; set; }

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Time window for scheduling
/// </summary>
public class TimeWindow
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string WindowType { get; set; } = string.Empty; // Optimal, Good, Acceptable, Avoid
    public double SystemLoad { get; set; } // 0-100%
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Scheduled batch job
/// </summary>
public class ScheduledBatch
{
    public string BatchJobId { get; set; } = string.Empty;
    public string BatchJobName { get; set; } = string.Empty;
    public TimeSpan RecommendedTime { get; set; }
    public int Priority { get; set; }
    public double EstimatedDuration { get; set; } // minutes
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Batch anti-pattern detection
/// </summary>
public class BatchAntiPattern
{
    public string PatternId { get; set; } = string.Empty;
    public string PatternName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical

    // Affected jobs
    public List<string> AffectedBatchJobs { get; set; } = new();

    // Description
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;

    // Detection details
    public double DetectionConfidence { get; set; }
    public int OccurrenceCount { get; set; }

    // Resolution
    public List<string> Recommendations { get; set; } = new();
    public string Example { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Parallelization strategy
/// </summary>
public class ParallelizationStrategy
{
    public string BatchJobId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Current state
    public int CurrentThreads { get; set; }
    public bool IsParallelizable { get; set; }
    public List<string> ParallelizationBlockers { get; set; } = new();

    // Recommended strategy
    public int RecommendedThreads { get; set; }
    public string Strategy { get; set; } = string.Empty; // Sequential, Parallel, Distributed
    public int OptimalChunkSize { get; set; }

    // Performance prediction
    public double CurrentEstimatedTime { get; set; }
    public double ParallelEstimatedTime { get; set; }
    public double SpeedupFactor { get; set; }
    public double Efficiency { get; set; } // 0-100%

    // Resource requirements
    public int RequiredCPUCores { get; set; }
    public long RequiredMemoryMB { get; set; }
    public string ResourceAvailability { get; set; } = string.Empty;

    // Implementation
    public List<string> ImplementationSteps { get; set; } = new();
    public List<string> Risks { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Chunk size recommendation
/// </summary>
public class ChunkSizeRecommendation
{
    public DateTime AnalysisDate { get; set; }

    // Input parameters
    public long TotalRecords { get; set; }
    public double AvgProcessingTimePerRecord { get; set; }
    public int AvailableThreads { get; set; }

    // Recommended chunk size
    public int OptimalChunkSize { get; set; }
    public int TotalChunks { get; set; }

    // Performance prediction
    public double EstimatedTotalTime { get; set; } // minutes
    public double EstimatedThroughput { get; set; } // records/second
    public double MemoryPerChunk { get; set; } // MB

    // Alternative sizes
    public List<ChunkSizeOption> Alternatives { get; set; } = new();

    // Analysis
    public string OptimizationGoal { get; set; } = string.Empty; // Speed, Memory, Balance
    public List<string> Considerations { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Alternative chunk size option
/// </summary>
public class ChunkSizeOption
{
    public int ChunkSize { get; set; }
    public int TotalChunks { get; set; }
    public double EstimatedTime { get; set; }
    public double MemoryUsage { get; set; }
    public string TradeOff { get; set; } = string.Empty;
}

/// <summary>
/// Batch completion prediction
/// </summary>
public class BatchCompletionPrediction
{
    public string BatchJobId { get; set; } = string.Empty;
    public DateTime PredictionDate { get; set; }
    public int RecordsToProcess { get; set; }

    // Time predictions
    public DateTime EstimatedStartTime { get; set; }
    public DateTime EstimatedCompletionTime { get; set; }
    public double EstimatedDuration { get; set; } // minutes

    // Confidence intervals
    public DateTime BestCaseCompletion { get; set; }
    public DateTime WorstCaseCompletion { get; set; }
    public DateTime MostLikelyCompletion { get; set; }

    // Prediction accuracy
    public double PredictionConfidence { get; set; } // 0-100%
    public string AccuracyLevel { get; set; } = string.Empty; // Low, Medium, High

    // Factors affecting completion
    public List<CompletionFactor> Factors { get; set; } = new();

    // Milestones
    public List<BatchMilestone> Milestones { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Factor affecting batch completion
/// </summary>
public class CompletionFactor
{
    public string FactorName { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty; // Positive, Negative, Neutral
    public double ImpactMagnitude { get; set; } // minutes
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Batch job milestone
/// </summary>
public class BatchMilestone
{
    public string MilestoneName { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public double PercentComplete { get; set; }
    public DateTime EstimatedTime { get; set; }
}

/// <summary>
/// System load profile
/// </summary>
public class SystemLoadProfile
{
    public DateTime ProfileDate { get; set; }
    public Dictionary<int, LoadMetrics> HourlyLoad { get; set; } = new(); // Hour -> Metrics

    // Peak times
    public List<TimeSpan> PeakHours { get; set; } = new();
    public List<TimeSpan> LowLoadHours { get; set; } = new();

    // Averages
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public double AverageDiskIO { get; set; }
}

/// <summary>
/// Load metrics for a time period
/// </summary>
public class LoadMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskIO { get; set; }
    public double NetworkIO { get; set; }
    public int ActiveBatchJobs { get; set; }
    public int ActiveQueries { get; set; }
}
