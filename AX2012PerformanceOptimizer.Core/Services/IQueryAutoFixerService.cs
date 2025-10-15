using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Automatic query fixing service
/// </summary>
public interface IQueryAutoFixerService
{
    /// <summary>
    /// Automatically fix common query performance issues
    /// </summary>
    Task<QueryFixResult> AutoFixQueryAsync(string queryText, QueryFixOptions? options = null);

    /// <summary>
    /// Preview fixes without applying them
    /// </summary>
    Task<List<QueryFix>> PreviewFixesAsync(string queryText);

    /// <summary>
    /// Apply a specific fix to a query
    /// </summary>
    Task<QueryFixResult> ApplyFixAsync(string queryText, QueryFix fix);

    /// <summary>
    /// Validate that a fixed query is semantically equivalent
    /// </summary>
    Task<QueryValidationResult> ValidateFixAsync(string originalQuery, string fixedQuery);
}

/// <summary>
/// Options for auto-fixing queries
/// </summary>
public class QueryFixOptions
{
    /// <summary>
    /// Only preview fixes, don't apply them
    /// </summary>
    public bool PreviewOnly { get; set; } = false;

    /// <summary>
    /// Minimum confidence score to apply a fix (0-1)
    /// </summary>
    public double MinConfidence { get; set; } = 0.7;

    /// <summary>
    /// Use AI for complex fixes
    /// </summary>
    public bool UseAi { get; set; } = true;

    /// <summary>
    /// Apply aggressive optimizations
    /// </summary>
    public bool AggressiveMode { get; set; } = false;

    /// <summary>
    /// Preserve query formatting
    /// </summary>
    public bool PreserveFormatting { get; set; } = true;
}

/// <summary>
/// Result of auto-fixing a query
/// </summary>
public class QueryFixResult
{
    public bool Success { get; set; }
    public string OriginalQuery { get; set; } = string.Empty;
    public string FixedQuery { get; set; } = string.Empty;
    public List<QueryFix> AppliedFixes { get; set; } = new();
    public QueryValidationResult? ValidationResult { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public double OverallConfidence { get; set; }
    public int EstimatedPerformanceImprovement { get; set; } // 0-100%
}

/// <summary>
/// A single query fix
/// </summary>
public class QueryFix
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public QueryFixType FixType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BeforeSnippet { get; set; } = string.Empty;
    public string AfterSnippet { get; set; } = string.Empty;
    public double Confidence { get; set; } // 0-1
    public int EstimatedImpact { get; set; } // 0-100%
    public QueryFixSafety Safety { get; set; }
    public bool RequiresValidation { get; set; } = true;
}

public enum QueryFixType
{
    SelectStarReplacement,
    OrToIn,
    OrToUnion,
    FunctionInWhereClause,
    NotInToNotExists,
    NotInToLeftJoin,
    LeadingWildcardRemoval,
    DistinctOptimization,
    ImplicitConversionFix,
    SubqueryOptimization,
    JoinReordering,
    IndexHint,
    ParameterizationFix
}

public enum QueryFixSafety
{
    Safe,           // No risk of changing results
    LowRisk,        // Minimal risk, well-tested pattern
    MediumRisk,     // Some risk, requires validation
    HighRisk,       // Significant risk, manual review recommended
    RequiresReview  // Must be reviewed before applying
}

/// <summary>
/// Validation result for a fixed query
/// </summary>
public class QueryValidationResult
{
    public bool IsValid { get; set; }
    public bool SemanticallyEquivalent { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public string ValidationMethod { get; set; } = string.Empty;
}
