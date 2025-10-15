using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Automatic query fixing service with rule-based and AI-powered fixes
/// </summary>
public class QueryAutoFixerService : IQueryAutoFixerService
{
    private readonly ILogger<QueryAutoFixerService> _logger;
    private readonly IAiQueryOptimizerService? _aiOptimizer;

    public QueryAutoFixerService(
        ILogger<QueryAutoFixerService> logger,
        IAiQueryOptimizerService? aiOptimizer = null)
    {
        _logger = logger;
        _aiOptimizer = aiOptimizer;
    }

    public async Task<QueryFixResult> AutoFixQueryAsync(string queryText, QueryFixOptions? options = null)
    {
        options ??= new QueryFixOptions();

        try
        {
            // 1. Preview all possible fixes
            var availableFixes = await PreviewFixesAsync(queryText);

            // 2. Filter by confidence threshold
            var applicableFixes = availableFixes
                .Where(f => f.Confidence >= options.MinConfidence)
                .OrderByDescending(f => f.EstimatedImpact)
                .ToList();

            if (options.PreviewOnly)
            {
                return new QueryFixResult
                {
                    Success = true,
                    OriginalQuery = queryText,
                    FixedQuery = queryText,
                    AppliedFixes = applicableFixes,
                    OverallConfidence = applicableFixes.Any() ? applicableFixes.Average(f => f.Confidence) : 0
                };
            }

            // 3. Apply fixes
            var fixedQuery = queryText;
            var appliedFixes = new List<QueryFix>();

            foreach (var fix in applicableFixes)
            {
                var result = await ApplyFixAsync(fixedQuery, fix);
                if (result.Success)
                {
                    fixedQuery = result.FixedQuery;
                    appliedFixes.Add(fix);
                }
            }

            // 4. Validate result
            var validation = await ValidateFixAsync(queryText, fixedQuery);

            // 5. Calculate overall improvement
            var estimatedImprovement = appliedFixes.Any()
                ? (int)appliedFixes.Average(f => f.EstimatedImpact)
                : 0;

            return new QueryFixResult
            {
                Success = true,
                OriginalQuery = queryText,
                FixedQuery = fixedQuery,
                AppliedFixes = appliedFixes,
                ValidationResult = validation,
                OverallConfidence = appliedFixes.Any() ? appliedFixes.Average(f => f.Confidence) : 0,
                EstimatedPerformanceImprovement = estimatedImprovement
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-fixing query");
            return new QueryFixResult
            {
                Success = false,
                OriginalQuery = queryText,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<QueryFix>> PreviewFixesAsync(string queryText)
    {
        var fixes = new List<QueryFix>();
        var upper = queryText.ToUpperInvariant();

        // Fix 1: SELECT * replacement
        if (Regex.IsMatch(upper, @"SELECT\s+\*\s+FROM"))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.SelectStarReplacement,
                Title = "Replace SELECT * with specific columns",
                Description = "SELECT * retrieves all columns including unused ones, increasing I/O and network traffic",
                BeforeSnippet = ExtractSnippet(queryText, "SELECT *"),
                AfterSnippet = "SELECT [Column1], [Column2], [Column3] -- specify needed columns",
                Confidence = 0.6, // Lower confidence as we need to know which columns are needed
                EstimatedImpact = 35,
                Safety = QueryFixSafety.MediumRisk,
                RequiresValidation = true
            });
        }

        // Fix 2: OR to IN conversion
        var orPattern = @"WHERE\s+(\w+)\s*=\s*[^=]+\s+OR\s+\1\s*=";
        if (Regex.IsMatch(upper, orPattern, RegexOptions.IgnoreCase))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.OrToIn,
                Title = "Convert OR conditions to IN clause",
                Description = "OR conditions can prevent index usage. IN clause is more efficient and readable",
                BeforeSnippet = ExtractSnippet(queryText, "OR"),
                AfterSnippet = "WHERE Column IN (value1, value2, value3)",
                Confidence = 0.85,
                EstimatedImpact = 45,
                Safety = QueryFixSafety.Safe,
                RequiresValidation = true
            });
        }

        // Fix 3: Function in WHERE clause
        if (Regex.IsMatch(upper, @"WHERE.*\w+\s*\([^\)]*\w+\s*\)\s*[=<>]"))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.FunctionInWhereClause,
                Title = "Remove function from WHERE clause (SARGable)",
                Description = "Functions on columns in WHERE prevent index usage. Make query SARGable",
                BeforeSnippet = ExtractSnippet(queryText, "WHERE"),
                AfterSnippet = "-- Example: WHERE Date >= '2024-01-01' AND Date < '2025-01-01' instead of YEAR(Date) = 2024",
                Confidence = 0.75,
                EstimatedImpact = 65,
                Safety = QueryFixSafety.LowRisk,
                RequiresValidation = true
            });
        }

        // Fix 4: NOT IN to NOT EXISTS
        if (upper.Contains("NOT IN"))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.NotInToNotExists,
                Title = "Replace NOT IN with NOT EXISTS",
                Description = "NOT IN can be slow with large datasets and doesn't handle NULLs correctly",
                BeforeSnippet = ExtractSnippet(queryText, "NOT IN"),
                AfterSnippet = "WHERE NOT EXISTS (SELECT 1 FROM ... WHERE ...)",
                Confidence = 0.90,
                EstimatedImpact = 50,
                Safety = QueryFixSafety.Safe,
                RequiresValidation = true
            });
        }

        // Fix 5: Leading wildcard in LIKE
        if (Regex.IsMatch(upper, @"LIKE\s+['\""]%"))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.LeadingWildcardRemoval,
                Title = "Remove leading wildcard in LIKE (if possible)",
                Description = "LIKE with leading wildcard prevents index usage, forcing table scan",
                BeforeSnippet = ExtractSnippet(queryText, "LIKE"),
                AfterSnippet = "LIKE 'value%' -- or use full-text search",
                Confidence = 0.65,
                EstimatedImpact = 55,
                Safety = QueryFixSafety.MediumRisk,
                RequiresValidation = true
            });
        }

        // Fix 6: DISTINCT optimization
        if (upper.Contains("DISTINCT"))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.DistinctOptimization,
                Title = "Review DISTINCT usage",
                Description = "DISTINCT requires sorting/grouping. May indicate data model issues or JOIN problems",
                BeforeSnippet = ExtractSnippet(queryText, "DISTINCT"),
                AfterSnippet = "-- Consider using GROUP BY or fixing underlying data duplication",
                Confidence = 0.70,
                EstimatedImpact = 40,
                Safety = QueryFixSafety.MediumRisk,
                RequiresValidation = true
            });
        }

        // Fix 7: Implicit conversions
        if (Regex.IsMatch(upper, @"WHERE\s+\w+\s*=\s*N'") || Regex.IsMatch(upper, @"=\s*CAST\s*\("))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.ImplicitConversionFix,
                Title = "Fix implicit data type conversions",
                Description = "Data type mismatch causes implicit conversions, preventing index usage",
                BeforeSnippet = ExtractSnippet(queryText, "WHERE"),
                AfterSnippet = "-- Ensure parameter types match column types",
                Confidence = 0.80,
                EstimatedImpact = 60,
                Safety = QueryFixSafety.LowRisk,
                RequiresValidation = true
            });
        }

        // Fix 8: Subquery in SELECT (correlated)
        if (Regex.IsMatch(upper, @"SELECT[^,]+\(\s*SELECT\s"))
        {
            fixes.Add(new QueryFix
            {
                FixType = QueryFixType.SubqueryOptimization,
                Title = "Optimize correlated subquery",
                Description = "Correlated subqueries in SELECT can execute for each row. Consider JOINs or window functions",
                BeforeSnippet = ExtractSnippet(queryText, "SELECT"),
                AfterSnippet = "-- Use LEFT JOIN or APPLY instead",
                Confidence = 0.75,
                EstimatedImpact = 70,
                Safety = QueryFixSafety.MediumRisk,
                RequiresValidation = true
            });
        }

        return await Task.FromResult(fixes);
    }

    public async Task<QueryFixResult> ApplyFixAsync(string queryText, QueryFix fix)
    {
        try
        {
            var fixedQuery = queryText;

            switch (fix.FixType)
            {
                case QueryFixType.OrToIn:
                    fixedQuery = ApplyOrToInFix(queryText);
                    break;

                case QueryFixType.NotInToNotExists:
                    fixedQuery = ApplyNotInToNotExistsFix(queryText);
                    break;

                case QueryFixType.FunctionInWhereClause:
                    fixedQuery = await ApplyFunctionInWhereFix(queryText);
                    break;

                case QueryFixType.SelectStarReplacement:
                    // This requires table schema knowledge, skip for now
                    fixedQuery = queryText;
                    break;

                default:
                    // For complex fixes, use AI if available
                    if (_aiOptimizer?.IsAvailable == true)
                    {
                        fixedQuery = await _aiOptimizer.GenerateOptimizedQueryAsync(queryText);
                    }
                    break;
            }

            return new QueryFixResult
            {
                Success = fixedQuery != queryText,
                OriginalQuery = queryText,
                FixedQuery = fixedQuery,
                AppliedFixes = new List<QueryFix> { fix }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying fix {FixType}", fix.FixType);
            return new QueryFixResult
            {
                Success = false,
                OriginalQuery = queryText,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<QueryValidationResult> ValidateFixAsync(string originalQuery, string fixedQuery)
    {
        var result = new QueryValidationResult
        {
            IsValid = true,
            SemanticallyEquivalent = true,
            ValidationMethod = "Rule-based"
        };

        // Basic validation checks
        if (string.IsNullOrWhiteSpace(fixedQuery))
        {
            result.IsValid = false;
            result.Errors.Add("Fixed query is empty");
            return result;
        }

        // Check for basic SQL syntax
        var upper = fixedQuery.ToUpperInvariant();
        if (!upper.Contains("SELECT"))
        {
            result.IsValid = false;
            result.Errors.Add("Fixed query does not contain SELECT statement");
            return result;
        }

        // Check for balanced parentheses
        var openCount = fixedQuery.Count(c => c == '(');
        var closeCount = fixedQuery.Count(c => c == ')');
        if (openCount != closeCount)
        {
            result.IsValid = false;
            result.Errors.Add($"Unbalanced parentheses: {openCount} open, {closeCount} close");
        }

        // Check for common errors
        if (upper.Contains("WHERE WHERE"))
            result.Warnings.Add("Duplicate WHERE clause detected");

        if (upper.Contains("SELECT SELECT"))
            result.Warnings.Add("Duplicate SELECT clause detected");

        // Use AI for semantic validation if available
        if (_aiOptimizer?.IsAvailable == true)
        {
            try
            {
                var comparison = await _aiOptimizer.CompareQueriesAsync(originalQuery, fixedQuery);
                result.ValidationMethod = "AI-powered";

                if (comparison.KeyDifferences.Any(d => d.ToLower().Contains("semantic")))
                {
                    result.SemanticallyEquivalent = false;
                    result.Warnings.Add("AI detected potential semantic differences");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI validation failed, using rule-based only");
            }
        }

        return result;
    }

    #region Private Fix Methods

    private string ApplyOrToInFix(string queryText)
    {
        // Convert "WHERE col = val1 OR col = val2 OR col = val3" to "WHERE col IN (val1, val2, val3)"
        var pattern = @"WHERE\s+(\w+)\s*=\s*([^\s]+)(\s+OR\s+\1\s*=\s*([^\s]+))+";

        var match = Regex.Match(queryText, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var column = match.Groups[1].Value;
            var values = new List<string>();

            // Extract all values
            var valuePattern = $@"{column}\s*=\s*([^\s]+)";
            foreach (Match valueMatch in Regex.Matches(match.Value, valuePattern, RegexOptions.IgnoreCase))
            {
                values.Add(valueMatch.Groups[1].Value);
            }

            if (values.Count > 1)
            {
                var inClause = $"WHERE {column} IN ({string.Join(", ", values.Distinct())})";
                return queryText.Replace(match.Value, inClause);
            }
        }

        return queryText;
    }

    private string ApplyNotInToNotExistsFix(string queryText)
    {
        // Convert "WHERE col NOT IN (SELECT ...)" to "WHERE NOT EXISTS (SELECT 1 FROM ... WHERE ...)"
        var pattern = @"WHERE\s+(\w+)\s+NOT\s+IN\s*\(\s*SELECT\s+([^\)]+)\)";

        var match = Regex.Match(queryText, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var column = match.Groups[1].Value;
            var subquery = match.Groups[2].Value;

            // Simple transformation (may need refinement for complex cases)
            var notExists = $"WHERE NOT EXISTS (SELECT 1 FROM ({match.Groups[2].Value}) sub WHERE sub.{column} = outer.{column})";

            // Note: This is a simplified transformation
            // In production, you'd need more sophisticated parsing
            return queryText; // Return original for now, as this needs table aliases
        }

        return queryText;
    }

    private async Task<string> ApplyFunctionInWhereFix(string queryText)
    {
        // This is complex and depends on the function used
        // Common patterns:
        // - YEAR(Date) = 2024 → Date >= '2024-01-01' AND Date < '2025-01-01'
        // - UPPER(Name) = 'VALUE' → Name = 'VALUE' (if collation is case-insensitive)
        // - LEFT(Code, 3) = 'ABC' → Code LIKE 'ABC%'

        var upper = queryText.ToUpperInvariant();

        // Pattern: YEAR(column) = value
        var yearPattern = @"YEAR\s*\(\s*(\w+)\s*\)\s*=\s*(\d{4})";
        var match = Regex.Match(queryText, yearPattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var column = match.Groups[1].Value;
            var year = match.Groups[2].Value;
            var nextYear = int.Parse(year) + 1;

            var replacement = $"{column} >= '{year}-01-01' AND {column} < '{nextYear}-01-01'";
            return queryText.Replace(match.Value, replacement);
        }

        // Pattern: LEFT(column, n) = 'value'
        var leftPattern = @"LEFT\s*\(\s*(\w+)\s*,\s*\d+\s*\)\s*=\s*'([^']+)'";
        match = Regex.Match(queryText, leftPattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var column = match.Groups[1].Value;
            var value = match.Groups[2].Value;

            var replacement = $"{column} LIKE '{value}%'";
            return queryText.Replace(match.Value, replacement);
        }

        return await Task.FromResult(queryText);
    }

    private string ExtractSnippet(string queryText, string keyword, int contextChars = 50)
    {
        var index = queryText.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return keyword;

        var start = Math.Max(0, index - contextChars);
        var length = Math.Min(queryText.Length - start, contextChars * 2 + keyword.Length);

        var snippet = queryText.Substring(start, length);
        if (start > 0) snippet = "..." + snippet;
        if (start + length < queryText.Length) snippet += "...";

        return snippet;
    }

    #endregion
}
