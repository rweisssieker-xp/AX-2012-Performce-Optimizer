using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text;
using System.IO;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class SqlPerformanceViewModel : ObservableObject
{
    private readonly ISqlQueryMonitorService _sqlMonitor;
    private readonly IQueryAnalyzerService _queryAnalyzer;
    private readonly IAiQueryOptimizerService _aiOptimizer;
    private readonly IQueryAutoFixerService? _autoFixer;
    private readonly IQueryDocumentationService? _docGenerator;
    private readonly IPerformanceCostCalculatorService? _costCalculator;
    private readonly IQueryPerformanceForecastingService? _forecasting;
    private readonly ISelfHealingQueryService? _selfHealing;
    private readonly IQueryCorrelationEngine? _correlationEngine;
    private readonly IQueryClusteringService? _clusteringService;
    private readonly IIntelligentQueryRewriter? _queryRewriter;
    private List<SqlQueryMetric> _allQueries = new();

    [ObservableProperty]
    private ObservableCollection<SqlQueryMetric> expensiveQueries = new();

    [ObservableProperty]
    private SqlQueryMetric? selectedQuery;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string selectedSortBy = "CPU Time";

    [ObservableProperty]
    private string selectedTimeRange = "All";

    [ObservableProperty]
    private double minCpuTimeMs = 0;

    [ObservableProperty]
    private bool isDetailsPanelVisible;

    // Statistics
    [ObservableProperty]
    private double totalCpuTimeMs;

    [ObservableProperty]
    private int problematicQueriesCount;

    [ObservableProperty]
    private double avgExecutionTimeMs;

    [ObservableProperty]
    private string? topBottleneck;

    // Recommendations
    [ObservableProperty]
    private ObservableCollection<string> recommendations = new();

    // Query-specific optimization suggestions
    [ObservableProperty]
    private ObservableCollection<QueryOptimizationSuggestion> optimizationSuggestions = new();

    // AI Analysis
    [ObservableProperty]
    private AiQueryAnalysisResult? aiAnalysisResult;

    [ObservableProperty]
    private bool hasAiAnalysis;

    [ObservableProperty]
    private bool isAiEnabled;

    [ObservableProperty]
    private bool isAnalyzingWithAi;

    public ObservableCollection<string> SortOptions { get; } = new()
    {
        "CPU Time", "Executions", "Logical Reads", "Elapsed Time", "Last Execution"
    };

    public ObservableCollection<string> TimeRangeOptions { get; } = new()
    {
        "All", "Last Hour", "Last 6 Hours", "Last 24 Hours"
    };

    public SqlPerformanceViewModel(
        ISqlQueryMonitorService sqlMonitor,
        IQueryAnalyzerService queryAnalyzer,
        IAiQueryOptimizerService aiOptimizer,
        IQueryAutoFixerService? autoFixer = null,
        IQueryDocumentationService? docGenerator = null,
        IPerformanceCostCalculatorService? costCalculator = null,
        IQueryPerformanceForecastingService? forecasting = null,
        ISelfHealingQueryService? selfHealing = null,
        IQueryCorrelationEngine? correlationEngine = null,
        IQueryClusteringService? clusteringService = null,
        IIntelligentQueryRewriter? queryRewriter = null)
    {
        _sqlMonitor = sqlMonitor;
        _queryAnalyzer = queryAnalyzer;
        _aiOptimizer = aiOptimizer;
        _autoFixer = autoFixer;
        _docGenerator = docGenerator;
        _costCalculator = costCalculator;
        _forecasting = forecasting;
        _selfHealing = selfHealing;
        _correlationEngine = correlationEngine;
        _clusteringService = clusteringService;
        _queryRewriter = queryRewriter;
        IsAiEnabled = _aiOptimizer.IsAvailable;
    }

    [RelayCommand]
    private async Task LoadQueriesAsync()
    {
        IsLoading = true;

        try
        {
            var queries = await _sqlMonitor.GetTopExpensiveQueriesAsync(100);
            _allQueries = queries;
            ApplyFiltersAndSort();
            CalculateStatistics();
        }
        catch
        {
            // Handle error gracefully
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadQueriesAsync();
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        ApplyFiltersAndSort();
        CalculateStatistics();
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allQueries.AsEnumerable();

        // Time range filter
        if (SelectedTimeRange != "All")
        {
            var cutoff = SelectedTimeRange switch
            {
                "Last Hour" => DateTime.UtcNow.AddHours(-1),
                "Last 6 Hours" => DateTime.UtcNow.AddHours(-6),
                "Last 24 Hours" => DateTime.UtcNow.AddHours(-24),
                _ => DateTime.MinValue
            };
            filtered = filtered.Where(q => q.LastExecutionTime >= cutoff);
        }

        // CPU threshold filter
        if (MinCpuTimeMs > 0)
        {
            filtered = filtered.Where(q => q.AvgCpuTimeMs >= MinCpuTimeMs);
        }

        // Sort
        filtered = SelectedSortBy switch
        {
            "CPU Time" => filtered.OrderByDescending(q => q.TotalCpuTimeMs),
            "Executions" => filtered.OrderByDescending(q => q.ExecutionCount),
            "Logical Reads" => filtered.OrderByDescending(q => q.TotalLogicalReads),
            "Elapsed Time" => filtered.OrderByDescending(q => q.TotalElapsedTimeMs),
            "Last Execution" => filtered.OrderByDescending(q => q.LastExecutionTime),
            _ => filtered.OrderByDescending(q => q.TotalCpuTimeMs)
        };

        ExpensiveQueries.Clear();
        foreach (var query in filtered.Take(50))
        {
            ExpensiveQueries.Add(query);
        }
    }

    private void CalculateStatistics()
    {
        if (!_allQueries.Any())
        {
            TotalCpuTimeMs = 0;
            ProblematicQueriesCount = 0;
            AvgExecutionTimeMs = 0;
            TopBottleneck = "No data";
            return;
        }

        TotalCpuTimeMs = _allQueries.Sum(q => q.TotalCpuTimeMs);
        ProblematicQueriesCount = _allQueries.Count(q => q.AvgCpuTimeMs > 100 || q.AvgLogicalReads > 10000);
        AvgExecutionTimeMs = _allQueries.Average(q => q.AvgElapsedTimeMs);

        var topQuery = _allQueries.OrderByDescending(q => q.TotalCpuTimeMs).FirstOrDefault();
        TopBottleneck = topQuery != null
            ? $"{topQuery.QueryText.Substring(0, Math.Min(50, topQuery.QueryText.Length))}..."
            : "None";

        GenerateRecommendations();
    }

    private void GenerateRecommendations()
    {
        Recommendations.Clear();

        foreach (var query in _allQueries.Take(10))
        {
            // High CPU usage
            if (query.AvgCpuTimeMs > 100)
            {
                Recommendations.Add($"⚠️ Query has high CPU time ({query.AvgCpuTimeMs:F2}ms avg) - Consider optimization");
            }

            // High logical reads
            if (query.AvgLogicalReads > 10000)
            {
                Recommendations.Add($"📖 Query has high logical reads ({query.AvgLogicalReads:N0}) - Index might be missing");
            }

            // High execution count with slow query
            if (query.ExecutionCount > 1000 && query.AvgCpuTimeMs > 50)
            {
                Recommendations.Add($"🔄 Frequently executed slow query ({query.ExecutionCount} times) - High impact optimization target");
            }

            // Physical reads warning
            if (query.AvgPhysicalReads > 1000)
            {
                Recommendations.Add($"💿 High physical reads ({query.AvgPhysicalReads:N0}) - Data not in cache");
            }
        }

        if (Recommendations.Count == 0)
        {
            Recommendations.Add("✅ No critical performance issues detected");
        }
    }

    [RelayCommand]
    private async Task SelectQuery(SqlQueryMetric? query)
    {
        SelectedQuery = query;
        IsDetailsPanelVisible = query != null;

        if (query != null)
        {
            // Analyze query and get optimization suggestions
            try
            {
                var suggestions = await _queryAnalyzer.AnalyzeQueryAsync(query);
                OptimizationSuggestions.Clear();
                foreach (var suggestion in suggestions.OrderByDescending(s => s.EstimatedImpact))
                {
                    OptimizationSuggestions.Add(suggestion);
                }
            }
            catch
            {
                // Handle error gracefully
            }
        }
    }

    [RelayCommand]
    private void CopyQuery()
    {
        if (SelectedQuery != null)
        {
            Clipboard.SetText(SelectedQuery.QueryText);
            MessageBox.Show("Query copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void CopySqlCode(string? sqlCode)
    {
        if (!string.IsNullOrEmpty(sqlCode))
        {
            Clipboard.SetText(sqlCode);
            MessageBox.Show("SQL code copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ExportToCsv()
    {
        try
        {
            var csv = new StringBuilder();
            csv.AppendLine("Query Text,Execution Count,Avg CPU Time (ms),Total CPU Time (ms),Avg Elapsed Time (ms),Avg Logical Reads,Avg Physical Reads,Last Execution");

            foreach (var query in ExpensiveQueries)
            {
                csv.AppendLine($"\"{query.QueryText.Replace("\"", "\"\"")}\",{query.ExecutionCount},{query.AvgCpuTimeMs},{query.TotalCpuTimeMs},{query.AvgElapsedTimeMs},{query.AvgLogicalReads},{query.AvgPhysicalReads},{query.LastExecutionTime}");
            }

            var fileName = $"SQL_Performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, fileName);

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            MessageBox.Show($"Export erfolgreich!\n\nDatei: {filePath}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export fehlgeschlagen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CloseDetailsPanel()
    {
        IsDetailsPanelVisible = false;
        SelectedQuery = null;
        HasAiAnalysis = false;
        AiAnalysisResult = null;
    }

    [RelayCommand]
    private async Task AnalyzeWithAiAsync()
    {
        if (SelectedQuery == null || !IsAiEnabled)
        {
            MessageBox.Show(
                "Please select a query and ensure AI is configured in Settings.",
                "AI Analysis",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        IsAnalyzingWithAi = true;

        try
        {
            var result = await _aiOptimizer.AnalyzeQueryWithAiAsync(SelectedQuery);

            if (result.Success)
            {
                AiAnalysisResult = result;
                HasAiAnalysis = true;

                // Add AI suggestions to the optimization suggestions list
                if (result.Suggestions.Any())
                {
                    OptimizationSuggestions.Clear();
                    foreach (var suggestion in result.Suggestions.OrderByDescending(s => s.EstimatedImpact))
                    {
                        OptimizationSuggestions.Add(new QueryOptimizationSuggestion
                        {
                            Category = suggestion.Category,
                            Severity = suggestion.Severity,
                            Title = $"🤖 {suggestion.Title}",
                            Description = suggestion.Explanation,
                            RecommendedAction = suggestion.Reasoning,
                            SqlCode = suggestion.CodeExample,
                            EstimatedImpact = suggestion.EstimatedImpact
                        });
                    }
                }

                MessageBox.Show(
                    $"AI analysis complete!\n\nPerformance Score: {result.PerformanceScore}/100\nEstimated Improvement: {result.EstimatedImprovement:F0}%",
                    "AI Analysis Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    $"AI analysis failed:\n\n{result.ErrorMessage}",
                    "AI Analysis Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error during AI analysis:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsAnalyzingWithAi = false;
        }
    }

    [RelayCommand]
    private async Task AutoFixQueryAsync()
    {
        if (SelectedQuery == null || _autoFixer == null)
        {
            MessageBox.Show(
                "Auto-Fixer is not available or no query selected.",
                "Auto-Fix",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // First, preview the fixes
            var previewFixes = await _autoFixer.PreviewFixesAsync(SelectedQuery.QueryText);

            if (!previewFixes.Any())
            {
                MessageBox.Show(
                    "No optimization opportunities found. Query is already well-optimized!",
                    "Auto-Fix Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Build detailed preview message
            var previewMessage = new StringBuilder();
            previewMessage.AppendLine("🔧 Auto-Fix Preview\n");
            previewMessage.AppendLine($"Found {previewFixes.Count} potential optimization(s):\n");

            foreach (var fix in previewFixes.OrderByDescending(f => f.EstimatedImpact))
            {
                var safetyIcon = fix.Safety switch
                {
                    QueryFixSafety.Safe => "✅",
                    QueryFixSafety.LowRisk => "⚠️",
                    QueryFixSafety.MediumRisk => "⚠️⚠️",
                    QueryFixSafety.HighRisk => "❌",
                    _ => "❓"
                };

                previewMessage.AppendLine($"{safetyIcon} {fix.Title}");
                previewMessage.AppendLine($"   Type: {fix.FixType}");
                previewMessage.AppendLine($"   Impact: +{fix.EstimatedImpact}% performance");
                previewMessage.AppendLine($"   Safety: {fix.Safety}");
                previewMessage.AppendLine($"   Confidence: {fix.Confidence:P0}");
                previewMessage.AppendLine($"   Why: {fix.Description}\n");

                if (!string.IsNullOrEmpty(fix.BeforeSnippet))
                {
                    previewMessage.AppendLine($"   Before: {fix.BeforeSnippet}");
                    previewMessage.AppendLine($"   After:  {fix.AfterSnippet}\n");
                }
            }

            var totalImpact = previewFixes.Sum(f => f.EstimatedImpact);
            previewMessage.AppendLine($"Total Estimated Improvement: +{totalImpact}%\n");
            previewMessage.AppendLine("Apply these fixes?");

            var dialogResult = MessageBox.Show(
                previewMessage.ToString(),
                "Auto-Fix Preview",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (dialogResult != MessageBoxResult.Yes)
            {
                MessageBox.Show("Auto-Fix cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Apply the fixes
            var result = await _autoFixer.AutoFixQueryAsync(SelectedQuery.QueryText);

            if (result.Success && result.AppliedFixes.Any())
            {
                var successMessage = new StringBuilder();
                successMessage.AppendLine("✅ Auto-Fix Complete!\n");
                successMessage.AppendLine($"Applied {result.AppliedFixes.Count} fix(es):\n");

                foreach (var fix in result.AppliedFixes)
                {
                    successMessage.AppendLine($"  ✓ {fix.Title}");
                    successMessage.AppendLine($"    Why: {fix.Description}");
                    successMessage.AppendLine($"    Impact: +{fix.EstimatedImpact}%\n");
                }

                successMessage.AppendLine($"Estimated Improvement: {result.EstimatedPerformanceImprovement}%");
                successMessage.AppendLine($"Overall Confidence: {result.OverallConfidence:P0}\n");
                successMessage.AppendLine("Fixed query copied to clipboard!");
                successMessage.AppendLine("\nSave fixed query to file?");

                // Copy to clipboard
                Clipboard.SetText(result.FixedQuery);

                var saveResult = MessageBox.Show(
                    successMessage.ToString(),
                    "Auto-Fix Success",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (saveResult == MessageBoxResult.Yes)
                {
                    // Save to file
                    var fileName = $"Fixed_Query_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var filePath = Path.Combine(desktopPath, fileName);

                    var fileContent = new StringBuilder();
                    fileContent.AppendLine("-- ========================================");
                    fileContent.AppendLine("-- Auto-Fixed Query");
                    fileContent.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-DD HH:mm:ss}");
                    fileContent.AppendLine("-- ========================================\n");

                    fileContent.AppendLine("-- ORIGINAL QUERY:");
                    fileContent.AppendLine("-- " + result.OriginalQuery.Replace("\n", "\n-- "));
                    fileContent.AppendLine("\n-- ========================================\n");

                    fileContent.AppendLine("-- APPLIED FIXES:");
                    foreach (var fix in result.AppliedFixes)
                    {
                        fileContent.AppendLine($"-- ✓ {fix.Title}");
                        fileContent.AppendLine($"--   {fix.Description}");
                        fileContent.AppendLine($"--   Impact: +{fix.EstimatedImpact}%");
                    }
                    fileContent.AppendLine($"-- Total Improvement: {result.EstimatedPerformanceImprovement}%\n");

                    fileContent.AppendLine("-- ========================================\n");
                    fileContent.AppendLine("-- FIXED QUERY:\n");
                    fileContent.AppendLine(result.FixedQuery);

                    File.WriteAllText(filePath, fileContent.ToString(), Encoding.UTF8);

                    MessageBox.Show(
                        $"Fixed query saved to:\n\n{filePath}",
                        "Saved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(
                    result.ErrorMessage ?? "No fixes were applied.",
                    "Auto-Fix Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Auto-Fix Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GenerateDocumentationAsync()
    {
        if (SelectedQuery == null || _docGenerator == null)
        {
            MessageBox.Show(
                "Documentation Generator is not available or no query selected.",
                "Documentation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var doc = await _docGenerator.GenerateDocumentationAsync(SelectedQuery.QueryText, SelectedQuery);
            var markdown = await _docGenerator.GenerateMarkdownAsync(doc);

            // Save to file
            var fileName = $"Query_Documentation_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, fileName);

            File.WriteAllText(filePath, markdown, Encoding.UTF8);

            MessageBox.Show(
                $"Documentation generated successfully!\n\nFile: {filePath}\n\nQuery: {doc.QueryName}\nComplexity: {doc.Complexity.Level} ({doc.Complexity.Score}/100)",
                "Documentation Generated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Documentation Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PredictPerformanceAsync()
    {
        if (SelectedQuery == null)
        {
            MessageBox.Show(
                "Please select a query first.",
                "Performance Prediction",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // Get baseline prediction
            var baseline = await _queryAnalyzer.PredictPerformanceAsync(SelectedQuery);

            // Get optimization prediction if suggestions exist
            PerformancePrediction? optimized = null;
            if (OptimizationSuggestions.Any())
            {
                optimized = await _queryAnalyzer.PredictPerformanceWithOptimizationsAsync(
                    SelectedQuery,
                    OptimizationSuggestions.ToList());
            }

            var message = new StringBuilder();
            message.AppendLine("=== Performance Prediction ===\n");
            message.AppendLine("Current Performance:");
            message.AppendLine($"  CPU Time: {baseline.PredictedCpuTimeMs:F2}ms");
            message.AppendLine($"  Logical Reads: {baseline.PredictedLogicalReads:N0}");
            message.AppendLine($"  Duration: {baseline.PredictedDurationMs:F2}ms");
            message.AppendLine($"  Confidence: {baseline.ConfidenceScore:P0}\n");

            if (baseline.ContributingFactors.Any())
            {
                message.AppendLine("Contributing Factors:");
                foreach (var factor in baseline.ContributingFactors.Take(3))
                {
                    message.AppendLine($"  • {factor.Factor} ({factor.ImpactPercent:F0}%)");
                }
                message.AppendLine();
            }

            if (optimized != null && optimized.OptimizationImpact != null)
            {
                message.AppendLine("After Optimization:");
                message.AppendLine($"  CPU Time: {optimized.PredictedCpuTimeMs:F2}ms");
                message.AppendLine($"  Logical Reads: {optimized.PredictedLogicalReads:N0}");
                message.AppendLine($"  Duration: {optimized.PredictedDurationMs:F2}ms\n");
                message.AppendLine($"Expected Improvement: {optimized.OptimizationImpact.OverallImprovementPercent:F0}%");
                message.AppendLine($"  • CPU: -{optimized.OptimizationImpact.CpuTimeReductionPercent:F0}%");
                message.AppendLine($"  • I/O: -{optimized.OptimizationImpact.LogicalReadsReductionPercent:F0}%\n");
                message.AppendLine(optimized.OptimizationImpact.Summary);
            }

            MessageBox.Show(message.ToString(), "Performance Prediction", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Prediction Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BatchAnalyzeAsync()
    {
        if (!IsAiEnabled)
        {
            MessageBox.Show(
                "AI features must be enabled in Settings for Batch Analysis.",
                "Batch Analysis",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var topQueries = _allQueries.OrderByDescending(q => q.TotalCpuTimeMs).Take(10).ToList();

            if (!topQueries.Any())
            {
                MessageBox.Show("No queries to analyze.", "Batch Analysis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var results = await _aiOptimizer.BatchAnalyzeQueriesAsync(topQueries);

            var message = new StringBuilder();
            message.AppendLine($"Batch Analysis Complete: {results.Count} queries analyzed\n");

            int successCount = results.Count(r => r.Success);
            message.AppendLine($"Successful: {successCount}/{results.Count}\n");

            message.AppendLine("Top Improvements:");
            var successful = results.Where(r => r.Success).OrderByDescending(r => r.EstimatedImprovement).Take(5);
            foreach (var result in successful)
            {
                message.AppendLine($"  • Score: {result.PerformanceScore}/100, Improvement: {result.EstimatedImprovement:F0}%");
            }

            MessageBox.Show(message.ToString(), "Batch Analysis", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Batch Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CalculateQueryCostAsync()
    {
        if (SelectedQuery == null || _costCalculator == null)
        {
            MessageBox.Show("Cost Calculator is not available or no query selected.", "Cost Analysis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var parameters = new CostParameters
            {
                AverageHourlySalary = 50.0,
                ActiveUsers = 100,
                QueriesPerUserPerDay = 500,
                MonthlyHardwareCost = 5000.0,
                Currency = "€"
            };

            var costAnalysis = await _costCalculator.CalculateQueryCostAsync(SelectedQuery, parameters);

            var message = new StringBuilder();
            message.AppendLine("💰 Query Cost Analysis\n");
            message.AppendLine($"Daily Cost: {parameters.Currency}{costAnalysis.DailyTotalCost:N2}");
            message.AppendLine($"Monthly Cost: {parameters.Currency}{costAnalysis.MonthlyTotalCost:N2}");
            message.AppendLine($"Yearly Cost: {parameters.Currency}{costAnalysis.YearlyTotalCost:N2}\n");
            message.AppendLine($"Breakdown:");
            message.AppendLine($"  User Productivity: {parameters.Currency}{costAnalysis.YearlyUserProductivityCost:N2}/year");
            message.AppendLine($"  Infrastructure: {parameters.Currency}{costAnalysis.YearlyInfrastructureCost:N2}/year\n");
            message.AppendLine($"Total Wait Time: {costAnalysis.TotalWaitTimeHoursPerYear:F0} hours/year");
            message.AppendLine($"Affected Users: {costAnalysis.AffectedUsers}");

            MessageBox.Show(message.ToString(), "Cost Analysis", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Cost Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ForecastPerformanceAsync()
    {
        if (SelectedQuery == null || _forecasting == null)
        {
            MessageBox.Show("Forecasting is not available or no query selected.", "Performance Forecast", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // Simulate historical data (in production, would come from HistoricalDataService)
            var history = new List<HistoricalQuerySnapshot>();
            for (int i = 0; i < 30; i++)
            {
                history.Add(new HistoricalQuerySnapshot
                {
                    Timestamp = DateTime.Now.AddDays(-i),
                    QueryHash = SelectedQuery.QueryHash,
                    AvgElapsedTimeMs = SelectedQuery.AvgElapsedTimeMs * (1 + (i * 0.01)),
                    ExecutionCount = SelectedQuery.ExecutionCount,
                    AvgCpuTimeMs = SelectedQuery.AvgCpuTimeMs,
                    AvgLogicalReads = SelectedQuery.AvgLogicalReads
                });
            }

            var forecast = await _forecasting.ForecastPerformanceAsync(SelectedQuery, history, 30);

            MessageBox.Show(forecast.Summary, "Performance Forecast", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Forecast Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task HealQueryAsync()
    {
        if (SelectedQuery == null || _selfHealing == null)
        {
            MessageBox.Show("Self-Healing is not available or no query selected.", "Self-Healing", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var options = new HealingOptions
            {
                AutoApply = false,
                RequireApproval = true,
                AutoRollback = true,
                TestBeforeApply = true
            };

            var result = await _selfHealing.HealQueryAsync(SelectedQuery, options);

            if (result.Success)
            {
                var message = new StringBuilder();
                message.AppendLine(result.Summary);
                message.AppendLine("\nApply healing?");

                var dialogResult = MessageBox.Show(message.ToString(), "Self-Healing Result", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    Clipboard.SetText(result.HealedQuery);
                    MessageBox.Show("Healed query copied to clipboard!", "Applied", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(result.Message, "Self-Healing", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Self-Healing Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AnalyzeQueryCorrelationsAsync()
    {
        if (_correlationEngine == null || !_allQueries.Any())
        {
            MessageBox.Show("Correlation Engine is not available or no queries loaded.", "Query Correlations", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // Simulate historical data
            var history = new List<HistoricalQuerySnapshot>();

            var result = await _correlationEngine.AnalyzeQueryCorrelationsAsync(_allQueries, history);

            var message = new StringBuilder();
            message.AppendLine(result.Summary);

            MessageBox.Show(message.ToString(), "Query Correlation Analysis", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Correlation Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClusterQueriesAsync()
    {
        if (_clusteringService == null || !_allQueries.Any())
        {
            MessageBox.Show("Clustering Service is not available or no queries loaded.", "Query Clustering", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var options = new ClusteringOptions
            {
                MinClusterSize = 3,
                SimilarityThreshold = 0.75,
                ClusteringMethod = "Similarity"
            };

            var result = await _clusteringService.ClusterQueriesAsync(_allQueries, options);

            var message = new StringBuilder();
            message.AppendLine(result.Summary);
            message.AppendLine($"\nFound {result.TotalClusters} clusters:\n");

            foreach (var cluster in result.Clusters.Take(5))
            {
                message.AppendLine($"📊 {cluster.ClusterName}");
                message.AppendLine($"   Queries: {cluster.QueryHashes.Count}");
                message.AppendLine($"   Total Time: {cluster.TotalExecutionTime:F0}ms");
                message.AppendLine($"   Executions: {cluster.TotalExecutions:N0}\n");
            }

            MessageBox.Show(message.ToString(), "Query Clustering", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Clustering Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FindDuplicatesAsync()
    {
        if (_clusteringService == null || !_allQueries.Any())
        {
            MessageBox.Show("Clustering Service is not available or no queries loaded.", "Find Duplicates", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            var duplicates = await _clusteringService.FindDuplicateQueriesAsync(_allQueries);

            var message = new StringBuilder();
            message.AppendLine($"🔍 Duplicate Query Analysis\n");
            message.AppendLine($"Found {duplicates.Count} duplicate groups:\n");

            foreach (var group in duplicates.Take(10))
            {
                message.AppendLine($"Group: {group.QueryHashes.Count} similar queries");
                message.AppendLine($"Similarity: {group.SimilarityScore:P0}");
                message.AppendLine($"💡 {group.Recommendation}\n");
            }

            if (duplicates.Count == 0)
            {
                message.AppendLine("✅ No duplicate queries found!");
            }

            MessageBox.Show(message.ToString(), "Duplicate Queries", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Find Duplicates Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RewriteQueryAsync()
    {
        if (SelectedQuery == null || _queryRewriter == null)
        {
            MessageBox.Show("Query Rewriter is not available or no query selected.", "Query Rewriter", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // Build context from selected query
            var context = new QueryRewriteContext
            {
                DatabaseName = "AX2012",
                IsAX2012Query = true,
                CurrentExecutionTimeMs = (long)SelectedQuery.AvgElapsedTimeMs,
                ExecutionCount = (int)SelectedQuery.ExecutionCount,
                LogicalReads = (long)SelectedQuery.AvgLogicalReads,
                Goals = new List<OptimizationGoal>
                {
                    OptimizationGoal.ReduceExecutionTime,
                    OptimizationGoal.ReduceIO
                },
                Priority = OptimizationPriority.Balanced,
                AllowTableHints = true,
                AllowIndexHints = true,
                AllowStructuralChanges = true,
                AXVersion = "AX 2012 R3"
            };

            // Get rewrite options
            var options = await _queryRewriter.SuggestRewriteOptionsAsync(SelectedQuery.QueryText, context);

            if (!options.Any())
            {
                MessageBox.Show("No rewrite options found. Query is already well-optimized!", "Query Rewriter", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Build options message
            var message = new StringBuilder();
            message.AppendLine($"✍️ Intelligent Query Rewriter\n");
            message.AppendLine($"Found {options.Count} rewrite option(s):\n");

            int optionNumber = 1;
            foreach (var option in options.OrderByDescending(o => o.EstimatedImprovementPercentage))
            {
                var recommendedIcon = option.Recommended ? "⭐" : "";
                message.AppendLine($"{optionNumber}. {recommendedIcon} {option.OptionName}");
                message.AppendLine($"   Approach: {option.Approach}");
                message.AppendLine($"   Improvement: +{option.EstimatedImprovementPercentage:F0}%");
                message.AppendLine($"   Complexity: {option.ComplexityScore}/10");
                message.AppendLine($"   Risk: {option.RiskScore}/10");
                message.AppendLine($"   {option.Description}\n");
                optionNumber++;
            }

            message.AppendLine("View detailed rewrite for best option?");

            var dialogResult = MessageBox.Show(message.ToString(), "Query Rewrite Options", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult != MessageBoxResult.Yes)
            {
                return;
            }

            // Get best option
            var bestOption = options.OrderByDescending(o => o.EstimatedImprovementPercentage).First();

            // Build detailed result using the full RewriteQueryAsync
            var result = await _queryRewriter.RewriteQueryAsync(SelectedQuery.QueryText, context);

            // Show detailed result
            var detailMessage = new StringBuilder();
            detailMessage.AppendLine($"✨ Query Rewrite Result\n");
            detailMessage.AppendLine($"Estimated Improvement: +{result.EstimatedImprovementPercentage:F0}%");
            detailMessage.AppendLine($"Confidence: {result.ConfidenceScore:F0}%");
            detailMessage.AppendLine($"Semantically Equivalent: {(result.IsSemanticallyEquivalent ? "✅ Yes" : "⚠️ Needs Verification")}\n");

            detailMessage.AppendLine("Changes Made:");
            foreach (var change in result.ChangesSummary)
            {
                detailMessage.AppendLine($"  • {change}");
            }
            detailMessage.AppendLine();

            detailMessage.AppendLine("Techniques Applied:");
            foreach (var technique in result.TechniquesApplied.OrderByDescending(t => t.ImpactPercentage))
            {
                detailMessage.AppendLine($"  • {technique.TechniqueName} (+{technique.ImpactPercentage:F0}%)");
            }
            detailMessage.AppendLine();

            if (result.AX2012BestPractices.Any())
            {
                detailMessage.AppendLine("AX 2012 Best Practices:");
                foreach (var practice in result.AX2012BestPractices)
                {
                    detailMessage.AppendLine($"  ✓ {practice}");
                }
                detailMessage.AppendLine();
            }

            detailMessage.AppendLine($"Reason: {result.ImprovementReason}\n");
            detailMessage.AppendLine("Copy rewritten query to clipboard?");

            var copyResult = MessageBox.Show(detailMessage.ToString(), "Query Rewrite Details", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (copyResult == MessageBoxResult.Yes)
            {
                // Build comparison for clipboard
                var clipboardContent = new StringBuilder();
                clipboardContent.AppendLine("-- ==========================================");
                clipboardContent.AppendLine("-- Intelligent Query Rewriter Result");
                clipboardContent.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                clipboardContent.AppendLine($"-- Estimated Improvement: +{result.EstimatedImprovementPercentage:F0}%");
                clipboardContent.AppendLine("-- ==========================================\n");

                clipboardContent.AppendLine("-- ORIGINAL QUERY:");
                clipboardContent.AppendLine(result.OriginalQuery);
                clipboardContent.AppendLine("\n-- ==========================================\n");

                clipboardContent.AppendLine("-- CHANGES:");
                foreach (var change in result.ChangesSummary)
                {
                    clipboardContent.AppendLine($"-- • {change}");
                }
                clipboardContent.AppendLine("\n-- ==========================================\n");

                clipboardContent.AppendLine("-- REWRITTEN QUERY:");
                clipboardContent.AppendLine(result.RewrittenQuery);

                Clipboard.SetText(clipboardContent.ToString());
                MessageBox.Show("Rewritten query copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Query Rewriter Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}


