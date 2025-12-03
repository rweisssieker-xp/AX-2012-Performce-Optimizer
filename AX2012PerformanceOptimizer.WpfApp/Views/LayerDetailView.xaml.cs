using System.Windows;
using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class LayerDetailView : UserControl
{
    public LayerDetailViewModel ViewModel { get; }

    public LayerDetailView()
    {
        ViewModel = App.GetService<LayerDetailViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void Breadcrumb_PerformanceStack_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Navigate back to Performance Stack view
        // This will be handled by the main navigation system
    }
}
