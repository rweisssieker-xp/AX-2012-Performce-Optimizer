using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Intelligent Query Rewriter - LLM-based SQL query optimization
/// UNIQUE FEATURE #10: Context-aware query rewriting with AI
/// </summary>
public interface IIntelligentQueryRewriter
{
    /// <summary>
    /// Rewrite a SQL query using AI to optimize performance
    /// </summary>
    Task<QueryRewriteResult> RewriteQueryAsync(
        string originalQuery,
        QueryRewriteContext context);

    /// <summary>
    /// Analyze a query and suggest multiple rewrite options
    /// </summary>
    Task<List<QueryRewriteOption>> SuggestRewriteOptionsAsync(
        string originalQuery,
        QueryRewriteContext context);

    /// <summary>
    /// Explain why a rewrite is beneficial
    /// </summary>
    Task<string> ExplainRewriteAsync(
        string originalQuery,
        string rewrittenQuery);

    /// <summary>
    /// Validate that a rewritten query is semantically equivalent
    /// </summary>
    Task<ValidationResult> ValidateRewriteAsync(
        string originalQuery,
        string rewrittenQuery);

    /// <summary>
    /// Get AX 2012-specific query patterns and anti-patterns
    /// </summary>
    Task<List<QueryPattern>> GetAX2012PatternsAsync();
}

/// <summary>
/// Query Rewrite Result
/// </summary>
public class QueryRewriteResult
{
    public string OriginalQuery { get; set; } = string.Empty;
    public string RewrittenQuery { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;

    // Performance improvement estimate
    public double EstimatedImprovementPercentage { get; set; }
    public string ImprovementReason { get; set; } = string.Empty;

    // Changes made
    public List<string> ChangesSummary { get; set; } = new();
    public List<QueryOptimizationTechnique> TechniquesApplied { get; set; } = new();

    // Validation
    public bool IsSemanticallyEquivalent { get; set; }
    public double ConfidenceScore { get; set; }

    // AX 2012 specific
    public List<string> AX2012BestPractices { get; set; } = new();
    public List<string> WarningsOrCaveats { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Query Rewrite Context
/// </summary>
public class QueryRewriteContext
{
    // Original query context
    public string DatabaseName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "dbo";
    public bool IsAX2012Query { get; set; } = true;

    // Performance context
    public long CurrentExecutionTimeMs { get; set; }
    public int ExecutionCount { get; set; }
    public long LogicalReads { get; set; }

    // Optimization goals
    public List<OptimizationGoal> Goals { get; set; } = new();
    public OptimizationPriority Priority { get; set; } = OptimizationPriority.Balanced;

    // Constraints
    public bool AllowTableHints { get; set; } = true;
    public bool AllowIndexHints { get; set; } = true;
    public bool AllowStructuralChanges { get; set; } = true;

    // AX 2012 specific
    public string AXVersion { get; set; } = "AX 2012 R3";
    public List<string> AvailableIndexes { get; set; } = new();
}

/// <summary>
/// Query Rewrite Option (alternative suggestions)
/// </summary>
public class QueryRewriteOption
{
    public string OptionName { get; set; } = string.Empty;
    public string RewrittenQuery { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public double EstimatedImprovementPercentage { get; set; }
    public string Approach { get; set; } = string.Empty; // "Index Hints", "Query Restructure", "CTE", etc.

    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();

    public int ComplexityScore { get; set; } // 1-10 (1=simple, 10=complex)
    public int RiskScore { get; set; } // 1-10 (1=safe, 10=risky)

    public bool Recommended { get; set; }
}

/// <summary>
/// Validation Result for rewritten queries
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public bool IsSemanticallyEquivalent { get; set; }
    public double ConfidenceScore { get; set; }

    public List<string> ValidationChecks { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public string ValidationMethod { get; set; } = string.Empty; // "Syntax", "Plan Comparison", "Sample Execution"
}

/// <summary>
/// Query Pattern (AX 2012 specific patterns)
/// </summary>
public class QueryPattern
{
    public string PatternName { get; set; } = string.Empty;
    public string PatternType { get; set; } = string.Empty; // "GoodPattern" or "AntiPattern"
    public string Description { get; set; } = string.Empty;

    public string ExampleBefore { get; set; } = string.Empty;
    public string ExampleAfter { get; set; } = string.Empty;

    public string ImpactLevel { get; set; } = string.Empty; // "High", "Medium", "Low"
    public List<string> AffectedTables { get; set; } = new();

    public bool IsAX2012Specific { get; set; }
}

/// <summary>
/// Query Optimization Technique
/// </summary>
public class QueryOptimizationTechnique
{
    public string TechniqueName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Indexing", "Join", "Subquery", "CTE", etc.
    public string Description { get; set; } = string.Empty;
    public double ImpactPercentage { get; set; }
}

/// <summary>
/// Optimization Goal
/// </summary>
public enum OptimizationGoal
{
    ReduceCPU,
    ReduceIO,
    ReduceMemory,
    ReduceExecutionTime,
    ReduceLocking,
    ImproveReadability
}

/// <summary>
/// Optimization Priority
/// </summary>
public enum OptimizationPriority
{
    Performance,    // Maximum performance, may sacrifice readability
    Balanced,       // Balance between performance and maintainability
    Maintainability // Focus on code clarity, minimal changes
}
