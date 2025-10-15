namespace AX2012PerformanceOptimizer.Core.Models;

public enum SuggestionSeverity
{
    Info,
    Warning,
    Critical
}

public enum SuggestionCategory
{
    Index,
    QueryRewrite,
    Statistics,
    Caching,
    Configuration,
    TableDesign
}

public class QueryOptimizationSuggestion
{
    public SuggestionCategory Category { get; set; }
    public SuggestionSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
    public string? SqlCode { get; set; }
    public double EstimatedImpact { get; set; } // 0-100%
}
