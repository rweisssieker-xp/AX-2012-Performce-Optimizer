namespace AX2012PerformanceOptimizer.Core.Models;

public class BatchJobMetric
{
    public string JobId { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public BatchJobStatus Status { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Company { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public double AvgDurationMinutes { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public enum BatchJobStatus
{
    Waiting = 0,
    Executing = 1,
    Error = 2,
    Finished = 3,
    Ready = 4,
    Canceling = 5,
    Canceled = 6,
    Withhold = 7
}

