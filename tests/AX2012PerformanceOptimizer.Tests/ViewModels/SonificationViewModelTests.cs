using AX2012PerformanceOptimizer.Core.Models.Sonification;
using AX2012PerformanceOptimizer.Core.Services.Sonification;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

public class SonificationViewModelTests
{
    private readonly Mock<ISonificationService> _sonificationServiceMock;
    private readonly SonificationViewModel _viewModel;

    public SonificationViewModelTests()
    {
        _sonificationServiceMock = new Mock<ISonificationService>();
        
        var defaultSettings = new SonificationSettings
        {
            IsEnabled = false,
            Volume = 50,
            MinPitchHz = 200.0,
            MaxPitchHz = 2000.0,
            EnableAlerts = true
        };

        _sonificationServiceMock.Setup(x => x.GetSettings())
            .Returns(defaultSettings);

        _viewModel = new SonificationViewModel(_sonificationServiceMock.Object);
    }

    [Fact]
    public void Constructor_LoadsSettings()
    {
        // Assert
        _viewModel.IsEnabled.Should().BeFalse();
        _viewModel.Volume.Should().Be(50);
        _viewModel.MinPitchHz.Should().Be(200.0);
        _viewModel.MaxPitchHz.Should().Be(2000.0);
        _viewModel.EnableAlerts.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleSonificationCommand_TogglesEnabledState()
    {
        // Arrange
        var initialState = _viewModel.IsEnabled;

        // Act
        await _viewModel.ToggleSonificationCommand.ExecuteAsync(null);

        // Assert
        _viewModel.IsEnabled.Should().Be(!initialState);
    }

    [Fact]
    public async Task TestSoundCommand_CallsService()
    {
        // Arrange
        _viewModel.Volume = 50;
        _viewModel.MinPitchHz = 200.0;
        _viewModel.MaxPitchHz = 2000.0;

        // Act
        await _viewModel.TestSoundCommand.ExecuteAsync(null);

        // Assert
        _sonificationServiceMock.Verify(x => x.TestSoundAsync(
            It.IsAny<double>(),
            It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public async Task StartMonitoringCommand_CallsService()
    {
        // Act
        await _viewModel.StartMonitoringCommand.ExecuteAsync(null);

        // Assert
        _sonificationServiceMock.Verify(x => x.StartMonitoringAsync(), Times.Once);
    }

    [Fact]
    public async Task StopMonitoringCommand_CallsService()
    {
        // Act
        await _viewModel.StopMonitoringCommand.ExecuteAsync(null);

        // Assert
        _sonificationServiceMock.Verify(x => x.StopMonitoringAsync(), Times.Once);
    }

    [Fact]
    public void Volume_Changed_UpdatesSettings()
    {
        // Arrange
        _viewModel.Volume = 50;

        // Act
        _viewModel.Volume = 75;

        // Assert
        _sonificationServiceMock.Verify(x => x.UpdateSettings(It.Is<SonificationSettings>(s => s.Volume == 75)), Times.Once);
    }
}
