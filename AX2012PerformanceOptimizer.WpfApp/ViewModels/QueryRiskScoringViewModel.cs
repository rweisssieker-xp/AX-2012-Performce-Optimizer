using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services.QueryRiskScoring;
using AX2012PerformanceOptimizer.Core.Models.QueryRiskScoring;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

/// <summary>
/// ViewModel for ML-based Query Risk Scoring feature
/// </summary>
public partial class QueryRiskScoringViewModel : ObservableObject
{
    private readonly IQueryRiskScoringService _riskScoringService;

    [ObservableProperty]
    private string sqlQuery = string.Empty;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private string statusMessage = "Ready to analyze query risk...";

    // Risk Score Properties
    [ObservableProperty]
    private double overallRiskScore;

    [ObservableProperty]
    private string riskLevel = string.Empty;

    [ObservableProperty]
    private string riskLevelColor = "#757575";

    [ObservableProperty]
    private string riskLevelIcon = "❓";

    [ObservableProperty]
    private double performanceRisk;

    [ObservableProperty]
    private double securityRisk;

    [ObservableProperty]
    private double complexityRisk;

    [ObservableProperty]
    private double resourceRisk;

    [ObservableProperty]
    private double dataIntegrityRisk;

    [ObservableProperty]
    private double confidenceLevel;

    [ObservableProperty]
    private double estimatedExecutionTimeMs;

    [ObservableProperty]
    private double estimatedCpuUsagePercent;

    [ObservableProperty]
    private double estimatedMemoryUsageMb;

    [ObservableProperty]
    private bool shouldExecute;

    [ObservableProperty]
    private bool requiresReview;

    [ObservableProperty]
    private double assessmentTimeMs;

    [ObservableProperty]
    private double mlConfidence;

    [ObservableProperty]
    private bool usedMachineLearning;

    // Collections
    [ObservableProperty]
    private ObservableCollection<QueryRiskFactor> riskFactors = new();

    [ObservableProperty]
    private ObservableCollection<string> recommendations = new();

    [ObservableProperty]
    private ObservableCollection<string> warnings = new();

    [ObservableProperty]
    private ObservableCollection<string> violatedBestPractices = new();

    [ObservableProperty]
    private ObservableCollection<string> antiPatterns = new();

    [ObservableProperty]
    private ObservableCollection<string> tablesInvolved = new();

    [ObservableProperty]
    private ObservableCollection<string> missingIndexes = new();

    [ObservableProperty]
    private ObservableCollection<string> executionPlanConcerns = new();

    [ObservableProperty]
    private ObservableCollection<string> suggestedRewrites = new();

    [ObservableProperty]
    private ObservableCollection<HistoricalQueryComparison> historicalComparisons = new();

    // Example queries
    [ObservableProperty]
    private ObservableCollection<string> exampleQueries = new();

    public QueryRiskScoringViewModel(IQueryRiskScoringService riskScoringService)
    {
        _riskScoringService = riskScoringService;
        LoadExampleQueries();
    }

    private void LoadExampleQueries()
    {
        ExampleQueries = new ObservableCollection<string>
        {
            "SELECT * FROM CUSTTABLE",
            "SELECT * FROM INVENTTRANS WHERE ITEMID = 'ABC123'",
            "DELETE FROM CUSTTRANS",
            "UPDATE INVENTTABLE SET PRICE = PRICE * 1.1",
            "SELECT c.ACCOUNTNUM, SUM(t.AMOUNTCUR) FROM CUSTTRANS t CROSS JOIN CUSTTABLE c GROUP BY c.ACCOUNTNUM",
            "SELECT * FROM INVENTTRANS WHERE ITEMID LIKE '%ABC%'",
            "SELECT RECID, ACCOUNTNUM FROM CUSTTABLE WHERE DATAAREAID = 'DAT'",
            "SELECT t1.*, t2.*, t3.* FROM INVENTTRANS t1 JOIN INVENTTRANSPOSTING t2 ON t1.RECID = t2.INVENTTRANS JOIN INVENTTABLE t3 ON t1.ITEMID = t3.ITEMID"
        };
    }

