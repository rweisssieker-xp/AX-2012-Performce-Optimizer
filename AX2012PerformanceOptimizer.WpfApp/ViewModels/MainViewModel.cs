using CommunityToolkit.Mvvm.ComponentModel;
using AX2012PerformanceOptimizer.Data.SqlServer;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISqlConnectionManager _sqlConnectionManager;

    [ObservableProperty]
    private string title = "AX 2012 Performance Optimizer";

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionStatusText = "Not Connected";

    [ObservableProperty]
    private string connectionServerName = string.Empty;

    [ObservableProperty]
    private string connectionDatabaseName = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MainViewModel(ISqlConnectionManager sqlConnectionManager)
    {
        _sqlConnectionManager = sqlConnectionManager;
        _sqlConnectionManager.ConnectionChanged += OnConnectionChanged;

        // Initialize with current connection state
        UpdateConnectionStatus(_sqlConnectionManager.IsConnected,
            _sqlConnectionManager.CurrentServerName,
            _sqlConnectionManager.CurrentDatabaseName);
    }

    private void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        UpdateConnectionStatus(e.IsConnected, e.ServerName, e.DatabaseName);
    }

    private void UpdateConnectionStatus(bool isConnected, string serverName, string databaseName)
    {
        IsConnected = isConnected;
        ConnectionServerName = serverName;
        ConnectionDatabaseName = databaseName;

        if (isConnected && !string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(databaseName))
        {
            ConnectionStatusText = $"Connected: {serverName}\\{databaseName}";
        }
        else
        {
            ConnectionStatusText = "Not Connected";
        }
    }
}

