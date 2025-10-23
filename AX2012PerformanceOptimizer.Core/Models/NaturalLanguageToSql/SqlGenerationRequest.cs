namespace AX2012PerformanceOptimizer.Core.Models.NaturalLanguageToSql;

/// <summary>
/// Request model for natural language to SQL generation
/// </summary>
public class SqlGenerationRequest
{
    /// <summary>
    /// The natural language query from the user
    /// </summary>
    public string NaturalLanguageQuery { get; set; } = string.Empty;

    /// <summary>
    /// Optional context about the database schema
    /// </summary>
    public string? DatabaseContext { get; set; }

    /// <summary>
    /// Optional: Specific tables to consider
    /// </summary>
    public List<string> TableHints { get; set; } = new();

    /// <summary>
    /// Optional: DataAreaId for AX 2012 multi-company filtering
    /// </summary>
    public string? DataAreaId { get; set; }

    /// <summary>
    /// Whether to include only read-only safe queries (SELECT only)
    /// </summary>
    public bool ReadOnlyMode { get; set; } = true;

    /// <summary>
    /// Optional: Example data to help with generation
    /// </summary>
    public Dictionary<string, string> Examples { get; set; } = new();

    /// <summary>
    /// Language of the natural language query (en, de, etc.)
    /// </summary>
    public string Language { get; set; } = "en";
}
