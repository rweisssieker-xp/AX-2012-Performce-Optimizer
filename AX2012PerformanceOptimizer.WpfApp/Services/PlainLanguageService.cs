using System.Collections.Generic;
using System.Linq;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for translating technical terms to plain language
/// </summary>
public class PlainLanguageService
{
    private readonly Dictionary<string, string> _translations = new()
    {
        // Query Performance
        { "Execution Time", "How long the query takes to run" },
        { "CPU Time", "Processing time used" },
        { "Logical Reads", "How much data was read from memory" },
        { "Physical Reads", "How much data was read from disk" },
        { "Query Hash", "Unique identifier for this query" },
        { "Execution Plan", "Step-by-step plan of how the query runs" },
        
        // Database Health
        { "Fragmentation", "How scattered your data is on disk" },
        { "Index", "Quick lookup table for finding data" },
        { "Statistics", "Information about your data that helps queries run faster" },
        { "Backup", "Copy of your data for safety" },
        { "Recovery", "Restoring data from a backup" },
        
        // Performance Metrics
        { "Wait Stats", "What the database is waiting for" },
        { "Blocking", "When one query stops another from running" },
        { "Deadlock", "When two queries are stuck waiting for each other" },
        { "Latch", "A lock on memory" },
        { "Spinlock", "A fast lock mechanism" },
        
        // AOS Performance
        { "AOS", "Application Object Server - the main AX server" },
        { "Batch Job", "Automated task that runs in the background" },
        { "Session", "A user's connection to the system" },
        { "Connection Pool", "Group of reusable connections" },
        
        // General
        { "Performance", "How fast and efficient the system is" },
        { "Optimization", "Making things run faster" },
        { "Bottleneck", "The slowest part that's holding everything back" },
        { "Throughput", "How much work gets done per second" },
        { "Latency", "Delay or lag time" },
        { "Scalability", "Ability to handle more work" }
    };

    public bool IsPlainLanguageEnabled { get; set; }

    public string Translate(string technicalTerm)
    {
        if (!IsPlainLanguageEnabled)
        {
            return technicalTerm;
        }

        return _translations.TryGetValue(technicalTerm, out var translation) 
            ? translation 
            : technicalTerm;
    }

    public string TranslateText(string text)
    {
        if (!IsPlainLanguageEnabled)
        {
            return text;
        }

        var result = text;
        foreach (var kvp in _translations.OrderByDescending(x => x.Key.Length))
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }
        return result;
    }

    public void AddTranslation(string technicalTerm, string plainLanguage)
    {
        _translations[technicalTerm] = plainLanguage;
    }
}

