using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class QuickFixView : UserControl
{
    public QuickFixViewModel ViewModel { get; }

    public QuickFixView()
    {
        ViewModel = App.GetService<QuickFixViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
