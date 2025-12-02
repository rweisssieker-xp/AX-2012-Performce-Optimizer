using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AX2012PerformanceOptimizer.WpfApp.Services;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class QuickActionsPanelViewModel : ObservableObject
{
    private readonly IQuickActionsService _quickActionsService;

    [ObservableProperty]
    private ObservableCollection<QuickAction> quickActions = new();

    public QuickActionsPanelViewModel(IQuickActionsService quickActionsService)
    {
        _quickActionsService = quickActionsService;
        LoadActions();
    }

    public void LoadActions()
    {
        QuickActions.Clear();
        var enabledActions = _quickActionsService.GetEnabledActions();

        foreach (var actionDef in enabledActions)
        {
            var command = actionDef.Id switch
            {
                "export" => new RelayCommand(() => ExecuteExport()),
                "dashboard" => new RelayCommand(() => ExecuteNavigateToDashboard()),
                "performance" => new RelayCommand(() => ExecuteNavigateToPerformance()),
                "settings" => new RelayCommand(() => ExecuteNavigateToSettings()),
                "executive-summary" => new RelayCommand(() => ExecuteGenerateExecutiveSummary()),
                "search-queries" => new RelayCommand(() => ExecuteSearchQueries()),
                _ => new RelayCommand(() => { })
            };

            QuickActions.Add(new QuickAction
            {
                DisplayText = actionDef.DisplayText,
                Description = actionDef.Description,
                ShortcutText = actionDef.ShortcutText,
                Command = command
            });
        }
    }

    private void ExecuteExport()
    {
        // Will be implemented in Epic 2
        System.Windows.MessageBox.Show("Export Wizard will open here (Epic 2)", 
            "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void ExecuteNavigateToDashboard()
    {
        // Trigger navigation event
        NavigateToTab?.Invoke(0);
    }

    private void ExecuteNavigateToPerformance()
    {
        NavigateToTab?.Invoke(2);
    }

    private void ExecuteNavigateToSettings()
    {
        NavigateToTab?.Invoke(1);
    }

    private void ExecuteGenerateExecutiveSummary()
    {
        System.Windows.MessageBox.Show("Executive Summary generation will be implemented here", 
            "Executive Summary", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void ExecuteSearchQueries()
    {
        System.Windows.MessageBox.Show("Query search will be implemented here", 
            "Search", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    public event Action<int>? NavigateToTab;
}

public class QuickAction
{
    public string DisplayText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortcutText { get; set; } = string.Empty;
    public ICommand Command { get; set; } = null!;
}

