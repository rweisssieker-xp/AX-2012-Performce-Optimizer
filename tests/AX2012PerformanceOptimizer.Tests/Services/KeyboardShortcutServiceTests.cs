using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AX2012PerformanceOptimizer.Tests.Support;
using AX2012PerformanceOptimizer.WpfApp.Services;
using FluentAssertions;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P0] Unit tests for KeyboardShortcutService
/// Tests critical keyboard shortcut functionality
/// </summary>
public class KeyboardShortcutServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly KeyboardShortcutService _service;

    public KeyboardShortcutServiceTests()
    {
        // GIVEN: A temporary directory for test settings
        _tempDir = TestHelpers.GetTempDirectory();
        
        // Override the settings path using reflection for testing
        var serviceType = typeof(KeyboardShortcutService);
        var constructor = serviceType.GetConstructor(Type.EmptyTypes);
        _service = (KeyboardShortcutService)constructor!.Invoke(null);
        
        // Use reflection to set private _settingsPath field
        var settingsPathField = serviceType.GetField("_settingsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var testSettingsPath = Path.Combine(_tempDir, "keyboard-shortcuts.json");
        settingsPathField?.SetValue(_service, testSettingsPath);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void RegisterShortcut_ShouldStoreShortcut()
    {
        // GIVEN: A keyboard shortcut service
        // WHEN: Registering a new shortcut
        var actionExecuted = false;
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { actionExecuted = true; }, "Test action");

        // THEN: Shortcut should be registered
        var shortcuts = _service.GetAllShortcuts();
        shortcuts.Should().ContainKey("test-action");
        shortcuts["test-action"].modifiers.Should().Be(ModifierKeys.Control);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void TryExecute_ShouldExecuteAction_WhenShortcutMatches()
    {
        // GIVEN: A registered shortcut
        var actionExecuted = false;
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { actionExecuted = true; }, "Test action");

        // WHEN: Executing the shortcut
        var result = _service.TryExecute(Key.K, ModifierKeys.Control);

        // THEN: Action should be executed
        result.Should().BeTrue();
        actionExecuted.Should().BeTrue();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void TryExecute_ShouldNotExecuteAction_WhenShortcutDoesNotMatch()
    {
        // GIVEN: A registered shortcut
        var actionExecuted = false;
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { actionExecuted = true; }, "Test action");

        // WHEN: Executing a different shortcut
        var result = _service.TryExecute(Key.E, ModifierKeys.Control);

        // THEN: Action should not be executed
        result.Should().BeFalse();
        actionExecuted.Should().BeFalse();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void UpdateShortcut_ShouldUpdateExistingShortcut()
    {
        // GIVEN: A registered shortcut
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { }, "Test action");

        // WHEN: Updating the shortcut
        _service.UpdateShortcut("test-action", "E", ModifierKeys.Control);

        // THEN: Shortcut should be updated
        var keyBinding = _service.GetKeyBinding("test-action");
        keyBinding.Should().NotBeNull();
        keyBinding!.Key.Should().Be(Key.E);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void SaveShortcuts_ShouldPersistToFile()
    {
        // GIVEN: Registered shortcuts
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { }, "Test action");
        var settingsPath = Path.Combine(_tempDir, "keyboard-shortcuts.json");

        // WHEN: Saving shortcuts
        _service.SaveShortcuts();

        // THEN: File should exist
        File.Exists(settingsPath).Should().BeTrue();
        var content = File.ReadAllText(settingsPath);
        content.Should().Contain("test-action");
        content.Should().Contain("K");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void LoadShortcuts_ShouldRestoreFromFile()
    {
        // GIVEN: Saved shortcuts
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { }, "Test action");
        _service.SaveShortcuts();

        // WHEN: Creating a new service instance, registering the same shortcut, and loading
        var newService = new KeyboardShortcutService();
        var settingsPathField = typeof(KeyboardShortcutService).GetField("_settingsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var testSettingsPath = Path.Combine(_tempDir, "keyboard-shortcuts.json");
        settingsPathField?.SetValue(newService, testSettingsPath);
        
        // Register shortcut first (LoadShortcuts only updates existing shortcuts)
        newService.RegisterShortcut("test-action", ModifierKeys.Control, Key.E, () => { }, "Test action");
        newService.LoadShortcuts();

        // THEN: Shortcuts should be restored (key should be K, not E)
        var shortcuts = newService.GetAllShortcuts();
        shortcuts.Should().ContainKey("test-action");
        var keyBinding = newService.GetKeyBinding("test-action");
        keyBinding.Should().NotBeNull();
        keyBinding!.Key.Should().Be(Key.K); // Should be restored from file, not default E
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void GetKeyBinding_ShouldReturnBinding_WhenShortcutExists()
    {
        // GIVEN: A registered shortcut
        _service.RegisterShortcut("test-action", ModifierKeys.Control, Key.K, () => { }, "Test action");

        // WHEN: Getting key binding
        var binding = _service.GetKeyBinding("test-action");

        // THEN: Binding should be returned
        binding.Should().NotBeNull();
        binding!.Key.Should().Be(Key.K);
        binding.Modifiers.Should().Be(ModifierKeys.Control);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void GetKeyBinding_ShouldReturnNull_WhenShortcutDoesNotExist()
    {
        // GIVEN: No registered shortcut
        // WHEN: Getting key binding for non-existent shortcut
        var binding = _service.GetKeyBinding("non-existent");

        // THEN: Binding should be null
        binding.Should().BeNull();
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetAllShortcuts_ShouldReturnAllRegisteredShortcuts()
    {
        // GIVEN: Multiple registered shortcuts
        _service.RegisterShortcut("action1", ModifierKeys.Control, Key.K, () => { }, "Action 1");
        _service.RegisterShortcut("action2", ModifierKeys.Control, Key.E, () => { }, "Action 2");

        // WHEN: Getting all shortcuts
        var shortcuts = _service.GetAllShortcuts();

        // THEN: All shortcuts should be returned
        shortcuts.Should().HaveCount(2);
        shortcuts.Should().ContainKey("action1");
        shortcuts.Should().ContainKey("action2");
    }

    public void Dispose()
    {
        TestHelpers.CleanupTempDirectory(_tempDir);
    }
}

