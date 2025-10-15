using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class SsrsMonitorService : ISsrsMonitorService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<SsrsMonitorService> _logger;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public SsrsMonitorService(
        ISqlConnectionManager connectionManager,
        ILogger<SsrsMonitorService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<Dictionary<string, int>> GetTopExecutedReportsAsync(int topCount = 10)
    {
        var reports = new Dictionary<string, int>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            // Note: This query assumes SSRS execution log is in the same database
            // In real scenarios, you might need to connect to the ReportServer database
            using var command = new SqlCommand($@"
                SELECT TOP {topCount}
                    REPORTPATH,
                    COUNT(*) AS ExecutionCount
                FROM SRSREPORTEXECUTIONLOG
                WHERE EXECUTIONTIME > DATEADD(DAY, -7, GETUTCDATE())
                GROUP BY REPORTPATH
                ORDER BY COUNT(*) DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reports[reader.GetString(0)] = reader.GetInt32(1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting top executed reports - table might not exist");
        }

        return reports;
    }

    public async Task<Dictionary<string, double>> GetLongRunningReportsAsync(double thresholdSeconds = 30)
    {
        var reports = new Dictionary<string, double>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT 
                    REPORTPATH,
                    AVG(EXECUTIONDURATION) AS AvgDuration
                FROM SRSREPORTEXECUTIONLOG
                WHERE EXECUTIONTIME > DATEADD(DAY, -7, GETUTCDATE())
                    AND EXECUTIONDURATION > @Threshold
                GROUP BY REPORTPATH
                ORDER BY AVG(EXECUTIONDURATION) DESC", connection);

            command.Parameters.AddWithValue("@Threshold", thresholdSeconds);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reports[reader.GetString(0)] = reader.GetDouble(1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting long running reports - table might not exist");
        }

        return reports;
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
                    await GetTopExecutedReportsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring");
                }

                await Task.Delay(TimeSpan.FromMinutes(10), _monitoringCts.Token);
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

