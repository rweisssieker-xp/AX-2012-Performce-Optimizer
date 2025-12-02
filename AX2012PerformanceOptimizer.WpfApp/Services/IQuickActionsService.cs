using System.Collections.Generic;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for managing customizable Quick Actions
/// </summary>
public interface IQuickActionsService
{
    /// <summary>
    /// Gets all available Quick Actions (both enabled and disabled)
    /// </summary>
    List<QuickActionDefinition> GetAllAvailableActions();

    /// <summary>
    /// Gets enabled Quick Actions in their configured order
    /// </summary>
    List<QuickActionDefinition> GetEnabledActions();

    /// <summary>
    /// Saves Quick Actions configuration
    /// </summary>
    void SaveActions(List<QuickActionDefinition> actions);

    /// <summary>
    /// Resets to default actions
    /// </summary>
    void ResetToDefaults();
}

/// <summary>
/// Definition of a Quick Action
/// </summary>
public class QuickActionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortcutText { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; } = 0;
}

