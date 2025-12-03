using AX2012PerformanceOptimizer.Core.Models.Sonification;

namespace AX2012PerformanceOptimizer.Core.Services.Sonification;

/// <summary>
/// Service for performance sonification (audio feedback)
/// </summary>
public interface ISonificationService
{
    /// <summary>
    /// Plays audio feedback for a metric value
    /// </summary>
    Task PlayMetricSoundAsync(MetricType metricType, double value);

    /// <summary>
    /// Plays an alert sound
    /// </summary>
    Task PlayAlertAsync(AlertType alertType, AlertSeverity severity);

    /// <summary>
    /// Updates sonification settings
    /// </summary>
    void UpdateSettings(SonificationSettings settings);

    /// <summary>
    /// Gets current settings
    /// </summary>
    SonificationSettings GetSettings();

    /// <summary>
    /// Starts background monitoring
    /// </summary>
    Task StartMonitoringAsync();

    /// <summary>
    /// Stops background monitoring
    /// </summary>
    Task StopMonitoringAsync();

    /// <summary>
    /// Tests audio playback with current settings
    /// </summary>
    Task TestSoundAsync(double pitchHz, double volume);
}
