using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.App.ViewModels;

public partial class DatabaseHealthViewModel : ObservableObject
{
    private readonly IDatabaseStatsService _databaseStats;

    [ObservableProperty]
    private DatabaseMetric? currentMetrics;

    [ObservableProperty]
    private ObservableCollection<TableMetric> topTables = new();

    [ObservableProperty]
    private ObservableCollection<IndexFragmentation> fragmentedIndexes = new();

    [ObservableProperty]
    private ObservableCollection<MissingIndex> missingIndexes = new();

    [ObservableProperty]
    private bool isLoading;

    public DatabaseHealthViewModel(IDatabaseStatsService databaseStats)
    {
        _databaseStats = databaseStats;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            CurrentMetrics = await _databaseStats.GetDatabaseMetricsAsync();

            var tables = await _databaseStats.GetTopTablesBySize(20);
            TopTables.Clear();
            foreach (var table in tables)
            {
                TopTables.Add(table);
            }

            var fragmented = await _databaseStats.GetFragmentedIndexesAsync(30);
            FragmentedIndexes.Clear();
            foreach (var index in fragmented)
            {
                FragmentedIndexes.Add(index);
            }

            var missing = await _databaseStats.GetMissingIndexesAsync();
            MissingIndexes.Clear();
            foreach (var index in missing)
            {
                MissingIndexes.Add(index);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }
}

