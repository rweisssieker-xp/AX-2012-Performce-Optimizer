using CommunityToolkit.Mvvm.ComponentModel;

namespace AX2012PerformanceOptimizer.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "AX 2012 Performance Optimizer";

    [ObservableProperty]
    private bool isConnected;

    public MainViewModel()
    {
    }
}

