using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class RecommendationsView : UserControl
{
    public RecommendationsViewModel ViewModel { get; }

    public RecommendationsView()
    {
        ViewModel = App.GetService<RecommendationsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}


