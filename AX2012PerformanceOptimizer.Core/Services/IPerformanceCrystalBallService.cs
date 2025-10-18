using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Predictive "what-if" scenario analysis for performance
/// </summary>
public interface IPerformanceCrystalBallService
{
    Task<ScenarioForecast> PredictScenarioAsync(BusinessScenario scenario);
    Task<List<BusinessScenario>> GetPredefinedScenariosAsync();
    Task<List<string>> IdentifyFutureBottlenecksAsync(BusinessScenario scenario);
}
