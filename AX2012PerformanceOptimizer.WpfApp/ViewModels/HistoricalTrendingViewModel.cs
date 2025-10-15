using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class HistoricalTrendingViewModel : ObservableObject
{
    private readonly IHistoricalDataService _historyService;
    private readonly IDataCollectionService _dataCollectionService;

    [ObservableProperty]
    private ObservableCollection<ISeries> activeUsersSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> databaseSizeSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> batchJobsSeries = new();

    [ObservableProperty]
    private string selectedTimeRange = "24h";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isCollecting;

    [ObservableProperty]
    private string collectionStatus = "Not collecting";

    [ObservableProperty]
    private TrendAnalysis? activeUsersTrend;

    [ObservableProperty]
    private TrendAnalysis? databaseSizeTrend;

    public HistoricalTrendingViewModel(
        IHistoricalDataService historyService,
        IDataCollectionService dataCollectionService)
    {
        _historyService = historyService;
        _dataCollectionService = dataCollectionService;

        IsCollecting = _dataCollectionService.IsCollecting;
        UpdateCollectionStatus();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var (from, to) = GetTimeRange();

            // Load dashboard history
            var dashboardHistory = await _historyService.GetDashboardHistoryAsync(from, to);

            if (dashboardHistory.Count > 0)
            {
                // Active Users Chart
                ActiveUsersSeries = new ObservableCollection<ISeries>
                {
                    new LineSeries<int>
                    {
                        Name = "Active Users",
                        Values = dashboardHistory.Select(h => h.ActiveUsers).ToArray(),
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 }
                    }
                };

                // Database Size Chart
                var dbHistory = await _historyService.GetDatabaseSizeHistoryAsync(from, to);
                DatabaseSizeSeries = new ObservableCollection<ISeries>
                {
                    new LineSeries<long>
                    {
                        Name = "Total Size (MB)",
                        Values = dbHistory.Select(h => h.TotalSizeMB).ToArray(),
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 2 }
                    }
                };

                // Batch Jobs Chart
                var batchHistory = await _historyService.GetBatchJobHistoryAsync(from, to);
                BatchJobsSeries = new ObservableCollection<ISeries>
                {
                    new LineSeries<int>
                    {
                        Name = "Running Jobs",
                        Values = batchHistory.Select(h => h.RunningCount).ToArray(),
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 }
                    },
                    new LineSeries<int>
                    {
                        Name = "Failed Jobs",
                        Values = batchHistory.Select(h => h.FailedCount).ToArray(),
                        Fill = null,
                        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 }
                    }
                };

                // Load trend analyses
                ActiveUsersTrend = await _historyService.AnalyzeTrendAsync("ActiveUsers", from, to);
                DatabaseSizeTrend = await _historyService.AnalyzeTrendAsync("DatabaseSize", from, to);
            }
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
    private async Task StartCollectionAsync()
    {
        await _dataCollectionService.StartCollectionAsync(15); // Every 15 minutes
        IsCollecting = true;
        UpdateCollectionStatus();
    }

    [RelayCommand]
    private async Task StopCollectionAsync()
    {
        await _dataCollectionService.StopCollectionAsync();
        IsCollecting = false;
        UpdateCollectionStatus();
    }

    [RelayCommand]
    private async Task ChangeTimeRangeAsync(string range)
    {
        SelectedTimeRange = range;
        await LoadDataAsync();
    }

    private (DateTime from, DateTime to) GetTimeRange()
    {
        var to = DateTime.UtcNow;
        var from = SelectedTimeRange switch
        {
            "24h" => to.AddHours(-24),
            "7d" => to.AddDays(-7),
            "30d" => to.AddDays(-30),
            "90d" => to.AddDays(-90),
            _ => to.AddHours(-24)
        };
        return (from, to);
    }

    private void UpdateCollectionStatus()
    {
        if (IsCollecting)
        {
            var lastTime = _dataCollectionService.LastCollectionTime;
            CollectionStatus = lastTime.HasValue
                ? $"Collecting (Last: {lastTime.Value.ToLocalTime():HH:mm:ss})"
                : "Collecting...";
        }
        else
        {
            CollectionStatus = "Not collecting";
        }
    }
}
