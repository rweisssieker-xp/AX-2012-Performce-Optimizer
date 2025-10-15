using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.Data.SqlServer;

public class SqlConnectionManager : ISqlConnectionManager
{
    private string _connectionString = string.Empty;

    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GetConnectionString()
    {
        return _connectionString;
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string has not been set.");
        }

        var connection = new SqlConnection(_connectionString);
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
}

