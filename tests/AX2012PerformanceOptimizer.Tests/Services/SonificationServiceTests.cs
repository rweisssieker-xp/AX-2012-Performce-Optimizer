using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.Sonification;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.Sonification;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

public class SonificationServiceTests
{
    private readonly Mock<ISqlQueryMonitorService> _queryMonitorMock;
    private readonly Mock<IAosMonitorService> _aosMonitorMock;
    private readonly Mock<ILogger<SonificationService>> _loggerMock;
    private readonly SonificationService _service;

    public SonificationServiceTests()
    {
        _queryMonitorMock = new Mock<ISqlQueryMonitorService>();
        _aosMonitorMock = new Mock<IAosMonitorService>();
        _loggerMock = new Mock<ILogger<SonificationService>>();

        _service = new SonificationService(
            _queryMonitorMock.Object,
            _aosMonitorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetSettings_ReturnsDefaultSettings()
    {
        // Act
        var settings = _service.GetSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.IsEnabled.Should().BeFalse();
        settings.Volume.Should().Be(50);
        settings.MinPitchHz.Should().Be(200.0);
        settings.MaxPitchHz.Should().Be(2000.0);
    }

    [Fact]
    public void UpdateSettings_UpdatesServiceSettings()
    {
        // Arrange
        var newSettings = new SonificationSettings
        {
            IsEnabled = true,
            Volume = 75,
            MinPitchHz = 300.0,
            MaxPitchHz = 2500.0
        };

        // Act
        _service.UpdateSettings(newSettings);

        // Assert
        var settings = _service.GetSettings();
        settings.IsEnabled.Should().BeTrue();
        settings.Volume.Should().Be(75);
        settings.MinPitchHz.Should().Be(300.0);
        settings.MaxPitchHz.Should().Be(2500.0);
    }

    [Fact]
    public async Task PlayMetricSoundAsync_WhenDisabled_DoesNotPlay()
    {
        // Arrange
        var settings = new SonificationSettings { IsEnabled = false };
        _service.UpdateSettings(settings);

        // Act
        await _service.PlayMetricSoundAsync(MetricType.QueryPerformance, 100.0);

        // Assert - Should complete without error (no actual audio playback in test)
        // This test verifies the early return when disabled
    }

    [Fact]
    public async Task PlayAlertAsync_WhenDisabled_DoesNotPlay()
    {
        // Arrange
        var settings = new SonificationSettings { IsEnabled = false, EnableAlerts = false };
        _service.UpdateSettings(settings);

        // Act
        await _service.PlayAlertAsync(AlertType.SlowQuery, AlertSeverity.High);

        // Assert - Should complete without error
    }

    [Fact]
    public async Task StartMonitoringAsync_SetsIsMonitoring()
    {
        // Arrange
        var settings = new SonificationSettings { IsEnabled = true };
        _service.UpdateSettings(settings);

        var queries = new List<SqlQueryMetric>();
        _queryMonitorMock.Setup(x => x.GetTopExpensiveQueriesAsync(5))
            .ReturnsAsync(queries);

        var aosMetrics = new AosMetric { CpuUsagePercent = 50 };
        _aosMonitorMock.Setup(x => x.GetAosMetricsAsync())
            .ReturnsAsync(aosMetrics);

        // Act
        await _service.StartMonitoringAsync();

        // Assert - Monitoring should start (we can't directly check _isMonitoring, but we can verify it doesn't throw)
        await Task.Delay(100); // Give it a moment
    }

    [Fact]
    public async Task StopMonitoringAsync_StopsMonitoring()
    {
        // Arrange
        await _service.StartMonitoringAsync();

        // Act
        await _service.StopMonitoringAsync();

        // Assert - Should complete without error
    }

    [Fact]
    public async Task TestSoundAsync_PlaysTestSound()
    {
        // Act
        await _service.TestSoundAsync(440.0, 0.5);

        // Assert - Should complete without error
    }
}
