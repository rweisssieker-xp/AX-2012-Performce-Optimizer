using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Time-travel debugging for performance problems - capture and replay
/// </summary>
public interface IPerformanceTimeMachineService
{
    Task<PerformanceSnapshot> CaptureSnapshotAsync(string description = "");
    Task<List<PerformanceSnapshot>> GetSnapshotHistoryAsync(DateTime from, DateTime to);
    Task<PerformanceSnapshot> LoadSnapshotAsync(DateTime timestamp);
    Task<ReplayAnalysis> AnalyzeProblemAsync(DateTime problemTime);
}
