using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.App.ViewModels;

public partial class SqlPerformanceViewModel : ObservableObject
{
    private readonly ISqlQueryMonitorService _sqlMonitor;

    [ObservableProperty]
    private ObservableCollection<SqlQueryMetric> expensiveQueries = new();

    [ObservableProperty]
    private SqlQueryMetric? selectedQuery;

    [ObservableProperty]
    private bool isLoading;

    public SqlPerformanceViewModel(ISqlQueryMonitorService sqlMonitor)
    {
        _sqlMonitor = sqlMonitor;
    }

    [RelayCommand]
    private async Task LoadQueriesAsync()
    {
        IsLoading = true;

        try
        {
            var queries = await _sqlMonitor.GetTopExpensiveQueriesAsync(50);
            ExpensiveQueries.Clear();
            foreach (var query in queries)
            {
                ExpensiveQueries.Add(query);
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
        await LoadQueriesAsync();
    }
}

