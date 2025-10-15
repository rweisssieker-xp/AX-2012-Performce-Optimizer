using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for generating comprehensive query documentation
/// </summary>
public class QueryDocumentationService : IQueryDocumentationService
{
    private readonly ILogger<QueryDocumentationService> _logger;
    private readonly IAiQueryOptimizerService? _aiOptimizer;

    public QueryDocumentationService(
        ILogger<QueryDocumentationService> logger,
        IAiQueryOptimizerService? aiOptimizer = null)
    {
        _logger = logger;
        _aiOptimizer = aiOptimizer;
    }

    public async Task<QueryDocumentation> GenerateDocumentationAsync(string queryText, SqlQueryMetric? metrics = null)
    {
        try
        {
            var doc = new QueryDocumentation
            {
                QueryText = queryText,
                QueryName = GenerateQueryName(queryText),
                Tables = ExtractTables(queryText),
                Columns = ExtractColumns(queryText),
                Parameters = ExtractParameters(queryText),
                Complexity = AnalyzeComplexity(queryText)
            };

            // Add performance info if metrics provided
            if (metrics != null)
            {
                doc.Performance = new QueryPerformanceInfo
                {
                    AvgExecutionTimeMs = metrics.AvgElapsedTimeMs,
                    AvgLogicalReads = metrics.AvgLogicalReads,
                    ExecutionCount = (int)metrics.ExecutionCount,
                    PerformanceRating = GetPerformanceRating(metrics),
                    PerformanceNotes = GeneratePerformanceNotes(metrics)
                };
            }

            // Use AI for detailed documentation if available
            if (_aiOptimizer?.IsAvailable == true)
            {
                await EnhanceWithAiAsync(doc, metrics);
            }
            else
            {
                // Fallback: Rule-based documentation
                doc.Purpose = GeneratePurpose(queryText);
                doc.Description = GenerateDescription(queryText);
                doc.BusinessRules = ExtractBusinessRules(queryText);
                doc.UseCases = GenerateUseCases(queryText);
            }

            return doc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation");
            return new QueryDocumentation
            {
                QueryText = queryText,
                Notes = new List<string> { $"Error: {ex.Message}" }
            };
        }
    }

    public async Task<string> ExplainQueryAsync(string queryText)
    {
        if (_aiOptimizer?.IsAvailable == true)
        {
            try
            {
                var prompt = $@"Explain this SQL query in simple, human-readable language.
Focus on:
1. What data is being retrieved
2. From which tables
3. What conditions are applied
4. What the business purpose might be

Query:
```sql
{queryText}
```

Provide a 2-3 sentence explanation suitable for a business user.";

                var explanation = await _aiOptimizer.ExplainQueryPerformanceAsync(
                    new SqlQueryMetric { QueryText = queryText });

                return explanation;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI explanation failed, using rule-based");
            }
        }

        // Fallback: Rule-based explanation
        return GenerateSimpleExplanation(queryText);
    }

    public async Task<string> AddInlineCommentsAsync(string queryText)
    {
        var lines = queryText.Split('\n');
        var commented = new StringBuilder();
        var upper = queryText.ToUpperInvariant();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var lineUpper = trimmed.ToUpperInvariant();

            // Add comments for key SQL keywords
            if (lineUpper.StartsWith("SELECT"))
            {
                commented.AppendLine("-- Retrieve the following columns:");
                commented.AppendLine(line);
            }
            else if (lineUpper.StartsWith("FROM"))
            {
                commented.AppendLine("-- From table:");
                commented.AppendLine(line);
            }
            else if (lineUpper.StartsWith("JOIN") || lineUpper.StartsWith("LEFT JOIN") || lineUpper.StartsWith("INNER JOIN"))
            {
                commented.AppendLine("-- Join with related table:");
                commented.AppendLine(line);
            }
            else if (lineUpper.StartsWith("WHERE"))
            {
                commented.AppendLine("-- Filter conditions:");
                commented.AppendLine(line);
            }
            else if (lineUpper.StartsWith("GROUP BY"))
            {
                commented.AppendLine("-- Group results by:");
                commented.AppendLine(line);
            }
            else if (lineUpper.StartsWith("ORDER BY"))
            {
                commented.AppendLine("-- Sort results by:");
                commented.AppendLine(line);
            }
            else if (lineUpper.StartsWith("HAVING"))
            {
                commented.AppendLine("-- Filter grouped results:");
                commented.AppendLine(line);
            }
            else
            {
                commented.AppendLine(line);
            }
        }

