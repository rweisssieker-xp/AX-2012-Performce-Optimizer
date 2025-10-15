namespace AX2012PerformanceOptimizer.Core.Models;

public class AosMetric
{
    public string ServerName { get; set; } = string.Empty;
    public int ActiveUserSessions { get; set; }
    public double CpuUsagePercent { get; set; }
    public long MemoryUsageMB { get; set; }
    public int ActiveThreads { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class UserSession
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ClientComputer { get; set; } = string.Empty;
    public DateTime LoginDateTime { get; set; }
    public string Company { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

