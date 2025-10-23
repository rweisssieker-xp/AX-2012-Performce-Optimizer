using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services.NaturalLanguageToSql;
using AX2012PerformanceOptimizer.Core.Models.NaturalLanguageToSql;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

/// <summary>
/// ViewModel for Natural Language to SQL Generation feature
/// </summary>
public partial class NaturalLanguageToSqlViewModel : ObservableObject
{
    private readonly INaturalLanguageToSqlService _sqlService;

    [ObservableProperty]
    private string naturalLanguageInput = string.Empty;

    [ObservableProperty]
    private string generatedSql = string.Empty;

    [ObservableProperty]
    private string explanation = string.Empty;

    [ObservableProperty]
    private string dataAreaId = "DAT";

    [ObservableProperty]
    private double confidenceScore;

    [ObservableProperty]
    private bool isGenerating;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private string statusMessage = "Ready to generate SQL...";

    [ObservableProperty]
    private ObservableCollection<string> warnings = new();

    [ObservableProperty]
    private ObservableCollection<string> suggestions = new();

    [ObservableProperty]
    private ObservableCollection<string> alternativeQueries = new();

    [ObservableProperty]
    private ObservableCollection<string> exampleQueries = new();

    [ObservableProperty]
    private bool isSafeToExecute;

    [ObservableProperty]
    private bool isValidationVisible;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool readOnlyMode = true;

    [ObservableProperty]
    private double generationTimeMs;

    public NaturalLanguageToSqlViewModel(INaturalLanguageToSqlService sqlService)
    {
        _sqlService = sqlService;
        _ = LoadExampleQueriesAsync();
    }

