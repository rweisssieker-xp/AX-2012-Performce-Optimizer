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
                "gpt-4o-mini",      // Primary: Cheapest, still very capable
                "gpt-3.5-turbo"     // Fallback: Legacy but reliable
            }
        },
        {
            AiModelTier.Balanced, new List<string>
            {
                "gpt-4o",           // Primary: Best balance
                "gpt-4o-mini",      // Fallback: Still good
                "gpt-4-turbo"       // Fallback: Older but solid
            }
        },
        {
            AiModelTier.Premium, new List<string>
            {
                "o1-mini",          // Primary: Reasoning model
                "o1-preview",       // Fallback: Most capable
                "gpt-4o",           // Fallback: Fast alternative
                "gpt-4"             // Fallback: Reliable
            }
        }
    };

    /// <summary>
    /// Model costs per 1M tokens (input, approximate 2025 pricing)
    /// </summary>
    private static readonly Dictionary<string, decimal> ModelCosts = new()
    {
        { "gpt-4o-mini", 0.15m },
        { "gpt-3.5-turbo", 0.50m },
        { "gpt-4o", 2.50m },
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
            "complexity-score" => "gpt-4o-mini",         // Ultra cheap, simple task
            "validation" => "gpt-4o-mini",               // Ultra cheap
            "documentation" => "gpt-4o-mini",            // Ultra cheap
            "simple-analysis" => "gpt-4o-mini",          // Ultra cheap

            "query-analysis" => "gpt-4o",                // Balanced
            "index-recommendation" => "gpt-4o",          // Balanced
            "cost-estimation" => "gpt-4o-mini",          // Ultra cheap
            "performance-prediction" => "gpt-4o",        // Balanced

            "batch-analysis" => "o1-mini",               // Premium
            "complex-optimization" => "o1-mini",         // Premium
            "cross-query-optimization" => "o1-mini",     // Premium
            "business-logic-analysis" => "o1-mini",      // Premium

            _ => "gpt-4o"                                // Default: balanced
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
