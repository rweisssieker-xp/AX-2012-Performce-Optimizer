using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class QuickActionsPanelViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<QuickAction> quickActions = new();

    public QuickActionsPanelViewModel()
    {
        InitializeDefaultActions();
    }

    private void InitializeDefaultActions()
    {
        QuickActions.Add(new QuickAction
        {
            DisplayText = "📊 Export Data",
            Description = "Export performance data to multiple formats",
            ShortcutText = "Ctrl+E",
            Command = new RelayCommand(() => ExecuteExport())
        });

        QuickActions.Add(new QuickAction
        {
            DisplayText = "🏠 Go to Dashboard",
            Description = "Navigate to main dashboard",
            ShortcutText = "Ctrl+D",
            Command = new RelayCommand(() => ExecuteNavigateToDashboard())
        });

        QuickActions.Add(new QuickAction
        {
            DisplayText = "📈 SQL Performance",
            Description = "View SQL performance analysis",
            ShortcutText = "Ctrl+P",
            Command = new RelayCommand(() => ExecuteNavigateToPerformance())
        });

        QuickActions.Add(new QuickAction
        {
            DisplayText = "⚙️ Settings",
            Description = "Open application settings",
            ShortcutText = "",
            Command = new RelayCommand(() => ExecuteNavigateToSettings())
        });

        QuickActions.Add(new QuickAction
        {
            DisplayText = "💡 Generate Executive Summary",
            Description = "Create executive summary report",
            ShortcutText = "",
            Command = new RelayCommand(() => ExecuteGenerateExecutiveSummary())
        });

        QuickActions.Add(new QuickAction
        {
            DisplayText = "🔍 Search Queries",
            Description = "Search for specific queries",
            ShortcutText = "",
            Command = new RelayCommand(() => ExecuteSearchQueries())
        });
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

