namespace AX2012PerformanceOptimizer.Core.Services.Explanation;

/// <summary>
/// Service for generating simple, plain-language explanations
/// </summary>
public interface ISimpleExplanationService
{
    /// <summary>
    /// Whether simple explanation mode is enabled
    /// </summary>
    bool IsSimpleModeEnabled { get; set; }

    /// <summary>
    /// Generates a simple explanation for a technical term
    /// </summary>
    Task<string> GenerateSimpleExplanationAsync(string technicalTerm, string? context = null);

    /// <summary>
    /// Generates a simple explanation with analogy
    /// </summary>
    Task<string> GenerateExplanationWithAnalogyAsync(string technicalTerm, string? context = null);

    /// <summary>
    /// Translates technical text to plain language
    /// </summary>
    string TranslateToPlainLanguage(string technicalText);

    /// <summary>
    /// Gets explanation examples for common terms
    /// </summary>
    Dictionary<string, string> GetExplanationExamples();
}
