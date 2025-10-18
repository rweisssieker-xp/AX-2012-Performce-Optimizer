using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class PerformanceTimeMachineViewModel : ObservableObject
{
    private readonly IPerformanceTimeMachineService _timeMachine;

    [ObservableProperty]
    private ObservableCollection<PerformanceSnapshot> snapshots = new();

    [ObservableProperty]
    private PerformanceSnapshot? selectedSnapshot;

    [ObservableProperty]
    private bool isCapturing;

    [ObservableProperty]
    private string statusMessage = "Ready to capture or analyze snapshots";

    [ObservableProperty]
    private ReplayAnalysis? currentAnalysis;

    [ObservableProperty]
    private string snapshotDescription = string.Empty;

    public PerformanceTimeMachineViewModel(IPerformanceTimeMachineService timeMachine)
    {
        _timeMachine = timeMachine;
        _ = LoadSnapshotsAsync();
    }

    private async Task LoadSnapshotsAsync()
    {
        try
        {
            var from = DateTime.UtcNow.AddDays(-7);
            var to = DateTime.UtcNow;
            var history = await _timeMachine.GetSnapshotHistoryAsync(from, to);
            
            Snapshots.Clear();
            foreach (var snap in history)
            {
                Snapshots.Add(snap);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading snapshots: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CaptureSnapshotAsync()
    {
        IsCapturing = true;
        StatusMessage = "⏰ Capturing performance snapshot...";

        try
        {
            var snapshot = await _timeMachine.CaptureSnapshotAsync(SnapshotDescription);
            Snapshots.Insert(0, snapshot);
            StatusMessage = $"✅ Snapshot captured at {snapshot.Timestamp:HH:mm:ss}";
            SnapshotDescription = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsCapturing = false;
        }
    }

    [RelayCommand]
    private async Task AnalyzeSnapshotAsync()
    {
        if (SelectedSnapshot == null) return;

        IsCapturing = true;
        StatusMessage = "🔍 Analyzing snapshot...";

        try
        {
            CurrentAnalysis = await _timeMachine.AnalyzeProblemAsync(SelectedSnapshot.Timestamp);
            StatusMessage = $"✅ Analysis complete (Confidence: {CurrentAnalysis.ConfidenceScore:P0})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsCapturing = false;
        }
    }
}
