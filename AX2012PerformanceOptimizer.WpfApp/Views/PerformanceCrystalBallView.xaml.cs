using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformanceCrystalBallView : UserControl
{
    public PerformanceCrystalBallViewModel ViewModel { get; }

    public PerformanceCrystalBallView()
    {
        ViewModel = App.GetService<PerformanceCrystalBallViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
