using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class DatabaseStatsService : IDatabaseStatsService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<DatabaseStatsService> _logger;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public event EventHandler<DatabaseMetric>? NewMetricCollected;

    public DatabaseStatsService(
        ISqlConnectionManager connectionManager,
        ILogger<DatabaseStatsService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<DatabaseMetric> GetDatabaseMetricsAsync()
    {
        var metric = new DatabaseMetric();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            metric.DatabaseName = connection.Database;

            using var command = new SqlCommand(@"
                SELECT 
                    SUM(size * 8.0 / 1024) AS TotalSizeMB,
                    SUM(CASE WHEN type_desc = 'ROWS' THEN size * 8.0 / 1024 ELSE 0 END) AS DataSizeMB,
                    SUM(CASE WHEN type_desc = 'LOG' THEN size * 8.0 / 1024 ELSE 0 END) AS LogSizeMB
                FROM sys.database_files", connection);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                metric.TotalSizeMB = Convert.ToInt64(reader.GetDecimal(0));
                metric.DataSizeMB = Convert.ToInt64(reader.GetDecimal(1));
                metric.LogSizeMB = Convert.ToInt64(reader.GetDecimal(2));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database metrics");
        }

        return metric;
    }

    public async Task<List<TableMetric>> GetTopTablesBySize(int topCount = 20)
    {
        var tables = new List<TableMetric>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand($@"
                SELECT TOP {topCount}
                    s.Name AS SchemaName,
                    t.Name AS TableName,
                    p.rows AS [RowCount],
                    SUM(a.total_pages) * 8 AS TotalSpaceKB,
                    SUM(a.used_pages) * 8 AS UsedSpaceKB,
                    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
                FROM sys.tables t
                INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
                INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
                INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
                LEFT OUTER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE t.is_ms_shipped = 0 AND i.OBJECT_ID > 255
                GROUP BY t.Name, s.Name, p.Rows
                ORDER BY SUM(a.total_pages) DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(new TableMetric
                {
                    SchemaName = reader.GetString(0),
                    TableName = reader.GetString(1),
                    RowCount = reader.GetInt64(2),
                    TotalSpaceKB = reader.GetInt64(3),
                    UsedSpaceKB = reader.GetInt64(4),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top tables by size");
        }

        return tables;
    }

    public async Task<List<IndexFragmentation>> GetFragmentedIndexesAsync(double thresholdPercent = 30)
    {
        var indexes = new List<IndexFragmentation>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT 
                    DB_NAME() AS DatabaseName,
                    OBJECT_SCHEMA_NAME(ips.object_id) AS SchemaName,
                    OBJECT_NAME(ips.object_id) AS TableName,
                    i.name AS IndexName,
                    ips.avg_fragmentation_in_percent AS FragmentationPercent,
                    ips.page_count AS PageCount,
                    i.type_desc AS IndexType
                FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
                INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
                WHERE ips.avg_fragmentation_in_percent > @Threshold
                    AND ips.page_count > 1000
                    AND i.name IS NOT NULL
                ORDER BY ips.avg_fragmentation_in_percent DESC", connection);

            command.Parameters.AddWithValue("@Threshold", thresholdPercent);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                indexes.Add(new IndexFragmentation
                {
                    DatabaseName = reader.GetString(0),
                    SchemaName = reader.GetString(1),
                    TableName = reader.GetString(2),
                    IndexName = reader.GetString(3),
                    FragmentationPercent = reader.GetDouble(4),
                    PageCount = reader.GetInt64(5),
                    IndexType = reader.GetString(6),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fragmented indexes");
        }

        return indexes;
    }

    public async Task<List<MissingIndex>> GetMissingIndexesAsync()
    {
        var indexes = new List<MissingIndex>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT TOP 20
                    DB_NAME() AS DatabaseName,
                    OBJECT_NAME(mid.object_id, DB_ID()) AS TableName,
                    mid.equality_columns AS EqualityColumns,
                    mid.inequality_columns AS InequalityColumns,
                    mid.included_columns AS IncludedColumns,
                    migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans) AS ImpactScore,
                    migs.user_seeks AS UserSeeks,
                    migs.user_scans AS UserScans
                FROM sys.dm_db_missing_index_groups mig
                INNER JOIN sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
                INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
                WHERE mid.database_id = DB_ID()
                ORDER BY ImpactScore DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                indexes.Add(new MissingIndex
                {
                    DatabaseName = reader.GetString(0),
                    TableName = reader.GetString(1),
                    EqualityColumns = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    InequalityColumns = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    IncludedColumns = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    ImpactScore = reader.GetDouble(5),
                    UserSeeks = reader.GetInt64(6),
                    UserScans = reader.GetInt64(7),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting missing indexes");
        }

        return indexes;
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
                    var metric = await GetDatabaseMetricsAsync();
                    NewMetricCollected?.Invoke(this, metric);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), _monitoringCts.Token);
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

