using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class DatabaseHealthView : UserControl
{
    public DatabaseHealthViewModel ViewModel { get; }

    public DatabaseHealthView()
    {
        ViewModel = App.GetService<DatabaseHealthViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
