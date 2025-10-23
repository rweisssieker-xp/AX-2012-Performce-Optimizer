using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformanceDnaView : UserControl
{
    public PerformanceDnaViewModel ViewModel { get; }

    public PerformanceDnaView()
    {
        ViewModel = App.GetService<PerformanceDnaViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
