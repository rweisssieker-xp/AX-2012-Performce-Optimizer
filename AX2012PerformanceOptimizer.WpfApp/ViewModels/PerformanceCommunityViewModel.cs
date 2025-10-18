using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class PerformanceCommunityViewModel : ObservableObject
{
    private readonly IPerformanceCommunityService _community;

    [ObservableProperty]
    private string industryType = "Manufacturing";

    [ObservableProperty]
    private string userCountRange = "100-500";

    [ObservableProperty]
    private string region = "EMEA";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Configure your profile to get benchmarks";

    [ObservableProperty]
    private BenchmarkReport? currentReport;

    [ObservableProperty]
    private ObservableCollection<BestPractice> bestPractices = new();

    public PerformanceCommunityViewModel(IPerformanceCommunityService community)
    {
        _community = community;
    }

    [RelayCommand]
    private async Task GetBenchmarkAsync()
    {
        IsLoading = true;
        StatusMessage = "🌍 Fetching industry benchmarks...";

        try
        {
            var profile = new BenchmarkProfile
            {
                IndustryType = IndustryType,
                UserCountRange = UserCountRange,
                Region = Region
            };

            CurrentReport = await _community.GetIndustryBenchmarkAsync(profile);
            
            BestPractices.Clear();
            foreach (var practice in CurrentReport.BestPractices)
            {
                BestPractices.Add(practice);
            }

            StatusMessage = $"✅ Benchmark loaded ({CurrentReport.PeerCount} peers)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
