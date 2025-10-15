namespace AX2012PerformanceOptimizer.Core.Models;

/// <summary>
/// Detailed index recommendation from AI
/// </summary>
public class IndexRecommendation
{
    public string TableName { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public List<string> KeyColumns { get; set; } = new();
    public List<string> IncludeColumns { get; set; } = new();
    public string CreateScript { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public int EstimatedImpact { get; set; } // 0-100
    public bool IsUnique { get; set; }
    public bool IsClustered { get; set; }
}

/// <summary>
/// Estimated cost of running a query
/// </summary>
public class QueryCostEstimate
{
    public double CpuCostMs { get; set; }
    public double IoCost { get; set; }
    public double MemoryCostMb { get; set; }
    public double MonetaryCostUsd { get; set; } // Estimated cloud cost
    public double EstimatedDailyCost { get; set; }
    public double EstimatedMonthlyCost { get; set; }
    public string Currency { get; set; } = "USD";
    public string CostBreakdown { get; set; } = string.Empty;
    public string OptimizationPotential { get; set; } = string.Empty;
}

/// <summary>
/// Comparison between original and optimized query
/// </summary>
public class QueryComparisonResult
{
    public string OriginalQuery { get; set; } = string.Empty;
    public string OptimizedQuery { get; set; } = string.Empty;
    public List<string> KeyDifferences { get; set; } = new();
    public double EstimatedSpeedup { get; set; } // Multiplier (e.g., 2.5x faster)
    public string Summary { get; set; } = string.Empty;
    public List<QueryImprovementArea> Improvements { get; set; } = new();
}

public class QueryImprovementArea
{
    public string Area { get; set; } = string.Empty; // e.g., "JOIN optimization", "WHERE clause"
    public string Before { get; set; } = string.Empty;
    public string After { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}
