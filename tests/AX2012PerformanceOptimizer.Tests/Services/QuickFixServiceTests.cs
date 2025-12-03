using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models.QuickFix;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.QuickFix;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P1] Unit tests for QuickFixService
/// Tests rapid quick fix analysis and application
/// </summary>
public class QuickFixServiceTests
{
    private readonly Mock<IRecommendationEngine> _mockRecommendationEngine;
    private readonly Mock<ISqlQueryMonitorService> _mockSqlMonitor;
    private readonly Mock<IDatabaseStatsService> _mockDatabaseStats;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<QuickFixService>> _mockLogger;
    private readonly QuickFixService _service;

    public QuickFixServiceTests()
    {
        _mockRecommendationEngine = new Mock<IRecommendationEngine>();
        _mockSqlMonitor = new Mock<ISqlQueryMonitorService>();
        _mockDatabaseStats = new Mock<IDatabaseStatsService>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<QuickFixService>>();

        _service = new QuickFixService(
            _mockRecommendationEngine.Object,
            _mockSqlMonitor.Object,
            _mockDatabaseStats.Object,
            _cache,
            _mockLogger.Object);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task AnalyzeQuickFixesAsync_ShouldCompleteWithin30Seconds()
    {
        // GIVEN: Mock services return data quickly
        _mockRecommendationEngine.Setup(x => x.GetRecommendationsByCategoryAsync(It.IsAny<RecommendationCategory>()))
            .ReturnsAsync(new List<Recommendation>());

        _mockSqlMonitor.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Core.Models.SqlQueryMetric>());

        var startTime = DateTime.UtcNow;

        // WHEN: Analyzing quick fixes
        var result = await _service.AnalyzeQuickFixesAsync();

        // THEN: Should complete within 30 seconds
        var duration = DateTime.UtcNow - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(35)); // With margin
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task AnalyzeQuickFixesAsync_ShouldReturnHighImpactFixes()
    {
        // GIVEN: Mock services return recommendations
        var recommendations = new List<Recommendation>
        {
            new Recommendation
            {
                Title = "Create Index",
                Description = "Missing index detected",
                Category = RecommendationCategory.IndexOptimization,
                Priority = RecommendationPriority.Critical,
                ActionScript = "CREATE INDEX..."
            }
        };

        _mockRecommendationEngine.Setup(x => x.GetRecommendationsByCategoryAsync(RecommendationCategory.IndexOptimization))
            .ReturnsAsync(recommendations);

        _mockSqlMonitor.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Core.Models.SqlQueryMetric>());

        // WHEN: Analyzing quick fixes
        var result = await _service.AnalyzeQuickFixesAsync();

        // THEN: Should return fixes sorted by impact/effort
        result.Should().NotBeNull();
        result.QuickFixes.Should().NotBeEmpty();
        result.QuickFixes.Should().BeInDescendingOrder(f => f.Impact / Math.Max(1, f.Effort));
        result.QuickFixes.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ApplyQuickFixAsync_ShouldApplyFixSuccessfully()
    {
        // GIVEN: Analysis result with a fix
        var fix = new QuickFix
        {
            Id = "test-fix-1",
            Type = QuickFixType.UpdateStatistics,
            Title = "Update Statistics"
        };

        var analysisResult = new QuickFixAnalysisResult
        {
            QuickFixes = new List<QuickFix> { fix }
        };

        _cache.Set("QuickFixAnalysis", analysisResult, TimeSpan.FromMinutes(5));

        _mockRecommendationEngine.Setup(x => x.GetRecommendationsByCategoryAsync(It.IsAny<RecommendationCategory>()))
            .ReturnsAsync(new List<Recommendation>());

        _mockSqlMonitor.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Core.Models.SqlQueryMetric>());

        // WHEN: Applying fix
        var result = await _service.ApplyQuickFixAsync("test-fix-1");

        // THEN: Should succeed
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.FixId.Should().Be("test-fix-1");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task CanApplyDirectlyAsync_ShouldReturnCorrectValue()
    {
        // GIVEN: Analysis result with different fix types
        var simpleFix = new QuickFix
        {
            Id = "simple-fix",
            Type = QuickFixType.UpdateStatistics,
            CanApplyDirectly = true,
            Priority = QuickFixPriority.Medium
        };

        var criticalFix = new QuickFix
        {
            Id = "critical-fix",
            Type = QuickFixType.KillBlockingQuery,
            CanApplyDirectly = false,
            Priority = QuickFixPriority.Critical
        };

        var analysisResult = new QuickFixAnalysisResult
        {
            QuickFixes = new List<QuickFix> { simpleFix, criticalFix }
        };

        _cache.Set("QuickFixAnalysis", analysisResult, TimeSpan.FromMinutes(5));

        // WHEN: Checking if fixes can be applied directly
        var canApplySimple = await _service.CanApplyDirectlyAsync("simple-fix");
        var canApplyCritical = await _service.CanApplyDirectlyAsync("critical-fix");

        // THEN: Should return correct values
        canApplySimple.Should().BeTrue();
        canApplyCritical.Should().BeFalse();
    }
}
