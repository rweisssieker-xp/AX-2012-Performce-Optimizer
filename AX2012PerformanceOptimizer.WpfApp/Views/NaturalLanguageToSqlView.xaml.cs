using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class NaturalLanguageToSqlView : UserControl
{
    public NaturalLanguageToSqlView()
    {
        InitializeComponent();
        DataContext = App.GetService<NaturalLanguageToSqlViewModel>();
    }
}
