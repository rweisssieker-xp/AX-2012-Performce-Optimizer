using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.ChainReaction;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Tests.ViewModels;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P1] Unit tests for ChainReactionViewModel
/// Tests view model commands and properties
/// </summary>
public class ChainReactionViewModelTests
{
    private readonly Mock<IQueryCorrelationEngine> _mockCorrelationEngine;
    private readonly Mock<ISqlQueryMonitorService> _mockSqlMonitor;
    private readonly Mock<ILogger<ChainReactionViewModel>> _mockLogger;

    public ChainReactionViewModelTests()
    {
        _mockCorrelationEngine = new Mock<IQueryCorrelationEngine>();
        _mockSqlMonitor = new Mock<ISqlQueryMonitorService>();
        _mockLogger = new Mock<ILogger<ChainReactionViewModel>>();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Constructor_ShouldInitializeProperties()
    {
        // GIVEN: A new ChainReactionViewModel
        // WHEN: Creating the view model
        var viewModel = new ChainReactionViewModel(
            _mockCorrelationEngine.Object,
            _mockSqlMonitor.Object,
            _mockLogger.Object);

        // THEN: Properties should be initialized
        viewModel.Graph.Should().BeNull();
        viewModel.ImpactResult.Should().BeNull();
        viewModel.SelectedQueryHash.Should().BeNull();
        viewModel.AvailableQueries.Should().NotBeNull();
        viewModel.AvailableQueries.Should().BeEmpty();
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task LoadGraphCommand_ShouldLoadDependencyGraph()
    {
        // GIVEN: Mock services return data
        var queries = new List<SqlQueryMetric>
        {
            new SqlQueryMetric { QueryHash = "hash1", AvgElapsedTimeMs = 100 },
            new SqlQueryMetric { QueryHash = "hash2", AvgElapsedTimeMs = 200 }
        };

        var expectedGraph = new DependencyGraph
        {
            TotalNodes = 2,
            TotalEdges = 1
        };

        _mockSqlMonitor.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(queries);

        _mockCorrelationEngine.Setup(x => x.BuildDependencyGraphAsync(It.IsAny<List<SqlQueryMetric>>()))
            .ReturnsAsync(expectedGraph);

        var viewModel = new ChainReactionViewModel(
            _mockCorrelationEngine.Object,
            _mockSqlMonitor.Object,
            _mockLogger.Object);

        // WHEN: Executing load graph command
        await viewModel.LoadGraphCommand.ExecuteAsync(null);

        // THEN: Graph should be loaded
        viewModel.Graph.Should().NotBeNull();
        viewModel.Graph.Should().BeEquivalentTo(expectedGraph);
        viewModel.AvailableQueries.Should().HaveCount(2);
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task SelectQueryCommand_ShouldSetSelectedQuery()
    {
        // GIVEN: A view model with loaded queries
        var queryHash = "test-hash-123";
        var expectedImpact = new CascadeImpactResult
        {
            SourceQueryHash = queryHash,
            QueriesAffected = 3,
            TotalTimeSaved = 500
        };

        _mockCorrelationEngine.Setup(x => x.PredictCascadeImpactAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedImpact);

        var viewModel = new ChainReactionViewModel(
            _mockCorrelationEngine.Object,
            _mockSqlMonitor.Object,
            _mockLogger.Object);

        // WHEN: Executing select query command
        await viewModel.SelectQueryCommand.ExecuteAsync(queryHash);

        // THEN: Selected query should be set and impact predicted
        viewModel.SelectedQueryHash.Should().Be(queryHash);
        viewModel.ImpactResult.Should().NotBeNull();
        viewModel.ImpactResult.SourceQueryHash.Should().Be(queryHash);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task PredictImpactCommand_ShouldCalculateImpact()
    {
        // GIVEN: A view model with selected query
        var queryHash = "test-hash-123";
        var expectedImpact = new CascadeImpactResult
        {
            SourceQueryHash = queryHash,
            QueriesAffected = 5,
            TotalTimeSaved = 1000,
            Confidence = 85.0
        };

        _mockCorrelationEngine.Setup(x => x.PredictCascadeImpactAsync(queryHash, It.IsAny<string>()))
            .ReturnsAsync(expectedImpact);

        var viewModel = new ChainReactionViewModel(
            _mockCorrelationEngine.Object,
            _mockSqlMonitor.Object,
            _mockLogger.Object);
        viewModel.SelectedQueryHash = queryHash;

        // WHEN: Executing predict impact command
        await viewModel.PredictImpactCommand.ExecuteAsync(null);

        // THEN: Impact result should be set
        viewModel.ImpactResult.Should().NotBeNull();
        viewModel.ImpactResult.Should().BeEquivalentTo(expectedImpact);
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetImpactTypeDisplayName_ShouldReturnCorrectName()
    {
        // GIVEN: Different impact types
        // WHEN: Getting display names
        // THEN: Should return correct names
        ChainReactionViewModel.GetImpactTypeDisplayName(ImpactType.Positive).Should().Be("Improved");
        ChainReactionViewModel.GetImpactTypeDisplayName(ImpactType.Neutral).Should().Be("Neutral");
        ChainReactionViewModel.GetImpactTypeDisplayName(ImpactType.Negative).Should().Be("Degraded");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetImpactTypeColor_ShouldReturnCorrectColor()
    {
        // GIVEN: Different impact types
        // WHEN: Getting colors
        // THEN: Should return correct color codes
        ChainReactionViewModel.GetImpactTypeColor(ImpactType.Positive).Should().Be("#4CAF50");
        ChainReactionViewModel.GetImpactTypeColor(ImpactType.Neutral).Should().Be("#FFC107");
        ChainReactionViewModel.GetImpactTypeColor(ImpactType.Negative).Should().Be("#F44336");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetConfidenceLevel_ShouldReturnCorrectLevel()
    {
        // GIVEN: Different confidence scores
        // WHEN: Getting confidence levels
        // THEN: Should return correct levels
        ChainReactionViewModel.GetConfidenceLevel(90).Should().Be("High");
        ChainReactionViewModel.GetConfidenceLevel(65).Should().Be("Medium");
        ChainReactionViewModel.GetConfidenceLevel(30).Should().Be("Low");
    }
}
