using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformanceCommunityView : UserControl
{
    public PerformanceCommunityViewModel ViewModel { get; }

    public PerformanceCommunityView()
    {
        ViewModel = App.GetService<PerformanceCommunityViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
