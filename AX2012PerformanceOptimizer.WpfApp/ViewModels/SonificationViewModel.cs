using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models.Sonification;
using AX2012PerformanceOptimizer.Core.Services.Sonification;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class SonificationViewModel : ObservableObject
{
    private readonly ISonificationService _sonificationService;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private int volume = 50;

    [ObservableProperty]
    private double minPitchHz = 200.0;

    [ObservableProperty]
    private double maxPitchHz = 2000.0;

    [ObservableProperty]
    private bool enableAlerts = true;

    [ObservableProperty]
    private double slowQueryThresholdMs = 1000.0;

    [ObservableProperty]
    private double highCpuThresholdPercent = 80.0;

    [ObservableProperty]
    private bool isMonitoring;

    [ObservableProperty]
    private string statusMessage = "Sonification disabled";

    public SonificationViewModel(ISonificationService sonificationService)
    {
        _sonificationService = sonificationService;
        LoadSettings();
    }

    partial void OnIsEnabledChanged(bool value)
    {
        if (value)
        {
            StartMonitoringAsync().ConfigureAwait(false);
        }
        else
        {
            StopMonitoringAsync().ConfigureAwait(false);
        }
        UpdateStatus();
    }

    partial void OnVolumeChanged(int value)
    {
        UpdateSettings();
    }

    partial void OnMinPitchHzChanged(double value)
    {
        UpdateSettings();
    }

    partial void OnMaxPitchHzChanged(double value)
    {
        UpdateSettings();
    }

    partial void OnEnableAlertsChanged(bool value)
    {
        UpdateSettings();
    }

    partial void OnSlowQueryThresholdMsChanged(double value)
    {
        UpdateSettings();
    }

    partial void OnHighCpuThresholdPercentChanged(double value)
    {
        UpdateSettings();
    }

    [RelayCommand]
    private async Task ToggleSonificationAsync()
    {
        IsEnabled = !IsEnabled;
    }

    [RelayCommand]
    private async Task TestSoundAsync()
    {
        try
        {
            var testPitch = (MinPitchHz + MaxPitchHz) / 2.0;
            var testVolume = Volume / 100.0;
            await _sonificationService.TestSoundAsync(testPitch, testVolume);
            StatusMessage = "Test sound played";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error playing test sound: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task StartMonitoringAsync()
    {
        try
        {
            await _sonificationService.StartMonitoringAsync();
            IsMonitoring = true;
            UpdateStatus();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error starting monitoring: {ex.Message}";
            MessageBox.Show($"Error starting monitoring: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task StopMonitoringAsync()
    {
        try
        {
            await _sonificationService.StopMonitoringAsync();
            IsMonitoring = false;
            UpdateStatus();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error stopping monitoring: {ex.Message}";
            MessageBox.Show($"Error stopping monitoring: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadSettings()
    {
        var settings = _sonificationService.GetSettings();
        IsEnabled = settings.IsEnabled;
        Volume = settings.Volume;
        MinPitchHz = settings.MinPitchHz;
        MaxPitchHz = settings.MaxPitchHz;
        EnableAlerts = settings.EnableAlerts;
        SlowQueryThresholdMs = settings.SlowQueryThresholdMs;
        HighCpuThresholdPercent = settings.HighCpuThresholdPercent;
        IsMonitoring = settings.IsEnabled;
        UpdateStatus();
    }

    private void UpdateSettings()
    {
        var settings = new SonificationSettings
        {
            IsEnabled = IsEnabled,
            Volume = Volume,
            MinPitchHz = MinPitchHz,
            MaxPitchHz = MaxPitchHz,
            EnableAlerts = EnableAlerts,
            SlowQueryThresholdMs = SlowQueryThresholdMs,
            HighCpuThresholdPercent = HighCpuThresholdPercent,
            QueryPerformanceMapping = new AudioMapping
            {
                IsEnabled = true,
                MinValue = 0.0,
                MaxValue = 10000.0, // 10 seconds
                MinPitchHz = MinPitchHz,
                MaxPitchHz = MaxPitchHz,
                VolumeMultiplier = 1.0
            },
            CpuUsageMapping = new AudioMapping
            {
                IsEnabled = true,
                MinValue = 0.0,
                MaxValue = 100.0,
                MinPitchHz = MinPitchHz,
                MaxPitchHz = MaxPitchHz,
                VolumeMultiplier = 0.8
            },
            DatabaseHealthMapping = new AudioMapping
            {
                IsEnabled = true,
                MinValue = 0.0,
                MaxValue = 100.0,
                MinPitchHz = MinPitchHz,
                MaxPitchHz = MaxPitchHz,
                VolumeMultiplier = 0.9
            }
        };

        _sonificationService.UpdateSettings(settings);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (!IsEnabled)
        {
            StatusMessage = "Sonification disabled";
        }
        else if (IsMonitoring)
        {
            StatusMessage = $"Monitoring active - Volume: {Volume}%";
        }
        else
        {
            StatusMessage = "Sonification enabled but not monitoring";
        }
    }
}
