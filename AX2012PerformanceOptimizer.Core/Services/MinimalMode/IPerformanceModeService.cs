using AX2012PerformanceOptimizer.Core.Models.MinimalMode;

namespace AX2012PerformanceOptimizer.Core.Services.MinimalMode;

/// <summary>
/// Service for managing performance mode (minimal mode)
/// </summary>
public interface IPerformanceModeService
{
    /// <summary>
    /// Whether minimal mode is currently enabled
    /// </summary>
    bool IsMinimalModeEnabled { get; }

    /// <summary>
    /// Gets current minimal mode settings
    /// </summary>
    MinimalModeSettings GetSettings();

    /// <summary>
    /// Updates minimal mode settings
    /// </summary>
    void UpdateSettings(MinimalModeSettings settings);

    /// <summary>
    /// Enables minimal mode
    /// </summary>
    Task EnableMinimalModeAsync();

    /// <summary>
    /// Disables minimal mode
    /// </summary>
    Task DisableMinimalModeAsync();

    /// <summary>
    /// Toggles minimal mode
    /// </summary>
    Task ToggleMinimalModeAsync();

    /// <summary>
    /// Event raised when minimal mode state changes
    /// </summary>
    event EventHandler<bool>? MinimalModeChanged;
}
