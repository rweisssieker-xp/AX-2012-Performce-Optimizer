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
        var aosServerName = _connectionManager.CurrentAosServerName;
        if (string.IsNullOrEmpty(aosServerName))
        {
            aosServerName = Environment.MachineName;
        }

        var metric = new AosMetric
        {
            ServerName = aosServerName,
            CollectedAt = DateTime.UtcNow
        };

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();

            // Get active user sessions count
            using var sessionCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM SYSCLIENTSESSIONS WHERE STATUS = 1", connection);
            var sessionResult = await sessionCmd.ExecuteScalarAsync();
            metric.ActiveUserSessions = sessionResult != null ? Convert.ToInt32(sessionResult) : 0;

            // Get SQL Server performance metrics as AOS proxy
            using var perfCmd = new SqlCommand(@"
                SELECT
                    cntr_value
                FROM sys.dm_os_performance_counters
                WHERE counter_name = 'SQL Compilations/sec'", connection);

            var result = await perfCmd.ExecuteScalarAsync();
            metric.IsHealthy = result != null;

            _logger.LogInformation("AOS Metrics collected successfully. Active Sessions: {Count}", metric.ActiveUserSessions);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Error getting AOS metrics. Check database connection and table permissions.");
            metric.IsHealthy = false;
            metric.ActiveUserSessions = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AOS metrics");
            metric.IsHealthy = false;
            metric.ActiveUserSessions = 0;
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
                try
                {
                    sessions.Add(new UserSession
                    {
                        SessionId = !reader.IsDBNull(0) ? reader.GetString(0) : string.Empty,
                        UserId = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                        ClientComputer = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                        LoginDateTime = !reader.IsDBNull(3) ? reader.GetDateTime(3) : DateTime.MinValue,
                        Company = string.Empty,
                        IsActive = !reader.IsDBNull(5) && reader.GetInt32(5) == 1
                    });
                }
                catch (Exception rowEx)
                {
                    _logger.LogWarning(rowEx, "Error reading session row, skipping");
                    continue;
                }
            }

            _logger.LogInformation("Retrieved {Count} active user sessions", sessions.Count);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Error getting active user sessions. Check if SYSCLIENTSESSIONS table exists and has correct permissions.");
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

