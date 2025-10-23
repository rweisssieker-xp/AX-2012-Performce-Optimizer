using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class QueryRiskScoringView : UserControl
{
    public QueryRiskScoringView()
    {
        InitializeComponent();
        DataContext = App.GetService<QueryRiskScoringViewModel>();
    }
}
