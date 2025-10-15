namespace AX2012PerformanceOptimizer.Core.Models;

public class SqlQueryMetric
{
    public string QueryHash { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public long ExecutionCount { get; set; }
    public double TotalCpuTimeMs { get; set; }
    public double AvgCpuTimeMs { get; set; }
    public double TotalElapsedTimeMs { get; set; }
    public double AvgElapsedTimeMs { get; set; }
    public long TotalLogicalReads { get; set; }
    public long AvgLogicalReads { get; set; }
    public long TotalPhysicalReads { get; set; }
    public long AvgPhysicalReads { get; set; }
    public DateTime LastExecutionTime { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

