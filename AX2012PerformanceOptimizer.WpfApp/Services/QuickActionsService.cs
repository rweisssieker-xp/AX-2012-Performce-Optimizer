using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for managing customizable Quick Actions with persistence
/// </summary>
public class QuickActionsService : IQuickActionsService
{
    private readonly string _settingsPath;
    private List<QuickActionDefinition> _actions = new();

    public QuickActionsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "AX2012PerformanceOptimizer");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "quick-actions.json");
        
        LoadActions();
    }

    public List<QuickActionDefinition> GetAllAvailableActions()
    {
        if (_actions.Count == 0)
        {
            InitializeDefaults();
        }
        return _actions.OrderBy(a => a.Order).ToList();
    }

    public List<QuickActionDefinition> GetEnabledActions()
    {
        return GetAllAvailableActions()
            .Where(a => a.IsEnabled)
            .OrderBy(a => a.Order)
            .ToList();
    }

    public void SaveActions(List<QuickActionDefinition> actions)
    {
        _actions = actions;
        SaveToFile();
    }

    public void ResetToDefaults()
    {
        _actions.Clear();
        InitializeDefaults();
        SaveToFile();
    }

    private void InitializeDefaults()
    {
        _actions = new List<QuickActionDefinition>
        {
            new QuickActionDefinition
            {
                Id = "export",
                DisplayText = "📊 Export Data",
                Description = "Export performance data to multiple formats",
                ShortcutText = "Ctrl+E",
                IsEnabled = true,
                Order = 0
            },
            new QuickActionDefinition
            {
                Id = "dashboard",
                DisplayText = "🏠 Go to Dashboard",
                Description = "Navigate to main dashboard",
                ShortcutText = "Ctrl+D",
                IsEnabled = true,
                Order = 1
            },
            new QuickActionDefinition
            {
                Id = "performance",
                DisplayText = "📈 SQL Performance",
                Description = "View SQL performance analysis",
                ShortcutText = "Ctrl+P",
                IsEnabled = true,
                Order = 2
            },
            new QuickActionDefinition
            {
                Id = "settings",
                DisplayText = "⚙️ Settings",
                Description = "Open application settings",
                ShortcutText = "",
                IsEnabled = true,
                Order = 3
            },
            new QuickActionDefinition
            {
                Id = "executive-summary",
                DisplayText = "💡 Generate Executive Summary",
                Description = "Create executive summary report",
                ShortcutText = "",
                IsEnabled = true,
                Order = 4
            },
            new QuickActionDefinition
            {
                Id = "search-queries",
                DisplayText = "🔍 Search Queries",
                Description = "Search for specific queries",
                ShortcutText = "",
                IsEnabled = true,
                Order = 5
            }
        };
    }

    private void LoadActions()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                var savedActions = JsonSerializer.Deserialize<List<QuickActionDefinition>>(json);
                if (savedActions != null && savedActions.Count > 0)
                {
                    _actions = savedActions;
                    return;
                }
            }
            catch (Exception)
            {
                // If loading fails, use defaults
            }
        }
        
        // Initialize defaults if file doesn't exist or loading failed
        InitializeDefaults();
    }

    private void SaveToFile()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_actions, options);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception)
        {
            // Silently fail - settings are not critical
        }
    }
}

