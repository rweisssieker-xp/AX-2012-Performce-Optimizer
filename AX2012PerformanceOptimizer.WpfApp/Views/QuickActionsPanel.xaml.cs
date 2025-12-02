using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class QuickActionsPanel : UserControl
{
    public QuickActionsPanel()
    {
        InitializeComponent();
        var service = App.GetService<Services.IQuickActionsService>();
        DataContext = new QuickActionsPanelViewModel(service);
    }
}

