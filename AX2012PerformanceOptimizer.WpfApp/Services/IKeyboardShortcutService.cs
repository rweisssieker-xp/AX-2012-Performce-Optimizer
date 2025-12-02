using System.Windows.Input;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for managing keyboard shortcuts and their customization
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Register a keyboard shortcut with an action
    /// </summary>
    void RegisterShortcut(string actionId, ModifierKeys modifiers, Key key, Action action, string description);

    /// <summary>
    /// Get all registered shortcuts
    /// </summary>
    Dictionary<string, (ModifierKeys modifiers, string description)> GetAllShortcuts();

    /// <summary>
    /// Update a shortcut key combination
    /// </summary>
    void UpdateShortcut(string actionId, string newKey, ModifierKeys newModifiers);

    /// <summary>
    /// Load shortcuts from user settings
    /// </summary>
    void LoadShortcuts();

    /// <summary>
    /// Save shortcuts to user settings
    /// </summary>
    void SaveShortcuts();
    
    /// <summary>
    /// Get KeyBinding for a specific action
    /// </summary>
    KeyBinding? GetKeyBinding(string actionId);
}

