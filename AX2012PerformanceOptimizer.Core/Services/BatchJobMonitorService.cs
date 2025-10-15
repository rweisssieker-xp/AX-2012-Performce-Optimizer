using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class BatchJobMonitorService : IBatchJobMonitorService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<BatchJobMonitorService> _logger;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public event EventHandler<BatchJobMetric>? NewMetricCollected;

    public BatchJobMonitorService(
        ISqlConnectionManager connectionManager,
        ILogger<BatchJobMonitorService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<List<BatchJobMetric>> GetRunningBatchJobsAsync()
    {
        var jobs = new List<BatchJobMetric>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT 
                    CAST(RECID AS VARCHAR) AS JobId,
                    CAPTION AS JobDescription,
                    COMPANY,
                    STATUS,
                    STARTDATETIME,
                    ENDDATETIME,
                    CREATEDBY
                FROM BATCHJOB
                WHERE STATUS IN (1, 4)
                ORDER BY STARTDATETIME DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var startTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                var endTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                jobs.Add(new BatchJobMetric
                {
                    JobId = reader.GetString(0),
                    JobDescription = reader.GetString(1),
                    Company = reader.GetString(2),
                    Status = (BatchJobStatus)reader.GetInt32(3),
                    StartDateTime = startTime,
                    EndDateTime = endTime,
                    Duration = startTime.HasValue && endTime.HasValue ? endTime.Value - startTime.Value : null,
                    CreatedBy = reader.GetString(6),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting running batch jobs");
        }

        return jobs;
    }

    public async Task<List<BatchJobMetric>> GetBatchJobHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var jobs = new List<BatchJobMetric>();
        startDate ??= DateTime.UtcNow.AddDays(-7);
        endDate ??= DateTime.UtcNow;

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT 
                    CAST(bj.RECID AS VARCHAR) AS JobId,
                    bj.CAPTION AS JobDescription,
                    bj.COMPANY,
                    bj.STATUS,
                    bj.STARTDATETIME,
                    bj.ENDDATETIME,
                    bj.CREATEDBY
                FROM BATCHJOBHISTORY bj
                WHERE bj.STARTDATETIME >= @StartDate AND bj.STARTDATETIME <= @EndDate
                ORDER BY bj.STARTDATETIME DESC", connection);

            command.Parameters.AddWithValue("@StartDate", startDate.Value);
            command.Parameters.AddWithValue("@EndDate", endDate.Value);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var startTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                var endTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                jobs.Add(new BatchJobMetric
                {
                    JobId = reader.GetString(0),
                    JobDescription = reader.GetString(1),
                    Company = reader.GetString(2),
                    Status = (BatchJobStatus)reader.GetInt32(3),
                    StartDateTime = startTime,
                    EndDateTime = endTime,
                    Duration = startTime.HasValue && endTime.HasValue ? endTime.Value - startTime.Value : null,
                    CreatedBy = reader.GetString(6),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch job history");
        }

        return jobs;
    }

    public async Task<List<BatchJobMetric>> GetFailedBatchJobsAsync()
    {
        var jobs = new List<BatchJobMetric>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT TOP 50
                    CAST(RECID AS VARCHAR) AS JobId,
                    CAPTION AS JobDescription,
                    COMPANY,
                    STATUS,
                    STARTDATETIME,
                    ENDDATETIME,
                    CREATEDBY
                FROM BATCHJOBHISTORY
                WHERE STATUS = 2
                ORDER BY STARTDATETIME DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var startTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                var endTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                jobs.Add(new BatchJobMetric
                {
                    JobId = reader.GetString(0),
                    JobDescription = reader.GetString(1),
                    Company = reader.GetString(2),
                    Status = (BatchJobStatus)reader.GetInt32(3),
                    StartDateTime = startTime,
                    EndDateTime = endTime,
                    Duration = startTime.HasValue && endTime.HasValue ? endTime.Value - startTime.Value : null,
                    CreatedBy = reader.GetString(6),
                    CollectedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed batch jobs");
        }

        return jobs;
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
                    var jobs = await GetRunningBatchJobsAsync();
                    foreach (var job in jobs)
                    {
                        NewMetricCollected?.Invoke(this, job);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), _monitoringCts.Token);
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

