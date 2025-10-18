namespace AX2012PerformanceOptimizer.Core.Models;

public class PerformanceSnapshot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> ServerMetrics { get; set; } = new();
    public List<QuerySnapshot> TopQueries { get; set; } = new();
    public List<string> ActiveSessions { get; set; } = new();
    public Dictionary<string, double> SystemCounters { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

public class QuerySnapshot
{
    public string QueryText { get; set; } = string.Empty;
    public string QueryHash { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public long CpuTimeMs { get; set; }
    public long LogicalReads { get; set; }
    public string ExecutionPlan { get; set; } = string.Empty;
}

public class ReplayAnalysis
{
    public PerformanceSnapshot OriginalSnapshot { get; set; } = new();
    public List<string> RootCauses { get; set; } = new();
    public Dictionary<string, string> WhatWouldHaveHelped { get; set; } = new();
    public List<string> PreventionStrategies { get; set; } = new();
    public double ConfidenceScore { get; set; }
}
