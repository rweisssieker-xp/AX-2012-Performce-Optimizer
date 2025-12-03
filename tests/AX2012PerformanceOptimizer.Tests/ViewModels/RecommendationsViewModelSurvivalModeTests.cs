using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Tests.ViewModels;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P1] Unit tests for RecommendationsViewModel Survival Mode functionality
/// Tests Survival Mode toggle and filtering
/// </summary>
public class RecommendationsViewModelSurvivalModeTests
{
    private readonly Mock<IRecommendationEngine> _mockRecommendationEngine;

    public RecommendationsViewModelSurvivalModeTests()
    {
        _mockRecommendationEngine = new Mock<IRecommendationEngine>();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Constructor_ShouldInitializeSurvivalModeProperties()
    {
        // GIVEN: A new RecommendationsViewModel
        // WHEN: Creating the view model
        var viewModel = new RecommendationsViewModel(_mockRecommendationEngine.Object);

        // THEN: Survival Mode properties should be initialized
        viewModel.IsSurvivalModeEnabled.Should().BeFalse();
        viewModel.FilteredCount.Should().Be(0);
        viewModel.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task SurvivalModeToggleCommand_ShouldToggleMode()
    {
        // GIVEN: A view model with recommendations loaded
        var recommendations = new List<Recommendation>
        {
            new Recommendation { Priority = RecommendationPriority.Critical },
            new Recommendation { Priority = RecommendationPriority.High },
            new Recommendation { Priority = RecommendationPriority.Medium }
        };

        _mockRecommendationEngine.Setup(x => x.GenerateRecommendationsAsync())
            .ReturnsAsync(recommendations);

        var viewModel = new RecommendationsViewModel(_mockRecommendationEngine.Object);
        await viewModel.LoadRecommendationsCommand.ExecuteAsync(null);

        var initialMode = viewModel.IsSurvivalModeEnabled;

        // WHEN: Toggling Survival Mode
        viewModel.ToggleSurvivalModeCommand.Execute(null);

        // THEN: Mode should be toggled
        viewModel.IsSurvivalModeEnabled.Should().Be(!initialMode);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task SurvivalModeEnabled_ShouldFilterRecommendations()
    {
        // GIVEN: Recommendations with different priorities
        var recommendations = new List<Recommendation>
        {
            new Recommendation { Id = "1", Priority = RecommendationPriority.Critical, ImpactAnalysis = "Critical" },
            new Recommendation { Id = "2", Priority = RecommendationPriority.High, ImpactAnalysis = "High" },
            new Recommendation { Id = "3", Priority = RecommendationPriority.Medium, ImpactAnalysis = "Medium" },
            new Recommendation { Id = "4", Priority = RecommendationPriority.Low, ImpactAnalysis = "Low" }
        };

        _mockRecommendationEngine.Setup(x => x.GenerateRecommendationsAsync())
            .ReturnsAsync(recommendations);

        var viewModel = new RecommendationsViewModel(_mockRecommendationEngine.Object);
        await viewModel.LoadRecommendationsCommand.ExecuteAsync(null);

        var initialCount = viewModel.Recommendations.Count;

        // WHEN: Enabling Survival Mode
        viewModel.IsSurvivalModeEnabled = true;

        // THEN: Should filter to only Critical and High priority
        viewModel.Recommendations.Should().OnlyContain(r => 
            r.Priority == RecommendationPriority.Critical || 
            r.Priority == RecommendationPriority.High);
        viewModel.FilteredCount.Should().BeLessThan(initialCount);
        viewModel.TotalCount.Should().Be(4);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task SurvivalModeEnabled_ShouldShowFilteredCount()
    {
        // GIVEN: Recommendations loaded
        var recommendations = new List<Recommendation>
        {
            new Recommendation { Priority = RecommendationPriority.Critical, ImpactAnalysis = "Critical" },
            new Recommendation { Priority = RecommendationPriority.High, ImpactAnalysis = "High" },
            new Recommendation { Priority = RecommendationPriority.Medium, ImpactAnalysis = "Medium" }
        };

        _mockRecommendationEngine.Setup(x => x.GenerateRecommendationsAsync())
            .ReturnsAsync(recommendations);

        var viewModel = new RecommendationsViewModel(_mockRecommendationEngine.Object);
        await viewModel.LoadRecommendationsCommand.ExecuteAsync(null);

        // WHEN: Enabling Survival Mode
        viewModel.IsSurvivalModeEnabled = true;

        // THEN: Filtered count should be set
        viewModel.FilteredCount.Should().BeGreaterThan(0);
        viewModel.FilteredCount.Should().BeLessThanOrEqualTo(viewModel.TotalCount);
    }
}
