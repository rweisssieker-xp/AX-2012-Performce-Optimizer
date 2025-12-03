using AX2012PerformanceOptimizer.Core.Models.Sonification;
using AX2012PerformanceOptimizer.Core.Services;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Collections.Concurrent;

namespace AX2012PerformanceOptimizer.Core.Services.Sonification;

/// <summary>
/// Service for performance sonification (audio feedback)
/// </summary>
public class SonificationService : ISonificationService, IDisposable
{
    private readonly ISqlQueryMonitorService _queryMonitor;
    private readonly IAosMonitorService _aosMonitor;
    private readonly ILogger<SonificationService> _logger;
    private SonificationSettings _settings;
    private WaveOutEvent? _waveOut;
    private bool _isMonitoring;
    private readonly ConcurrentQueue<SonificationEvent> _eventQueue = new();
    private CancellationTokenSource? _monitoringCancellationTokenSource;

    public SonificationService(
        ISqlQueryMonitorService queryMonitor,
        IAosMonitorService aosMonitor,
        ILogger<SonificationService> logger)
    {
        _queryMonitor = queryMonitor;
        _aosMonitor = aosMonitor;
        _logger = logger;
        _settings = new SonificationSettings();
    }

    public async Task PlayMetricSoundAsync(MetricType metricType, double value)
    {
        if (!_settings.IsEnabled)
            return;

        try
        {
            var mapping = GetMappingForMetric(metricType);
            if (!mapping.IsEnabled)
                return;

            // Calculate pitch based on value
            var normalizedValue = NormalizeValue(value, mapping.MinValue, mapping.MaxValue);
            var pitchHz = MapToPitch(normalizedValue, mapping.MinPitchHz, mapping.MaxPitchHz);

            // Calculate volume
            var volume = (_settings.Volume / 100.0) * mapping.VolumeMultiplier;
            volume = Math.Clamp(volume, 0.0, 1.0);

            await PlayToneAsync(pitchHz, volume, 0.1); // 100ms tone

            _eventQueue.Enqueue(new SonificationEvent
            {
                MetricType = metricType,
                Value = value,
                PitchHz = pitchHz,
                Volume = volume
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing metric sound for {MetricType} with value {Value}", metricType, value);
        }
    }

    public async Task PlayAlertAsync(AlertType alertType, AlertSeverity severity)
    {
        if (!_settings.IsEnabled || !_settings.EnableAlerts)
            return;

        try
        {
            // Different alert tones based on severity
            var pitchHz = severity switch
            {
                AlertSeverity.Low => 400.0,
                AlertSeverity.Medium => 600.0,
                AlertSeverity.High => 800.0,
                AlertSeverity.Critical => 1000.0,
                _ => 600.0
            };

            var volume = (_settings.Volume / 100.0) * 0.8; // Slightly quieter for alerts
            var duration = severity switch
            {
                AlertSeverity.Critical => 0.3,
                AlertSeverity.High => 0.2,
                _ => 0.15
            };

            // Play alert pattern (beep-beep)
            await PlayToneAsync(pitchHz, volume, duration);
            await Task.Delay(50);
            await PlayToneAsync(pitchHz, volume, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing alert for {AlertType} with severity {Severity}", alertType, severity);
        }
    }

    public void UpdateSettings(SonificationSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger.LogInformation("Sonification settings updated. Enabled: {IsEnabled}, Volume: {Volume}", 
            _settings.IsEnabled, _settings.Volume);
    }

    public SonificationSettings GetSettings()
    {
        return _settings;
    }

    public async Task StartMonitoringAsync()
    {
        if (_isMonitoring)
            return;

        _isMonitoring = true;
        _monitoringCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _monitoringCancellationTokenSource.Token;

        _logger.LogInformation("Starting sonification monitoring");

        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested && _isMonitoring)
            {
                try
                {
                    await MonitorMetricsAsync(cancellationToken);
                    await Task.Delay(5000, cancellationToken); // Check every 5 seconds
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in sonification monitoring loop");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }, cancellationToken);

        await Task.CompletedTask;
    }

    public async Task StopMonitoringAsync()
    {
        if (!_isMonitoring)
            return;

        _isMonitoring = false;
        _monitoringCancellationTokenSource?.Cancel();
        _monitoringCancellationTokenSource?.Dispose();
        _monitoringCancellationTokenSource = null;

        _logger.LogInformation("Stopped sonification monitoring");
        await Task.CompletedTask;
    }

    public async Task TestSoundAsync(double pitchHz, double volume)
    {
        try
        {
            await PlayToneAsync(pitchHz, Math.Clamp(volume, 0.0, 1.0), 0.2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing test sound");
        }
    }

    private async Task MonitorMetricsAsync(CancellationToken cancellationToken)
    {
        if (!_settings.IsEnabled)
            return;

        try
        {
            // Monitor query performance
            var queries = await _queryMonitor.GetTopExpensiveQueriesAsync(5);
            foreach (var query in queries)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (query.AvgElapsedTimeMs > _settings.SlowQueryThresholdMs)
                {
                    await PlayAlertAsync(AlertType.SlowQuery, 
                        query.AvgElapsedTimeMs > _settings.SlowQueryThresholdMs * 2 
                            ? AlertSeverity.High 
                            : AlertSeverity.Medium);
                }
                else
                {
                    await PlayMetricSoundAsync(MetricType.QueryPerformance, query.AvgElapsedTimeMs);
                }
            }

            // Monitor AOS metrics
            var aosMetrics = await _aosMonitor.GetAosMetricsAsync();
            if (aosMetrics.CpuUsagePercent > _settings.HighCpuThresholdPercent)
            {
                await PlayAlertAsync(AlertType.HighCpuUsage,
                    aosMetrics.CpuUsagePercent > 90 ? AlertSeverity.Critical : AlertSeverity.High);
            }
            else
            {
                await PlayMetricSoundAsync(MetricType.CpuUsage, aosMetrics.CpuUsagePercent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring metrics for sonification");
        }
    }

    private async Task PlayToneAsync(double frequencyHz, double volume, double durationSeconds)
    {
        if (_waveOut == null)
        {
            _waveOut = new WaveOutEvent();
        }

        try
        {
            var sampleRate = 44100;
            var samples = (int)(sampleRate * durationSeconds);
            var buffer = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                var time = (double)i / sampleRate;
                buffer[i] = (float)(Math.Sin(2 * Math.PI * frequencyHz * time) * volume);
            }

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            var waveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = samples * 4,
                DiscardOnBufferOverflow = true
            };

            // Convert float[] to byte[]
            var byteBuffer = new byte[buffer.Length * 4];
            Buffer.BlockCopy(buffer, 0, byteBuffer, 0, byteBuffer.Length);
            waveProvider.AddSamples(byteBuffer, 0, byteBuffer.Length);

            _waveOut.Init(waveProvider);
            _waveOut.Volume = (float)Math.Clamp(volume, 0.0, 1.0);
            _waveOut.Play();

            await Task.Delay((int)(durationSeconds * 1000));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing tone at {Frequency}Hz", frequencyHz);
        }
    }

    private AudioMapping GetMappingForMetric(MetricType metricType)
    {
        return metricType switch
        {
            MetricType.QueryPerformance => _settings.QueryPerformanceMapping,
            MetricType.CpuUsage => _settings.CpuUsageMapping,
            MetricType.DatabaseHealth => _settings.DatabaseHealthMapping,
            _ => new AudioMapping { IsEnabled = false }
        };
    }

    private double NormalizeValue(double value, double minValue, double maxValue)
    {
        if (maxValue == minValue)
            return 0.5;

        var normalized = (value - minValue) / (maxValue - minValue);
        return Math.Clamp(normalized, 0.0, 1.0);
    }

    private double MapToPitch(double normalizedValue, double minPitchHz, double maxPitchHz)
    {
        // Inverse mapping: low values = high pitch (fast queries), high values = low pitch (slow queries)
        var invertedValue = 1.0 - normalizedValue;
        return minPitchHz + (invertedValue * (maxPitchHz - minPitchHz));
    }

    public void Dispose()
    {
        StopMonitoringAsync().Wait(1000);
        _waveOut?.Dispose();
        _monitoringCancellationTokenSource?.Dispose();
    }
}
