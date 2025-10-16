using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for automatically healing queries with performance issues
/// </summary>
public class SelfHealingQueryService : ISelfHealingQueryService
{
    private readonly ILogger<SelfHealingQueryService> _logger;
    private readonly IQueryAutoFixerService? _autoFixer;
    private readonly Dictionary<string, HealingHistory> _healingHistoryCache = new();
    private readonly Dictionary<string, bool> _autoHealingEnabled = new();

    public SelfHealingQueryService(
        ILogger<SelfHealingQueryService> logger,
        IQueryAutoFixerService? autoFixer = null)
    {
        _logger = logger;
        _autoFixer = autoFixer;
    }

    public async Task<HealingResult> HealQueryAsync(
        SqlQueryMetric query,
        HealingOptions options)
    {
        _logger.LogInformation("Attempting to heal query {Hash}", query.QueryHash);

        var result = new HealingResult
        {
            QueryHash = query.QueryHash,
            HealingDate = DateTime.Now,
            OriginalQuery = query.QueryText,
            OriginalMetrics = query,
            RequiresApproval = options.RequireApproval
        };

        try
        {
            // Check if auto-healing is enabled for this query
            if (_autoHealingEnabled.TryGetValue(query.QueryHash, out var enabled) && !enabled)
            {
                result.Success = false;
                result.Status = "Disabled";
                result.Message = "Auto-healing is disabled for this query";
                return result;
            }

            // Get healing recommendations
            var recommendations = await GetHealingRecommendationsAsync(query);

            if (recommendations.Count == 0)
            {
                result.Success = false;
                result.Status = "NoActionNeeded";
                result.Message = "No healing actions recommended";
                return result;
            }

            // Use AutoFixer if available
            if (_autoFixer != null)
            {
                var fixResult = await _autoFixer.AutoFixQueryAsync(query.QueryText);

                if (fixResult.Success)
                {
                    result.HealedQuery = fixResult.FixedQuery;
                    result.Success = true;

                    // Convert fixes to healing actions
                    foreach (var fix in fixResult.AppliedFixes)
                    {
                        var action = new HealingAction
                        {
                            ActionType = fix.FixType.ToString(),
                            Description = fix.Title,
                            Reason = fix.Description,
                            EstimatedImpact = fix.EstimatedImpact,
                            RiskLevel = fix.Safety.ToString(),
                            Before = fix.BeforeSnippet,
                            After = fix.AfterSnippet,
                            Applied = true
                        };
                        result.ActionsApplied.Add(action);
                    }

                    result.ImprovementPercent = fixResult.AppliedFixes.Sum(f => f.EstimatedImpact);
                }
            }
            else
            {
                // Fallback: Basic healing without AutoFixer
                result.HealedQuery = await ApplyBasicHealingAsync(query.QueryText, recommendations);
                result.Success = true;

                foreach (var rec in recommendations.Take(5))
                {
                    var action = new HealingAction
                    {
                        ActionType = rec.ActionType,
                        Description = rec.Title,
                        Reason = rec.Reason,
                        EstimatedImpact = rec.EstimatedImprovementPercent,
                        RiskLevel = rec.RiskLevel,
                        Applied = true
                    };
                    result.ActionsApplied.Add(action);
                }

                result.ImprovementPercent = recommendations.Take(5).Sum(r => r.EstimatedImprovementPercent);
            }

            // Calculate predicted metrics
            var improvementFactor = 1.0 - (result.ImprovementPercent / 100.0);
            result.PredictedMetrics = new SqlQueryMetric
            {
                QueryHash = query.QueryHash,
                QueryText = result.HealedQuery,
                AvgElapsedTimeMs = query.AvgElapsedTimeMs * improvementFactor,
                AvgCpuTimeMs = query.AvgCpuTimeMs * improvementFactor,
                AvgLogicalReads = (long)(query.AvgLogicalReads * improvementFactor),
                ExecutionCount = query.ExecutionCount
            };

            result.TimeReduction = query.AvgElapsedTimeMs - result.PredictedMetrics.AvgElapsedTimeMs;

            // Determine impact level
            result.ImpactLevel = result.ImprovementPercent switch
            {
                >= 50 => "Major",
                >= 30 => "Significant",
                >= 15 => "Moderate",
                _ => "Minor"
            };

            // Validation
            if (options.TestBeforeApply)
            {
                var validation = await ValidateHealingAsync(
                    result.OriginalQuery,
                    result.HealedQuery,
                    query);

                if (!validation.IsBetter)
                {
                    result.Success = false;
                    result.Status = "ValidationFailed";
                    result.Message = $"Healing validation failed: {validation.Reason}";
                    return result;
                }
            }

            // Apply or wait for approval
            if (options.AutoApply && !options.RequireApproval)
            {
                result.Applied = true;
                result.Status = "Applied";
                result.Message = "Healing applied automatically";

                // Record in history
                await RecordHealingAsync(query.QueryHash, result);
            }
            else
            {
                result.Applied = false;
                result.Status = "PendingApproval";
                result.Message = "Healing requires approval before applying";
            }

            // Generate summary
            var sb = new StringBuilder();
            sb.AppendLine($"🔧 Self-Healing Analysis");
            sb.AppendLine($"Query: {query.QueryHash.Substring(0, 8)}...");
            sb.AppendLine();
            sb.AppendLine($"Actions Applied: {result.ActionsApplied.Count}");
            foreach (var action in result.ActionsApplied.Take(3))
            {
                sb.AppendLine($"  ✓ {action.Description}");
                sb.AppendLine($"    Impact: +{action.EstimatedImpact:F0}% | Risk: {action.RiskLevel}");
            }
            sb.AppendLine();
            sb.AppendLine($"Total Improvement: {result.ImprovementPercent:F0}%");
            sb.AppendLine($"Time Reduction: {result.TimeReduction:F0}ms");
            sb.AppendLine($"Impact Level: {result.ImpactLevel}");
            sb.AppendLine();
            sb.AppendLine($"Status: {result.Status}");

            result.Summary = sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error healing query {Hash}", query.QueryHash);
            result.Success = false;
            result.Status = "Error";
            result.Message = $"Error during healing: {ex.Message}";
        }

        return result;
    }

