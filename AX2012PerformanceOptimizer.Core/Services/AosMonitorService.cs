using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class AosMonitorService : IAosMonitorService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<AosMonitorService> _logger;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public event EventHandler<AosMetric>? NewMetricCollected;

    public AosMonitorService(
        ISqlConnectionManager connectionManager,
        ILogger<AosMonitorService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<AosMetric> GetAosMetricsAsync()
    {
        var metric = new AosMetric
        {
            ServerName = Environment.MachineName,
            CollectedAt = DateTime.UtcNow
        };

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            
            // Get active user sessions count
            using var sessionCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM SYSCLIENTSESSIONS WHERE STATUS = 1", connection);
            metric.ActiveUserSessions = (int)await sessionCmd.ExecuteScalarAsync();

            // Get SQL Server performance metrics as AOS proxy
            using var perfCmd = new SqlCommand(@"
                SELECT 
                    cntr_value 
                FROM sys.dm_os_performance_counters 
                WHERE counter_name = 'SQL Compilations/sec'", connection);
            
            var result = await perfCmd.ExecuteScalarAsync();
            metric.IsHealthy = result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AOS metrics");
            metric.IsHealthy = false;
        }

        return metric;
    }

    public async Task<List<UserSession>> GetActiveUserSessionsAsync()
    {
        var sessions = new List<UserSession>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT 
                    SESSIONID,
                    USERID,
                    CLIENTCOMPUTER,
                    LOGONDATETIME,
                    CLIENTTYPE,
                    STATUS
                FROM SYSCLIENTSESSIONS 
                WHERE STATUS = 1
                ORDER BY LOGONDATETIME DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sessions.Add(new UserSession
                {
                    SessionId = reader.GetString(0),
                    UserId = reader.GetString(1),
                    ClientComputer = reader.GetString(2),
                    LoginDateTime = reader.GetDateTime(3),
                    Company = string.Empty,
                    IsActive = reader.GetInt32(5) == 1
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active user sessions");
        }

        return sessions;
    }

    public Task StartMonitoringAsync(CancellationToken cancellationToken = default)
    {
        _monitoringCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _monitoringTask = Task.Run(async () =>
        {
            while (!_monitoringCts.Token.IsCancellationRequested)
            {
                try
                {
                    var metric = await GetAosMetricsAsync();
                    NewMetricCollected?.Invoke(this, metric);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), _monitoringCts.Token);
            }
        }, _monitoringCts.Token);

        return Task.CompletedTask;
    }

    public Task StopMonitoringAsync()
    {
        _monitoringCts?.Cancel();
        return _monitoringTask ?? Task.CompletedTask;
    }
}

