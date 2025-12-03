using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;
using AX2012PerformanceOptimizer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services.PerformanceStack;

/// <summary>
/// Implementation of Performance Stack Service
/// Aggregates performance metrics across all system layers (Database, AOS, Network, Client)
/// </summary>
public class PerformanceStackService : IPerformanceStackService
{
    private readonly ISqlQueryMonitorService _sqlQueryMonitorService;
    private readonly IAosMonitorService _aosMonitorService;
    private readonly ILogger<PerformanceStackService> _logger;

    public PerformanceStackService(
        ISqlQueryMonitorService sqlQueryMonitorService,
        IAosMonitorService aosMonitorService,
        ILogger<PerformanceStackService> logger)
    {
        _sqlQueryMonitorService = sqlQueryMonitorService;
        _aosMonitorService = aosMonitorService;
        _logger = logger;
    }

    public async Task<PerformanceStackData> GetStackMetricsAsync(TimeRange timeRange)
    {
        _logger.LogInformation("Getting performance stack metrics for time range: {StartTime} to {EndTime}",
            timeRange.StartTime, timeRange.EndTime);

        try
        {
            // Collect metrics from all layers in parallel
            var databaseTask = GetDatabaseMetricsAsync(timeRange);
            var aosTask = GetAosMetricsAsync(timeRange);
            var networkTask = GetNetworkMetricsAsync(timeRange);
            var clientTask = GetClientMetricsAsync(timeRange);

            await Task.WhenAll(databaseTask, aosTask, networkTask, clientTask);

            var stackData = new PerformanceStackData
            {
                Timestamp = DateTime.UtcNow,
                Database = await databaseTask,
                AosServer = await aosTask,
                Network = await networkTask,
                Client = await clientTask
            };

            // Detect bottlenecks
            stackData.Bottlenecks = await DetectBottlenecksAsync(stackData);

            _logger.LogInformation("Performance stack metrics collected successfully. Bottlenecks detected: {Count}",
                stackData.Bottlenecks.Count);

            return stackData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance stack metrics");
            throw;
        }
    }

    public async Task<object> GetLayerDetailsAsync(LayerType layer, TimeRange timeRange)
    {
        _logger.LogInformation("Getting layer details for {Layer} in time range: {StartTime} to {EndTime}",
            layer, timeRange.StartTime, timeRange.EndTime);

        try
        {
            return layer switch
            {
                LayerType.Database => await GetDatabaseMetricsAsync(timeRange),
                LayerType.AosServer => await GetAosMetricsAsync(timeRange),
                LayerType.Network => await GetNetworkMetricsAsync(timeRange),
                LayerType.Client => await GetClientMetricsAsync(timeRange),
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Unknown layer type")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting layer details for {Layer}", layer);
            throw;
        }
    }

    public async Task<List<Bottleneck>> DetectBottlenecksAsync(PerformanceStackData stackData)
    {
        _logger.LogInformation("Detecting bottlenecks in performance stack data");

        var bottlenecks = new List<Bottleneck>();

        try
        {
            // Database layer bottlenecks
            bottlenecks.AddRange(DetectDatabaseBottlenecks(stackData.Database));

            // AOS Server layer bottlenecks
            bottlenecks.AddRange(DetectAosBottlenecks(stackData.AosServer));

            // Network layer bottlenecks
            bottlenecks.AddRange(DetectNetworkBottlenecks(stackData.Network));

            // Client layer bottlenecks
            bottlenecks.AddRange(DetectClientBottlenecks(stackData.Client));

            _logger.LogInformation("Bottleneck detection complete. Found {Count} bottlenecks", bottlenecks.Count);

            return bottlenecks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting bottlenecks");
            return bottlenecks; // Return partial results
        }
    }

