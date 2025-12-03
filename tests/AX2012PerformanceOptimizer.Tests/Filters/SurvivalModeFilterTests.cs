using System.Collections.Generic;
using System.Linq;
using AX2012PerformanceOptimizer.Core.Filters;
using AX2012PerformanceOptimizer.Core.Models;
using FluentAssertions;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Filters;

/// <summary>
/// [P1] Unit tests for SurvivalModeFilter
/// Tests filtering logic for Survival Mode
/// </summary>
public class SurvivalModeFilterTests
{
    [Fact]
    [Trait("Priority", "P1")]
    public void Filter_ShouldReturnOnlyCriticalAndHighPriority()
    {
        // GIVEN: Recommendations with different priorities
        var recommendations = new List<Recommendation>
        {
            new Recommendation { Id = "1", Priority = RecommendationPriority.Critical, ImpactAnalysis = "High impact" },
            new Recommendation { Id = "2", Priority = RecommendationPriority.High, ImpactAnalysis = "High impact" },
            new Recommendation { Id = "3", Priority = RecommendationPriority.Medium, ImpactAnalysis = "Medium impact" },
            new Recommendation { Id = "4", Priority = RecommendationPriority.Low, ImpactAnalysis = "Low impact" }
        };

        // WHEN: Filtering for Survival Mode
        var filtered = SurvivalModeFilter.Filter(recommendations);

        // THEN: Should return only Critical and High priority
        filtered.Should().OnlyContain(r => 
            r.Priority == RecommendationPriority.Critical || 
            r.Priority == RecommendationPriority.High);
        filtered.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Filter_ShouldReturnMax10Recommendations()
    {
        // GIVEN: More than 10 eligible recommendations
        var recommendations = Enumerable.Range(1, 20)
            .Select(i => new Recommendation
            {
                Id = i.ToString(),
                Priority = RecommendationPriority.Critical,
                ImpactAnalysis = "High impact"
            })
            .ToList();

        // WHEN: Filtering for Survival Mode
        var filtered = SurvivalModeFilter.Filter(recommendations);

        // THEN: Should return maximum 10
        filtered.Should().HaveCountLessThanOrEqualTo(SurvivalModeFilter.MaxRecommendations);
        filtered.Should().HaveCount(10);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Filter_ShouldSortByCriticality()
    {
        // GIVEN: Recommendations with different priorities
        var recommendations = new List<Recommendation>
        {
            new Recommendation { Id = "1", Priority = RecommendationPriority.High, ImpactAnalysis = "High impact", Category = RecommendationCategory.General },
            new Recommendation { Id = "2", Priority = RecommendationPriority.Critical, ImpactAnalysis = "Critical impact", Category = RecommendationCategory.SqlQueryOptimization },
            new Recommendation { Id = "3", Priority = RecommendationPriority.Critical, ImpactAnalysis = "Critical impact", Category = RecommendationCategory.General }
        };

        // WHEN: Filtering for Survival Mode
        var filtered = SurvivalModeFilter.Filter(recommendations);

        // THEN: Should be sorted by criticality (Critical first)
        filtered.First().Priority.Should().Be(RecommendationPriority.Critical);
        filtered.Last().Priority.Should().Be(RecommendationPriority.High);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetFilteredCount_ShouldReturnCorrectCount()
    {
        // GIVEN: Recommendations and filtered list
        var allRecommendations = new List<Recommendation>
        {
            new Recommendation { Priority = RecommendationPriority.Critical },
            new Recommendation { Priority = RecommendationPriority.High },
            new Recommendation { Priority = RecommendationPriority.Medium }
        };

        var filtered = SurvivalModeFilter.Filter(allRecommendations);

        // WHEN: Getting filtered count
        var count = SurvivalModeFilter.GetFilteredCount(allRecommendations, filtered);

        // THEN: Should return filtered count
        count.Should().Be(filtered.Count);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetTotalCount_ShouldReturnTotalCount()
    {
        // GIVEN: List of recommendations
        var recommendations = new List<Recommendation>
        {
            new Recommendation(),
            new Recommendation(),
            new Recommendation()
        };

        // WHEN: Getting total count
        var count = SurvivalModeFilter.GetTotalCount(recommendations);

        // THEN: Should return total count
        count.Should().Be(3);
    }
}
