using System.Threading.Tasks;

namespace AX2012PerformanceOptimizer.Data.SqlServer;

public interface ISqlExecutionPlanService
{
    Task<string?> GetEstimatedPlanXmlAsync(string sqlText, int commandTimeoutSeconds = 15);
}
