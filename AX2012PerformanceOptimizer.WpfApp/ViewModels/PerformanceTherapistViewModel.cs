using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services.PerformanceTherapist;
using AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class PerformanceTherapistViewModel : ObservableObject
{
    private readonly IPerformanceTherapistService _therapistService;

    [ObservableProperty] private string currentSessionId = string.Empty;
    [ObservableProperty] private string sessionTitle = "No Active Session";
    [ObservableProperty] private string currentPhase = "Ready";
    [ObservableProperty] private string userInput = string.Empty;
    [ObservableProperty] private bool isSessionActive;
    [ObservableProperty] private bool isProcessing;
    [ObservableProperty] private bool showDiagnosis;
    [ObservableProperty] private string statusMessage = "Ready to start therapy session...";
    [ObservableProperty] private int messageCount;

    // Collections
    [ObservableProperty] private ObservableCollection<TherapyMessage> messages = new();
    [ObservableProperty] private ObservableCollection<string> suggestedQuestions = new();
    [ObservableProperty] private ObservableCollection<SystemSymptom> detectedSymptoms = new();

    // Diagnosis Result
    [ObservableProperty] private string diagnosisTitle = string.Empty;
    [ObservableProperty] private string diagnosisSummary = string.Empty;
    [ObservableProperty] private double diagnosisConfidence;
    [ObservableProperty] private string diagnosisPrognosis = string.Empty;
    [ObservableProperty] private ObservableCollection<SystemSymptom> diagnosisSymptoms = new();
    [ObservableProperty] private ObservableCollection<string> rootCauses = new();
    [ObservableProperty] private ObservableCollection<string> immediateActions = new();
    [ObservableProperty] private ObservableCollection<string> longTermRecommendations = new();
    [ObservableProperty] private ObservableCollection<string> preventiveMeasures = new();

    public PerformanceTherapistViewModel(IPerformanceTherapistService therapistService)
    {
        _therapistService = therapistService;
        LoadInitialSuggestions();
    }

    private void LoadInitialSuggestions()
    {
        SuggestedQuestions = new ObservableCollection<string>
        {
            "Queries are running slowly",
            "Batch jobs taking too long",
            "Users reporting system lag",
            "Database is growing rapidly"
        };
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        if (IsSessionActive)
        {
            var result = MessageBox.Show("End current session and start a new one?",
                "Active Session", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;
        }

        IsProcessing = true;
        StatusMessage = "Starting therapy session...";
        ShowDiagnosis = false;

        try
        {
            var response = await _therapistService.StartSessionAsync(UserInput);

            if (response.Success && response.Session != null)
            {
                CurrentSessionId = response.Session.Id;
                SessionTitle = response.Session.Title;
                CurrentPhase = response.Session.CurrentPhase;
                IsSessionActive = true;

                Messages.Clear();
                foreach (var msg in response.Session.Messages)
                    Messages.Add(msg);

                SuggestedQuestions.Clear();
                foreach (var q in response.SuggestedQuestions)
                    SuggestedQuestions.Add(q);

                DetectedSymptoms.Clear();
                foreach (var symptom in response.Session.DetectedSymptoms)
                    DetectedSymptoms.Add(symptom);

                MessageCount = response.Session.MessageCount;
                UserInput = string.Empty;
                StatusMessage = $"Session started - Phase: {CurrentPhase}";
            }
            else
            {
                MessageBox.Show($"Failed to start session:\n\n{response.ErrorMessage}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Failed to start session";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || !IsSessionActive)
            return;

        IsProcessing = true;
        StatusMessage = "Analyzing your response...";

        try
        {
            var request = new TherapyRequest
            {
                SessionId = CurrentSessionId,
                UserMessage = UserInput,
                IncludeSystemMetrics = true,
                IncludeHistoricalData = true
            };

            var response = await _therapistService.ContinueSessionAsync(request);

            if (response.Success && response.Session != null)
            {
                CurrentPhase = response.Session.CurrentPhase;

                // Add user message
                Messages.Add(new TherapyMessage
                {
                    Role = MessageRole.User,
                    Content = UserInput,
                    Timestamp = DateTime.UtcNow
                });

                // Add therapist response
                if (response.LatestMessage != null)
                    Messages.Add(response.LatestMessage);

                // Update suggestions
                SuggestedQuestions.Clear();
                foreach (var q in response.SuggestedQuestions)
                    SuggestedQuestions.Add(q);

                // Update symptoms
                DetectedSymptoms.Clear();
                foreach (var symptom in response.Session.DetectedSymptoms)
                    DetectedSymptoms.Add(symptom);

                MessageCount = response.Session.MessageCount;
                UserInput = string.Empty;

                // Check if session is complete
                if (response.SessionComplete && response.FinalDiagnosis != null)
                {
                    ShowDiagnosisResult(response.FinalDiagnosis);
                    IsSessionActive = false;
                    StatusMessage = "Diagnosis complete!";
                }
                else
                {
                    StatusMessage = $"Phase: {CurrentPhase} - {MessageCount} messages";
                }
            }
            else
            {
                MessageBox.Show($"Failed to send message:\n\n{response.ErrorMessage}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Failed to send message";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void UseSuggestedQuestion(string question)
    {
        UserInput = question;
    }

    [RelayCommand]
    private async Task EndSessionAsync()
    {
        if (!IsSessionActive)
            return;

        var result = MessageBox.Show("End session and generate diagnosis?",
            "End Session", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.No)
            return;

        IsProcessing = true;
        StatusMessage = "Generating final diagnosis...";

        try
        {
            var diagnosis = await _therapistService.EndSessionAsync(CurrentSessionId);
            ShowDiagnosisResult(diagnosis);
            IsSessionActive = false;
            StatusMessage = "Session ended - Diagnosis complete";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void ShowDiagnosisResult(DiagnosisResult diagnosis)
    {
        DiagnosisTitle = diagnosis.Title;
        DiagnosisSummary = diagnosis.Summary;
        DiagnosisConfidence = diagnosis.ConfidenceScore;
        DiagnosisPrognosis = diagnosis.Prognosis;

        DiagnosisSymptoms.Clear();
        foreach (var symptom in diagnosis.Symptoms)
            DiagnosisSymptoms.Add(symptom);

        RootCauses.Clear();
        foreach (var cause in diagnosis.RootCauses)
            RootCauses.Add(cause);

        ImmediateActions.Clear();
        foreach (var action in diagnosis.ImmediateActions)
            ImmediateActions.Add(action);

        LongTermRecommendations.Clear();
        foreach (var rec in diagnosis.LongTermRecommendations)
            LongTermRecommendations.Add(rec);

        PreventiveMeasures.Clear();
        foreach (var measure in diagnosis.PreventiveMeasures)
            PreventiveMeasures.Add(measure);

        ShowDiagnosis = true;
    }

    [RelayCommand]
    private void ClearDiagnosis()
    {
        ShowDiagnosis = false;
        DiagnosisSymptoms.Clear();
        RootCauses.Clear();
        ImmediateActions.Clear();
        LongTermRecommendations.Clear();
        PreventiveMeasures.Clear();
    }

    [RelayCommand]
    private async Task ExportDiagnosisAsync()
    {
        if (!ShowDiagnosis)
            return;

        try
        {
            var fileName = $"Diagnosis_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            var content = new System.Text.StringBuilder();
            content.AppendLine($"# {DiagnosisTitle}");
            content.AppendLine();
            content.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            content.AppendLine($"Confidence: {DiagnosisConfidence:F1}%");
            content.AppendLine();
            content.AppendLine("## Summary");
            content.AppendLine(DiagnosisSummary);
            content.AppendLine();
            content.AppendLine("## Root Causes");
            foreach (var cause in RootCauses)
                content.AppendLine($"- {cause}");
            content.AppendLine();
            content.AppendLine("## Immediate Actions");
            foreach (var action in ImmediateActions)
                content.AppendLine($"- {action}");
            content.AppendLine();
            content.AppendLine("## Long-term Recommendations");
            foreach (var rec in LongTermRecommendations)
                content.AppendLine($"- {rec}");
            content.AppendLine();
            content.AppendLine("## Preventive Measures");
            foreach (var measure in PreventiveMeasures)
                content.AppendLine($"- {measure}");
            content.AppendLine();
            content.AppendLine("## Prognosis");
            content.AppendLine(DiagnosisPrognosis);

            await System.IO.File.WriteAllTextAsync(filePath, content.ToString());

            MessageBox.Show($"Diagnosis exported!\n\nFile: {filePath}",
                "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = "Diagnosis exported";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed:\n\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ClearSession()
    {
        Messages.Clear();
        DetectedSymptoms.Clear();
        SuggestedQuestions.Clear();
        IsSessionActive = false;
        CurrentSessionId = string.Empty;
        SessionTitle = "No Active Session";
        CurrentPhase = "Ready";
        MessageCount = 0;
        ShowDiagnosis = false;
        UserInput = string.Empty;
        StatusMessage = "Ready to start therapy session...";
        LoadInitialSuggestions();
    }
}
