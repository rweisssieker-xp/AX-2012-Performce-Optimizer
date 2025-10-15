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
    StorageOptimization
}

public enum RecommendationPriority
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4,
    Informational = 5
}

