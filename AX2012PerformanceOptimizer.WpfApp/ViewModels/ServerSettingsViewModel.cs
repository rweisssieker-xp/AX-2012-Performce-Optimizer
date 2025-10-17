using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using AX2012PerformanceOptimizer.WpfApp.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class ServerSettingsViewModel : ObservableObject
{
    private readonly IServerConfigurationService _configService;
    private readonly ISqlConnectionManager _connectionManager;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "No database connection - Please connect in Settings";

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private ServerMemoryInfo? memoryInfo;

    [ObservableProperty]
    private ObservableCollection<ServerConfiguration> configurations = new();

    [ObservableProperty]
    private ObservableCollection<ConfigurationRecommendation> recommendations = new();

    [ObservableProperty]
    private ServerConfiguration? selectedConfiguration;

    [ObservableProperty]
    private ConfigurationRecommendation? selectedRecommendation;

    // Memory display properties
    [ObservableProperty]
    private string totalMemoryDisplay = "-";

    [ObservableProperty]
    private string sqlMemoryDisplay = "-";

    [ObservableProperty]
    private string maxMemoryDisplay = "-";

    [ObservableProperty]
    private string recommendedMemoryDisplay = "-";

    public ServerSettingsViewModel(
        IServerConfigurationService configService,
        ISqlConnectionManager connectionManager,
        IDialogService dialogService)
    {
        _configService = configService;
        _connectionManager = connectionManager;
        _dialogService = dialogService;
        _connectionManager.ConnectionChanged += OnConnectionChanged;

        IsConnected = _connectionManager.IsConnected;
        if (!IsConnected)
        {
            StatusMessage = "No database connection - Please connect in Settings";
        }
    }

    private async void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        IsConnected = e.IsConnected;

        if (e.IsConnected)
        {
            StatusMessage = $"Connected to {e.DatabaseName} on {e.ServerName}";
            await LoadAllDataAsync();
        }
        else
        {
            StatusMessage = "Database connection lost - Please reconnect in Settings";
            Configurations.Clear();
            Recommendations.Clear();
            MemoryInfo = null;
            UpdateMemoryDisplays();
        }
    }

    [RelayCommand]
    private async Task LoadAllDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading server configuration...";

        try
        {
            // Load memory info
            MemoryInfo = await _configService.GetServerMemoryInfoAsync();
            UpdateMemoryDisplays();

            // Load all configurations
            var configs = await _configService.GetAllConfigurationsAsync();
            Configurations.Clear();
            foreach (var config in configs)
            {
                Configurations.Add(config);
            }

            // Load recommendations
            var recs = await _configService.AnalyzeConfigurationAsync();
            Recommendations.Clear();
            foreach (var rec in recs)
            {
                Recommendations.Add(rec);
            }

            StatusMessage = $"Loaded {Configurations.Count} configurations, {Recommendations.Count} recommendations";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Error Loading Configuration",
                $"Failed to load server configuration:\n\n{ex.Message}",
                ex);
            StatusMessage = "Error loading configuration";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyRecommendation(ConfigurationRecommendation? recommendation)
    {
        if (recommendation == null) return;

        var result = MessageBox.Show(
            $"Apply recommendation for '{recommendation.SettingName}'?\n\n" +
            $"Current: {recommendation.CurrentValue}\n" +
            $"Recommended: {recommendation.RecommendedValue}\n\n" +
            $"Reason: {recommendation.Reason}\n\n" +
            $"Expected Impact: {recommendation.ExpectedImpact}\n\n" +
            (recommendation.RequiresRestart ? "⚠️ WARNING: This change requires a SQL Server restart!\n\n" : "") +
            $"Do you want to apply this change?",
            "Apply Recommendation",
            MessageBoxButton.YesNo,
            recommendation.RequiresRestart ? MessageBoxImage.Warning : MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsLoading = true;
        StatusMessage = $"Applying recommendation for {recommendation.SettingName}...";

        try
        {
            var success = await _configService.ApplyRecommendationAsync(recommendation);

            if (success)
            {
                await _dialogService.ShowSuccessAsync(
                    "Recommendation Applied",
                    "Configuration change applied successfully!",
                    recommendation.RequiresRestart
                        ? "Note: SQL Server restart required for this change to take effect."
                        : "The change is now active.");

                // Reload data
                await LoadAllDataAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(
                    "Failed to Apply",
                    "Failed to apply the recommendation. Check logs for details.",
                    null);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Error",
                $"Error applying recommendation:\n\n{ex.Message}",
                ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CopyRecommendationScript(ConfigurationRecommendation? recommendation)
    {
        if (recommendation == null) return;

        var script = _configService.GenerateApplyScript(recommendation);
        Clipboard.SetText(script);

        StatusMessage = "Script copied to clipboard";
        MessageBox.Show(
            "The T-SQL script has been copied to your clipboard.\n\n" +
            "You can now paste and execute it in SQL Server Management Studio.",
            "Script Copied",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task ViewRecommendationDetails(ConfigurationRecommendation? recommendation)
    {
        if (recommendation == null) return;

        var content = $"Setting: {recommendation.SettingName}\n\n" +
                     $"Priority: {recommendation.Priority} | Category: {recommendation.Category}\n" +
                     $"Impact Score: {recommendation.ImpactScore}/100\n\n" +
                     $"Current Value: {recommendation.CurrentValue}\n" +
                     $"Recommended Value: {recommendation.RecommendedValue}\n\n" +
                     $"Reason:\n{recommendation.Reason}\n\n" +
                     $"Expected Impact:\n{recommendation.ExpectedImpact}\n\n" +
                     $"Details:\n{recommendation.Details}\n\n" +
                     $"Requires Restart: {(recommendation.RequiresRestart ? "Yes" : "No")}";

        var script = _configService.GenerateApplyScript(recommendation);

        await _dialogService.ShowDetailsAsync(
            $"💡 {recommendation.SettingName}",
            content,
            script,
            GetPriorityIcon(recommendation.Priority));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAllDataAsync();
    }

    private void UpdateMemoryDisplays()
    {
        if (MemoryInfo != null)
        {
            TotalMemoryDisplay = $"{MemoryInfo.TotalPhysicalMemoryMB:N0} MB";
            SqlMemoryDisplay = $"{MemoryInfo.SqlServerCommittedMB:N0} MB";
            MaxMemoryDisplay = $"{MemoryInfo.MaxServerMemoryMB:N0} MB";
            RecommendedMemoryDisplay = $"{MemoryInfo.RecommendedMaxServerMemoryMB:N0} MB";
        }
        else
        {
            TotalMemoryDisplay = "-";
            SqlMemoryDisplay = "-";
            MaxMemoryDisplay = "-";
            RecommendedMemoryDisplay = "-";
        }
    }

    private string GetPriorityIcon(RecommendationPriority priority)
    {
        return priority switch
        {
            RecommendationPriority.Critical => "🔴",
            RecommendationPriority.High => "🟠",
            RecommendationPriority.Medium => "🟡",
            RecommendationPriority.Low => "🔵",
            _ => "⚪"
        };
    }
}
