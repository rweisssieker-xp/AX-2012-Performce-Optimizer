using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformancePersonasView : UserControl
{
    public PerformancePersonasViewModel ViewModel { get; }

    public PerformancePersonasView()
    {
        ViewModel = App.GetService<PerformancePersonasViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
