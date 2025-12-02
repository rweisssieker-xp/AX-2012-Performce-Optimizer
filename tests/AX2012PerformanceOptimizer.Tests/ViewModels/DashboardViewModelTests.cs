using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Tests.Support;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P0] Unit tests for DashboardViewModel (Epic 4 Story 4.1: Prominent Cost Display)
/// Tests cost metrics display functionality
/// </summary>
public class DashboardViewModelTests : IDisposable
{
    private readonly Mock<ISqlQueryMonitorService> _mockSqlMonitor;
    private readonly Mock<IAosMonitorService> _mockAosMonitor;
    private readonly Mock<IBatchJobMonitorService> _mockBatchJobMonitor;
    private readonly Mock<IDatabaseStatsService> _mockDatabaseStats;
    private readonly string _tempDir;

    public DashboardViewModelTests()
    {
        // GIVEN: Mocked dependencies
        _mockSqlMonitor = new Mock<ISqlQueryMonitorService>();
        _mockAosMonitor = new Mock<IAosMonitorService>();
        _mockBatchJobMonitor = new Mock<IBatchJobMonitorService>();
        _mockDatabaseStats = new Mock<IDatabaseStatsService>();
        _tempDir = TestHelpers.GetTempDirectory();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void Constructor_ShouldInitializeCostProperties()
    {
        // GIVEN: Mocked services
        // WHEN: Creating DashboardViewModel
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // THEN: Cost properties should be initialized with demo data
        viewModel.DailyCost.Should().Be(125.50m);
        viewModel.MonthlyCost.Should().Be(3765.00m);
        viewModel.PotentialSavings.Should().Be(850.00m);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void DailyCost_ShouldBeSettable()
    {
        // GIVEN: DashboardViewModel instance
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // WHEN: Setting DailyCost to a new value
        viewModel.DailyCost = 200.75m;

        // THEN: DailyCost should be updated
        viewModel.DailyCost.Should().Be(200.75m);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void MonthlyCost_ShouldBeSettable()
    {
        // GIVEN: DashboardViewModel instance
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // WHEN: Setting MonthlyCost to a new value
        viewModel.MonthlyCost = 5000.00m;

        // THEN: MonthlyCost should be updated
        viewModel.MonthlyCost.Should().Be(5000.00m);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void PotentialSavings_ShouldBeSettable()
    {
        // GIVEN: DashboardViewModel instance
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // WHEN: Setting PotentialSavings to a new value
        viewModel.PotentialSavings = 1200.50m;

        // THEN: PotentialSavings should be updated
        viewModel.PotentialSavings.Should().Be(1200.50m);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void LoadDemoData_ShouldSetCostProperties()
    {
        // GIVEN: DashboardViewModel instance
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // WHEN: LoadDemoData is called (via constructor)
        // THEN: Cost properties should have demo values
        viewModel.DailyCost.Should().Be(125.50m);
        viewModel.MonthlyCost.Should().Be(3765.00m);
        viewModel.PotentialSavings.Should().Be(850.00m);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void CostProperties_ShouldHaveCorrectInitialValues()
    {
        // GIVEN: DashboardViewModel instance
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // WHEN: ViewModel is initialized
        // THEN: Cost properties should have correct demo values
        viewModel.DailyCost.Should().Be(125.50m);
        viewModel.MonthlyCost.Should().Be(3765.00m);
        viewModel.PotentialSavings.Should().Be(850.00m);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void CostProperties_ShouldSupportDecimalPrecision()
    {
        // GIVEN: DashboardViewModel instance
        var viewModel = new DashboardViewModel(
            _mockSqlMonitor.Object,
            _mockAosMonitor.Object,
            _mockBatchJobMonitor.Object,
            _mockDatabaseStats.Object);

        // WHEN: Setting cost properties with decimal values
        viewModel.DailyCost = 123.456m;
        viewModel.MonthlyCost = 3701.234m;
        viewModel.PotentialSavings = 999.999m;

        // THEN: Values should be stored correctly
        viewModel.DailyCost.Should().Be(123.456m);
        viewModel.MonthlyCost.Should().Be(3701.234m);
        viewModel.PotentialSavings.Should().Be(999.999m);
    }

    public void Dispose()
    {
        TestHelpers.CleanupTempDirectory(_tempDir);
    }
}

