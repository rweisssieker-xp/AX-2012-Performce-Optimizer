using System;
using System.Collections.Generic;
using AX2012PerformanceOptimizer.Data.Configuration;
using AX2012PerformanceOptimizer.Data.SqlServer;
using AX2012PerformanceOptimizer.Tests.Support;
using AX2012PerformanceOptimizer.WpfApp.Services;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Moq;
using System.Windows.Input;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P2] Unit tests for SettingsViewModel Quick Actions functionality (Epic 5 Story 5.2)
/// Tests Quick Actions customization
/// </summary>
public class SettingsViewModelQuickActionsTests : IDisposable
{
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<ISqlConnectionManager> _mockSqlConnectionManager;
    private readonly Mock<IKeyboardShortcutService> _mockKeyboardShortcutService;
    private readonly PlainLanguageService _plainLanguageService;
    private readonly Mock<IQuickActionsService> _mockQuickActionsService;
    private readonly string _tempDir;

    public SettingsViewModelQuickActionsTests()
    {
        _mockConfigService = new Mock<IConfigurationService>();
        _mockSqlConnectionManager = new Mock<ISqlConnectionManager>();
        _mockKeyboardShortcutService = new Mock<IKeyboardShortcutService>();
        _plainLanguageService = new PlainLanguageService();
        _mockQuickActionsService = new Mock<IQuickActionsService>();
        _tempDir = TestHelpers.GetTempDirectory();

        // Setup keyboard shortcut service mock (required for constructor)
        _mockKeyboardShortcutService.Setup(x => x.GetAllShortcuts())
            .Returns(new Dictionary<string, (System.Windows.Input.ModifierKeys modifiers, string description)>());
        _mockKeyboardShortcutService.Setup(x => x.GetKeyBinding(It.IsAny<string>()))
            .Returns((System.Windows.Input.KeyBinding?)null);

        _mockQuickActionsService.Setup(x => x.GetAllAvailableActions())
            .Returns(new List<QuickActionDefinition>
            {
                new QuickActionDefinition { Id = "export", DisplayText = "Export", Description = "Export data", ShortcutText = "Ctrl+E", IsEnabled = true, Order = 0 },
                new QuickActionDefinition { Id = "dashboard", DisplayText = "Dashboard", Description = "Go to dashboard", ShortcutText = "Ctrl+D", IsEnabled = true, Order = 1 }
            });
    }

    private SettingsViewModel CreateViewModel()
    {
        return new SettingsViewModel(
            _mockConfigService.Object,
            _mockSqlConnectionManager.Object,
            _mockKeyboardShortcutService.Object,
            _plainLanguageService,
            _mockQuickActionsService.Object
        );
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void LoadQuickActions_ShouldLoadActionsFromService()
    {
        // GIVEN: SettingsViewModel with QuickActionsService
        var viewModel = CreateViewModel();

        // WHEN: QuickActions are loaded (via constructor)
        // THEN: QuickActions collection should be populated
        viewModel.QuickActions.Should().NotBeEmpty();
        viewModel.QuickActions.Should().HaveCount(2);
        viewModel.QuickActions.Should().Contain(a => a.Id == "export");
        viewModel.QuickActions.Should().Contain(a => a.Id == "dashboard");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void SaveQuickActions_ShouldSaveToService()
    {
        // GIVEN: SettingsViewModel with modified QuickActions
        var viewModel = CreateViewModel();
        viewModel.QuickActions[0].IsEnabled = false;

        // WHEN: Saving QuickActions
        viewModel.SaveQuickActionsCommand.Execute(null);

        // THEN: Service should be called to save
        _mockQuickActionsService.Verify(x => x.SaveActions(It.IsAny<List<QuickActionDefinition>>()), Times.Once);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void ResetQuickActions_ShouldResetToDefaults()
    {
        // GIVEN: SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Resetting QuickActions
        viewModel.ResetQuickActionsCommand.Execute(null);

        // THEN: Service should reset and actions should be reloaded
        _mockQuickActionsService.Verify(x => x.ResetToDefaults(), Times.Once);
        _mockQuickActionsService.Verify(x => x.GetAllAvailableActions(), Times.AtLeast(2)); // Once in constructor, once in reset
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void MoveQuickActionUp_ShouldChangeOrder()
    {
        // GIVEN: SettingsViewModel with QuickActions
        var viewModel = CreateViewModel();
        var secondAction = viewModel.QuickActions[1];
        var initialOrder = secondAction.Order;

        // WHEN: Moving action up
        viewModel.MoveQuickActionUpCommand.Execute(secondAction);

        // THEN: Order should be updated
        secondAction.Order.Should().BeLessThan(initialOrder);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void MoveQuickActionDown_ShouldChangeOrder()
    {
        // GIVEN: SettingsViewModel with QuickActions
        var viewModel = CreateViewModel();
        var firstAction = viewModel.QuickActions[0];
        var initialOrder = firstAction.Order;

        // WHEN: Moving action down
        viewModel.MoveQuickActionDownCommand.Execute(firstAction);

        // THEN: Order should be updated
        firstAction.Order.Should().BeGreaterThan(initialOrder);
    }

    public void Dispose()
    {
        TestHelpers.CleanupTempDirectory(_tempDir);
    }
}

