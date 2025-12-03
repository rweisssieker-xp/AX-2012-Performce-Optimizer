using AX2012PerformanceOptimizer.Core.Models.MinimalMode;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services.MinimalMode;

/// <summary>
/// Service for managing performance mode (minimal mode)
/// </summary>
public class PerformanceModeService : IPerformanceModeService
{
    private readonly ILogger<PerformanceModeService> _logger;
    private MinimalModeSettings _settings;

    public bool IsMinimalModeEnabled => _settings.IsEnabled;

    public event EventHandler<bool>? MinimalModeChanged;

    public PerformanceModeService(ILogger<PerformanceModeService> logger)
    {
        _logger = logger;
        _settings = new MinimalModeSettings();
    }

    public MinimalModeSettings GetSettings()
    {
        return _settings;
    }

    public void UpdateSettings(MinimalModeSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger.LogInformation("Minimal mode settings updated. Enabled: {IsEnabled}, RefreshInterval: {RefreshInterval}s",
            _settings.IsEnabled, _settings.RefreshIntervalSeconds);
    }

    public async Task EnableMinimalModeAsync()
    {
        if (_settings.IsEnabled)
            return;

        _settings.IsEnabled = true;
        _logger.LogInformation("Minimal mode enabled");
        MinimalModeChanged?.Invoke(this, true);
        await Task.CompletedTask;
    }

    public async Task DisableMinimalModeAsync()
    {
        if (!_settings.IsEnabled)
            return;

        _settings.IsEnabled = false;
        _logger.LogInformation("Minimal mode disabled");
        MinimalModeChanged?.Invoke(this, false);
        await Task.CompletedTask;
    }

    public async Task ToggleMinimalModeAsync()
    {
        if (_settings.IsEnabled)
        {
            await DisableMinimalModeAsync();
        }
        else
        {
            await EnableMinimalModeAsync();
        }
    }
}
