using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AX2012PerformanceOptimizer.App.ViewModels;

namespace AX2012PerformanceOptimizer.App.Views;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        
        // Navigate to Dashboard by default
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardView));
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsView));
        }
        else if (args.SelectedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            var pageType = tag switch
            {
                "Dashboard" => typeof(DashboardView),
                "SqlPerformance" => typeof(SqlPerformanceView),
                "AosMonitoring" => typeof(AosMonitoringView),
                "BatchJobs" => typeof(BatchJobsView),
                "DatabaseHealth" => typeof(DatabaseHealthView),
                "Recommendations" => typeof(RecommendationsView),
                _ => null
            };

            if (pageType != null)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}

