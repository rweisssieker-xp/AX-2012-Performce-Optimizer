using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using AX2012PerformanceOptimizer.WpfApp.Services;

namespace AX2012PerformanceOptimizer.WpfApp;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private readonly IKeyboardShortcutService _keyboardShortcutService;
    private TabControl? _mainTabControl;
    private System.Windows.Controls.Primitives.Popup? _quickActionsPopup;

    public MainWindow()
    {
        ViewModel = App.GetService<MainViewModel>();
        _keyboardShortcutService = App.GetService<IKeyboardShortcutService>();
        DataContext = ViewModel;
        InitializeComponent();
        
        // Find controls after initialization
        Loaded += MainWindow_Loaded;
        
        // Register keyboard shortcuts
        RegisterKeyboardShortcuts();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _mainTabControl = FindName("MainTabControl") as TabControl;
        _quickActionsPopup = FindName("QuickActionsPopup") as System.Windows.Controls.Primitives.Popup;
        
        if (_quickActionsPopup != null)
        {
            _quickActionsPopup.PlacementTarget = this;
        }
        
        // Connect Quick Actions Panel navigation
        if (_quickActionsPopup?.Child is Views.QuickActionsPanel panel)
        {
            var viewModel = panel.DataContext as ViewModels.QuickActionsPanelViewModel;
            if (viewModel != null)
            {
                viewModel.NavigateToTab += NavigateToTab;
            }
        }
    }

    private void RegisterKeyboardShortcuts()
    {
        // Ctrl+K: Quick Actions Panel (will be implemented in Epic 5)
        _keyboardShortcutService.RegisterShortcut(
            "QuickActions",
            ModifierKeys.Control,
            Key.K,
            () => ShowQuickActionsPanel(),
            "Open Quick Actions Panel"
        );

        // Ctrl+E: Export Wizard (will be implemented in Epic 2)
        _keyboardShortcutService.RegisterShortcut(
            "Export",
            ModifierKeys.Control,
            Key.E,
            () => ShowExportWizard(),
            "Open Export Wizard"
        );

        // Ctrl+D: Dashboard view
        _keyboardShortcutService.RegisterShortcut(
            "Dashboard",
            ModifierKeys.Control,
            Key.D,
            () => NavigateToTab(0),
            "Navigate to Dashboard"
        );

        // Ctrl+P: Performance analysis (SQL Performance tab)
        _keyboardShortcutService.RegisterShortcut(
            "Performance",
            ModifierKeys.Control,
            Key.P,
            () => NavigateToTab(2),
            "Navigate to SQL Performance"
        );

        // Ctrl+Shift+Q: Quick-Fix Mode
        _keyboardShortcutService.RegisterShortcut(
            "QuickFix",
            ModifierKeys.Control | ModifierKeys.Shift,
            Key.Q,
            () => ShowQuickFixView(),
            "Open Quick-Fix Mode"
        );

        // Ctrl+Shift+S: Survival Mode Toggle
        _keyboardShortcutService.RegisterShortcut(
            "SurvivalMode",
            ModifierKeys.Control | ModifierKeys.Shift,
            Key.S,
            () => ToggleSurvivalMode(),
            "Toggle Survival Mode"
        );

        // F1: Help documentation
        _keyboardShortcutService.RegisterShortcut(
            "Help",
            ModifierKeys.None,
            Key.F1,
            () => ShowHelp(),
            "Show Help Documentation"
        );

        // Register KeyBindings in XAML
        RegisterKeyBindings();
    }

    private void RegisterKeyBindings()
    {
        var bindings = new InputBindingCollection();
        
        // Ctrl+K
        var quickActionsBinding = new KeyBinding(
            new RelayCommand(() => ShowQuickActionsPanel()),
            Key.K,
            ModifierKeys.Control
        );
        bindings.Add(quickActionsBinding);

        // Ctrl+E
        var exportBinding = new KeyBinding(
            new RelayCommand(() => ShowExportWizard()),
            Key.E,
            ModifierKeys.Control
        );
        bindings.Add(exportBinding);

        // Ctrl+D
        var dashboardBinding = new KeyBinding(
            new RelayCommand(() => NavigateToTab(0)),
            Key.D,
            ModifierKeys.Control
        );
        bindings.Add(dashboardBinding);

        // Ctrl+P
        var performanceBinding = new KeyBinding(
            new RelayCommand(() => NavigateToTab(2)),
            Key.P,
            ModifierKeys.Control
        );
        bindings.Add(performanceBinding);

        // Ctrl+Shift+Q
        var quickFixBinding = new KeyBinding(
            new RelayCommand(() => ShowQuickFixView()),
            Key.Q,
            ModifierKeys.Control | ModifierKeys.Shift
        );
        bindings.Add(quickFixBinding);

        // Ctrl+Shift+S
        var survivalModeBinding = new KeyBinding(
            new RelayCommand(() => ToggleSurvivalMode()),
            Key.S,
            ModifierKeys.Control | ModifierKeys.Shift
        );
        bindings.Add(survivalModeBinding);

        // F1
        var helpBinding = new KeyBinding(
            new RelayCommand(() => ShowHelp()),
            Key.F1,
            ModifierKeys.None
        );
        bindings.Add(helpBinding);

        InputBindings.AddRange(bindings);
    }

    private void NavigateToTab(int index)
    {
        if (_mainTabControl != null && index >= 0 && index < _mainTabControl.Items.Count)
        {
            _mainTabControl.SelectedIndex = index;
        }
    }

    private void ShowQuickActionsPanel()
    {
        if (_quickActionsPopup != null)
        {
            _quickActionsPopup.IsOpen = !_quickActionsPopup.IsOpen;
        }
    }

    private void ShowExportWizard()
    {
        var dialog = new Dialogs.ExportWizardDialog
        {
            Owner = this
        };
        
        // TODO: Set DataToExport based on current view context
        dialog.ViewModel.DataToExport = null; // Will be set from context
        dialog.ViewModel.ExportTitle = "Performance Data Export";
        
        dialog.ShowDialog();
    }

    private void ShowQuickFixView()
    {
        // Navigate to Quick-Fix tab (will be added as a tab or modal)
        // For now, find the Quick-Fix tab index
        if (_mainTabControl != null)
        {
            for (int i = 0; i < _mainTabControl.Items.Count; i++)
            {
                if (_mainTabControl.Items[i] is TabItem tab && tab.Header?.ToString()?.Contains("Quick-Fix") == true)
                {
                    NavigateToTab(i);
                    return;
                }
            }
        }
        
        // If tab not found, show as modal dialog
        var quickFixView = new Views.QuickFixView();
        var dialog = new Window
        {
            Title = "⚡ Quick-Fix Mode",
            Content = quickFixView,
            Width = 1000,
            Height = 700,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this
        };
        dialog.ShowDialog();
    }

    private void QuickFixButton_Click(object sender, RoutedEventArgs e)
    {
        ShowQuickFixView();
    }

    private void ToggleSurvivalMode()
    {
        // Navigate to Recommendations tab and toggle Survival Mode
        if (_mainTabControl != null)
        {
            for (int i = 0; i < _mainTabControl.Items.Count; i++)
            {
                if (_mainTabControl.Items[i] is TabItem tab && tab.Header?.ToString()?.Contains("Recommendations") == true)
                {
                    NavigateToTab(i);
                    // Toggle will be handled by the ViewModel when tab is selected
                    break;
                }
            }
        }
    }

    private void ShowHelp()
    {
        MessageBox.Show("Help Documentation (F1)\n\nKeyboard Shortcuts:\n" +
            "• Ctrl+K - Quick Actions Panel\n" +
            "• Ctrl+E - Export Wizard\n" +
            "• Ctrl+D - Dashboard\n" +
            "• Ctrl+P - SQL Performance\n" +
            "• Ctrl+Shift+Q - Quick-Fix Mode\n" +
            "• F1 - Help\n\n" +
            "For more information, visit the Settings tab.",
            "Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();
    }
}
