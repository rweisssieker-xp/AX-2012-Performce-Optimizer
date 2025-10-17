using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class AiHealthDashboardView : UserControl
{
    public AiHealthDashboardView()
    {
        InitializeComponent();
        DataContext = App.GetService<AiHealthDashboardViewModel>();
    }
}
