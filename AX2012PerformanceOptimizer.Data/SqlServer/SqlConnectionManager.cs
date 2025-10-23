using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.Data.SqlServer;

public class SqlConnectionManager : ISqlConnectionManager
{
    private readonly object _lock = new();
    private string _connectionString = string.Empty;
    private string _currentServerName = string.Empty;
    private string _currentDatabaseName = string.Empty;
    private string _currentAosServerName = string.Empty;
    private bool _isConnected = false;

    public event EventHandler<ConnectionChangedEventArgs>? ConnectionChanged;

    public bool IsConnected
    {
        get
        {
            lock (_lock)
            {
                return _isConnected;
            }
        }
    }

    public string CurrentServerName
    {
        get
        {
            lock (_lock)
            {
                return _currentServerName;
            }
        }
    }

    public string CurrentDatabaseName
    {
        get
        {
            lock (_lock)
            {
                return _currentDatabaseName;
            }
        }
    }

    public string CurrentAosServerName
    {
        get
        {
            lock (_lock)
            {
                return _currentAosServerName;
            }
        }
    }

    public void SetConnectionString(string connectionString)
    {
        lock (_lock)
        {
            _connectionString = connectionString;

            // Parse connection string to extract server and database names
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    _currentServerName = builder.DataSource;
                    _currentDatabaseName = builder.InitialCatalog;
                    _isConnected = true;
                }
                catch
                {
                    _currentServerName = string.Empty;
                    _currentDatabaseName = string.Empty;
                    _isConnected = false;
                }
            }
            else
            {
                _currentServerName = string.Empty;
                _currentDatabaseName = string.Empty;
                _isConnected = false;
            }
        }

        // Fire event outside of lock to prevent potential deadlocks
        OnConnectionChanged();
    }

    public string GetConnectionString()
    {
        lock (_lock)
        {
            return _connectionString;
        }
    }

    public void SetAosServerName(string aosServerName)
    {
        lock (_lock)
        {
            _currentAosServerName = aosServerName ?? string.Empty;
        }
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        string connectionString;
        lock (_lock)
        {
            connectionString = _connectionString;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string has not been set.");
        }

        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            using var connection = await GetConnectionAsync();
            return connection.State == System.Data.ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    private void OnConnectionChanged()
    {
        ConnectionChangedEventArgs args;
        lock (_lock)
        {
            args = new ConnectionChangedEventArgs
            {
                IsConnected = _isConnected,
                ServerName = _currentServerName,
                DatabaseName = _currentDatabaseName
            };
        }

        ConnectionChanged?.Invoke(this, args);
    }
}

