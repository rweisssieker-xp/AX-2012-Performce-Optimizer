using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Advanced Query Correlation Engine - Discovers hidden relationships and dependencies between queries
/// UNIQUE FEATURE: No other monitoring tool offers deep query correlation analysis
/// </summary>
public interface IQueryCorrelationEngine
{
    /// <summary>
    /// Analyze correlations between queries to discover hidden dependencies
    /// </summary>
    Task<CorrelationAnalysisResult> AnalyzeQueryCorrelationsAsync(
        List<SqlQueryMetric> queries,
        List<HistoricalQuerySnapshot> historicalData);

    /// <summary>
    /// Find queries that always execute together (cascade patterns)
    /// </summary>
    Task<List<QueryCascade>> FindQueryCascadesAsync(
        List<SqlQueryMetric> queries,
        TimeSpan timeWindow);

    /// <summary>
    /// Detect queries that compete for same resources (contention)
    /// </summary>
    Task<List<ResourceContention>> DetectResourceContentionAsync(
        List<SqlQueryMetric> queries);

    /// <summary>
    /// Identify optimal execution order to minimize total execution time
    /// </summary>
    Task<ExecutionPlan> OptimizeExecutionOrderAsync(
        List<SqlQueryMetric> queries);

    /// <summary>
    /// Predict performance impact if one query is optimized
    /// </summary>
    Task<CorrelationImpact> PredictOptimizationImpactAsync(
        SqlQueryMetric targetQuery,
        List<SqlQueryMetric> relatedQueries);

    /// <summary>
    /// Build dependency graph showing query relationships
    /// </summary>
    Task<DependencyGraph> BuildDependencyGraphAsync(
        List<SqlQueryMetric> queries);
}

/// <summary>
/// Complete correlation analysis result
/// </summary>
public class CorrelationAnalysisResult
{
    public DateTime AnalysisDate { get; set; }
    public int TotalQueriesAnalyzed { get; set; }
    public int CorrelationsFound { get; set; }

    // Discovered patterns
    public List<QueryCascade> Cascades { get; set; } = new();
    public List<ResourceContention> Contentions { get; set; } = new();
    public List<QueryCorrelation> Correlations { get; set; } = new();

    // Insights
    public List<string> KeyFindings { get; set; } = new();
    public List<string> OptimizationOpportunities { get; set; } = new();

    // Performance impact
    public double EstimatedTimeWasted { get; set; }
    public double PotentialSavings { get; set; }

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Query cascade - queries that execute together
/// </summary>
public class QueryCascade
{
    public string CascadeId { get; set; } = string.Empty;
    public string TriggerQuery { get; set; } = string.Empty;
    public List<string> FollowingQueries { get; set; } = new();

    // Timing
    public double AverageDelay { get; set; } // ms between queries
    public double Confidence { get; set; } // 0-100%
    public int ObservationCount { get; set; }

    // Impact
    public double TotalCascadeTime { get; set; }
    public string CascadeType { get; set; } = string.Empty; // Sequential, Parallel, Mixed

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Resource contention between queries
/// </summary>
public class ResourceContention
{
    public string ContentionId { get; set; } = string.Empty;
    public List<string> CompetingQueries { get; set; } = new();

    // Resources
    public string ResourceType { get; set; } = string.Empty; // Table, Index, Lock, CPU, IO
    public string ResourceName { get; set; } = string.Empty;

    // Severity
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public double ContentionScore { get; set; } // 0-100

    // Impact
    public double AverageWaitTime { get; set; }
    public double PerformanceDegradation { get; set; } // %
    public int AffectedExecutions { get; set; }

    // Resolution
    public List<string> Recommendations { get; set; } = new();

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Correlation between two queries
/// </summary>
public class QueryCorrelation
{
    public string Query1Hash { get; set; } = string.Empty;
    public string Query2Hash { get; set; } = string.Empty;

    // Correlation metrics
    public double CorrelationCoefficient { get; set; } // -1 to +1
    public string CorrelationType { get; set; } = string.Empty; // Positive, Negative, None

    // Patterns
    public bool ExecuteTogether { get; set; }
    public bool ShareResources { get; set; }
    public bool CausativeRelationship { get; set; }

    // Confidence
    public double Confidence { get; set; }
    public int ObservationCount { get; set; }

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Optimized execution plan
/// </summary>
public class ExecutionPlan
{
    public string PlanId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Execution order
    public List<ExecutionStep> Steps { get; set; } = new();

    // Performance prediction
    public double CurrentTotalTime { get; set; }
    public double OptimizedTotalTime { get; set; }
    public double TimeSavings { get; set; }
    public double ImprovementPercent { get; set; }

    // Parallelization
    public int ParallelBatches { get; set; }
    public List<List<string>> ParallelGroups { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual execution step
/// </summary>
public class ExecutionStep
{
    public int StepNumber { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public string QueryPreview { get; set; } = string.Empty;

    public string ExecutionMode { get; set; } = string.Empty; // Sequential, Parallel
    public int ParallelBatch { get; set; }

    public double EstimatedTime { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public List<string> BlockedBy { get; set; } = new();

    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Correlation impact prediction
/// </summary>
public class CorrelationImpact
{
    public string TargetQueryHash { get; set; } = string.Empty;
    public int AffectedQueries { get; set; }

    // Direct impact
    public double DirectTimeSaving { get; set; }
    public double DirectImprovementPercent { get; set; }

    // Indirect impact (cascade effects)
    public double IndirectTimeSaving { get; set; }
    public double IndirectImprovementPercent { get; set; }

    // Total impact
    public double TotalTimeSaving { get; set; }
    public double TotalImprovementPercent { get; set; }

    // Affected queries
    public List<AffectedQuery> AffectedQueryDetails { get; set; } = new();

    // Risk assessment
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
    public List<string> Risks { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Query affected by optimization
/// </summary>
public class AffectedQuery
{
    public string QueryHash { get; set; } = string.Empty;
    public string QueryPreview { get; set; } = string.Empty;

    public string ImpactType { get; set; } = string.Empty; // Positive, Negative, Neutral
    public double ExpectedChange { get; set; } // ms
    public double ExpectedChangePercent { get; set; }

    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Dependency graph of queries
/// </summary>
public class DependencyGraph
{
    public DateTime CreatedDate { get; set; }
    public int TotalNodes { get; set; }
    public int TotalEdges { get; set; }

    // Graph data
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();

    // Analysis
    public List<string> CriticalPaths { get; set; } = new();
    public List<string> BottleneckQueries { get; set; } = new();
    public List<string> IndependentQueries { get; set; } = new();

    // Metrics
    public int MaxDependencyDepth { get; set; }
    public double AverageDependenciesPerQuery { get; set; }

    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Node in dependency graph
/// </summary>
public class GraphNode
{
    public string QueryHash { get; set; } = string.Empty;
    public string QueryPreview { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    // Position (for visualization)
    public int Level { get; set; }
    public int Position { get; set; }

    // Metrics
    public double ExecutionTime { get; set; }
    public int IncomingEdges { get; set; }
    public int OutgoingEdges { get; set; }

    public string NodeType { get; set; } = string.Empty; // Source, Intermediate, Sink
}

/// <summary>
/// Edge in dependency graph
/// </summary>
public class GraphEdge
{
    public string FromQueryHash { get; set; } = string.Empty;
    public string ToQueryHash { get; set; } = string.Empty;

    public string RelationshipType { get; set; } = string.Empty; // Triggers, Follows, Blocks, Shares
    public double Strength { get; set; } // 0-1
    public double AverageDelay { get; set; } // ms

    public string Description { get; set; } = string.Empty;
}
