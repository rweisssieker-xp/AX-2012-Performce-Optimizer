namespace AX2012PerformanceOptimizer.Data.AxConnector;

public interface IAxConnectorService
{
    Task<bool> ConnectAsync(string aosServer, int port, string company);
    Task DisconnectAsync();
    Task<bool> IsConnectedAsync();
    Task<T> ExecuteQueryAsync<T>(string queryText);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string queryText);
}

