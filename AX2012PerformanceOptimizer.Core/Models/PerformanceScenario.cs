namespace AX2012PerformanceOptimizer.Core.Models;

public enum ScenarioType
{
    UserGrowth,
    DataGrowth,
    LoadIncrease,
    NewFeatureRollout,
    SeasonalPeak
}

public class BusinessScenario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ScenarioType Type { get; set; }
    public double UserMultiplier { get; set; } = 1.0;
    public double DataGrowthRate { get; set; } = 0.0;
    public double LoadMultiplier { get; set; } = 1.0;
    public DateTime TargetDate { get; set; }
}

public class ScenarioForecast
{
    public string ScenarioId { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public List<Prediction> Predictions { get; set; } = new();
    public List<string> BottleneckAlerts { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

public class Prediction
{
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PredictedValue { get; set; }
    public double ChangePercent { get; set; }
    public RiskLevel Risk { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}
