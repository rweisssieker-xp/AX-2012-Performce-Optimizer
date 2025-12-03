using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AX2012PerformanceOptimizer.Tests.Support;
using AX2012PerformanceOptimizer.WpfApp.Services;
using FluentAssertions;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P2] Unit tests for QuickActionsService (Epic 5 Story 5.2: Customizable Quick Actions)
/// Tests Quick Actions customization and persistence
/// </summary>
public class QuickActionsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly QuickActionsService _service;

    public QuickActionsServiceTests()
    {
        // GIVEN: Temporary directory for test files
        _tempDir = TestHelpers.GetTempDirectory();
        
        // Create service with custom path
        _service = new QuickActionsService();
        var settingsPathField = typeof(QuickActionsService).GetField("_settingsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (settingsPathField != null)
        {
            var testSettingsPath = Path.Combine(_tempDir, "quick-actions.json");
            settingsPathField.SetValue(_service, testSettingsPath);
        }
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetAllAvailableActions_ShouldReturnDefaultActions()
    {
        // GIVEN: QuickActionsService instance
        // WHEN: Getting all available actions
        var actions = _service.GetAllAvailableActions();

        // THEN: Default actions should be returned
        actions.Should().NotBeEmpty();
        actions.Should().HaveCount(6);
        actions.Should().Contain(a => a.Id == "export");
        actions.Should().Contain(a => a.Id == "dashboard");
        actions.Should().Contain(a => a.Id == "performance");
        actions.Should().Contain(a => a.Id == "settings");
        actions.Should().Contain(a => a.Id == "executive-summary");
        actions.Should().Contain(a => a.Id == "search-queries");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetEnabledActions_ShouldReturnOnlyEnabledActions()
    {
        // GIVEN: QuickActionsService with some disabled actions
        var allActions = _service.GetAllAvailableActions();
        var modifiedActions = allActions.Select(a =>
        {
            var action = new QuickActionDefinition
            {
                Id = a.Id,
                DisplayText = a.DisplayText,
                Description = a.Description,
                ShortcutText = a.ShortcutText,
                IsEnabled = a.Id != "executive-summary", // Disable one action
                Order = a.Order
            };
            return action;
        }).ToList();

        _service.SaveActions(modifiedActions);

        // WHEN: Getting enabled actions
        var enabledActions = _service.GetEnabledActions();

        // THEN: Only enabled actions should be returned
        enabledActions.Should().HaveCount(5);
        enabledActions.Should().NotContain(a => a.Id == "executive-summary");
        enabledActions.Should().Contain(a => a.Id == "export");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void SaveActions_ShouldPersistToFile()
    {
        // GIVEN: Modified actions
        var actions = new List<QuickActionDefinition>
        {
            new QuickActionDefinition
            {
                Id = "export",
                DisplayText = "Custom Export",
                Description = "Custom description",
                ShortcutText = "Ctrl+E",
                IsEnabled = true,
                Order = 0
            }
        };

        // WHEN: Saving actions
        _service.SaveActions(actions);

        // THEN: File should be created
        var settingsPathField = typeof(QuickActionsService).GetField("_settingsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var settingsPath = settingsPathField?.GetValue(_service) as string;
        if (settingsPath != null)
        {
            File.Exists(settingsPath).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void SaveActions_ShouldPreserveOrder()
    {
        // GIVEN: Actions with custom order
        var actions = new List<QuickActionDefinition>
        {
            new QuickActionDefinition { Id = "dashboard", Order = 0, IsEnabled = true, DisplayText = "Dashboard", Description = "", ShortcutText = "" },
            new QuickActionDefinition { Id = "export", Order = 1, IsEnabled = true, DisplayText = "Export", Description = "", ShortcutText = "" },
            new QuickActionDefinition { Id = "settings", Order = 2, IsEnabled = true, DisplayText = "Settings", Description = "", ShortcutText = "" }
        };

        // WHEN: Saving actions
        _service.SaveActions(actions);
        
        // THEN: Actions should be saved and order preserved
        var reloadedActions = _service.GetAllAvailableActions();
        reloadedActions.Should().HaveCount(3);
        reloadedActions[0].Id.Should().Be("dashboard");
        reloadedActions[1].Id.Should().Be("export");
        reloadedActions[2].Id.Should().Be("settings");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void ResetToDefaults_ShouldRestoreDefaultActions()
    {
        // GIVEN: Modified actions
        var modifiedActions = new List<QuickActionDefinition>
        {
            new QuickActionDefinition { Id = "custom", DisplayText = "Custom", Description = "", ShortcutText = "", IsEnabled = true, Order = 0 }
        };
        _service.SaveActions(modifiedActions);

        // WHEN: Resetting to defaults
        _service.ResetToDefaults();
        var defaultActions = _service.GetAllAvailableActions();

        // THEN: Default actions should be restored
        defaultActions.Should().HaveCount(6);
        defaultActions.Should().Contain(a => a.Id == "export");
        defaultActions.Should().NotContain(a => a.Id == "custom");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetEnabledActions_ShouldReturnActionsInOrder()
    {
        // GIVEN: Actions with custom order
        var actions = new List<QuickActionDefinition>
        {
            new QuickActionDefinition { Id = "settings", Order = 0, IsEnabled = true, DisplayText = "Settings", Description = "", ShortcutText = "" },
            new QuickActionDefinition { Id = "export", Order = 1, IsEnabled = true, DisplayText = "Export", Description = "", ShortcutText = "" },
            new QuickActionDefinition { Id = "dashboard", Order = 2, IsEnabled = false, DisplayText = "Dashboard", Description = "", ShortcutText = "" },
            new QuickActionDefinition { Id = "performance", Order = 3, IsEnabled = true, DisplayText = "Performance", Description = "", ShortcutText = "" }
        };
        _service.SaveActions(actions);

        // WHEN: Getting enabled actions
        var enabledActions = _service.GetEnabledActions();

        // THEN: Actions should be in order and only enabled
        enabledActions.Should().HaveCount(3);
        enabledActions[0].Id.Should().Be("settings");
        enabledActions[1].Id.Should().Be("export");
        enabledActions[2].Id.Should().Be("performance");
    }

    public void Dispose()
    {
        TestHelpers.CleanupTempDirectory(_tempDir);
    }
}