    private async Task LoadExampleQueriesAsync()
    {
        try
        {
            var examples = await _sqlService.GetExampleQueriesAsync();
            ExampleQueries.Clear();
            foreach (var example in examples)
            {
                ExampleQueries.Add(example);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load examples: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GenerateSqlAsync()
    {
        if (string.IsNullOrWhiteSpace(NaturalLanguageInput))
        {
            MessageBox.Show(
                "Please enter a description of what you want to query.",
                "Input Required",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        IsGenerating = true;
        StatusMessage = "Generating SQL query...";
        HasResult = false;
        ClearPreviousResults();

        try
        {
            var request = new SqlGenerationRequest
            {
                NaturalLanguageQuery = NaturalLanguageInput,
                DataAreaId = DataAreaId,
                ReadOnlyMode = ReadOnlyMode,
                Language = "en" // Could be made configurable
            };

            var result = await _sqlService.GenerateSqlAsync(request);

            if (result.Success)
            {
                GeneratedSql = result.GeneratedSql;
                Explanation = result.Explanation;
                ConfidenceScore = result.ConfidenceScore;
                IsSafeToExecute = result.IsSafeToExecute;
                GenerationTimeMs = result.GenerationTimeMs;

                // Populate warnings
                Warnings.Clear();
                foreach (var warning in result.Warnings)
                {
                    Warnings.Add(warning);
                }

                // Populate suggestions
                Suggestions.Clear();
                foreach (var suggestion in result.OptimizationSuggestions)
                {
                    Suggestions.Add(suggestion);
                }

                // Populate alternatives
                AlternativeQueries.Clear();
                foreach (var alt in result.AlternativeQueries)
                {
                    AlternativeQueries.Add(alt);
                }

                HasResult = true;
                StatusMessage = $"SQL generated successfully! (Confidence: {ConfidenceScore:F0}%, {GenerationTimeMs:F0}ms)";

                if (!IsSafeToExecute)
                {
                    MessageBox.Show(
                        "⚠️ Warning: This query may not be safe to execute.\nPlease review it carefully before running.",
                        "Safety Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    $"SQL generation failed:\n\n{result.ErrorMessage}",
                    "Generation Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusMessage = "Generation failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error generating SQL:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task ValidateSqlAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql))
        {
            MessageBox.Show(
                "No SQL query to validate. Please generate a query first.",
                "No Query",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        IsGenerating = true;
        StatusMessage = "Validating SQL query...";

        try
        {
            var validationResult = await _sqlService.ValidateSqlAsync(GeneratedSql);

            IsValidationVisible = true;
            ValidationMessage = BuildValidationMessage(validationResult);

            if (validationResult.IsValid && validationResult.IsSafe)
            {
                StatusMessage = "✅ Query is valid and safe";
                IsSafeToExecute = true;
            }
            else
            {
                StatusMessage = "⚠️ Validation found issues";
                IsSafeToExecute = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Validation error:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusMessage = "Validation failed";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        if (!string.IsNullOrWhiteSpace(GeneratedSql))
        {
            Clipboard.SetText(GeneratedSql);
            StatusMessage = "SQL copied to clipboard";
        }
    }

    [RelayCommand]
    private void Clear()
    {
        NaturalLanguageInput = string.Empty;
        ClearPreviousResults();
        HasResult = false;
        StatusMessage = "Ready to generate SQL...";
    }

    [RelayCommand]
    private void UseExample(string example)
    {
        if (!string.IsNullOrEmpty(example))
        {
            NaturalLanguageInput = example;
            StatusMessage = "Example loaded - click Generate SQL";
        }
    }

    [RelayCommand]
    private void UseAlternative(string alternative)
    {
        if (!string.IsNullOrEmpty(alternative))
        {
            GeneratedSql = alternative;
            StatusMessage = "Alternative query loaded";
        }
    }

    [RelayCommand]
    private async Task SaveQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql))
        {
            return;
        }

        try
        {
            var fileName = $"Generated_SQL_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            var content = new System.Text.StringBuilder();
            content.AppendLine("-- Natural Language to SQL");
            content.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            content.AppendLine($"-- Request: {NaturalLanguageInput}");
            content.AppendLine($"-- Confidence: {ConfidenceScore:F0}%");
            content.AppendLine();
            content.AppendLine("-- Explanation:");
            content.AppendLine($"-- {Explanation}");
            content.AppendLine();
            content.AppendLine(GeneratedSql);

            if (Warnings.Any())
            {
                content.AppendLine();
                content.AppendLine("-- WARNINGS:");
                foreach (var warning in Warnings)
                {
                    content.AppendLine($"-- ⚠️ {warning}");
                }
            }

            await System.IO.File.WriteAllTextAsync(filePath, content.ToString());

            MessageBox.Show(
                $"SQL query saved!\n\nFile: {filePath}",
                "Saved Successfully",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusMessage = "Query saved to file";
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
        GeneratedSql = string.Empty;
        Explanation = string.Empty;
        ConfidenceScore = 0;
        IsSafeToExecute = false;
        IsValidationVisible = false;
        ValidationMessage = string.Empty;
        GenerationTimeMs = 0;
        Warnings.Clear();
        Suggestions.Clear();
        AlternativeQueries.Clear();
    }

    private string BuildValidationMessage(SqlValidationResult result)
    {
        var message = new System.Text.StringBuilder();

        message.AppendLine($"Validation Result:");
        message.AppendLine($"✓ Valid: {(result.IsValid ? "Yes" : "No")}");
        message.AppendLine($"✓ Safe: {(result.IsSafe ? "Yes" : "No")}");
        message.AppendLine($"✓ Type: {result.CommandType}");
        message.AppendLine($"✓ Complexity: {result.ComplexityScore:F0}/100");
        message.AppendLine();

        if (result.Errors.Any())
        {
            message.AppendLine("❌ Errors:");
            foreach (var error in result.Errors)
            {
                message.AppendLine($"  • {error}");
            }
            message.AppendLine();
        }

        if (result.Warnings.Any())
        {
            message.AppendLine("⚠️ Warnings:");
            foreach (var warning in result.Warnings)
            {
                message.AppendLine($"  • {warning}");
            }
            message.AppendLine();
        }

        if (result.SecurityConcerns.Any())
        {
            message.AppendLine("🔒 Security Concerns:");
            foreach (var concern in result.SecurityConcerns)
            {
                message.AppendLine($"  • {concern}");
            }
            message.AppendLine();
        }

        if (result.PerformanceConcerns.Any())
        {
            message.AppendLine("⚡ Performance Concerns:");
            foreach (var concern in result.PerformanceConcerns)
            {
                message.AppendLine($"  • {concern}");
            }
        }

        return message.ToString();
    }
}
