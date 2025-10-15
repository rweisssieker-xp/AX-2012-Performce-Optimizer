using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AX2012PerformanceOptimizer.App.ViewModels;

namespace AX2012PerformanceOptimizer.App.Views;

public sealed partial class BatchJobsView : Page
{
    public BatchJobsViewModel ViewModel { get; }

    public BatchJobsView()
    {
        InitializeComponent();
        ViewModel = App.GetService<BatchJobsViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadDataCommand.ExecuteAsync(null);
    }
}

