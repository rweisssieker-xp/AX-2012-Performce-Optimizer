using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class SqlQueryMonitorService : ISqlQueryMonitorService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<SqlQueryMonitorService> _logger;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public event EventHandler<SqlQueryMetric>? NewMetricCollected;

    public SqlQueryMonitorService(
        ISqlConnectionManager connectionManager,
        ILogger<SqlQueryMonitorService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<List<SqlQueryMetric>> GetTopExpensiveQueriesAsync(int topCount = 20)
    {
        var metrics = new List<SqlQueryMetric>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT TOP (@TopCount)
                    CONVERT(VARCHAR(MAX), HASHBYTES('SHA2_256', qs.sql_handle), 2) AS QueryHash,
                    SUBSTRING(qt.text, (qs.statement_start_offset/2) + 1,
                        ((CASE qs.statement_end_offset
                            WHEN -1 THEN DATALENGTH(qt.text)
                            ELSE qs.statement_end_offset
                        END - qs.statement_start_offset)/2) + 1) AS QueryText,
                    qs.execution_count AS ExecutionCount,
                    qs.total_worker_time / 1000.0 AS TotalCpuTimeMs,
                    (qs.total_worker_time / qs.execution_count) / 1000.0 AS AvgCpuTimeMs,
                    qs.total_elapsed_time / 1000.0 AS TotalElapsedTimeMs,
                    (qs.total_elapsed_time / qs.execution_count) / 1000.0 AS AvgElapsedTimeMs,
                    qs.total_logical_reads AS TotalLogicalReads,
                    qs.total_logical_reads / qs.execution_count AS AvgLogicalReads,
                    qs.total_physical_reads AS TotalPhysicalReads,
                    qs.total_physical_reads / qs.execution_count AS AvgPhysicalReads,
                    qs.last_execution_time AS LastExecutionTime
                FROM sys.dm_exec_query_stats qs
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
                WHERE qt.text NOT LIKE '%sys.dm_exec%'
                ORDER BY qs.total_worker_time DESC", connection);

            command.Parameters.AddWithValue("@TopCount", topCount);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                metrics.Add(new SqlQueryMetric
                {
                    QueryHash = reader.GetString(0),
                    QueryText = reader.GetString(1),
                    ExecutionCount = reader.GetInt64(2),
                    TotalCpuTimeMs = Convert.ToDouble(reader.GetValue(3)),
                    AvgCpuTimeMs = Convert.ToDouble(reader.GetValue(4)),
                    TotalElapsedTimeMs = Convert.ToDouble(reader.GetValue(5)),
                    AvgElapsedTimeMs = Convert.ToDouble(reader.GetValue(6)),
                    TotalLogicalReads = reader.GetInt64(7),
                    AvgLogicalReads = reader.GetInt64(8),
                    TotalPhysicalReads = reader.GetInt64(9),
                    AvgPhysicalReads = reader.GetInt64(10),
                    LastExecutionTime = reader.GetDateTime(11),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top expensive queries");
        }

        return metrics;
    }

    public async Task<List<SqlQueryMetric>> GetQueryStatisticsAsync()
    {
        return await GetTopExpensiveQueriesAsync(100);
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
                    var metrics = await GetTopExpensiveQueriesAsync(10);
                    foreach (var metric in metrics)
                    {
                        NewMetricCollected?.Invoke(this, metric);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), _monitoringCts.Token);
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

