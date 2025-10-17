using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.WpfApp.Services;
using AX2012PerformanceOptimizer.Data.SqlServer;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class AiHealthDashboardViewModel : ObservableObject
{
    private readonly ISystemHealthScoreService _healthScoreService;
    private readonly IDialogService _dialogService;
    private readonly ISqlConnectionManager _connectionManager;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private SystemHealthScore? currentHealthScore;

    [ObservableProperty]
    private ObservableCollection<HealthAction> recommendedActions = new();

    [ObservableProperty]
    private ObservableCollection<HealthScoreHistory> scoreHistory = new();

    [ObservableProperty]
    private HealthAction? selectedAction;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    // Health Score Display Properties
    [ObservableProperty]
    private int overallScore;

    [ObservableProperty]
    private int scoreChange;

    [ObservableProperty]
    private string scoreChangeText = string.Empty;

    [ObservableProperty]
    private string scoreChangeColor = "#4CAF50";

    [ObservableProperty]
    private string healthStatus = string.Empty;

    [ObservableProperty]
    private string healthStatusColor = "#4CAF50";

    [ObservableProperty]
    private string healthStatusIcon = "✅";

    // Category Scores
    [ObservableProperty]
    private HealthCategory? sqlPerformanceCategory;

    [ObservableProperty]
    private HealthCategory? indexHealthCategory;

    [ObservableProperty]
    private HealthCategory? batchJobsCategory;

    [ObservableProperty]
    private HealthCategory? databaseSizeCategory;

    // Top Action
    [ObservableProperty]
    private HealthAction? topImpactAction;

    public AiHealthDashboardViewModel(
        ISystemHealthScoreService healthScoreService,
        IDialogService dialogService,
        ISqlConnectionManager connectionManager)
    {
        _healthScoreService = healthScoreService;
        _dialogService = dialogService;
        _connectionManager = connectionManager;
        _connectionManager.ConnectionChanged += OnConnectionChanged;

        // Update initial status based on connection
        if (!_connectionManager.IsConnected)
        {
            StatusMessage = "Keine Datenbankverbindung - Bitte verbinden Sie sich in Settings";
        }
    }

    private async void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        if (e.IsConnected)
        {
            // Connection established - enable health score calculation
            StatusMessage = "Verbindung hergestellt - Bereit für Health Score Berechnung";

            // Optionally auto-calculate health score on connection
            // Uncomment if you want automatic calculation:
            // await CalculateHealthScoreAsync();
        }
        else
        {
            // Connection lost - clear health score
            CurrentHealthScore = null;
            RecommendedActions.Clear();
            ScoreHistory.Clear();
            TopImpactAction = null;
            SqlPerformanceCategory = null;
            IndexHealthCategory = null;
            BatchJobsCategory = null;
            DatabaseSizeCategory = null;
            OverallScore = 0;
            HealthStatus = string.Empty;
            StatusMessage = "Keine Datenbankverbindung - Bitte verbinden Sie sich in Settings";
        }
    }

    [RelayCommand]
    private async Task CalculateHealthScoreAsync()
    {
        IsLoading = true;
        StatusMessage = "Berechne System Health Score...";

        try
        {
            var score = await _healthScoreService.CalculateHealthScoreAsync();
            CurrentHealthScore = score;

            // Update display properties
            OverallScore = score.OverallScore;
            ScoreChange = score.ScoreChange;
            HealthStatus = score.Status.ToString();

            // Score change text and color
            if (ScoreChange > 0)
            {
                ScoreChangeText = $"↗️ +{ScoreChange} Punkte";
                ScoreChangeColor = "#4CAF50"; // Green
            }
            else if (ScoreChange < 0)
            {
                ScoreChangeText = $"↘️ {ScoreChange} Punkte";
                ScoreChangeColor = "#F44336"; // Red
            }
            else
            {
                ScoreChangeText = "→ Unverändert";
                ScoreChangeColor = "#757575"; // Gray
            }

            // Health status color and icon
            HealthStatusColor = score.Status switch
            {
                Core.Models.HealthStatus.Excellent => "#4CAF50", // Green
                Core.Models.HealthStatus.Good => "#8BC34A",      // Light Green
                Core.Models.HealthStatus.Fair => "#FF9800",      // Orange
                Core.Models.HealthStatus.Poor => "#FF5722",      // Deep Orange
                Core.Models.HealthStatus.Critical => "#F44336",  // Red
                _ => "#9E9E9E"
            };

            HealthStatusIcon = score.Status switch
            {
                Core.Models.HealthStatus.Excellent => "✅",
                Core.Models.HealthStatus.Good => "✅",
                Core.Models.HealthStatus.Fair => "⚠️",
                Core.Models.HealthStatus.Poor => "❌",
                Core.Models.HealthStatus.Critical => "❌",
                _ => "⚪"
            };

            // Category scores
            SqlPerformanceCategory = score.SqlPerformance;
            IndexHealthCategory = score.IndexHealth;
            BatchJobsCategory = score.BatchJobs;
            DatabaseSizeCategory = score.DatabaseSize;

            // Actions
            RecommendedActions.Clear();
            foreach (var action in score.RecommendedActions)
            {
                RecommendedActions.Add(action);
            }

            TopImpactAction = score.TopImpactAction;

            // Save to history
            await _healthScoreService.SaveHealthScoreToHistoryAsync(score);

            // Load history for trend
            var history = await _healthScoreService.GetHealthScoreHistoryAsync(30);
            ScoreHistory.Clear();
            foreach (var entry in history)
            {
                ScoreHistory.Add(entry);
            }

            StatusMessage = $"Health Score berechnet: {OverallScore}/100 ({HealthStatus})";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Fehler",
                $"Fehler beim Berechnen des Health Scores:\n\n{ex.Message}",
                ex);

            StatusMessage = "Fehler bei der Berechnung";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CopyActionScriptAsync()
    {
        if (SelectedAction?.Script != null)
        {
            Clipboard.SetText(SelectedAction.Script);
            StatusMessage = "Script in Zwischenablage kopiert";
        }
        else
        {
            await _dialogService.ShowInfoAsync(
                "Info",
                "Diese Aktion hat kein Script zum Kopieren.");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await CalculateHealthScoreAsync();
    }

    [RelayCommand]
    private async Task ViewHistoryAsync()
    {
        try
        {
            var history = await _healthScoreService.GetHealthScoreHistoryAsync(90);
            ScoreHistory.Clear();
            foreach (var entry in history)
            {
                ScoreHistory.Add(entry);
            }

            if (history.Any())
            {
                // Convert to HistoryEntry format
                var historyEntries = history.OrderByDescending(h => h.Timestamp)
                    .Select(h => new HistoryEntry
                    {
                        Timestamp = h.Timestamp,
                        Title = $"Health Score: {h.OverallScore}/100",
                        Description = $"SQL: {h.SqlPerformanceScore} | Index: {h.IndexHealthScore} | Batch: {h.BatchJobsScore} | DB: {h.DatabaseSizeScore}",
                        Score = h.OverallScore,
                        StatusColor = h.OverallScore >= 75 ? "#4CAF50" : h.OverallScore >= 60 ? "#FF9800" : "#F44336",
                        Icon = h.OverallScore >= 75 ? "✅" : h.OverallScore >= 60 ? "⚠️" : "❌"
                    })
                    .ToList();

                await _dialogService.ShowHistoryAsync("Health Score Historie (letzte 90 Tage)", historyEntries);
            }
            else
            {
                await _dialogService.ShowInfoAsync(
                    "Keine Historie",
                    "Noch keine Historie vorhanden. Berechnen Sie zuerst einen Health Score.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Fehler",
                $"Fehler beim Laden der Historie:\n\n{ex.Message}",
                ex);
        }
    }

    [RelayCommand]
    private async Task ShowActionDetailsAsync(HealthAction action)
    {
        if (action == null) return;

        var content = $"Kategorie: {action.Category}\n";
        content += $"Priorität: {action.Priority}\n\n";
        content += $"Beschreibung:\n{action.Description}\n\n";
        content += $"Geschätzter Impact: +{action.EstimatedScoreImpact} Punkte\n";
        content += $"Geschätzte Zeit: {action.EstimatedTimeMinutes} Minuten\n";
        content += $"Automatisierbar: {(action.IsAutomatable ? "Ja" : "Nein")}";

        await _dialogService.ShowDetailsAsync(
            $"💡 {action.Title}",
            content,
            action.Script,
            "🔧");
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        if (CurrentHealthScore == null)
        {
            await _dialogService.ShowInfoAsync(
                "Kein Score",
                "Bitte berechnen Sie zuerst einen Health Score.");
            return;
        }

        try
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("===========================================");
            report.AppendLine("AX 2012 System Health Score Report");
            report.AppendLine($"Generiert: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            report.AppendLine("===========================================\n");

            report.AppendLine($"🏥 OVERALL HEALTH SCORE: {OverallScore}/100");
            report.AppendLine($"Status: {HealthStatus}");
            report.AppendLine($"Change: {ScoreChangeText}\n");

            report.AppendLine("--- CATEGORY BREAKDOWN ---\n");
            report.AppendLine($"{SqlPerformanceCategory?.StatusIcon} SQL Performance: {SqlPerformanceCategory?.Score}/100 ({SqlPerformanceCategory?.StatusText})");
            if (SqlPerformanceCategory?.Issues.Any() == true)
            {
                foreach (var issue in SqlPerformanceCategory.Issues)
                {
                    report.AppendLine($"   - {issue}");
                }
            }
            report.AppendLine();

            report.AppendLine($"{IndexHealthCategory?.StatusIcon} Index Health: {IndexHealthCategory?.Score}/100 ({IndexHealthCategory?.StatusText})");
            if (IndexHealthCategory?.Issues.Any() == true)
            {
                foreach (var issue in IndexHealthCategory.Issues)
                {
                    report.AppendLine($"   - {issue}");
                }
            }
            report.AppendLine();

            report.AppendLine($"{BatchJobsCategory?.StatusIcon} Batch Jobs: {BatchJobsCategory?.Score}/100 ({BatchJobsCategory?.StatusText})");
            if (BatchJobsCategory?.Issues.Any() == true)
            {
                foreach (var issue in BatchJobsCategory.Issues)
                {
                    report.AppendLine($"   - {issue}");
                }
            }
            report.AppendLine();

            report.AppendLine($"{DatabaseSizeCategory?.StatusIcon} Database Size: {DatabaseSizeCategory?.Score}/100 ({DatabaseSizeCategory?.StatusText})");
            if (DatabaseSizeCategory?.Issues.Any() == true)
            {
                foreach (var issue in DatabaseSizeCategory.Issues)
                {
                    report.AppendLine($"   - {issue}");
                }
            }
            report.AppendLine();

            report.AppendLine("--- TOP RECOMMENDED ACTIONS ---\n");
            foreach (var action in RecommendedActions.Take(5))
            {
                report.AppendLine($"[{action.Priority}] {action.Title}");
                report.AppendLine($"    Impact: +{action.EstimatedScoreImpact} points | Time: {action.EstimatedTimeMinutes} min");
                report.AppendLine($"    {action.Description}");
                report.AppendLine();
            }

            var fileName = $"HealthScore_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            System.IO.File.WriteAllText(filePath, report.ToString());

            await _dialogService.ShowSuccessAsync(
                "Export erfolgreich",
                "Report exportiert!",
                $"Datei: {filePath}");

            StatusMessage = "Report exportiert";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Fehler",
                $"Fehler beim Exportieren:\n\n{ex.Message}",
                ex);
        }
    }
}
