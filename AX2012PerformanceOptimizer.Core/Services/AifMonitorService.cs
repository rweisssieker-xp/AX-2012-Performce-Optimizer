using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

public class AifMonitorService : IAifMonitorService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<AifMonitorService> _logger;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;

    public AifMonitorService(
        ISqlConnectionManager connectionManager,
        ILogger<AifMonitorService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<int> GetInboundQueueCountAsync()
    {
        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM AIFGATEWAYQUEUE 
                WHERE DIRECTION = 0 AND STATUS IN (0, 1)", connection);
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inbound queue count");
            return 0;
        }
    }

    public async Task<int> GetOutboundQueueCountAsync()
    {
        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM AIFGATEWAYQUEUE 
                WHERE DIRECTION = 1 AND STATUS IN (0, 1)", connection);
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outbound queue count");
            return 0;
        }
    }

    public async Task<int> GetErrorQueueCountAsync()
    {
        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM AIFGATEWAYQUEUE 
                WHERE STATUS = 3", connection);
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error queue count");
            return 0;
        }
    }

    public async Task<double> GetAvgProcessingTimeAsync()
    {
        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT AVG(DATEDIFF(SECOND, CREATEDDATETIME, MODIFIEDDATETIME))
                FROM AIFGATEWAYQUEUE 
                WHERE STATUS = 2 AND MODIFIEDDATETIME > DATEADD(HOUR, -1, GETUTCDATE())", connection);
            
            var result = await command.ExecuteScalarAsync();
            return result != DBNull.Value && result != null ? Convert.ToDouble(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average processing time");
            return 0;
        }
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
                    await GetInboundQueueCountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during monitoring");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), _monitoringCts.Token);
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