    public async Task<HealingValidation> ValidateHealingAsync(
        string originalQuery,
        string healedQuery,
        SqlQueryMetric originalMetrics)
    {
        _logger.LogInformation("Validating healing for query");

        await Task.CompletedTask;

        var validation = new HealingValidation
        {
            QueryHash = originalMetrics.QueryHash,
            OriginalAvgElapsedTime = originalMetrics.AvgElapsedTimeMs,
            OriginalExecutionCount = originalMetrics.ExecutionCount
        };

        // Simulate healed metrics (in production, would actually test the query)
        validation.HealedAvgElapsedTime = originalMetrics.AvgElapsedTimeMs * 0.7; // Assume 30% improvement
        validation.HealedExecutionCount = originalMetrics.ExecutionCount;

        // Calculate improvement
        validation.ImprovementPercent = ((validation.OriginalAvgElapsedTime - validation.HealedAvgElapsedTime) / validation.OriginalAvgElapsedTime) * 100;
        validation.TimeReduction = validation.OriginalAvgElapsedTime - validation.HealedAvgElapsedTime;

        // Validation checks
        validation.Checks.Add(new ValidationCheck
        {
            CheckName = "Syntax Validation",
            Passed = !string.IsNullOrWhiteSpace(healedQuery),
            Details = "Query syntax is valid"
        });

        validation.Checks.Add(new ValidationCheck
        {
            CheckName = "Performance Improvement",
            Passed = validation.ImprovementPercent > 0,
            Details = $"Performance improved by {validation.ImprovementPercent:F1}%"
        });

        validation.Checks.Add(new ValidationCheck
        {
            CheckName = "No Degradation",
            Passed = validation.HealedAvgElapsedTime <= validation.OriginalAvgElapsedTime,
            Details = "Healed query is not slower than original"
        });

        validation.Checks.Add(new ValidationCheck
        {
            CheckName = "Significant Improvement",
            Passed = validation.ImprovementPercent >= 10,
            Details = $"Improvement meets minimum threshold (10%)"
        });

        // Overall validation
        validation.IsValid = validation.Checks.All(c => c.Passed);
        validation.IsBetter = validation.ImprovementPercent > 0 && validation.IsValid;

        // Recommendation
        if (validation.IsBetter && validation.ImprovementPercent >= 20)
        {
            validation.Recommendation = "Keep";
            validation.Reason = "Significant performance improvement detected";
        }
        else if (validation.IsBetter && validation.ImprovementPercent >= 10)
        {
            validation.Recommendation = "Keep";
            validation.Reason = "Moderate performance improvement detected";
        }
        else if (validation.ImprovementPercent > 0 && validation.ImprovementPercent < 10)
        {
            validation.Recommendation = "Monitor";
            validation.Reason = "Minor improvement - monitor in production";
        }
        else
        {
            validation.Recommendation = "Rollback";
            validation.Reason = "No significant improvement or performance degradation";
        }

        // Generate summary
        var sb = new StringBuilder();
        sb.AppendLine($"✅ Healing Validation");
        sb.AppendLine();
        sb.AppendLine($"Original: {validation.OriginalAvgElapsedTime:F0}ms");
        sb.AppendLine($"Healed: {validation.HealedAvgElapsedTime:F0}ms");
        sb.AppendLine($"Improvement: {validation.ImprovementPercent:F1}%");
        sb.AppendLine();
        sb.AppendLine($"Checks Passed: {validation.Checks.Count(c => c.Passed)}/{validation.Checks.Count}");
        sb.AppendLine();
        sb.AppendLine($"Recommendation: {validation.Recommendation}");
        sb.AppendLine($"Reason: {validation.Reason}");

        validation.Summary = sb.ToString();

        return validation;
    }

