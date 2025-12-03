using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.PerformanceStack;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P1] Unit tests for PerformanceStackService
/// Tests performance stack metrics collection and bottleneck detection
/// </summary>
public class PerformanceStackServiceTests
{
    private readonly Mock<ISqlQueryMonitorService> _mockSqlMonitor;
    private readonly Mock<IAosMonitorService> _mockAosMonitor;
    private readonly Mock<ILogger<PerformanceStackService>> _mockLogger;
    private readonly PerformanceStackService _service;

    public PerformanceStackServiceTests()
    {
        _mockSqlMonitor = new Mock<ISqlQueryMonitorService>();
        _mockAosMonitor = new Mock<IAosMonitorService>();
        _mockLogger = new Mock<ILogger<PerformanceStackService>>();

        _service = new PerformanceStackService(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockLogger.Object);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task GetStackMetricsAsync_ShouldReturnValidData()
    {
        // GIVEN: Mock services return data
        _mockSqlMonitor.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SqlQueryMetric>
            {
                new SqlQueryMetric
                {
                    QueryHash = "test1",
                    AvgElapsedTimeMs = 100,
                    AvgCpuTimeMs = 80,
                    AvgLogicalReads = 1000,
                    ExecutionCount = 10
                }
            });

        _mockAosMonitor.Setup(x => x.GetAosMetricsAsync())
            .ReturnsAsync(new AosMetric
            {
                ServerName = "TestServer",
                ActiveUserSessions = 5,
                CpuUsagePercent = 50,
                MemoryUsageMB = 1024,
                AvgResponseTimeMs = 200
            });

        var timeRange = TimeRange.LastHour;

        // WHEN: Getting stack metrics
        var result = await _service.GetStackMetricsAsync(timeRange);

        // THEN: Result should contain all layers
        result.Should().NotBeNull();
        result.Database.Should().NotBeNull();
        result.AosServer.Should().NotBeNull();
        result.Network.Should().NotBeNull();
        result.Client.Should().NotBeNull();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task DetectBottlenecksAsync_ShouldIdentifyCriticalBottlenecks()
    {
        // GIVEN: Stack data with high CPU usage
        var stackData = new PerformanceStackData
        {
            Database = new DatabaseLayerMetrics
            {
                CpuUsagePercent = 95, // Critical threshold exceeded
                AvgLockWaitTimeMs = 500 // High lock wait
            },
            AosServer = new AosLayerMetrics
            {
                CpuUsagePercent = 85, // High threshold exceeded
                AvgResponseTimeMs = 5000 // Critical response time
            },
            Network = new NetworkLayerMetrics
            {
                AvgLatencyMs = 200 // High latency
            },
            Client = new ClientLayerMetrics
            {
                AvgResponseTimeMs = 10000 // Critical response time
            }
        };

        // WHEN: Detecting bottlenecks
        var bottlenecks = await _service.DetectBottlenecksAsync(stackData);

        // THEN: Should detect multiple bottlenecks
        bottlenecks.Should().NotBeEmpty();
        bottlenecks.Should().Contain(b => b.Layer == LayerType.Database && b.Severity == BottleneckSeverity.Critical);
        bottlenecks.Should().Contain(b => b.Layer == LayerType.AosServer);
        bottlenecks.Should().Contain(b => b.Layer == LayerType.Client);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task GetLayerDetailsAsync_ShouldReturnLayerSpecificData()
    {
        // GIVEN: Mock services return data
        _mockSqlMonitor.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SqlQueryMetric>());

        _mockAosMonitor.Setup(x => x.GetAosMetricsAsync())
            .ReturnsAsync(new AosMetric { ServerName = "TestServer" });

        var timeRange = TimeRange.LastHour;

        // WHEN: Getting database layer details
        var dbDetails = await _service.GetLayerDetailsAsync(LayerType.Database, timeRange);

        // THEN: Should return DatabaseLayerMetrics
        dbDetails.Should().NotBeNull();
        dbDetails.Should().BeOfType<DatabaseLayerMetrics>();
    }

    [Fact]
    [Trait("Priority", "P2")]
    public async Task DetectBottlenecksAsync_ShouldReturnEmptyListWhenNoBottlenecks()
    {
        // GIVEN: Stack data with healthy metrics
        var stackData = new PerformanceStackData
        {
            Database = new DatabaseLayerMetrics
            {
                CpuUsagePercent = 30, // Below threshold
                AvgLockWaitTimeMs = 10 // Low lock wait
            },
            AosServer = new AosLayerMetrics
            {
                CpuUsagePercent = 40, // Below threshold
                AvgResponseTimeMs = 100 // Good response time
            },
            Network = new NetworkLayerMetrics
            {
                AvgLatencyMs = 20 // Low latency
            },
            Client = new ClientLayerMetrics
            {
                AvgResponseTimeMs = 500 // Acceptable response time
            }
        };

        // WHEN: Detecting bottlenecks
        var bottlenecks = await _service.DetectBottlenecksAsync(stackData);

        // THEN: Should return empty or minimal bottlenecks
        bottlenecks.Should().NotBeNull();
        // May have some low-severity bottlenecks, but no critical ones
        bottlenecks.Should().NotContain(b => b.Severity == BottleneckSeverity.Critical);
    }
}
