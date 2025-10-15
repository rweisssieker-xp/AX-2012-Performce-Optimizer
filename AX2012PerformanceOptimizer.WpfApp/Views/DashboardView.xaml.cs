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
        // Don't auto-load data on startup - user must configure connection first
    }
}

