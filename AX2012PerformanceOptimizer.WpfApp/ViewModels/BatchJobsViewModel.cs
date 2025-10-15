using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class BatchJobsViewModel : ObservableObject
{
    private readonly IBatchJobMonitorService _batchJobMonitor;

    [ObservableProperty]
    private ObservableCollection<BatchJobMetric> runningJobs = new();

    [ObservableProperty]
    private ObservableCollection<BatchJobMetric> failedJobs = new();

    [ObservableProperty]
    private bool isLoading;

    public BatchJobsViewModel(IBatchJobMonitorService batchJobMonitor)
    {
        _batchJobMonitor = batchJobMonitor;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var running = await _batchJobMonitor.GetRunningBatchJobsAsync();
            RunningJobs.Clear();
            foreach (var job in running)
            {
                RunningJobs.Add(job);
            }

            var failed = await _batchJobMonitor.GetFailedBatchJobsAsync();
            FailedJobs.Clear();
            foreach (var job in failed)
            {
                FailedJobs.Add(job);
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
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }
}


