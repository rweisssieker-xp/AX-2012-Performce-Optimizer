using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IRecommendationEngine
{
    Task<List<Recommendation>> GenerateRecommendationsAsync();
    Task<List<Recommendation>> GetAllRecommendationsAsync();
    Task<List<Recommendation>> GetRecommendationsByCategoryAsync(RecommendationCategory category);
    Task<List<Recommendation>> GetRecommendationsByPriorityAsync(RecommendationPriority priority);
    Task MarkAsImplementedAsync(string recommendationId);
    Task RefreshRecommendationsAsync();
}

