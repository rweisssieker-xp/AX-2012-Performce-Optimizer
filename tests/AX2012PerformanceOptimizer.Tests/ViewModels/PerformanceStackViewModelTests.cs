using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;
using AX2012PerformanceOptimizer.Core.Services.PerformanceStack;
using AX2012PerformanceOptimizer.Tests.ViewModels;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P1] Unit tests for PerformanceStackViewModel
/// Tests view model commands and properties
/// </summary>
public class PerformanceStackViewModelTests
{
    private readonly Mock<IPerformanceStackService> _mockService;
    private readonly Mock<ILogger<PerformanceStackViewModel>> _mockLogger;

    public PerformanceStackViewModelTests()
    {
        _mockService = new Mock<IPerformanceStackService>();
        _mockLogger = new Mock<ILogger<PerformanceStackViewModel>>();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Constructor_ShouldInitializeProperties()
    {
        // GIVEN: A new PerformanceStackViewModel
        // WHEN: Creating the view model
        var viewModel = new PerformanceStackViewModel(_mockService.Object, _mockLogger.Object);

        // THEN: Properties should be initialized
        viewModel.StackData.Should().BeNull();
        viewModel.Bottlenecks.Should().NotBeNull();
        viewModel.Bottlenecks.Should().BeEmpty();
        viewModel.SelectedTimeRange.Should().NotBeNull();
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task RefreshCommand_ShouldUpdateStackData()
    {
        // GIVEN: Mock service returns stack data
        var expectedData = new PerformanceStackData
        {
            Timestamp = DateTime.UtcNow,
            Database = new DatabaseLayerMetrics { CpuUsagePercent = 50 },
            AosServer = new AosLayerMetrics { ActiveSessions = 5 },
            Network = new NetworkLayerMetrics { AvgLatencyMs = 50 },
            Client = new ClientLayerMetrics { ActiveUsers = 10 },
            Bottlenecks = new List<Bottleneck>
            {
                new Bottleneck { Layer = LayerType.Database, Severity = BottleneckSeverity.Medium }
            }
        };

        _mockService.Setup(x => x.GetStackMetricsAsync(It.IsAny<TimeRange>()))
            .ReturnsAsync(expectedData);

        var viewModel = new PerformanceStackViewModel(_mockService.Object, _mockLogger.Object);

        // WHEN: Executing refresh command
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // THEN: Stack data should be updated
        viewModel.StackData.Should().NotBeNull();
        viewModel.StackData.Should().BeEquivalentTo(expectedData);
        viewModel.Bottlenecks.Should().HaveCount(1);
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task DrillDownCommand_ShouldSetSelectedLayer()
    {
        // GIVEN: A view model
        _mockService.Setup(x => x.GetLayerDetailsAsync(It.IsAny<LayerType>(), It.IsAny<TimeRange>()))
            .ReturnsAsync(new DatabaseLayerMetrics());

        var viewModel = new PerformanceStackViewModel(_mockService.Object, _mockLogger.Object);

        // WHEN: Executing drill down command for Database layer
        await viewModel.DrillDownCommand.ExecuteAsync(LayerType.Database);

        // THEN: Selected layer should be set
        viewModel.SelectedLayer.Should().Be(LayerType.Database);
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task DrillDownCommand_ShouldDeselectWhenSameLayerClicked()
    {
        // GIVEN: A view model with selected layer
        var viewModel = new PerformanceStackViewModel(_mockService.Object, _mockLogger.Object);
        viewModel.SelectedLayer = LayerType.Database;

        // WHEN: Executing drill down command for same layer
        await viewModel.DrillDownCommand.ExecuteAsync(LayerType.Database);

        // THEN: Selected layer should be cleared
        viewModel.SelectedLayer.Should().BeNull();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task TimeRangeChangedCommand_ShouldRefreshData()
    {
        // GIVEN: A view model with existing data
        var initialData = new PerformanceStackData { Timestamp = DateTime.UtcNow };
        _mockService.Setup(x => x.GetStackMetricsAsync(It.IsAny<TimeRange>()))
            .ReturnsAsync(initialData);

        var viewModel = new PerformanceStackViewModel(_mockService.Object, _mockLogger.Object);
        viewModel.StackData = initialData;

        // WHEN: Changing time range
        viewModel.SelectedTimeRange = TimeRange.Last24Hours;
        await viewModel.TimeRangeChangedCommand.ExecuteAsync(null);

        // THEN: Data should be refreshed
        _mockService.Verify(x => x.GetStackMetricsAsync(It.IsAny<TimeRange>()), Times.Once);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetLayerDisplayName_ShouldReturnCorrectName()
    {
        // GIVEN: Different layer types
        // WHEN: Getting display names
        // THEN: Should return correct names
        PerformanceStackViewModel.GetLayerDisplayName(LayerType.Database).Should().Be("Database");
        PerformanceStackViewModel.GetLayerDisplayName(LayerType.AosServer).Should().Be("AOS Server");
        PerformanceStackViewModel.GetLayerDisplayName(LayerType.Network).Should().Be("Network");
        PerformanceStackViewModel.GetLayerDisplayName(LayerType.Client).Should().Be("Client");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetSeverityColor_ShouldReturnCorrectColor()
    {
        // GIVEN: Different severity levels
        // WHEN: Getting colors
        // THEN: Should return correct color codes
        PerformanceStackViewModel.GetSeverityColor(BottleneckSeverity.Critical).Should().Be("#FF0000");
        PerformanceStackViewModel.GetSeverityColor(BottleneckSeverity.High).Should().Be("#FF8800");
        PerformanceStackViewModel.GetSeverityColor(BottleneckSeverity.Medium).Should().Be("#FFAA00");
        PerformanceStackViewModel.GetSeverityColor(BottleneckSeverity.Low).Should().Be("#88AA00");
    }
}
