using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class QuickActionsPanel : UserControl
{
    public QuickActionsPanel()
    {
        InitializeComponent();
        DataContext = App.GetService<QuickActionsPanelViewModel>();
    }
}

