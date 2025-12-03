using AX2012PerformanceOptimizer.Core.Models.StakeholderDashboard;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Services.StakeholderDashboard;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

public class DashboardViewModelRoleTests
{
    private readonly Mock<ISqlQueryMonitorService> _sqlMonitorMock;
    private readonly Mock<IAosMonitorService> _aosMonitorMock;
    private readonly Mock<IBatchJobMonitorService> _batchJobMonitorMock;
    private readonly Mock<IDatabaseStatsService> _databaseStatsMock;
    private readonly Mock<IRoleBasedDashboardService> _roleDashboardServiceMock;
    private readonly DashboardViewModel _viewModel;

    public DashboardViewModelRoleTests()
    {
        _sqlMonitorMock = new Mock<ISqlQueryMonitorService>();
        _aosMonitorMock = new Mock<IAosMonitorService>();
        _batchJobMonitorMock = new Mock<IBatchJobMonitorService>();
        _databaseStatsMock = new Mock<IDatabaseStatsService>();
        _roleDashboardServiceMock = new Mock<IRoleBasedDashboardService>();

        _roleDashboardServiceMock.Setup(x => x.GetAvailableRoles())
            .Returns(new List<UserRole> { UserRole.Executive, UserRole.DBA, UserRole.Developer, UserRole.EndUser });

        _viewModel = new DashboardViewModel(
            _sqlMonitorMock.Object,
            _aosMonitorMock.Object,
            _batchJobMonitorMock.Object,
            _databaseStatsMock.Object,
            _roleDashboardServiceMock.Object);
    }

    [Fact]
    public void Constructor_InitializesAvailableRoles()
    {
        // Assert
        _viewModel.AvailableRoles.Should().HaveCount(4);
        _viewModel.AvailableRoles.Should().Contain(UserRole.Executive);
        _viewModel.AvailableRoles.Should().Contain(UserRole.DBA);
        _viewModel.AvailableRoles.Should().Contain(UserRole.Developer);
        _viewModel.AvailableRoles.Should().Contain(UserRole.EndUser);
    }

    [Fact]
    public void SelectedRole_DefaultValue_IsDBA()
    {
        // Assert
        _viewModel.SelectedRole.Should().Be(UserRole.DBA);
    }

    [Fact]
    public void SelectedRole_Changed_UpdatesRole()
    {
        // Act
        _viewModel.SelectedRole = UserRole.Developer;

        // Assert
        _viewModel.SelectedRole.Should().Be(UserRole.Developer);
    }
}
