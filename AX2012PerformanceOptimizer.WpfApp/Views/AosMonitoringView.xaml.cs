using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class AosMonitoringView : UserControl
{
    public AosMonitoringViewModel ViewModel { get; }

    public AosMonitoringView()
    {
        ViewModel = App.GetService<AosMonitoringViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}


