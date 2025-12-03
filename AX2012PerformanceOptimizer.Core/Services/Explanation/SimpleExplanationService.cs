using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services.Explanation;

/// <summary>
/// Service for generating simple, plain-language explanations
/// </summary>
public class SimpleExplanationService : ISimpleExplanationService
{
    private readonly ILogger<SimpleExplanationService> _logger;
    private readonly Dictionary<string, string> _explanations = new();
    private readonly Dictionary<string, string> _analogies = new();

    public bool IsSimpleModeEnabled { get; set; } = false;

    public SimpleExplanationService(ILogger<SimpleExplanationService> logger)
    {
        _logger = logger;
        InitializeExplanations();
        InitializeAnalogies();
    }

    public async Task<string> GenerateSimpleExplanationAsync(string technicalTerm, string? context = null)
    {
        if (!IsSimpleModeEnabled)
        {
            return technicalTerm;
        }

        await Task.Delay(10); // Simulate async operation

        if (_explanations.TryGetValue(technicalTerm, out var explanation))
        {
            return explanation;
        }

        // Fallback: return a generic explanation
        return $"This is a technical term related to database performance. In simple terms, it affects how fast your system runs.";
    }

    public async Task<string> GenerateExplanationWithAnalogyAsync(string technicalTerm, string? context = null)
    {
        if (!IsSimpleModeEnabled)
        {
            return technicalTerm;
        }

        await Task.Delay(10);

        var explanation = await GenerateSimpleExplanationAsync(technicalTerm, context);
        
        if (_analogies.TryGetValue(technicalTerm, out var analogy))
        {
            return $"{explanation}\n\nThink of it like this: {analogy}";
        }

        return explanation;
    }

    public string TranslateToPlainLanguage(string technicalText)
    {
        if (!IsSimpleModeEnabled)
        {
            return technicalText;
        }

        var result = technicalText;
        foreach (var kvp in _explanations.OrderByDescending(x => x.Key.Length))
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }
        return result;
    }

    public Dictionary<string, string> GetExplanationExamples()
    {
        return new Dictionary<string, string>(_explanations);
    }

    private void InitializeExplanations()
    {
        _explanations["Execution Time"] = "How long the query takes to run";
        _explanations["CPU Time"] = "Processing time used by the query";
        _explanations["Logical Reads"] = "How much data was read from memory";
        _explanations["Physical Reads"] = "How much data was read from disk (slower)";
        _explanations["Query Hash"] = "Unique identifier for this query";
        _explanations["Execution Plan"] = "Step-by-step plan showing how the query runs";
        _explanations["Fragmentation"] = "How scattered your data is on disk";
        _explanations["Index"] = "Quick lookup table for finding data faster";
        _explanations["Statistics"] = "Information about your data that helps queries run faster";
        _explanations["Wait Stats"] = "What the database is waiting for";
        _explanations["Blocking"] = "When one query stops another from running";
        _explanations["Deadlock"] = "When two queries are stuck waiting for each other";
        _explanations["Bottleneck"] = "The slowest part that's holding everything back";
        _explanations["Throughput"] = "How much work gets done per second";
        _explanations["Latency"] = "Delay or lag time";
        _explanations["Slow Query"] = "A query that takes too long to run";
        _explanations["Missing Index"] = "A helpful lookup table that doesn't exist yet";
        _explanations["Table Scan"] = "Reading through the entire table instead of using an index";
    }

    private void InitializeAnalogies()
    {
        _analogies["Execution Time"] = "Like how long it takes to find a book in a library";
        _analogies["Index"] = "Like an index in a book - helps you find what you need quickly";
        _analogies["Fragmentation"] = "Like files scattered all over your hard drive instead of organized";
        _analogies["Blocking"] = "Like one person holding a door closed so others can't get through";
        _analogies["Deadlock"] = "Like two cars at an intersection, each waiting for the other to move";
        _analogies["Bottleneck"] = "Like a narrow bridge that slows down all traffic";
        _analogies["Table Scan"] = "Like reading every page of a book to find one word, instead of using the index";
        _analogies["Missing Index"] = "Like trying to find a word in a dictionary without alphabetical order";
    }
}
