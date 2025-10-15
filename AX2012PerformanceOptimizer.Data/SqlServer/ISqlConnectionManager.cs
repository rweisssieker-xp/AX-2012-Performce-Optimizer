using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.Data.SqlServer;

public interface ISqlConnectionManager
{
    Task<SqlConnection> GetConnectionAsync();
    Task<bool> IsConnectedAsync();
    void SetConnectionString(string connectionString);
    string GetConnectionString();
}

