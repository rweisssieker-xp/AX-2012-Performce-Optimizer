namespace AX2012PerformanceOptimizer.Core.Models;

public class Recommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationCategory Category { get; set; }
    public RecommendationPriority Priority { get; set; }
    public string ImpactAnalysis { get; set; } = string.Empty;
    public string ActionScript { get; set; } = string.Empty;
    public List<string> RelatedObjects { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsImplemented { get; set; }
    public DateTime? ImplementedAt { get; set; }
}

public enum RecommendationCategory
{
    SqlQueryOptimization,
    IndexManagement,
    StatisticsUpdate,
    BatchJobScheduling,
    AosConfiguration,
    DatabaseMaintenance,
    MemoryOptimization,
    StorageOptimization,
    // Server Configuration Categories
    Memory,
    Parallelism,
    Performance,
    Backup,
    Security,
    Maintenance,
    General
}

public enum RecommendationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Informational = 5
}

