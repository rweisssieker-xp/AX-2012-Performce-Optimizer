using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class ExecutiveDashboardView : UserControl
{
    public ExecutiveDashboardView()
    {
        InitializeComponent();
        DataContext = App.GetService<ExecutiveDashboardViewModel>();
    }
}
