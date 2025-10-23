using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformanceTimeMachineView : UserControl
{
    public PerformanceTimeMachineViewModel ViewModel { get; }

    public PerformanceTimeMachineView()
    {
        ViewModel = App.GetService<PerformanceTimeMachineViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
