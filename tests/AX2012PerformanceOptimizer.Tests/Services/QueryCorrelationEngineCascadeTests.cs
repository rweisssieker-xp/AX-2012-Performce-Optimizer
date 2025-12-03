using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.ChainReaction;
using AX2012PerformanceOptimizer.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P1] Unit tests for QueryCorrelationEngine PredictCascadeImpactAsync extension
/// Tests cascade impact prediction functionality
/// </summary>
public class QueryCorrelationEngineCascadeTests
{
    private readonly Mock<ILogger<QueryCorrelationEngine>> _mockLogger;
    private readonly QueryCorrelationEngine _engine;

    public QueryCorrelationEngineCascadeTests()
    {
        _mockLogger = new Mock<ILogger<QueryCorrelationEngine>>();
        _engine = new QueryCorrelationEngine(_mockLogger.Object);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task PredictCascadeImpactAsync_ShouldReturnValidImpact()
    {
        // GIVEN: A query hash and optimization type
        var queryHash = "test-query-hash-123";
        var optimizationType = "index";

        // WHEN: Predicting cascade impact
        var result = await _engine.PredictCascadeImpactAsync(queryHash, optimizationType);

        // THEN: Result should be valid
        result.Should().NotBeNull();
        result.SourceQueryHash.Should().Be(queryHash);
        result.OptimizationType.Should().Be(optimizationType);
        result.AffectedQueries.Should().NotBeNull();
        result.PredictedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task PredictCascadeImpactAsync_ShouldCalculateImpactForDependentQueries()
    {
        // GIVEN: A query hash
        var queryHash = "test-query-hash-123";

        // WHEN: Predicting cascade impact
        var result = await _engine.PredictCascadeImpactAsync(queryHash, "general");

        // THEN: Should calculate impact metrics
        result.TotalTimeSaved.Should().BeGreaterThanOrEqualTo(0);
        result.QueriesAffected.Should().BeGreaterThanOrEqualTo(0);
        result.Confidence.Should().BeInRange(0, 100);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task PredictCascadeImpactAsync_ShouldHandleDifferentOptimizationTypes()
    {
        // GIVEN: Different optimization types
        var queryHash = "test-query-hash-123";
        var optimizationTypes = new[] { "index", "query_rewrite", "statistics", "general" };

        // WHEN: Predicting impact for each type
        var results = new List<CascadeImpactResult>();
        foreach (var type in optimizationTypes)
        {
            var result = await _engine.PredictCascadeImpactAsync(queryHash, type);
            results.Add(result);
        }

        // THEN: All should return valid results
        results.Should().AllSatisfy(r => r.Should().NotBeNull());
        results.Should().AllSatisfy(r => r.OptimizationType.Should().BeOneOf(optimizationTypes));
    }

    [Fact]
    [Trait("Priority", "P2")]
    public async Task PredictCascadeImpactAsync_ShouldReturnEmptyWhenNoDependencies()
    {
        // GIVEN: A query hash with no dependencies
        var queryHash = "isolated-query-hash";

        // WHEN: Predicting cascade impact
        var result = await _engine.PredictCascadeImpactAsync(queryHash, "general");

        // THEN: Should return result with no affected queries
        result.Should().NotBeNull();
        result.QueriesAffected.Should().Be(0);
        result.AffectedQueries.Should().BeEmpty();
        result.Summary.Should().Contain("no detected cascade effects");
    }
}
