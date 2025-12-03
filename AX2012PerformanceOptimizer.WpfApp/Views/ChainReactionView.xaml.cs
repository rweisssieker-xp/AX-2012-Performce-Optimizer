using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class ChainReactionView : UserControl
{
    public ChainReactionViewModel ViewModel { get; }

    public ChainReactionView()
    {
        ViewModel = App.GetService<ChainReactionViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
