using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformanceStackView : UserControl
{
    public PerformanceStackViewModel ViewModel { get; }

    public PerformanceStackView()
    {
        ViewModel = App.GetService<PerformanceStackViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void LayerCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is LayerType layerType)
        {
            ViewModel.DrillDownCommand.ExecuteAsync(layerType);
        }
    }
}
