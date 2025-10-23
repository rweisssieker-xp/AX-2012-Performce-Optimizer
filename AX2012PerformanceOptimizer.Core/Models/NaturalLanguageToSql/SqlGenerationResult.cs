namespace AX2012PerformanceOptimizer.Core.Models.NaturalLanguageToSql;

/// <summary>
/// Result of SQL generation from natural language
/// </summary>
public class SqlGenerationResult
{
    /// <summary>
    /// The generated SQL query
    /// </summary>
    public string GeneratedSql { get; set; } = string.Empty;

    /// <summary>
    /// Explanation of what the SQL does
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score (0-100) of the generation
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Tables used in the query
    /// </summary>
    public List<string> TablesUsed { get; set; } = new();

    /// <summary>
    /// Columns selected
    /// </summary>
    public List<string> ColumnsUsed { get; set; } = new();

    /// <summary>
    /// Whether the query is safe to execute (read-only, no DDL)
    /// </summary>
    public bool IsSafeToExecute { get; set; }

    /// <summary>
    /// Any warnings or caveats about the generated SQL
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Suggested optimizations
    /// </summary>
    public List<string> OptimizationSuggestions { get; set; } = new();

    /// <summary>
    /// Alternative SQL variations
    /// </summary>
    public List<string> AlternativeQueries { get; set; } = new();

    /// <summary>
    /// Time taken to generate (milliseconds)
    /// </summary>
    public double GenerationTimeMs { get; set; }

    /// <summary>
    /// Whether generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
