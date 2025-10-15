using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IQueryAnalyzerService
{
    Task<List<QueryOptimizationSuggestion>> AnalyzeQueryAsync(SqlQueryMetric query);
    Task<List<MissingIndex>> DetectMissingIndexesForQueryAsync(string queryText);
    Task<PerformancePrediction> PredictPerformanceAsync(SqlQueryMetric query);
    Task<PerformancePrediction> PredictPerformanceWithOptimizationsAsync(SqlQueryMetric query, List<QueryOptimizationSuggestion> suggestions);
}

public class PerformancePrediction
{
    public double PredictedCpuTimeMs { get; set; }
    public long PredictedLogicalReads { get; set; }
    public long PredictedPhysicalReads { get; set; }
    public double PredictedDurationMs { get; set; }
    public double ConfidenceScore { get; set; }
    public string PredictionModel { get; set; } = "Statistical";
    public List<PerformanceFactor> ContributingFactors { get; set; } = new();
    public PerformanceImpact? OptimizationImpact { get; set; }
}

public class PerformanceFactor
{
    public string Factor { get; set; } = string.Empty;
    public double ImpactPercent { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PerformanceImpact
{
    public double CpuTimeReductionPercent { get; set; }
    public double LogicalReadsReductionPercent { get; set; }
    public double DurationReductionPercent { get; set; }
    public double OverallImprovementPercent { get; set; }
    public string Summary { get; set; } = string.Empty;
}
