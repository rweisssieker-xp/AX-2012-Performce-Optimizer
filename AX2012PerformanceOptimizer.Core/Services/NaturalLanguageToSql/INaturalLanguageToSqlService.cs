using AX2012PerformanceOptimizer.Core.Models.NaturalLanguageToSql;

namespace AX2012PerformanceOptimizer.Core.Services.NaturalLanguageToSql;

/// <summary>
/// Service for generating SQL queries from natural language descriptions
/// </summary>
public interface INaturalLanguageToSqlService
{
    /// <summary>
    /// Whether the AI service is available and configured
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Configure the AI service with API credentials
    /// </summary>
    void Configure(string apiKey, string endpoint, string model = "gpt-4o", bool isAzure = false);

    /// <summary>
    /// Generate SQL query from natural language description
    /// </summary>
    Task<SqlGenerationResult> GenerateSqlAsync(SqlGenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate a generated SQL query
    /// </summary>
    Task<SqlValidationResult> ValidateSqlAsync(string sqlQuery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Improve an existing SQL query based on feedback
    /// </summary>
    Task<SqlGenerationResult> RefineSqlAsync(string originalSql, string feedback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate example queries based on common patterns
    /// </summary>
    Task<List<string>> GetExampleQueriesAsync(CancellationToken cancellationToken = default);
}