    private async Task<DatabaseLayerMetrics> GetDatabaseMetricsAsync(TimeRange timeRange)
    {
        try
        {
            var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(100);

            var metrics = new DatabaseLayerMetrics
            {
                CollectedAt = DateTime.UtcNow
            };

            if (queries.Any())
            {
                metrics.AvgQueryExecutionTimeMs = queries.Average(q => q.AvgElapsedTimeMs);
                metrics.LogicalReadsPerSecond = queries.Sum(q => q.AvgLogicalReads);
                metrics.PhysicalReadsPerSecond = queries.Sum(q => q.AvgPhysicalReads);
                metrics.ActiveConnections = queries.Count;

                // Estimate CPU usage based on query CPU time
                var totalCpuTime = queries.Sum(q => q.TotalCpuTimeMs);
                var timeWindowMs = (timeRange.EndTime - timeRange.StartTime).TotalMilliseconds;
                metrics.CpuUsagePercent = Math.Min(100, (totalCpuTime / timeWindowMs) * 100);

                // Estimate lock wait time (simplified - would need actual lock wait stats)
                metrics.AvgLockWaitTimeMs = queries.Any(q => q.AvgElapsedTimeMs > q.AvgCpuTimeMs)
                    ? queries.Where(q => q.AvgElapsedTimeMs > q.AvgCpuTimeMs)
                        .Average(q => q.AvgElapsedTimeMs - q.AvgCpuTimeMs)
                    : 0;
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database metrics");
            return new DatabaseLayerMetrics { CollectedAt = DateTime.UtcNow };
        }
    }

    private async Task<AosLayerMetrics> GetAosMetricsAsync(TimeRange timeRange)
    {
        try
        {
            var aosMetric = await _aosMonitorService.GetAosMetricsAsync();

            return new AosLayerMetrics
            {
                ServerName = aosMetric.ServerName,
                CpuUsagePercent = aosMetric.CpuUsagePercent,
                MemoryUsageMB = aosMetric.MemoryUsageMB,
                ActiveSessions = aosMetric.ActiveUserSessions,
                ActiveThreads = aosMetric.ActiveThreads,
                AvgResponseTimeMs = aosMetric.AvgResponseTimeMs,
                RequestQueueLength = 0, // Would need additional monitoring for this
                CollectedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AOS metrics");
            return new AosLayerMetrics { CollectedAt = DateTime.UtcNow };
        }
    }

    private async Task<NetworkLayerMetrics> GetNetworkMetricsAsync(TimeRange timeRange)
    {
        // Network metrics are not directly available from existing services
        // For now, we'll derive estimates from database and AOS metrics
        // In a real implementation, this would query network monitoring tools

        await Task.CompletedTask; // Placeholder for async pattern

        try
        {
            // Estimate network latency based on query response times
            var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(50);
            var aosMetric = await _aosMonitorService.GetAosMetricsAsync();

            var metrics = new NetworkLayerMetrics
            {
                CollectedAt = DateTime.UtcNow
            };

            if (queries.Any())
            {
                // Estimate latency as difference between elapsed time and CPU time
                var networkDelays = queries
                    .Where(q => q.AvgElapsedTimeMs > q.AvgCpuTimeMs)
                    .Select(q => q.AvgElapsedTimeMs - q.AvgCpuTimeMs)
                    .ToList();

                metrics.AvgLatencyMs = networkDelays.Any() ? networkDelays.Average() : 10; // Default 10ms
            }
            else
            {
                metrics.AvgLatencyMs = 10; // Default estimate
            }

            // Estimate bandwidth based on logical reads (simplified)
            metrics.BandwidthUsageMbps = queries.Sum(q => q.AvgLogicalReads) * 8.0 / 1024 / 1024; // Rough estimate
            metrics.ActiveConnections = aosMetric.ActiveUserSessions;
            metrics.PacketLossPercent = 0; // Would need network monitoring
            metrics.NetworkErrors = 0; // Would need network monitoring

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting network metrics");
            return new NetworkLayerMetrics { CollectedAt = DateTime.UtcNow };
        }
    }

    private async Task<ClientLayerMetrics> GetClientMetricsAsync(TimeRange timeRange)
    {
        // Client metrics are derived from AOS response times and user sessions
        await Task.CompletedTask; // Placeholder for async pattern

        try
        {
            var aosMetric = await _aosMonitorService.GetAosMetricsAsync();
            var queries = await _sqlQueryMonitorService.GetTopExpensiveQueriesAsync(100);

            var metrics = new ClientLayerMetrics
            {
                CollectedAt = DateTime.UtcNow,
                AvgResponseTimeMs = aosMetric.AvgResponseTimeMs,
                AvgUserWaitTimeMs = aosMetric.AvgResponseTimeMs * 1.2, // Estimate: wait time is slightly higher
                ActiveUsers = aosMetric.ActiveUserSessions,
                RequestCount = queries.Sum(q => q.ExecutionCount),
                ErrorRatePercent = 0, // Would need error tracking
                RequestsPerSecond = queries.Any()
                    ? queries.Sum(q => q.ExecutionCount) / Math.Max(1, (timeRange.EndTime - timeRange.StartTime).TotalSeconds)
                    : 0
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client metrics");
            return new ClientLayerMetrics { CollectedAt = DateTime.UtcNow };
        }
    }

    private List<Bottleneck> DetectDatabaseBottlenecks(DatabaseLayerMetrics metrics)
    {
        var bottlenecks = new List<Bottleneck>();

        // High CPU usage
        if (metrics.CpuUsagePercent > 80)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Database,
                Severity = metrics.CpuUsagePercent > 95 ? BottleneckSeverity.Critical : BottleneckSeverity.High,
                Description = $"High CPU usage: {metrics.CpuUsagePercent:F1}%",
                ImpactScore = metrics.CpuUsagePercent,
                MetricName = "CpuUsagePercent",
                CurrentValue = metrics.CpuUsagePercent,
                ThresholdValue = 80
            });
        }

        // High lock wait time
        if (metrics.AvgLockWaitTimeMs > 100)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Database,
                Severity = metrics.AvgLockWaitTimeMs > 500 ? BottleneckSeverity.Critical : BottleneckSeverity.Medium,
                Description = $"High lock wait time: {metrics.AvgLockWaitTimeMs:F1}ms",
                ImpactScore = Math.Min(100, metrics.AvgLockWaitTimeMs / 10),
                MetricName = "AvgLockWaitTimeMs",
                CurrentValue = metrics.AvgLockWaitTimeMs,
                ThresholdValue = 100
            });
        }

        // High I/O
        if (metrics.PhysicalReadsPerSecond > 10000)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Database,
                Severity = BottleneckSeverity.Medium,
                Description = $"High physical I/O: {metrics.PhysicalReadsPerSecond:N0} reads/sec",
                ImpactScore = Math.Min(100, metrics.PhysicalReadsPerSecond / 100),
                MetricName = "PhysicalReadsPerSecond",
                CurrentValue = metrics.PhysicalReadsPerSecond,
                ThresholdValue = 10000
            });
        }

        return bottlenecks;
    }

    private List<Bottleneck> DetectAosBottlenecks(AosLayerMetrics metrics)
    {
        var bottlenecks = new List<Bottleneck>();

        // High CPU usage
        if (metrics.CpuUsagePercent > 80)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.AosServer,
                Severity = metrics.CpuUsagePercent > 95 ? BottleneckSeverity.Critical : BottleneckSeverity.High,
                Description = $"High AOS CPU usage: {metrics.CpuUsagePercent:F1}%",
                ImpactScore = metrics.CpuUsagePercent,
                MetricName = "CpuUsagePercent",
                CurrentValue = metrics.CpuUsagePercent,
                ThresholdValue = 80
            });
        }

        // High response time
        if (metrics.AvgResponseTimeMs > 1000)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.AosServer,
                Severity = metrics.AvgResponseTimeMs > 5000 ? BottleneckSeverity.Critical : BottleneckSeverity.High,
                Description = $"High AOS response time: {metrics.AvgResponseTimeMs:F1}ms",
                ImpactScore = Math.Min(100, metrics.AvgResponseTimeMs / 50),
                MetricName = "AvgResponseTimeMs",
                CurrentValue = metrics.AvgResponseTimeMs,
                ThresholdValue = 1000
            });
        }

        // High request queue
        if (metrics.RequestQueueLength > 10)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.AosServer,
                Severity = metrics.RequestQueueLength > 50 ? BottleneckSeverity.Critical : BottleneckSeverity.Medium,
                Description = $"High request queue length: {metrics.RequestQueueLength}",
                ImpactScore = Math.Min(100, metrics.RequestQueueLength * 2),
                MetricName = "RequestQueueLength",
                CurrentValue = metrics.RequestQueueLength,
                ThresholdValue = 10
            });
        }

        return bottlenecks;
    }

    private List<Bottleneck> DetectNetworkBottlenecks(NetworkLayerMetrics metrics)
    {
        var bottlenecks = new List<Bottleneck>();

        // High latency
        if (metrics.AvgLatencyMs > 100)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Network,
                Severity = metrics.AvgLatencyMs > 500 ? BottleneckSeverity.Critical : BottleneckSeverity.Medium,
                Description = $"High network latency: {metrics.AvgLatencyMs:F1}ms",
                ImpactScore = Math.Min(100, metrics.AvgLatencyMs / 5),
                MetricName = "AvgLatencyMs",
                CurrentValue = metrics.AvgLatencyMs,
                ThresholdValue = 100
            });
        }

        // Packet loss
        if (metrics.PacketLossPercent > 1)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Network,
                Severity = metrics.PacketLossPercent > 5 ? BottleneckSeverity.Critical : BottleneckSeverity.High,
                Description = $"Packet loss detected: {metrics.PacketLossPercent:F1}%",
                ImpactScore = metrics.PacketLossPercent * 10,
                MetricName = "PacketLossPercent",
                CurrentValue = metrics.PacketLossPercent,
                ThresholdValue = 1
            });
        }

        return bottlenecks;
    }

    private List<Bottleneck> DetectClientBottlenecks(ClientLayerMetrics metrics)
    {
        var bottlenecks = new List<Bottleneck>();

        // High response time
        if (metrics.AvgResponseTimeMs > 2000)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Client,
                Severity = metrics.AvgResponseTimeMs > 10000 ? BottleneckSeverity.Critical : BottleneckSeverity.High,
                Description = $"High client response time: {metrics.AvgResponseTimeMs:F1}ms",
                ImpactScore = Math.Min(100, metrics.AvgResponseTimeMs / 100),
                MetricName = "AvgResponseTimeMs",
                CurrentValue = metrics.AvgResponseTimeMs,
                ThresholdValue = 2000
            });
        }

        // High error rate
        if (metrics.ErrorRatePercent > 1)
        {
            bottlenecks.Add(new Bottleneck
            {
                Layer = LayerType.Client,
                Severity = metrics.ErrorRatePercent > 5 ? BottleneckSeverity.Critical : BottleneckSeverity.High,
                Description = $"High error rate: {metrics.ErrorRatePercent:F1}%",
                ImpactScore = metrics.ErrorRatePercent * 10,
                MetricName = "ErrorRatePercent",
                CurrentValue = metrics.ErrorRatePercent,
                ThresholdValue = 1
            });
        }

        return bottlenecks;
    }
}
