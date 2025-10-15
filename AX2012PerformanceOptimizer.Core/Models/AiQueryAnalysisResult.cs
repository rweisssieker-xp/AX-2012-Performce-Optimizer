namespace AX2012PerformanceOptimizer.Core.Models;

/// <summary>
/// Result of AI-powered query analysis
/// </summary>
public class AiQueryAnalysisResult
{
    /// <summary>
    /// Human-readable explanation of performance issues
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// AI-generated optimization suggestions
    /// </summary>
    public List<AiOptimizationSuggestion> Suggestions { get; set; } = new();

    /// <summary>
    /// Optimized version of the query (if AI could generate one)
    /// </summary>
    public string? OptimizedQuery { get; set; }

    /// <summary>
    /// Overall performance assessment (0-100, higher is better)
    /// </summary>
    public int PerformanceScore { get; set; }

    /// <summary>
    /// Estimated performance improvement if suggestions are applied (percentage)
    /// </summary>
    public double EstimatedImprovement { get; set; }

    /// <summary>
    /// Whether AI analysis was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// AI-generated optimization suggestion with detailed reasoning
/// </summary>
public class AiOptimizationSuggestion
{
    /// <summary>
    /// Category of the optimization
    /// </summary>
    public SuggestionCategory Category { get; set; }

    /// <summary>
    /// Severity of the issue
    /// </summary>
    public SuggestionSeverity Severity { get; set; }

    /// <summary>
    /// Short title of the suggestion
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed AI-generated explanation
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// Specific code example or fix
    /// </summary>
    public string? CodeExample { get; set; }

    /// <summary>
    /// Why this optimization will help
    /// </summary>
    public string? Reasoning { get; set; }

    /// <summary>
    /// Estimated impact (0-100%)
    /// </summary>
    public double EstimatedImpact { get; set; }

    /// <summary>
    /// Difficulty of implementation (Easy, Medium, Hard)
    /// </summary>
    public ImplementationDifficulty Difficulty { get; set; }
}

public enum ImplementationDifficulty
{
    Easy,
    Medium,
    Hard,
    RequiresArchitectureChange
}