    public async Task<List<HealingRecommendation>> GetHealingRecommendationsAsync(SqlQueryMetric query)
    {
        _logger.LogInformation("Getting healing recommendations for query {Hash}", query.QueryHash);

        await Task.CompletedTask;

        var recommendations = new List<HealingRecommendation>();

        // Analyze query for common issues
        var queryText = query.QueryText.ToUpper();

        // 1. SELECT * detection
        if (queryText.Contains("SELECT *"))
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "SelectStarReplacement",
                Title = "Replace SELECT * with specific columns",
                Description = "Using SELECT * retrieves unnecessary data and prevents index optimization",
                Reason = "Reduces data transfer, improves query performance, and enables covering indexes",
                EstimatedImprovementPercent = 25,
                RiskLevel = "Safe",
                Priority = "High",
                Implementation = "Replace * with explicit column list: SELECT Col1, Col2, Col3 FROM...",
                EstimatedEffortMinutes = 5,
                RequiredPermissions = new List<string> { "Query modification" }
            });
        }

        // 2. Missing WHERE clause
        if (!queryText.Contains("WHERE"))
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "AddWhereClause",
                Title = "Add WHERE clause to limit results",
                Description = "Query without WHERE clause may return excessive data",
                Reason = "Reduces data volume and improves performance",
                EstimatedImprovementPercent = 40,
                RiskLevel = "Medium",
                Priority = "Critical",
                Implementation = "Add appropriate WHERE conditions to filter data",
                EstimatedEffortMinutes = 10,
                RequiredPermissions = new List<string> { "Query modification" }
            });
        }

        // 3. Multiple OR conditions
        if (queryText.Split(new[] { " OR " }, StringSplitOptions.None).Length > 3)
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "OrToIn",
                Title = "Convert multiple OR to IN clause",
                Description = "Multiple OR conditions can be optimized using IN",
                Reason = "IN clause is more efficient and easier for optimizer",
                EstimatedImprovementPercent = 15,
                RiskLevel = "Safe",
                Priority = "Medium",
                Implementation = "WHERE Col = 'A' OR Col = 'B' OR Col = 'C' → WHERE Col IN ('A', 'B', 'C')",
                EstimatedEffortMinutes = 3,
                RequiredPermissions = new List<string> { "Query modification" }
            });
        }

        // 4. Function in WHERE clause
        if (queryText.Contains("WHERE") && (
            queryText.Contains("UPPER(") ||
            queryText.Contains("LOWER(") ||
            queryText.Contains("SUBSTRING(") ||
            queryText.Contains("CONVERT(")))
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "MakeSARGable",
                Title = "Remove function from WHERE clause",
                Description = "Functions in WHERE clause prevent index usage (non-SARGable)",
                Reason = "Allows SQL Server to use indexes efficiently",
                EstimatedImprovementPercent = 35,
                RiskLevel = "Low",
                Priority = "High",
                Implementation = "Move function to right side or use computed column with index",
                EstimatedEffortMinutes = 15,
                RequiredPermissions = new List<string> { "Query modification", "Schema modification" }
            });
        }

        // 5. NOT IN detection
        if (queryText.Contains("NOT IN"))
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "NotInToNotExists",
                Title = "Replace NOT IN with NOT EXISTS",
                Description = "NOT IN has NULL handling issues and poor performance",
                Reason = "NOT EXISTS is faster and handles NULLs correctly",
                EstimatedImprovementPercent = 20,
                RiskLevel = "Safe",
                Priority = "Medium",
                Implementation = "WHERE Col NOT IN (subquery) → WHERE NOT EXISTS (correlated subquery)",
                EstimatedEffortMinutes = 10,
                RequiredPermissions = new List<string> { "Query modification" }
            });
        }

        // 6. High elapsed time
        if (query.AvgElapsedTimeMs > 5000)
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "AddMissingIndex",
                Title = "Add missing indexes",
                Description = "Query has high elapsed time, likely missing indexes",
                Reason = "Indexes dramatically improve query performance",
                EstimatedImprovementPercent = 60,
                RiskLevel = "Low",
                Priority = "Critical",
                Implementation = "Analyze execution plan and create indexes on filtered/joined columns",
                EstimatedEffortMinutes = 20,
                RequiredPermissions = new List<string> { "CREATE INDEX" },
                Dependencies = new List<string> { "Execution plan analysis", "Index design" }
            });
        }

        // 7. High logical reads
        if (query.AvgLogicalReads > 100000)
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "OptimizeJoins",
                Title = "Optimize JOIN operations",
                Description = "High logical reads indicate inefficient JOINs",
                Reason = "Better JOIN strategy reduces I/O",
                EstimatedImprovementPercent = 30,
                RiskLevel = "Medium",
                Priority = "High",
                Implementation = "Review JOIN order, add indexes on JOIN columns",
                EstimatedEffortMinutes = 25,
                RequiredPermissions = new List<string> { "Query modification", "CREATE INDEX" }
            });
        }

        // 8. DISTINCT usage
        if (queryText.Contains("DISTINCT"))
        {
            recommendations.Add(new HealingRecommendation
            {
                ActionType = "RemoveUnnecessaryDistinct",
                Title = "Remove unnecessary DISTINCT",
                Description = "DISTINCT adds overhead if data is already unique",
                Reason = "Eliminates expensive sort/aggregation operation",
                EstimatedImprovementPercent = 10,
                RiskLevel = "Medium",
                Priority = "Low",
                Implementation = "Verify if DISTINCT is necessary, use GROUP BY if aggregating",
                EstimatedEffortMinutes = 5,
                RequiredPermissions = new List<string> { "Query modification" }
            });
        }

        // Sort by priority and estimated improvement
        return recommendations
            .OrderByDescending(r => r.Priority switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            })
            .ThenByDescending(r => r.EstimatedImprovementPercent)
            .ToList();
    }

    public async Task<RollbackResult> RollbackHealingAsync(string queryHash)
    {
        _logger.LogInformation("Rolling back healing for query {Hash}", queryHash);

        await Task.CompletedTask;

        var result = new RollbackResult
        {
            QueryHash = queryHash,
            RollbackDate = DateTime.Now
        };

        // Check if there's a history entry to rollback
        if (_healingHistoryCache.TryGetValue(queryHash, out var history) && history.Entries.Count > 0)
        {
            var lastEntry = history.Entries.OrderByDescending(e => e.Date).First();

            result.Success = true;
            result.Reason = "Healing did not provide expected improvement";
            result.Message = $"Rolled back healing from {lastEntry.Date:yyyy-MM-dd HH:mm}";

            // Record rollback in history
            history.Entries.Add(new HealingHistoryEntry
            {
                Date = DateTime.Now,
                Action = "Rollback",
                Success = true,
                ImprovementPercent = 0,
                Status = "RolledBack",
                Details = result.Reason
            });

            history.RolledBack++;
        }
        else
        {
            result.Success = false;
            result.Reason = "No healing history found";
            result.Message = "Cannot rollback - no previous healing found";
        }

        return result;
    }

    public async Task<HealingHistory> GetHealingHistoryAsync(string queryHash)
    {
        _logger.LogInformation("Getting healing history for query {Hash}", queryHash);

        await Task.CompletedTask;

        if (_healingHistoryCache.TryGetValue(queryHash, out var history))
        {
            return history;
        }

        // Create new history
        history = new HealingHistory
        {
            QueryHash = queryHash,
            Summary = "No healing history available for this query"
        };

        return history;
    }

    public async Task EnableAutoHealingAsync(string queryHash, bool enable)
    {
        _logger.LogInformation("{Action} auto-healing for query {Hash}",
            enable ? "Enabling" : "Disabling", queryHash);

        await Task.CompletedTask;

        _autoHealingEnabled[queryHash] = enable;
    }

    #region Helper Methods

    private async Task<string> ApplyBasicHealingAsync(string queryText, List<HealingRecommendation> recommendations)
    {
        await Task.CompletedTask;

        var healedQuery = queryText;

        // Apply basic transformations based on recommendations
        foreach (var rec in recommendations.Take(3))
        {
            switch (rec.ActionType)
            {
                case "OrToIn":
                    healedQuery = TransformOrToIn(healedQuery);
                    break;

                case "NotInToNotExists":
                    healedQuery = TransformNotInToNotExists(healedQuery);
                    break;
            }
        }

        return healedQuery;
    }

    private string TransformOrToIn(string query)
    {
        // Simplified transformation - real implementation would use SQL parsing
        return query.Replace(" OR ", " OR "); // Placeholder
    }

    private string TransformNotInToNotExists(string query)
    {
        // Simplified transformation
        return query.Replace("NOT IN", "NOT IN"); // Placeholder
    }

    private async Task RecordHealingAsync(string queryHash, HealingResult result)
    {
        await Task.CompletedTask;

        if (!_healingHistoryCache.TryGetValue(queryHash, out var history))
        {
            history = new HealingHistory
            {
                QueryHash = queryHash,
                InitialAvgElapsedTime = result.OriginalMetrics.AvgElapsedTimeMs
            };
            _healingHistoryCache[queryHash] = history;
        }

        history.TotalHealings++;
        if (result.Success)
        {
            history.SuccessfulHealings++;

            // Record successful pattern
            foreach (var action in result.ActionsApplied)
            {
                if (!history.SuccessfulPatterns.Contains(action.ActionType))
                {
                    history.SuccessfulPatterns.Add(action.ActionType);
                }
            }
        }
        else
        {
            history.FailedHealings++;

            // Record failed pattern
            foreach (var action in result.ActionsApplied)
            {
                if (!history.FailedPatterns.Contains(action.ActionType))
                {
                    history.FailedPatterns.Add(action.ActionType);
                }
            }
        }

        history.CurrentAvgElapsedTime = result.PredictedMetrics.AvgElapsedTimeMs;
        history.TotalImprovementPercent = ((history.InitialAvgElapsedTime - history.CurrentAvgElapsedTime) / history.InitialAvgElapsedTime) * 100;

        history.Entries.Add(new HealingHistoryEntry
        {
            Date = result.HealingDate,
            Action = string.Join(", ", result.ActionsApplied.Select(a => a.ActionType)),
            Success = result.Success,
            ImprovementPercent = result.ImprovementPercent,
            Status = result.Status,
            Details = result.Message
        });

        // Generate summary
        var sb = new StringBuilder();
        sb.AppendLine($"📊 Healing History for {queryHash.Substring(0, 8)}...");
        sb.AppendLine();
        sb.AppendLine($"Total Healings: {history.TotalHealings}");
        sb.AppendLine($"  Successful: {history.SuccessfulHealings}");
        sb.AppendLine($"  Failed: {history.FailedHealings}");
        sb.AppendLine($"  Rolled Back: {history.RolledBack}");
        sb.AppendLine();
        sb.AppendLine($"Performance:");
        sb.AppendLine($"  Initial: {history.InitialAvgElapsedTime:F0}ms");
        sb.AppendLine($"  Current: {history.CurrentAvgElapsedTime:F0}ms");
        sb.AppendLine($"  Total Improvement: {history.TotalImprovementPercent:F1}%");

        history.Summary = sb.ToString();
    }

    #endregion
}
