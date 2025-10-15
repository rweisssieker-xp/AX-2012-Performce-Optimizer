using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class HistoricalTrendingView : UserControl
{
    public HistoricalTrendingViewModel ViewModel { get; }

    public HistoricalTrendingView()
    {
        ViewModel = App.GetService<HistoricalTrendingViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        Loaded += async (s, e) =>
        {
            try
            {
                await ViewModel.LoadDataCommand.ExecuteAsync(null);
            }
            catch
            {
                // Ignore errors on first load
            }
        };
    }
}
