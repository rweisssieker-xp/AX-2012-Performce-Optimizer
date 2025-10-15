using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AX2012PerformanceOptimizer.App.ViewModels;

namespace AX2012PerformanceOptimizer.App.Views;

public sealed partial class SettingsView : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsView()
    {
        InitializeComponent();
        ViewModel = App.GetService<SettingsViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadProfilesCommand.ExecuteAsync(null);
    }
}

