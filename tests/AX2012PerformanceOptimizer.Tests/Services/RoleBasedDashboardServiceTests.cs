using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;
using AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;
using AX2012PerformanceOptimizer.Core.Services.StakeholderDashboard;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AX2012PerformanceOptimizer.Tests.Services;

public class RoleBasedDashboardServiceTests
{
    private readonly Mock<IExecutiveScorecardService> _executiveScorecardMock;
    private readonly Mock<ISqlQueryMonitorService> _sqlQueryMonitorMock;
    private readonly Mock<IQueryAnalyzerService> _queryAnalyzerMock;
    private readonly Mock<IAosMonitorService> _aosMonitorMock;
    private readonly Mock<IPerformanceCostCalculatorService> _costCalculatorMock;
    private readonly Mock<ILogger<RoleBasedDashboardService>> _loggerMock;
    private readonly RoleBasedDashboardService _service;

    public RoleBasedDashboardServiceTests()
    {
        _executiveScorecardMock = new Mock<IExecutiveScorecardService>();
        _sqlQueryMonitorMock = new Mock<ISqlQueryMonitorService>();
        _queryAnalyzerMock = new Mock<IQueryAnalyzerService>();
        _aosMonitorMock = new Mock<IAosMonitorService>();
        _costCalculatorMock = new Mock<IPerformanceCostCalculatorService>();
        _loggerMock = new Mock<ILogger<RoleBasedDashboardService>>();

        _service = new RoleBasedDashboardService(
            _executiveScorecardMock.Object,
            _sqlQueryMonitorMock.Object,
            _queryAnalyzerMock.Object,
            _aosMonitorMock.Object,
            _costCalculatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetAvailableRoles_ReturnsAllRoles()
    {
        // Act
        var roles = _service.GetAvailableRoles();

        // Assert
        roles.Should().HaveCount(4);
        roles.Should().Contain(UserRole.Executive);
        roles.Should().Contain(UserRole.DBA);
        roles.Should().Contain(UserRole.Developer);
        roles.Should().Contain(UserRole.EndUser);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ExecutiveRole_ReturnsExecutiveData()
    {
        // Arrange
        var timeRange = new TimeRange
        {
            StartTime = DateTime.UtcNow.AddHours(-24),
            EndTime = DateTime.UtcNow
        };

        var scorecard = new ExecutiveScorecard { OverallScore = 85 };
        _executiveScorecardMock.Setup(x => x.GenerateScorecardAsync())
            .ReturnsAsync(scorecard);

        var queries = new List<SqlQueryMetric>
        {
            new SqlQueryMetric { QueryHash = "hash1", AvgElapsedTimeMs = 100 }
        };
        _sqlQueryMonitorMock.Setup(x => x.GetTopExpensiveQueriesAsync(10))
            .ReturnsAsync(queries);

        var executiveSummary = new ExecutiveSummary { TotalDailyCost = 150.0 };
        _costCalculatorMock.Setup(x => x.GenerateExecutiveSummaryAsync(It.IsAny<List<SqlQueryMetric>>(), It.IsAny<CostParameters>()))
            .ReturnsAsync(executiveSummary);

        // Act
        var result = await _service.GetDashboardDataAsync(UserRole.Executive, timeRange);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Executive);
        result.Widgets.Should().NotBeEmpty();
        result.Widgets.Should().Contain(w => w.Type == WidgetType.CostImpact);
        result.Widgets.Should().Contain(w => w.Type == WidgetType.ROI);
        result.Widgets.Should().Contain(w => w.Type == WidgetType.SystemHealthScore);
    }

    [Fact]
    public async Task GetDashboardDataAsync_DBARole_ReturnsDBAData()
    {
        // Arrange
        var timeRange = new TimeRange
        {
            StartTime = DateTime.UtcNow.AddHours(-24),
            EndTime = DateTime.UtcNow
        };

        var queries = new List<SqlQueryMetric>
        {
            new SqlQueryMetric { QueryHash = "hash1", AvgElapsedTimeMs = 500 }
        };
        _sqlQueryMonitorMock.Setup(x => x.GetTopExpensiveQueriesAsync(10))
            .ReturnsAsync(queries);

        // Act
        var result = await _service.GetDashboardDataAsync(UserRole.DBA, timeRange);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.DBA);
        result.Widgets.Should().NotBeEmpty();
        result.Widgets.Should().Contain(w => w.Type == WidgetType.QueryPerformance);
        result.Widgets.Should().Contain(w => w.Type == WidgetType.DatabaseHealth);
    }

    [Fact]
    public async Task GetDashboardDataAsync_DeveloperRole_ReturnsDeveloperData()
    {
        // Arrange
        var timeRange = new TimeRange
        {
            StartTime = DateTime.UtcNow.AddHours(-24),
            EndTime = DateTime.UtcNow
        };

        var queries = new List<SqlQueryMetric>
        {
            new SqlQueryMetric { QueryHash = "hash1", AvgCpuTimeMs = 200 }
        };
        _sqlQueryMonitorMock.Setup(x => x.GetTopExpensiveQueriesAsync(10))
            .ReturnsAsync(queries);

        // Act
        var result = await _service.GetDashboardDataAsync(UserRole.Developer, timeRange);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Developer);
        result.Widgets.Should().NotBeEmpty();
        result.Widgets.Should().Contain(w => w.Type == WidgetType.CodePerformance);
        result.Widgets.Should().Contain(w => w.Type == WidgetType.QueryPerformance);
    }

