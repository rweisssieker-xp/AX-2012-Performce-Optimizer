using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class PerformancePersonasViewModel : ObservableObject
{
    private readonly IPerformancePersonaService _personaService;

    [ObservableProperty]
    private ObservableCollection<PerformancePersona> availablePersonas = new();

    [ObservableProperty]
    private PerformancePersona? selectedPersona;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private string statusMessage = "Select an expert to get advice";

    [ObservableProperty]
    private ExpertRecommendation? currentAdvice;

    [ObservableProperty]
    private ConsensusRecommendation? consensus;

    public PerformancePersonasViewModel(IPerformancePersonaService personaService)
    {
        _personaService = personaService;
        _ = LoadPersonasAsync();
    }

    private async Task LoadPersonasAsync()
    {
        try
        {
            var personas = await _personaService.GetAvailablePersonasAsync();
            foreach (var persona in personas)
            {
                AvailablePersonas.Add(persona);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading personas: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GetExpertAdviceAsync()
    {
        if (SelectedPersona == null) return;

        IsAnalyzing = true;
        StatusMessage = $"🎭 Consulting {SelectedPersona.ExpertName}...";

        try
        {
            var problem = new PerformanceProblem
            {
                Severity = "High",
                Icon = "⚡",
                Title = "Performance Optimization Required",
                Explanation = "General performance optimization needed",
                Impact = "System performance degraded",
                Why = "Multiple optimization opportunities identified"
            };

            CurrentAdvice = await _personaService.GetExpertAdviceAsync(SelectedPersona.Id, problem);
            StatusMessage = $"✅ Advice received from {CurrentAdvice.ExpertName} (Confidence: {CurrentAdvice.Confidence:P0})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task GetConsensusAdviceAsync()
    {
        IsAnalyzing = true;
        StatusMessage = "🎭 Consulting all experts for consensus...";

        try
        {
            var problem = new PerformanceProblem
            {
                Severity = "Critical",
                Icon = "🔍",
                Title = "Comprehensive Performance Review",
                Explanation = "Complete system performance analysis required",
                Impact = "Overall system health assessment needed",
                Why = "Regular performance review for optimization"
            };

            Consensus = await _personaService.GetConsensusAdviceAsync(problem);
            StatusMessage = $"✅ Consensus reached ({Consensus.AgreementScore:P0} agreement)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }
}
