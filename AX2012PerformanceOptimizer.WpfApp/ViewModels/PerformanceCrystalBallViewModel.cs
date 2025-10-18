using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class PerformanceCrystalBallViewModel : ObservableObject
{
    private readonly IPerformanceCrystalBallService _crystalBall;

    [ObservableProperty]
    private ObservableCollection<BusinessScenario> predefinedScenarios = new();

    [ObservableProperty]
    private BusinessScenario? selectedScenario;

    [ObservableProperty]
    private bool isPredicting;

    [ObservableProperty]
    private string statusMessage = "Select a scenario to predict future performance";

    [ObservableProperty]
    private ScenarioForecast? currentForecast;

    public PerformanceCrystalBallViewModel(IPerformanceCrystalBallService crystalBall)
    {
        _crystalBall = crystalBall;
        _ = LoadScenariosAsync();
    }

    private async Task LoadScenariosAsync()
    {
        try
        {
            var scenarios = await _crystalBall.GetPredefinedScenariosAsync();
            foreach (var scenario in scenarios)
            {
                PredefinedScenarios.Add(scenario);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading scenarios: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PredictScenarioAsync()
    {
        if (SelectedScenario == null) return;

        IsPredicting = true;
        StatusMessage = $"🔮 Predicting scenario: {SelectedScenario.Name}...";

        try
        {
            CurrentForecast = await _crystalBall.PredictScenarioAsync(SelectedScenario);
            StatusMessage = $"✅ Forecast complete! Confidence: {CurrentForecast.ConfidenceScore:P0}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsPredicting = false;
        }
    }
}
