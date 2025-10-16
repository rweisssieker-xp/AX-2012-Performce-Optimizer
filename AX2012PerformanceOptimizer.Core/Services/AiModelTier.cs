namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// AI Model tiers based on cost and capability
/// </summary>
public enum AiModelTier
{
    /// <summary>
    /// Ultra cheap models for simple tasks ($0.001-0.003 per request)
    /// </summary>
    UltraCheap,

    /// <summary>
    /// Balanced cost/performance for standard tasks ($0.005-0.01 per request)
    /// </summary>
    Balanced,

    /// <summary>
    /// Premium models for complex analysis ($0.02-0.05 per request)
    /// </summary>
    Premium
}

/// <summary>
/// Smart model selection based on task complexity
/// </summary>
public static class AiModelSelector
{
    private static readonly Dictionary<AiModelTier, List<string>> TierModels = new()
    {
        {
            AiModelTier.UltraCheap, new List<string>
            {
                "gpt-5-nano",       // Primary: Ultra-cheap GPT-5 (16k context)
                "gpt-5-mini",       // Fallback: Lightweight GPT-5 (64k context)
                "gpt-4o-mini",      // Fallback: Previous gen cheap
                "gpt-3.5-turbo"     // Fallback: Legacy but reliable
            }
        },
        {
            AiModelTier.Balanced, new List<string>
            {
                "gpt-5",            // Primary: GPT-5 main model (200k context)
                "gpt-5-mini",       // Fallback: Lightweight but fast
                "gpt-4o",           // Fallback: GPT-4o best balance
                "gpt-4o-mini",      // Fallback: Still good
                "gpt-4-turbo"       // Fallback: Older but solid
            }
        },
        {
            AiModelTier.Premium, new List<string>
            {
                "gpt-5-thinking",   // Primary: BEST for SQL DMV Analysis (256k context)
                "gpt-5-thinking-mini", // Fallback: Good reasoning, lower cost (128k)
                "o1",               // Fallback: Full reasoning model
                "o1-mini",          // Fallback: Reasoning model
                "gpt-5",            // Fallback: Fast GPT-5
                "o1-preview"        // Fallback: Most capable
            }
        }
    };

    /// <summary>
    /// Model costs per 1M tokens (input, approximate 2025 pricing)
    /// </summary>
    private static readonly Dictionary<string, decimal> ModelCosts = new()
    {
        // GPT-5 Series (Latest 2025)
        { "gpt-5-nano", 0.10m },              // Ultra-cheap, 16k context
        { "gpt-5-mini", 0.20m },              // Lightweight, 64k context
        { "gpt-5", 3.00m },                   // Main model, 200k context (balanced)
        { "gpt-5-thinking-nano", 3.50m },     // Light reasoning, 64k context
        { "gpt-5-thinking-mini", 6.00m },     // Good reasoning, 128k context
        { "gpt-5-thinking", 12.00m },         // BEST for SQL DMV, 256k context

        // GPT-4 Series
        { "gpt-4o-mini", 0.15m },
        { "gpt-3.5-turbo", 0.50m },
        { "gpt-4o", 2.50m },
        { "o1", 5.00m },                      // Full reasoning model
        { "o1-mini", 3.00m },
        { "gpt-4-turbo", 10.00m },
        { "o1-preview", 15.00m },
        { "gpt-4", 30.00m },
        { "gpt-4-32k", 60.00m },
        { "gpt-3.5-turbo-16k", 1.00m }
    };

    /// <summary>
    /// Get recommended model for a given tier
    /// </summary>
    public static string GetModelForTier(AiModelTier tier)
    {
        return TierModels[tier].First();
    }

    /// <summary>
    /// Get all models for a tier (including fallbacks)
    /// </summary>
    public static List<string> GetModelsForTier(AiModelTier tier)
    {
        return TierModels[tier];
    }

    /// <summary>
    /// Automatically select tier based on query complexity
    /// </summary>
    public static AiModelTier SelectTierForComplexity(int complexityScore)
    {
        return complexityScore switch
        {
            <= 30 => AiModelTier.UltraCheap,    // Simple queries
            <= 70 => AiModelTier.Balanced,       // Medium complexity
            _ => AiModelTier.Premium             // Complex queries
        };
    }

    /// <summary>
    /// Estimate cost for a request
    /// </summary>
    public static decimal EstimateCost(string model, int estimatedTokens)
    {
        if (!ModelCosts.ContainsKey(model))
            return 0.01m; // Default estimate

        var costPer1M = ModelCosts[model];
        return (costPer1M / 1_000_000m) * estimatedTokens;
    }

    /// <summary>
    /// Get cost per 1M tokens for a model
    /// </summary>
    public static decimal GetModelCost(string model)
    {
        return ModelCosts.GetValueOrDefault(model, 5.00m);
    }

    /// <summary>
    /// Recommend model based on task type
    /// </summary>
    public static string RecommendModelForTask(string taskType)
    {
        return taskType.ToLower() switch
        {
            // Ultra cheap tasks
            "complexity-score" => "gpt-5-nano",          // Ultra cheap, simple task
            "validation" => "gpt-5-nano",                // Ultra cheap
            "documentation" => "gpt-5-mini",             // Ultra cheap
            "simple-analysis" => "gpt-5-mini",           // Ultra cheap

            // Balanced tasks
            "query-analysis" => "gpt-5",                 // Balanced - fast analysis
            "index-recommendation" => "gpt-5",           // Balanced
            "cost-estimation" => "gpt-5-mini",           // Balanced but cheap
            "performance-prediction" => "gpt-5",         // Balanced

            // Premium reasoning tasks (SQL-DMV specific!)
            "dmv-analysis" => "gpt-5-thinking",          // 🔥 BEST for SQL DMV Analysis
            "wait-stats-analysis" => "gpt-5-thinking",   // 🔥 BEST for Wait Stats patterns
            "lock-analysis" => "gpt-5-thinking",         // 🔥 BEST for Lock detection
            "io-bottleneck-analysis" => "gpt-5-thinking", // 🔥 BEST for I/O bottlenecks
            "query-plan-interpretation" => "gpt-5-thinking", // 🔥 BEST for Query Plans
            "performance-consulting" => "gpt-5-thinking-mini", // Good for consulting

            // Complex reasoning tasks
            "batch-analysis" => "gpt-5-thinking-mini",   // Premium reasoning
            "complex-optimization" => "gpt-5-thinking-mini", // Premium reasoning
            "cross-query-optimization" => "gpt-5-thinking", // Premium reasoning
            "business-logic-analysis" => "gpt-5-thinking-mini", // Premium reasoning

            _ => "gpt-5"                                 // Default: balanced GPT-5
        };
    }

    /// <summary>
    /// Calculate estimated savings by using tiered approach vs. always premium
    /// </summary>
    public static decimal CalculateSavings(Dictionary<string, int> taskCounts)
    {
        decimal premiumCost = 0;
        decimal tieredCost = 0;
        const int avgTokens = 2000;

        foreach (var (task, count) in taskCounts)
        {
            var recommendedModel = RecommendModelForTask(task);
            var premiumModel = "gpt-4"; // Expensive baseline

            premiumCost += EstimateCost(premiumModel, avgTokens * count);
            tieredCost += EstimateCost(recommendedModel, avgTokens * count);
        }

        return premiumCost - tieredCost;
    }
}
