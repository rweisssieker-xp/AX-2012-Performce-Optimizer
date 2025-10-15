using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class SqlPerformanceView : UserControl
{
    public SqlPerformanceViewModel ViewModel { get; }

    public SqlPerformanceView()
    {
        ViewModel = App.GetService<SqlPerformanceViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        Loaded += async (s, e) => await ViewModel.LoadQueriesCommand.ExecuteAsync(null);
    }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.ApplyFiltersCommand.Execute(null);
        }
    }

    private void OnQuerySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is SqlQueryMetric query)
        {
            ViewModel.SelectQueryCommand.Execute(query);
        }
    }
}


