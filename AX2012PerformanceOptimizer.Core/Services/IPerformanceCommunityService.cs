using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Anonymized community benchmarking and best practices sharing
/// </summary>
public interface IPerformanceCommunityService
{
    Task<BenchmarkReport> GetIndustryBenchmarkAsync(BenchmarkProfile profile);
    Task<List<BestPractice>> GetTopRatedOptimizationsAsync();
    Task<List<string>> GetCommunityAlertsAsync();
    Task SubmitAnonymousMetricsAsync(Dictionary<string, double> metrics);
}
