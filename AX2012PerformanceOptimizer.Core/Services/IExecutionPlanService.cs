using System.Threading.Tasks;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IExecutionPlanService
{
    Task<string?> GetEstimatedPlanXmlAsync(string sqlText, int commandTimeoutSeconds = 15);
}
