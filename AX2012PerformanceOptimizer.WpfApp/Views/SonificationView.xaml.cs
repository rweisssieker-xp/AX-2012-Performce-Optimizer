using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class SonificationView : UserControl
{
    public SonificationViewModel ViewModel { get; }

    public SonificationView()
    {
        ViewModel = App.GetService<SonificationViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
