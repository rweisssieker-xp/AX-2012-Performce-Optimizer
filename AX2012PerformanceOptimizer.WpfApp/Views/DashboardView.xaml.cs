using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class DashboardView : UserControl
{
    public DashboardViewModel ViewModel { get; }

    public DashboardView()
    {
        ViewModel = App.GetService<DashboardViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
        
        // Load role data when view is loaded
        Loaded += async (s, e) =>
        {
            try
            {
                await ViewModel.LoadRoleDataCommand.ExecuteAsync(null);
            }
            catch
            {
                // Ignore errors - will show default dashboard
            }
        };
    }
}

