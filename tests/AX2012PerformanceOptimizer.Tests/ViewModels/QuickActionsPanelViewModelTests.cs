using System;
using System.Linq;
using AX2012PerformanceOptimizer.Tests.ViewModels;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P1] Unit tests for QuickActionsPanelViewModel
/// Tests quick actions panel functionality
/// </summary>
public class QuickActionsPanelViewModelTests
{
    [Fact]
    [Trait("Priority", "P1")]
    public void Constructor_ShouldInitializeDefaultActions()
    {
        // GIVEN: A new QuickActionsPanelViewModel
        // WHEN: Creating the view model
        var viewModel = new QuickActionsPanelViewModel();

        // THEN: Default actions should be initialized
        viewModel.QuickActions.Should().NotBeEmpty();
        viewModel.QuickActions.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void QuickActions_ShouldContainExportAction()
    {
        // GIVEN: A QuickActionsPanelViewModel
        var viewModel = new QuickActionsPanelViewModel();

        // WHEN: Checking actions
        // THEN: Export action should be present
        viewModel.QuickActions.Should().Contain(a => a.DisplayText.Contains("Export"));
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void QuickActions_ShouldContainDashboardAction()
    {
        // GIVEN: A QuickActionsPanelViewModel
        var viewModel = new QuickActionsPanelViewModel();

        // WHEN: Checking actions
        // THEN: Dashboard action should be present
        viewModel.QuickActions.Should().Contain(a => a.DisplayText.Contains("Dashboard"));
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void QuickActions_ShouldContainSettingsAction()
    {
        // GIVEN: A QuickActionsPanelViewModel
        var viewModel = new QuickActionsPanelViewModel();

        // WHEN: Checking actions
        // THEN: Settings action should be present
        viewModel.QuickActions.Should().Contain(a => a.DisplayText.Contains("Settings"));
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void ExecuteNavigateToDashboard_ShouldRaiseNavigateToTabEvent()
    {
        // GIVEN: A QuickActionsPanelViewModel with event handler
        var viewModel = new QuickActionsPanelViewModel();
        int? navigatedTab = null;
        viewModel.NavigateToTab += (tab) => { navigatedTab = tab; };

        // WHEN: Executing dashboard navigation
        var dashboardAction = viewModel.QuickActions.FirstOrDefault(a => a.DisplayText.Contains("Dashboard"));
        dashboardAction?.Command.Execute(null);

        // THEN: NavigateToTab event should be raised with tab 0
        navigatedTab.Should().Be(0);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void ExecuteNavigateToPerformance_ShouldRaiseNavigateToTabEvent()
    {
        // GIVEN: A QuickActionsPanelViewModel with event handler
        var viewModel = new QuickActionsPanelViewModel();
        int? navigatedTab = null;
        viewModel.NavigateToTab += (tab) => { navigatedTab = tab; };

        // WHEN: Executing performance navigation
        var performanceAction = viewModel.QuickActions.FirstOrDefault(a => a.DisplayText.Contains("Performance"));
        performanceAction?.Command.Execute(null);

        // THEN: NavigateToTab event should be raised with tab 2
        navigatedTab.Should().Be(2);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void ExecuteNavigateToSettings_ShouldRaiseNavigateToTabEvent()
    {
        // GIVEN: A QuickActionsPanelViewModel with event handler
        var viewModel = new QuickActionsPanelViewModel();
        int? navigatedTab = null;
        viewModel.NavigateToTab += (tab) => { navigatedTab = tab; };

        // WHEN: Executing settings navigation
        var settingsAction = viewModel.QuickActions.FirstOrDefault(a => a.DisplayText.Contains("Settings"));
        settingsAction?.Command.Execute(null);

        // THEN: NavigateToTab event should be raised with tab 1
        navigatedTab.Should().Be(1);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void QuickActions_ShouldHaveCommands()
    {
        // GIVEN: A QuickActionsPanelViewModel
        var viewModel = new QuickActionsPanelViewModel();

        // WHEN: Checking all actions
        // THEN: All actions should have commands
        viewModel.QuickActions.Should().OnlyContain(a => a.Command != null);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void QuickActions_ShouldHaveDescriptions()
    {
        // GIVEN: A QuickActionsPanelViewModel
        var viewModel = new QuickActionsPanelViewModel();

        // WHEN: Checking all actions
        // THEN: All actions should have descriptions
        viewModel.QuickActions.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Description));
    }
}

