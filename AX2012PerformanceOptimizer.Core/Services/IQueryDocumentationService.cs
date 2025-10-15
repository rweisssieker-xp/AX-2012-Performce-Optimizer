using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for generating query documentation
/// </summary>
public interface IQueryDocumentationService
{
    /// <summary>
    /// Generate comprehensive documentation for a query
    /// </summary>
    Task<QueryDocumentation> GenerateDocumentationAsync(string queryText, SqlQueryMetric? metrics = null);

    /// <summary>
    /// Generate simple human-readable explanation
    /// </summary>
    Task<string> ExplainQueryAsync(string queryText);

    /// <summary>
    /// Generate inline comments for query
    /// </summary>
    Task<string> AddInlineCommentsAsync(string queryText);

    /// <summary>
    /// Generate README for multiple queries (query catalog)
    /// </summary>
    Task<string> GenerateCatalogDocumentationAsync(List<SqlQueryMetric> queries);

    /// <summary>
    /// Generate markdown documentation
    /// </summary>
    Task<string> GenerateMarkdownAsync(QueryDocumentation documentation);

    /// <summary>
    /// Generate HTML documentation
    /// </summary>
    Task<string> GenerateHtmlAsync(QueryDocumentation documentation);
}

/// <summary>
/// Comprehensive query documentation
/// </summary>
public class QueryDocumentation
{
    public string QueryText { get; set; } = string.Empty;
    public string QueryName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tables { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public List<QueryParameter> Parameters { get; set; } = new();
    public QueryComplexityInfo Complexity { get; set; } = new();
    public QueryPerformanceInfo? Performance { get; set; }
    public List<string> BusinessRules { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public List<string> UseCases { get; set; } = new();
    public List<string> Notes { get; set; } = new();
    public string Author { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class QueryParameter
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public string? DefaultValue { get; set; }
}

public class QueryComplexityInfo
{
    public int Score { get; set; } // 0-100
    public string Level { get; set; } = string.Empty; // Simple, Medium, Complex, Very Complex
    public int JoinCount { get; set; }
    public int SubqueryCount { get; set; }
    public int AggregationCount { get; set; }
    public bool HasCte { get; set; }
    public bool HasWindowFunctions { get; set; }
    public List<string> ComplexityFactors { get; set; } = new();
}

public class QueryPerformanceInfo
{
    public double AvgExecutionTimeMs { get; set; }
    public long AvgLogicalReads { get; set; }
    public int ExecutionCount { get; set; }
    public string PerformanceRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
    public List<string> PerformanceNotes { get; set; } = new();
}
