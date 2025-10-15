using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class BatchJobsView : UserControl
{
    public BatchJobsViewModel ViewModel { get; }

    public BatchJobsView()
    {
        ViewModel = App.GetService<BatchJobsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}


