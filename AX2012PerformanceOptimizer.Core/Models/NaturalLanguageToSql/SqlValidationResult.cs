namespace AX2012PerformanceOptimizer.Core.Models.NaturalLanguageToSql;

/// <summary>
/// Result of SQL validation
/// </summary>
public class SqlValidationResult
{
    /// <summary>
    /// Whether the SQL is syntactically valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether the SQL is safe to execute (no DDL, DML, etc.)
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// Detected SQL command type (SELECT, INSERT, UPDATE, etc.)
    /// </summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Security concerns detected
    /// </summary>
    public List<string> SecurityConcerns { get; set; } = new();

    /// <summary>
    /// Performance concerns detected
    /// </summary>
    public List<string> PerformanceConcerns { get; set; } = new();

    /// <summary>
    /// Estimated complexity score (0-100)
    /// </summary>
    public double ComplexityScore { get; set; }

    /// <summary>
    /// Detailed validation message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