        return await Task.FromResult(commented.ToString());
    }

    public async Task<string> GenerateCatalogDocumentationAsync(List<SqlQueryMetric> queries)
    {
        var markdown = new StringBuilder();

        markdown.AppendLine("# Query Catalog Documentation");
        markdown.AppendLine();
        markdown.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        markdown.AppendLine($"Total Queries: {queries.Count}");
        markdown.AppendLine();

        // Summary Statistics
        markdown.AppendLine("## Summary Statistics");
        markdown.AppendLine();
        markdown.AppendLine($"- Total Executions: {queries.Sum(q => q.ExecutionCount):N0}");
        markdown.AppendLine($"- Avg CPU Time: {queries.Average(q => q.AvgCpuTimeMs):F2}ms");
        markdown.AppendLine($"- Total CPU Time: {queries.Sum(q => q.TotalCpuTimeMs):N0}ms");
        markdown.AppendLine();

        // Query Breakdown
        markdown.AppendLine("## Queries");
        markdown.AppendLine();

        int index = 1;
        foreach (var query in queries.OrderByDescending(q => q.TotalCpuTimeMs).Take(50))
        {
            markdown.AppendLine($"### Query {index}: {GenerateQueryName(query.QueryText)}");
            markdown.AppendLine();

            // Performance Metrics
            markdown.AppendLine("**Performance:**");
            markdown.AppendLine($"- Executions: {query.ExecutionCount:N0}");
            markdown.AppendLine($"- Avg CPU Time: {query.AvgCpuTimeMs:F2}ms");
            markdown.AppendLine($"- Avg Logical Reads: {query.AvgLogicalReads:N0}");
            markdown.AppendLine($"- Rating: {GetPerformanceRating(query)}");
            markdown.AppendLine();

            // Complexity
            var complexity = AnalyzeComplexity(query.QueryText);
            markdown.AppendLine("**Complexity:**");
            markdown.AppendLine($"- Level: {complexity.Level}");
            markdown.AppendLine($"- Score: {complexity.Score}/100");
            markdown.AppendLine($"- JOINs: {complexity.JoinCount}");
            markdown.AppendLine($"- Subqueries: {complexity.SubqueryCount}");
            markdown.AppendLine();

            // Query Text (truncated)
            markdown.AppendLine("**SQL:**");
            markdown.AppendLine("```sql");
            var queryPreview = query.QueryText.Length > 500
                ? query.QueryText.Substring(0, 500) + "..."
                : query.QueryText;
            markdown.AppendLine(queryPreview);
            markdown.AppendLine("```");
            markdown.AppendLine();

            index++;
        }

        return await Task.FromResult(markdown.ToString());
    }

    public async Task<string> GenerateMarkdownAsync(QueryDocumentation documentation)
    {
        var md = new StringBuilder();

        md.AppendLine($"# {documentation.QueryName}");
        md.AppendLine();

        // Purpose
        if (!string.IsNullOrEmpty(documentation.Purpose))
        {
            md.AppendLine("## Purpose");
            md.AppendLine(documentation.Purpose);
            md.AppendLine();
        }

        // Description
        if (!string.IsNullOrEmpty(documentation.Description))
        {
            md.AppendLine("## Description");
            md.AppendLine(documentation.Description);
            md.AppendLine();
        }

        // Complexity
        md.AppendLine("## Complexity");
        md.AppendLine($"- **Level:** {documentation.Complexity.Level}");
        md.AppendLine($"- **Score:** {documentation.Complexity.Score}/100");
        md.AppendLine($"- **JOINs:** {documentation.Complexity.JoinCount}");
        md.AppendLine($"- **Subqueries:** {documentation.Complexity.SubqueryCount}");
        if (documentation.Complexity.ComplexityFactors.Any())
        {
            md.AppendLine("- **Factors:**");
            foreach (var factor in documentation.Complexity.ComplexityFactors)
            {
                md.AppendLine($"  - {factor}");
            }
        }
        md.AppendLine();

        // Performance
        if (documentation.Performance != null)
        {
            md.AppendLine("## Performance");
            md.AppendLine($"- **Rating:** {documentation.Performance.PerformanceRating}");
            md.AppendLine($"- **Avg Execution Time:** {documentation.Performance.AvgExecutionTimeMs:F2}ms");
            md.AppendLine($"- **Avg Logical Reads:** {documentation.Performance.AvgLogicalReads:N0}");
            md.AppendLine($"- **Execution Count:** {documentation.Performance.ExecutionCount:N0}");
            if (documentation.Performance.PerformanceNotes.Any())
            {
                md.AppendLine("- **Notes:**");
                foreach (var note in documentation.Performance.PerformanceNotes)
                {
                    md.AppendLine($"  - {note}");
                }
            }
            md.AppendLine();
        }

        // Tables
        if (documentation.Tables.Any())
        {
            md.AppendLine("## Tables");
            foreach (var table in documentation.Tables)
            {
                md.AppendLine($"- `{table}`");
            }
            md.AppendLine();
        }

        // Parameters
        if (documentation.Parameters.Any())
        {
            md.AppendLine("## Parameters");
            foreach (var param in documentation.Parameters)
            {
                var optional = param.IsOptional ? " (optional)" : "";
                var defaultVal = param.DefaultValue != null ? $" = {param.DefaultValue}" : "";
                md.AppendLine($"- **{param.Name}** `{param.DataType}`{optional}{defaultVal}");
                if (!string.IsNullOrEmpty(param.Description))
                {
                    md.AppendLine($"  - {param.Description}");
                }
            }
            md.AppendLine();
        }

        // Business Rules
        if (documentation.BusinessRules.Any())
        {
            md.AppendLine("## Business Rules");
            foreach (var rule in documentation.BusinessRules)
            {
                md.AppendLine($"- {rule}");
            }
            md.AppendLine();
        }

        // Use Cases
        if (documentation.UseCases.Any())
        {
            md.AppendLine("## Use Cases");
            foreach (var useCase in documentation.UseCases)
            {
                md.AppendLine($"- {useCase}");
            }
            md.AppendLine();
        }

        // SQL Query
        md.AppendLine("## SQL Query");
        md.AppendLine("```sql");
        md.AppendLine(documentation.QueryText);
        md.AppendLine("```");
        md.AppendLine();

        // Metadata
        md.AppendLine("---");
        md.AppendLine($"*Generated: {documentation.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC*");
        if (!string.IsNullOrEmpty(documentation.Author))
        {
            md.AppendLine($"*Author: {documentation.Author}*");
        }

        return await Task.FromResult(md.ToString());
    }

    public async Task<string> GenerateHtmlAsync(QueryDocumentation documentation)
    {
        var markdown = await GenerateMarkdownAsync(documentation);

        // Simple markdown to HTML conversion
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine($"<title>{documentation.QueryName}</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; max-width: 1200px; margin: 0 auto; padding: 20px; }");
        html.AppendLine("h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }");
        html.AppendLine("h2 { color: #34495e; margin-top: 30px; }");
        html.AppendLine("pre { background: #f4f4f4; padding: 15px; border-radius: 5px; overflow-x: auto; }");
        html.AppendLine("code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; }");
        html.AppendLine("ul { line-height: 1.8; }");
        html.AppendLine(".badge { display: inline-block; padding: 3px 8px; border-radius: 3px; font-size: 12px; font-weight: bold; }");
        html.AppendLine(".badge-excellent { background: #27ae60; color: white; }");
        html.AppendLine(".badge-good { background: #3498db; color: white; }");
        html.AppendLine(".badge-fair { background: #f39c12; color: white; }");
        html.AppendLine(".badge-poor { background: #e74c3c; color: white; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Convert markdown to HTML (simplified)
        var lines = markdown.Split('\n');
        bool inCodeBlock = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("```"))
            {
                if (inCodeBlock)
                {
                    html.AppendLine("</pre>");
                    inCodeBlock = false;
                }
                else
                {
                    html.AppendLine("<pre><code>");
                    inCodeBlock = true;
                }
            }
            else if (inCodeBlock)
            {
                html.AppendLine(System.Net.WebUtility.HtmlEncode(line));
            }
            else if (line.StartsWith("# "))
            {
                html.AppendLine($"<h1>{line.Substring(2)}</h1>");
            }
            else if (line.StartsWith("## "))
            {
                html.AppendLine($"<h2>{line.Substring(3)}</h2>");
            }
            else if (line.StartsWith("- "))
            {
                html.AppendLine($"<li>{line.Substring(2)}</li>");
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                html.AppendLine($"<p>{line}</p>");
            }
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return await Task.FromResult(html.ToString());
    }

    #region Private Helper Methods

    private string GenerateQueryName(string queryText)
    {
        // Extract main table from FROM clause
        var fromMatch = Regex.Match(queryText, @"FROM\s+(\w+)", RegexOptions.IgnoreCase);
        if (fromMatch.Success)
        {
            var tableName = fromMatch.Groups[1].Value;

            // Check for operation type
            var upper = queryText.ToUpperInvariant();
            if (upper.Contains("INSERT"))
                return $"Insert into {tableName}";
            if (upper.Contains("UPDATE"))
                return $"Update {tableName}";
            if (upper.Contains("DELETE"))
                return $"Delete from {tableName}";

            return $"Query {tableName}";
        }

        return "Untitled Query";
    }

    private List<string> ExtractTables(string queryText)
    {
        var tables = new HashSet<string>();

        // FROM clause
        var fromMatches = Regex.Matches(queryText, @"FROM\s+(\w+)", RegexOptions.IgnoreCase);
        foreach (Match match in fromMatches)
        {
            tables.Add(match.Groups[1].Value);
        }

        // JOIN clauses
        var joinMatches = Regex.Matches(queryText, @"JOIN\s+(\w+)", RegexOptions.IgnoreCase);
        foreach (Match match in joinMatches)
        {
            tables.Add(match.Groups[1].Value);
        }

        return tables.ToList();
    }

    private List<string> ExtractColumns(string queryText)
    {
        var columns = new List<string>();

        // Simple extraction from SELECT clause
        var selectMatch = Regex.Match(queryText, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (selectMatch.Success)
        {
            var selectClause = selectMatch.Groups[1].Value;
            if (!selectClause.Contains("*"))
            {
                var parts = selectClause.Split(',');
                foreach (var part in parts)
                {
                    var cleaned = part.Trim().Split(' ').First();
                    columns.Add(cleaned);
                }
            }
        }

        return columns;
    }

    private List<QueryParameter> ExtractParameters(string queryText)
    {
        var parameters = new List<QueryParameter>();

        // Extract @parameter or :parameter patterns
        var matches = Regex.Matches(queryText, @"[@:](\w+)", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            var paramName = match.Groups[1].Value;
            if (!parameters.Any(p => p.Name == paramName))
            {
                parameters.Add(new QueryParameter
                {
                    Name = paramName,
                    DataType = "Unknown",
                    Description = $"Parameter {paramName}"
                });
            }
        }

        return parameters;
    }

    private QueryComplexityInfo AnalyzeComplexity(string queryText)
    {
        var upper = queryText.ToUpperInvariant();
        int score = 10; // Base score

        var joinCount = Regex.Matches(upper, @"\bJOIN\b").Count;
        var subqueryCount = Regex.Matches(upper, @"\(\s*SELECT\b").Count;
        var aggregationCount = Regex.Matches(upper, @"\b(SUM|COUNT|AVG|MIN|MAX)\s*\(").Count;
        var hasCte = upper.Contains("WITH") && upper.Contains("AS (");
        var hasWindowFunctions = Regex.IsMatch(upper, @"\b(ROW_NUMBER|RANK|DENSE_RANK|PARTITION BY)\b");

        score += joinCount * 8;
        score += subqueryCount * 12;
        score += aggregationCount * 4;
        if (hasCte) score += 15;
        if (hasWindowFunctions) score += 20;

        score = Math.Clamp(score, 0, 100);

        var level = score switch
        {
            <= 30 => "Simple",
            <= 60 => "Medium",
            <= 85 => "Complex",
            _ => "Very Complex"
        };

        var factors = new List<string>();
        if (joinCount > 0) factors.Add($"{joinCount} JOIN(s)");
        if (subqueryCount > 0) factors.Add($"{subqueryCount} Subquer{(subqueryCount == 1 ? "y" : "ies")}");
        if (aggregationCount > 0) factors.Add($"{aggregationCount} Aggregation(s)");
        if (hasCte) factors.Add("Uses CTE");
        if (hasWindowFunctions) factors.Add("Uses Window Functions");

        return new QueryComplexityInfo
        {
            Score = score,
            Level = level,
            JoinCount = joinCount,
            SubqueryCount = subqueryCount,
            AggregationCount = aggregationCount,
            HasCte = hasCte,
            HasWindowFunctions = hasWindowFunctions,
            ComplexityFactors = factors
        };
    }

    private string GetPerformanceRating(SqlQueryMetric metrics)
    {
        // Rating based on CPU time and logical reads
        if (metrics.AvgCpuTimeMs < 10 && metrics.AvgLogicalReads < 1000)
            return "Excellent";
        if (metrics.AvgCpuTimeMs < 50 && metrics.AvgLogicalReads < 10000)
            return "Good";
        if (metrics.AvgCpuTimeMs < 200 && metrics.AvgLogicalReads < 50000)
            return "Fair";
        if (metrics.AvgCpuTimeMs < 1000)
            return "Poor";

        return "Critical";
    }

    private List<string> GeneratePerformanceNotes(SqlQueryMetric metrics)
    {
        var notes = new List<string>();

        if (metrics.AvgCpuTimeMs > 100)
            notes.Add($"High CPU usage: {metrics.AvgCpuTimeMs:F2}ms average");

        if (metrics.AvgLogicalReads > 10000)
            notes.Add($"High logical reads: {metrics.AvgLogicalReads:N0} - consider adding indexes");

        if (metrics.AvgPhysicalReads > 1000)
            notes.Add($"High physical reads: {metrics.AvgPhysicalReads:N0} - data not in cache");

        if (metrics.ExecutionCount > 1000 && metrics.AvgCpuTimeMs > 50)
            notes.Add($"Frequently executed ({metrics.ExecutionCount:N0}x) - high optimization priority");

        return notes;
    }

    private async Task EnhanceWithAiAsync(QueryDocumentation doc, SqlQueryMetric? metrics)
    {
        if (_aiOptimizer?.IsAvailable != true) return;

        try
        {
            var prompt = $@"Analyze this SQL query and provide documentation:

Query:
```sql
{doc.QueryText}
```

Provide:
1. Purpose (one sentence)
2. Description (2-3 sentences)
3. Business rules (bullet points)
4. Use cases (bullet points)

Format as JSON.";

            // This would need a specialized AI endpoint, for now use explanation
            var explanation = await _aiOptimizer.ExplainQueryPerformanceAsync(
                new SqlQueryMetric { QueryText = doc.QueryText });

            doc.Description = explanation;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI enhancement failed");
        }
    }

    private string GeneratePurpose(string queryText)
    {
        var tables = ExtractTables(queryText);
        if (tables.Any())
        {
            return $"Retrieve data from {string.Join(", ", tables)}";
        }

        return "SQL data retrieval query";
    }

    private string GenerateDescription(string queryText)
    {
        var complexity = AnalyzeComplexity(queryText);
        var tables = ExtractTables(queryText);

        var desc = $"This is a {complexity.Level.ToLower()} query";

        if (tables.Any())
        {
            desc += $" that accesses {tables.Count} table{(tables.Count == 1 ? "" : "s")}";
        }

        if (complexity.JoinCount > 0)
        {
            desc += $" with {complexity.JoinCount} JOIN{(complexity.JoinCount == 1 ? "" : "s")}";
        }

        desc += ".";

        return desc;
    }

    private List<string> ExtractBusinessRules(string queryText)
    {
        var rules = new List<string>();

        // Look for WHERE conditions that might represent business rules
        var upper = queryText.ToUpperInvariant();

        if (upper.Contains("DATAAREAID"))
            rules.Add("Company-specific data filtering (DATAAREAID)");

        if (Regex.IsMatch(upper, @"STATUS\s*=\s*['\""]?ACTIVE"))
            rules.Add("Only active records are included");

        if (upper.Contains("DELETED") && (upper.Contains("= 0") || upper.Contains("= '0'")))
            rules.Add("Excludes deleted records");

        return rules;
    }

    private List<string> GenerateUseCases(string queryText)
    {
        var useCases = new List<string>();
        var tables = ExtractTables(queryText);

        if (tables.Any(t => t.ToUpper().Contains("CUST")))
            useCases.Add("Customer data retrieval and reporting");

        if (tables.Any(t => t.ToUpper().Contains("VEND")))
            useCases.Add("Vendor/Supplier data management");

        if (tables.Any(t => t.ToUpper().Contains("INVENT")))
            useCases.Add("Inventory tracking and management");

        if (tables.Any(t => t.ToUpper().Contains("SALES")))
            useCases.Add("Sales order processing");

        if (useCases.Count == 0)
            useCases.Add("General data retrieval");

        return useCases;
    }

    private string GenerateSimpleExplanation(string queryText)
    {
        var tables = ExtractTables(queryText);
        var complexity = AnalyzeComplexity(queryText);

        var sb = new StringBuilder();
        sb.Append("This query retrieves data");

        if (tables.Any())
        {
            sb.Append($" from {string.Join(", ", tables)}");
        }

        if (complexity.JoinCount > 0)
        {
            sb.Append($", combining data from {complexity.JoinCount + 1} tables");
        }

        var upper = queryText.ToUpperInvariant();
        if (upper.Contains("WHERE"))
        {
            sb.Append(" with specific filtering conditions");
        }

        if (complexity.AggregationCount > 0)
        {
            sb.Append(" and calculates aggregated values (SUM, COUNT, etc.)");
        }

        sb.Append(".");

        return sb.ToString();
    }

    #endregion
}
