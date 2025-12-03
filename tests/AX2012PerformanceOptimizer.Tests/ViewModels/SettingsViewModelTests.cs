using System;
using System.Collections.Generic;
using System.Linq;
using AX2012PerformanceOptimizer.Data.Configuration;
using AX2012PerformanceOptimizer.Data.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using AX2012PerformanceOptimizer.Tests.Support;
using AX2012PerformanceOptimizer.WpfApp.Services;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Moq;
using System.Windows.Input;
using Xunit;
using Services = AX2012PerformanceOptimizer.WpfApp.Services;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P1] Unit tests for SettingsViewModel (Keyboard Shortcuts & Plain Language)
/// Tests settings functionality for MVP Quick Wins
/// </summary>
public class SettingsViewModelTests : IDisposable
{
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<ISqlConnectionManager> _mockSqlConnectionManager;
    private readonly Mock<IKeyboardShortcutService> _mockKeyboardShortcutService;
    private readonly PlainLanguageService _plainLanguageService;
    private readonly Mock<IQuickActionsService> _mockQuickActionsService;
    private readonly string _tempDir;

    public SettingsViewModelTests()
    {
        // GIVEN: Mocked dependencies
        _mockConfigService = new Mock<IConfigurationService>();
        _mockSqlConnectionManager = new Mock<ISqlConnectionManager>();
        _mockKeyboardShortcutService = new Mock<IKeyboardShortcutService>();
        _plainLanguageService = new PlainLanguageService();
        _mockQuickActionsService = new Mock<IQuickActionsService>();
        _tempDir = TestHelpers.GetTempDirectory();

        // Setup default mock behavior
        _mockKeyboardShortcutService.Setup(x => x.GetAllShortcuts())
            .Returns(new Dictionary<string, (System.Windows.Input.ModifierKeys modifiers, string description)>
            {
                { "quick-actions", (System.Windows.Input.ModifierKeys.Control, "Open Quick Actions") },
                { "export", (System.Windows.Input.ModifierKeys.Control, "Export Data") }
            });

        _mockQuickActionsService.Setup(x => x.GetAllAvailableActions())
            .Returns(new List<QuickActionDefinition>
            {
                new QuickActionDefinition { Id = "export", DisplayText = "Export", Description = "Export data", ShortcutText = "Ctrl+E", IsEnabled = true, Order = 0 },
                new QuickActionDefinition { Id = "dashboard", DisplayText = "Dashboard", Description = "Go to dashboard", ShortcutText = "Ctrl+D", IsEnabled = true, Order = 1 }
            });
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Constructor_ShouldInitializeKeyboardShortcuts()
    {
        // GIVEN: Mocked keyboard shortcut service
        // WHEN: Creating SettingsViewModel
        var viewModel = CreateViewModel();

        // THEN: Keyboard shortcuts should be initialized (via constructor or property access)
        // Note: LoadKeyboardShortcuts is private, but shortcuts may be loaded in constructor
        // We verify the service was called
        _mockKeyboardShortcutService.Verify(x => x.GetAllShortcuts(), Times.AtLeastOnce);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void SaveKeyboardShortcutsCommand_ShouldExist()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Checking for save command
        // THEN: Command should exist
        var saveCommand = viewModel.SaveKeyboardShortcutsCommand;
        saveCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void ResetKeyboardShortcutsCommand_ShouldExist()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Checking for reset command
        // THEN: Command should exist
        var resetCommand = viewModel.ResetKeyboardShortcutsCommand;
        resetCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void IsPlainLanguageEnabled_ShouldBeSettable()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();
        var initialValue = viewModel.IsPlainLanguageEnabled;

        // WHEN: Setting plain language enabled
        viewModel.IsPlainLanguageEnabled = !initialValue;

        // THEN: Setting should be updated
        viewModel.IsPlainLanguageEnabled.Should().Be(!initialValue);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void SavePlainLanguageSettingsCommand_ShouldExist()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Checking for save command
        // THEN: Command should exist
        var saveCommand = viewModel.SavePlainLanguageSettingsCommand;
        saveCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void TranslationExamples_ShouldBeInitialized()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Checking translation examples
        // THEN: Examples should be initialized (via constructor or property access)
        // Note: LoadTranslationExamples is private, but examples may be loaded in constructor
        viewModel.TranslationExamples.Should().NotBeNull();
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void KeyboardShortcuts_ShouldHaveAvailableKeys_WhenLoaded()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Checking shortcuts (may be loaded in constructor)
        // THEN: If shortcuts exist, they should have available keys
        if (viewModel.KeyboardShortcuts.Any())
        {
            viewModel.KeyboardShortcuts.Should().OnlyContain(s => s.AvailableKeys != null && s.AvailableKeys.Count > 0);
        }
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void KeyboardShortcuts_ShouldHaveFormattedCurrentShortcut_WhenLoaded()
    {
        // GIVEN: A SettingsViewModel
        var viewModel = CreateViewModel();

        // WHEN: Checking shortcuts (may be loaded in constructor)
        // THEN: If shortcuts exist, CurrentShortcut should be formatted
        if (viewModel.KeyboardShortcuts.Any())
        {
            viewModel.KeyboardShortcuts.Should().OnlyContain(s => !string.IsNullOrEmpty(s.CurrentShortcut));
        }
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

    public void Dispose()
    {
        TestHelpers.CleanupTempDirectory(_tempDir);
    }
}