    [RelayCommand]
    private async Task AnalyzeRiskAsync()
    {
        if (string.IsNullOrWhiteSpace(SqlQuery))
        {
            MessageBox.Show(
                "Please enter a SQL query to analyze.",
                "Input Required",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        IsAnalyzing = true;
        StatusMessage = "Analyzing query risk with ML algorithms...";
        HasResult = false;
        ClearPreviousResults();

        try
        {
            var assessment = await _riskScoringService.AssessQueryRiskAsync(SqlQuery);

            if (assessment.Success)
            {
                // Populate risk score properties
                var risk = assessment.RiskScore;
                OverallRiskScore = risk.OverallScore;
                RiskLevel = risk.RiskLevel.ToString();
                RiskLevelColor = risk.ColorCode;
                RiskLevelIcon = risk.Icon;

                PerformanceRisk = risk.PerformanceRisk;
                SecurityRisk = risk.SecurityRisk;
                ComplexityRisk = risk.ComplexityRisk;
                ResourceRisk = risk.ResourceRisk;
                DataIntegrityRisk = risk.DataIntegrityRisk;
                ConfidenceLevel = risk.ConfidenceLevel;

                EstimatedExecutionTimeMs = risk.EstimatedExecutionTimeMs;
                EstimatedCpuUsagePercent = risk.EstimatedCpuUsagePercent;
                EstimatedMemoryUsageMb = risk.EstimatedMemoryUsageMb;

                ShouldExecute = risk.ShouldExecute;
                RequiresReview = risk.RequiresReview;

                AssessmentTimeMs = assessment.AssessmentTimeMs;
                MlConfidence = assessment.MlConfidence;
                UsedMachineLearning = assessment.UsedMachineLearning;

                // Populate collections
                RiskFactors.Clear();
                foreach (var factor in risk.RiskFactors.OrderByDescending(f => f.WeightedScore))
                {
                    RiskFactors.Add(factor);
                }

                Recommendations.Clear();
                foreach (var recommendation in assessment.Recommendations)
                {
                    Recommendations.Add(recommendation);
                }

                Warnings.Clear();
                foreach (var warning in assessment.Warnings)
                {
                    Warnings.Add(warning);
                }

                ViolatedBestPractices.Clear();
                foreach (var violation in assessment.ViolatedBestPractices)
                {
                    ViolatedBestPractices.Add(violation);
                }

                AntiPatterns.Clear();
                foreach (var pattern in assessment.AntiPatterns)
                {
                    AntiPatterns.Add(pattern);
                }

                TablesInvolved.Clear();
                foreach (var table in assessment.TablesInvolved)
                {
                    TablesInvolved.Add(table);
                }

                MissingIndexes.Clear();
                foreach (var index in assessment.MissingIndexes)
                {
                    MissingIndexes.Add(index);
                }

                ExecutionPlanConcerns.Clear();
                foreach (var concern in assessment.ExecutionPlanConcerns)
                {
                    ExecutionPlanConcerns.Add(concern);
                }

                SuggestedRewrites.Clear();
                foreach (var rewrite in assessment.SuggestedRewrites)
                {
                    SuggestedRewrites.Add(rewrite);
                }

                HistoricalComparisons.Clear();
                foreach (var comparison in assessment.HistoricalComparisons)
                {
                    HistoricalComparisons.Add(comparison);
                }

                HasResult = true;
                StatusMessage = $"Analysis complete! Risk Level: {RiskLevel} ({OverallRiskScore:F1}/100) - {AssessmentTimeMs:F0}ms";

                // Show warning for high-risk queries
                if (risk.RiskLevel >= QueryRiskLevel.High)
                {
                    MessageBox.Show(
                        $"⚠️ {risk.RiskLevel.ToString().ToUpper()} RISK DETECTED!\n\n" +
                        $"Risk Score: {risk.OverallScore:F1}/100\n\n" +
                        $"This query poses significant risks. Please review the recommendations carefully.",
                        "Risk Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    $"Risk analysis failed:\n\n{assessment.ErrorMessage}",
                    "Analysis Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusMessage = "Analysis failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error analyzing query risk:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        SqlQuery = string.Empty;
        ClearPreviousResults();
        HasResult = false;
        StatusMessage = "Ready to analyze query risk...";
    }

    [RelayCommand]
    private void UseExample(string example)
    {
        if (!string.IsNullOrEmpty(example))
        {
            SqlQuery = example;
            StatusMessage = "Example loaded - click Analyze Risk";
        }
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        if (!string.IsNullOrWhiteSpace(SqlQuery))
        {
            Clipboard.SetText(SqlQuery);
            StatusMessage = "Query copied to clipboard";
        }
    }

    [RelayCommand]
    private async Task SaveReportAsync()
    {
        if (!HasResult)
        {
            return;
        }

        try
        {
            var fileName = $"RiskAssessment_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            var content = new System.Text.StringBuilder();
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine("QUERY RISK ASSESSMENT REPORT");
            content.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine();

            content.AppendLine("QUERY:");
            content.AppendLine(SqlQuery);
            content.AppendLine();

            content.AppendLine("RISK SUMMARY:");
            content.AppendLine($"  Overall Risk Score: {OverallRiskScore:F1}/100");
            content.AppendLine($"  Risk Level: {RiskLevel}");
            content.AppendLine($"  Should Execute: {(ShouldExecute ? "Yes" : "No")}");
            content.AppendLine($"  Requires Review: {(RequiresReview ? "Yes" : "No")}");
            content.AppendLine();

            content.AppendLine("RISK BREAKDOWN:");
            content.AppendLine($"  Performance Risk: {PerformanceRisk:F1}/100");
            content.AppendLine($"  Security Risk: {SecurityRisk:F1}/100");
            content.AppendLine($"  Complexity Risk: {ComplexityRisk:F1}/100");
            content.AppendLine($"  Resource Risk: {ResourceRisk:F1}/100");
            content.AppendLine($"  Data Integrity Risk: {DataIntegrityRisk:F1}/100");
            content.AppendLine();

            content.AppendLine("ESTIMATED IMPACT:");
            content.AppendLine($"  Execution Time: {EstimatedExecutionTimeMs:F0}ms");
            content.AppendLine($"  CPU Usage: {EstimatedCpuUsagePercent:F1}%");
            content.AppendLine($"  Memory Usage: {EstimatedMemoryUsageMb:F1}MB");
            content.AppendLine();

            if (Recommendations.Any())
            {
                content.AppendLine("RECOMMENDATIONS:");
                foreach (var rec in Recommendations)
                {
                    content.AppendLine($"  • {rec}");
                }
                content.AppendLine();
            }

            if (Warnings.Any())
            {
                content.AppendLine("WARNINGS:");
                foreach (var warning in Warnings)
                {
                    content.AppendLine($"  • {warning}");
                }
                content.AppendLine();
            }

            if (RiskFactors.Any())
            {
                content.AppendLine("RISK FACTORS:");
                foreach (var factor in RiskFactors)
                {
                    content.AppendLine($"  • {factor.FactorName} ({factor.Category})");
                    content.AppendLine($"    Score: {factor.Score:F1}, Weight: {factor.Weight:F2}");
                    content.AppendLine($"    {factor.Description}");
                    content.AppendLine($"    Recommendation: {factor.Recommendation}");
                    content.AppendLine();
                }
            }

            if (ViolatedBestPractices.Any())
            {
                content.AppendLine("VIOLATED BEST PRACTICES:");
                foreach (var violation in ViolatedBestPractices)
                {
                    content.AppendLine($"  • {violation}");
                }
                content.AppendLine();
            }

            if (AntiPatterns.Any())
            {
                content.AppendLine("DETECTED ANTI-PATTERNS:");
                foreach (var pattern in AntiPatterns)
                {
                    content.AppendLine($"  • {pattern}");
                }
                content.AppendLine();
            }

            content.AppendLine("ASSESSMENT METADATA:");
            content.AppendLine($"  ML Confidence: {MlConfidence:F1}%");
            content.AppendLine($"  Used Machine Learning: {UsedMachineLearning}");
            content.AppendLine($"  Assessment Time: {AssessmentTimeMs:F0}ms");
            content.AppendLine();

            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine("End of Report");
            content.AppendLine("=".PadRight(80, '='));

            await System.IO.File.WriteAllTextAsync(filePath, content.ToString());

            MessageBox.Show(
                $"Risk assessment report saved!\n\nFile: {filePath}",
                "Saved Successfully",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusMessage = "Report saved to file";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Save failed:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ClearPreviousResults()
    {
        OverallRiskScore = 0;
        RiskLevel = string.Empty;
        RiskLevelColor = "#757575";
        RiskLevelIcon = "❓";

        PerformanceRisk = 0;
        SecurityRisk = 0;
        ComplexityRisk = 0;
        ResourceRisk = 0;
        DataIntegrityRisk = 0;
        ConfidenceLevel = 0;

        EstimatedExecutionTimeMs = 0;
        EstimatedCpuUsagePercent = 0;
        EstimatedMemoryUsageMb = 0;

        ShouldExecute = false;
        RequiresReview = false;

        AssessmentTimeMs = 0;
        MlConfidence = 0;
        UsedMachineLearning = false;

        RiskFactors.Clear();
        Recommendations.Clear();
        Warnings.Clear();
        ViolatedBestPractices.Clear();
        AntiPatterns.Clear();
        TablesInvolved.Clear();
        MissingIndexes.Clear();
        ExecutionPlanConcerns.Clear();
        SuggestedRewrites.Clear();
        HistoricalComparisons.Clear();
    }
}
