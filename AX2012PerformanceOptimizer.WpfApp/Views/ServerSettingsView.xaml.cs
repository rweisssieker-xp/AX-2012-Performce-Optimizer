using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class ServerSettingsView : UserControl
{
    public ServerSettingsViewModel ViewModel { get; }

    public ServerSettingsView()
    {
        ViewModel = App.GetService<ServerSettingsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
