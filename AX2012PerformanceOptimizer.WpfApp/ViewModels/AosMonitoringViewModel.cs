using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class AosMonitoringViewModel : ObservableObject
{
    private readonly IAosMonitorService _aosMonitor;

    [ObservableProperty]
    private AosMetric? currentMetrics;

    [ObservableProperty]
    private ObservableCollection<UserSession> userSessions = new();

    [ObservableProperty]
    private bool isLoading;

    public AosMonitoringViewModel(IAosMonitorService aosMonitor)
    {
        _aosMonitor = aosMonitor;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            CurrentMetrics = await _aosMonitor.GetAosMetricsAsync();
            
            var sessions = await _aosMonitor.GetActiveUserSessionsAsync();
            UserSessions.Clear();
            foreach (var session in sessions)
            {
                UserSessions.Add(session);
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


