using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class AiInsightsDashboardView : UserControl
{
    public AiInsightsDashboardView()
    {
        InitializeComponent();
        DataContext = App.GetService<AiInsightsDashboardViewModel>();
    }
}
