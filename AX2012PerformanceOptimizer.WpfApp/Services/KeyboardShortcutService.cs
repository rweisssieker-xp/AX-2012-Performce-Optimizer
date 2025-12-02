using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Win32;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for managing keyboard shortcuts and their customization
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly Dictionary<string, (ModifierKeys modifiers, Key key, Action action, string description)> _shortcuts = new Dictionary<string, (ModifierKeys modifiers, Key key, Action action, string description)>();
    private readonly string _settingsPath;

    public KeyboardShortcutService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "AX2012PerformanceOptimizer");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "keyboard-shortcuts.json");
        
        LoadShortcuts();
    }

    public void RegisterShortcut(string actionId, ModifierKeys modifiers, Key key, Action action, string description)
    {
        _shortcuts[actionId] = (modifiers, key, action, description);
    }

    public Dictionary<string, (ModifierKeys modifiers, string description)> GetAllShortcuts()
    {
        return _shortcuts.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.modifiers, kvp.Value.description)
        );
    }

    public void UpdateShortcut(string actionId, string newKey, ModifierKeys newModifiers)
    {
        if (_shortcuts.TryGetValue(actionId, out var shortcut))
        {
            var key = (Key)Enum.Parse(typeof(Key), newKey);
            _shortcuts[actionId] = (newModifiers, key, shortcut.action, shortcut.description);
            SaveShortcuts();
        }
    }

    public void LoadShortcuts()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                var saved = JsonSerializer.Deserialize<Dictionary<string, ShortcutConfig>>(json);
                if (saved != null)
                {
                    // Restore saved shortcuts (actions will be registered separately)
                    foreach (var kvp in saved)
                    {
                        if (_shortcuts.ContainsKey(kvp.Key))
                        {
                            var existing = _shortcuts[kvp.Key];
                            var key = (Key)Enum.Parse(typeof(Key), kvp.Value.Key);
                            var modifiers = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), kvp.Value.Modifiers);
                            _shortcuts[kvp.Key] = (modifiers, key, existing.action, existing.description);
                        }
                    }
                }
            }
            catch
            {
                // If loading fails, use defaults
            }
        }
    }

    public void SaveShortcuts()
    {
        try
        {
            var config = _shortcuts.ToDictionary(
                kvp => kvp.Key,
                kvp => new ShortcutConfig
                {
                    Key = kvp.Value.key.ToString(),
                    Modifiers = kvp.Value.modifiers.ToString(),
                    Description = kvp.Value.description
                }
            );
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently fail if saving doesn't work
        }
    }

    /// <summary>
    /// Get KeyBinding for a specific action
    /// </summary>
    public KeyBinding? GetKeyBinding(string actionId)
    {
        if (_shortcuts.TryGetValue(actionId, out var shortcut))
        {
            var binding = new KeyBinding
            {
                Key = shortcut.key,
                Modifiers = shortcut.modifiers
            };
            return binding;
        }
        return null;
    }

    /// <summary>
    /// Execute action for a key combination
    /// </summary>
    public bool TryExecute(Key key, ModifierKeys modifiers)
    {
        var shortcut = _shortcuts.Values.FirstOrDefault(s => s.key == key && s.modifiers == modifiers);
        if (shortcut.action != null)
        {
            shortcut.action();
            return true;
        }
        return false;
    }

    private class ShortcutConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Modifiers { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

