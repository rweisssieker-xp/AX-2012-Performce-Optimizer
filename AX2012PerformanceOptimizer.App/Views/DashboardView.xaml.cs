using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AX2012PerformanceOptimizer.App.ViewModels;

namespace AX2012PerformanceOptimizer.App.Views;

public sealed partial class DashboardView : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardView()
    {
        InitializeComponent();
        ViewModel = App.GetService<DashboardViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadDataCommand.ExecuteAsync(null);
    }
}

