using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.Data.SqlServer;

public class ExecutionPlanService : ISqlExecutionPlanService
{
    private readonly ISqlConnectionManager _connManager;

    public ExecutionPlanService(ISqlConnectionManager connManager)
    {
        _connManager = connManager;
    }

    public async Task<string?> GetEstimatedPlanXmlAsync(string sqlText, int commandTimeoutSeconds = 15)
    {
        if (string.IsNullOrWhiteSpace(sqlText)) return null;

        await using var conn = new SqlConnection(_connManager.GetConnectionString());
        await conn.OpenAsync();

        await using var enableCmd = conn.CreateCommand();
        enableCmd.CommandText = "SET SHOWPLAN_XML ON;";
        enableCmd.CommandType = CommandType.Text;
        enableCmd.CommandTimeout = commandTimeoutSeconds;
        await enableCmd.ExecuteNonQueryAsync();

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = commandTimeoutSeconds;

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            if (await reader.ReadAsync())
            {
                // The first resultset should contain the plan XML in the first column
                var xml = reader.GetValue(0)?.ToString();
                return xml;
            }
            return null;
        }
        finally
        {
            await using var disableCmd = conn.CreateCommand();
            disableCmd.CommandText = "SET SHOWPLAN_XML OFF;";
            disableCmd.CommandType = CommandType.Text;
            disableCmd.CommandTimeout = commandTimeoutSeconds;
            await disableCmd.ExecuteNonQueryAsync();
        }
    }
}
