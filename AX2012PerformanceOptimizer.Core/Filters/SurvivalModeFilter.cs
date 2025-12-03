using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Filters;

/// <summary>
/// Filter for Survival Mode - shows only critical and high-priority optimizations
/// </summary>
public static class SurvivalModeFilter
{
    /// <summary>
    /// Maximum number of recommendations to show in Survival Mode
    /// </summary>
    public const int MaxRecommendations = 10;

    /// <summary>
    /// Minimum impact threshold (0-100)
    /// </summary>
    public const double MinImpactThreshold = 70.0;

    /// <summary>
    /// Filters recommendations for Survival Mode
    /// </summary>
    public static List<Recommendation> Filter(List<Recommendation> recommendations)
    {
        return recommendations
            .Where(r => IsSurvivalModeEligible(r))
            .OrderByDescending(r => GetCriticalityScore(r))
            .ThenByDescending(r => r.Priority)
            .Take(MaxRecommendations)
            .ToList();
    }

    /// <summary>
    /// Checks if a recommendation is eligible for Survival Mode
    /// </summary>
    private static bool IsSurvivalModeEligible(Recommendation recommendation)
    {
        // Only Critical or High priority
        if (recommendation.Priority != RecommendationPriority.Critical &&
            recommendation.Priority != RecommendationPriority.High)
        {
            return false;
        }

        // Check impact threshold (if ImpactAnalysis contains percentage)
        var impactScore = ExtractImpactScore(recommendation.ImpactAnalysis);
        if (impactScore < MinImpactThreshold)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets criticality score for sorting (higher = more critical)
    /// </summary>
    private static int GetCriticalityScore(Recommendation recommendation)
    {
        var baseScore = recommendation.Priority switch
        {
            RecommendationPriority.Critical => 100,
            RecommendationPriority.High => 50,
            _ => 0
        };

        // Add bonus for certain categories
        var categoryBonus = recommendation.Category switch
        {
            RecommendationCategory.SqlQueryOptimization => 20,
            RecommendationCategory.IndexManagement => 15,
            RecommendationCategory.DatabaseMaintenance => 10,
            _ => 0
        };

        return baseScore + categoryBonus;
    }

    /// <summary>
    /// Extracts impact score from ImpactAnalysis text (simplified)
    /// </summary>
    private static double ExtractImpactScore(string impactAnalysis)
    {
        if (string.IsNullOrEmpty(impactAnalysis))
        {
            return 50; // Default medium impact
        }

        // Look for percentage patterns in the text
        var lowerText = impactAnalysis.ToLowerInvariant();
        
        // Check for high impact keywords
        if (lowerText.Contains("critical") || lowerText.Contains("severe") || lowerText.Contains("urgent"))
        {
            return 90;
        }

        if (lowerText.Contains("high") || lowerText.Contains("significant"))
        {
            return 80;
        }

        if (lowerText.Contains("medium") || lowerText.Contains("moderate"))
        {
            return 60;
        }

        // Default based on priority
        return 70;
    }

    /// <summary>
    /// Gets the count of filtered recommendations
    /// </summary>
    public static int GetFilteredCount(List<Recommendation> allRecommendations, List<Recommendation> filteredRecommendations)
    {
        return filteredRecommendations.Count;
    }

    /// <summary>
    /// Gets the total count of recommendations
    /// </summary>
    public static int GetTotalCount(List<Recommendation> recommendations)
    {
        return recommendations.Count;
    }
}
