using AX2012PerformanceOptimizer.Core.Models.MinimalMode;
using AX2012PerformanceOptimizer.Core.Services.MinimalMode;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

public class PerformanceModeServiceTests
{
    private readonly Mock<ILogger<PerformanceModeService>> _loggerMock;
    private readonly PerformanceModeService _service;

    public PerformanceModeServiceTests()
    {
        _loggerMock = new Mock<ILogger<PerformanceModeService>>();
        _service = new PerformanceModeService(_loggerMock.Object);
    }

    [Fact]
    public void IsMinimalModeEnabled_Default_IsFalse()
    {
        // Assert
        _service.IsMinimalModeEnabled.Should().BeFalse();
    }

    [Fact]
    public void GetSettings_ReturnsDefaultSettings()
    {
        // Act
        var settings = _service.GetSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.IsEnabled.Should().BeFalse();
        settings.RefreshIntervalSeconds.Should().Be(300);
        settings.UseSimplifiedUI.Should().BeTrue();
        settings.DisableAnimations.Should().BeTrue();
    }

    [Fact]
    public void UpdateSettings_UpdatesServiceSettings()
    {
        // Arrange
        var newSettings = new MinimalModeSettings
        {
            IsEnabled = true,
            RefreshIntervalSeconds = 600,
            UseSimplifiedUI = false
        };

        // Act
        _service.UpdateSettings(newSettings);

        // Assert
        var settings = _service.GetSettings();
        settings.IsEnabled.Should().BeTrue();
        settings.RefreshIntervalSeconds.Should().Be(600);
        settings.UseSimplifiedUI.Should().BeFalse();
    }

    [Fact]
    public async Task EnableMinimalModeAsync_SetsEnabledToTrue()
    {
        // Act
        await _service.EnableMinimalModeAsync();

        // Assert
        _service.IsMinimalModeEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task DisableMinimalModeAsync_SetsEnabledToFalse()
    {
        // Arrange
        await _service.EnableMinimalModeAsync();

        // Act
        await _service.DisableMinimalModeAsync();

        // Assert
        _service.IsMinimalModeEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleMinimalModeAsync_TogglesState()
    {
        // Arrange
        var initialState = _service.IsMinimalModeEnabled;

        // Act
        await _service.ToggleMinimalModeAsync();

        // Assert
        _service.IsMinimalModeEnabled.Should().Be(!initialState);

        // Act - Toggle again
        await _service.ToggleMinimalModeAsync();

        // Assert
        _service.IsMinimalModeEnabled.Should().Be(initialState);
    }

    [Fact]
    public void MinimalModeChanged_Event_RaisedOnToggle()
    {
        // Arrange
        bool eventRaised = false;
        _service.MinimalModeChanged += (sender, enabled) => eventRaised = true;

        // Act
        _service.ToggleMinimalModeAsync().Wait();

        // Assert
        eventRaised.Should().BeTrue();
    }
}