    [Fact]
    public async Task GetDashboardDataAsync_EndUserRole_ReturnsEndUserData()
    {
        // Arrange
        var timeRange = new TimeRange
        {
            StartTime = DateTime.UtcNow.AddHours(-24),
            EndTime = DateTime.UtcNow
        };

        var aosMetrics = new AosMetric
        {
            AvgResponseTimeMs = 500,
            ActiveUserSessions = 50
        };
        _aosMonitorMock.Setup(x => x.GetAosMetricsAsync())
            .ReturnsAsync(aosMetrics);

        // Act
        var result = await _service.GetDashboardDataAsync(UserRole.EndUser, timeRange);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.EndUser);
        result.Widgets.Should().NotBeEmpty();
        result.Widgets.Should().Contain(w => w.Type == WidgetType.UserExperience);
        result.Widgets.Should().Contain(w => w.Type == WidgetType.SystemAvailability);
    }

    [Fact]
    public async Task GetRoleSpecificWidgetsAsync_ExecutiveRole_ReturnsCorrectWidgets()
    {
        // Arrange
        var scorecard = new ExecutiveScorecard { OverallScore = 85 };
        _executiveScorecardMock.Setup(x => x.GenerateScorecardAsync())
            .ReturnsAsync(scorecard);

        var queries = new List<SqlQueryMetric>();
        _sqlQueryMonitorMock.Setup(x => x.GetTopExpensiveQueriesAsync(It.IsAny<int>()))
            .ReturnsAsync(queries);

        var executiveSummary = new ExecutiveSummary { TotalDailyCost = 100.0 };
        _costCalculatorMock.Setup(x => x.GenerateExecutiveSummaryAsync(It.IsAny<List<SqlQueryMetric>>(), It.IsAny<CostParameters>()))
            .ReturnsAsync(executiveSummary);

        // Act
        var widgets = await _service.GetRoleSpecificWidgetsAsync(UserRole.Executive);

        // Assert
        widgets.Should().NotBeEmpty();
        widgets.Should().Contain(w => w.Type == WidgetType.CostImpact);
        widgets.Should().Contain(w => w.Type == WidgetType.ROI);
        widgets.Should().Contain(w => w.Type == WidgetType.SystemHealthScore);
        widgets.Should().Contain(w => w.Type == WidgetType.SummaryCard);
        widgets.Should().Contain(w => w.Type == WidgetType.TrendVisualization);
        widgets.Should().Contain(w => w.Type == WidgetType.ActionItems);
    }

    [Fact]
    public async Task GetRoleConfigurationAsync_ReturnsCorrectConfiguration()
    {
        // Act
        var executiveConfig = await _service.GetRoleConfigurationAsync(UserRole.Executive);
        var dbaConfig = await _service.GetRoleConfigurationAsync(UserRole.DBA);
        var developerConfig = await _service.GetRoleConfigurationAsync(UserRole.Developer);
        var endUserConfig = await _service.GetRoleConfigurationAsync(UserRole.EndUser);

        // Assert
        executiveConfig.Role.Should().Be(UserRole.Executive);
        executiveConfig.EnabledWidgets.Should().Contain(WidgetType.CostImpact);

        dbaConfig.Role.Should().Be(UserRole.DBA);
        dbaConfig.EnabledWidgets.Should().Contain(WidgetType.QueryPerformance);

        developerConfig.Role.Should().Be(UserRole.Developer);
        developerConfig.EnabledWidgets.Should().Contain(WidgetType.CodePerformance);

        endUserConfig.Role.Should().Be(UserRole.EndUser);
        endUserConfig.EnabledWidgets.Should().Contain(WidgetType.UserExperience);
    }
}
