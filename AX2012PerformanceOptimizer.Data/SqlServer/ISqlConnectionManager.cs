using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.Data.SqlServer;

public class ConnectionChangedEventArgs : EventArgs
{
    public bool IsConnected { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public interface ISqlConnectionManager
{
    event EventHandler<ConnectionChangedEventArgs>? ConnectionChanged;

    Task<SqlConnection> GetConnectionAsync();
    Task<bool> IsConnectedAsync();
    void SetConnectionString(string connectionString);
    string GetConnectionString();
    void SetAosServerName(string aosServerName);

    bool IsConnected { get; }
    string CurrentServerName { get; }
    string CurrentDatabaseName { get; }
    string CurrentAosServerName { get; }
}

