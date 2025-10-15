using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AX2012PerformanceOptimizer.App.ViewModels;

namespace AX2012PerformanceOptimizer.App.Views;

public sealed partial class SqlPerformanceView : Page
{
    public SqlPerformanceViewModel ViewModel { get; }

    public SqlPerformanceView()
    {
        InitializeComponent();
        ViewModel = App.GetService<SqlPerformanceViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadQueriesCommand.ExecuteAsync(null);
    }
}

