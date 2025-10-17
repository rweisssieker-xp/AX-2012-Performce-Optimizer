using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using System.Collections.ObjectModel;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class NaturalLanguageAssistantViewModel : ObservableObject
{
    private readonly INaturalLanguageQueryAssistant _assistant;
    private readonly ISqlConnectionManager _connectionManager;
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private string userQuery = string.Empty;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private bool hasConversation;

    [ObservableProperty]
    private ObservableCollection<ConversationMessage> conversationHistory = new();

    [ObservableProperty]
    private ObservableCollection<string> suggestedQuestions = new();

    [ObservableProperty]
    private string statusMessage = "Bereit für Ihre Frage...";

    [ObservableProperty]
    private bool isDatabaseConnected;

    [ObservableProperty]
    private string connectionWarning = string.Empty;

    public NaturalLanguageAssistantViewModel(
        INaturalLanguageQueryAssistant assistant,
        ISqlConnectionManager connectionManager)
    {
        _assistant = assistant;
        _connectionManager = connectionManager;
        _connectionManager.ConnectionChanged += OnConnectionChanged;
        InitializeAsync();
    }

    private void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        IsDatabaseConnected = e.IsConnected;

        if (e.IsConnected)
        {
            // Connection established - exit demo mode
            ConnectionWarning = string.Empty;
            StatusMessage = "Verbunden - Bereit für echte Daten";

            // Add notification to conversation
            ConversationHistory.Add(new ConversationMessage
            {
                Role = "System",
                Content = "✅ Datenbankverbindung hergestellt!\n\nSie können jetzt echte Performance-Daten abfragen.",
                Timestamp = DateTime.Now,
                IsUser = false
            });
        }
        else
        {
            // Connection lost - enter demo mode
            ConnectionWarning = "⚠️ Demo-Modus: Bitte verbinden Sie sich mit einer Datenbank für echte Daten";
            StatusMessage = "Demo-Modus aktiv - Beispiel-Daten werden verwendet";

            // Add notification to conversation
            ConversationHistory.Add(new ConversationMessage
            {
                Role = "System",
                Content = "⚠️ Datenbankverbindung getrennt!\n\nDemo-Modus aktiv - Es werden Beispiel-Daten angezeigt.",
                Timestamp = DateTime.Now,
                IsUser = false
            });
        }
    }

    private async void InitializeAsync()
    {
        _sessionId = await _assistant.StartNewSessionAsync();

        // Check connection status
        IsDatabaseConnected = _connectionManager.IsConnected;

        // Build welcome message based on connection status
        var welcomeMessage = new System.Text.StringBuilder();
        welcomeMessage.AppendLine("👋 Willkommen beim AI Performance Assistant!");
        welcomeMessage.AppendLine();

        if (!IsDatabaseConnected)
        {
            welcomeMessage.AppendLine("⚠️ HINWEIS: Keine Datenbankverbindung gefunden!");
            welcomeMessage.AppendLine("   → Gehen Sie zu 'Settings' und klicken Sie auf 'Connect'");
            welcomeMessage.AppendLine("   → Demo-Modus aktiv: Ich zeige Ihnen Beispiel-Daten");
            welcomeMessage.AppendLine();
            ConnectionWarning = "⚠️ Demo-Modus: Bitte verbinden Sie sich mit einer Datenbank für echte Daten";
            StatusMessage = "Demo-Modus aktiv - Beispiel-Daten werden verwendet";
        }
        else
        {
            ConnectionWarning = string.Empty;
            StatusMessage = "Bereit für Ihre Frage...";
        }

        welcomeMessage.AppendLine("Ich kann Ihnen bei folgenden Themen helfen:");
        welcomeMessage.AppendLine("• Performance-Probleme analysieren");
        welcomeMessage.AppendLine("• Kostenanalysen durchführen");
        welcomeMessage.AppendLine("• Optimierungsempfehlungen geben");
        welcomeMessage.AppendLine("• Batch-Job Performance");
        welcomeMessage.AppendLine("• Query-Analysen");
        welcomeMessage.AppendLine("• Trends vorhersagen");
        welcomeMessage.AppendLine();
        welcomeMessage.AppendLine("Stellen Sie mir einfach eine Frage in natürlicher Sprache!");

        // Add welcome message
        ConversationHistory.Add(new ConversationMessage
        {
            Role = "Assistant",
            Content = welcomeMessage.ToString(),
            Timestamp = DateTime.Now,
            IsUser = false
        });

        // Add initial suggested questions
        SuggestedQuestions.Add("Zeig mir die langsamsten Queries heute");
        SuggestedQuestions.Add("Was kostet mich die Performance?");
        SuggestedQuestions.Add("Welche Optimierungen empfiehlst du?");

        HasConversation = true;
    }

    [RelayCommand]
    private async Task SendQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(UserQuery))
            return;

        var query = UserQuery.Trim();
        UserQuery = string.Empty;
        IsProcessing = true;
        StatusMessage = "Verarbeite Ihre Anfrage...";

        try
        {
            // Add user message to conversation
            ConversationHistory.Add(new ConversationMessage
            {
                Role = "User",
                Content = query,
                Timestamp = DateTime.Now,
                IsUser = true
            });

            // Build context
            var context = new NLQueryContext
            {
                SessionId = _sessionId,
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now,
                ConversationHistory = new List<NLConversationMessage>()
            };

            // Process query
            var response = await _assistant.ProcessQueryAsync(query, context);

            // Add assistant response
            var assistantMessage = new ConversationMessage
            {
                Role = "Assistant",
                Content = response.TextResponse,
                Timestamp = DateTime.Now,
                IsUser = false,
                IntentDetected = response.IntentDetected,
                ConfidenceScore = response.ConfidenceScore,
                ProcessingTimeMs = response.ProcessingTimeMs
            };

            // Add query results if available
            if (response.QueryResults != null && response.QueryResults.Any())
            {
                assistantMessage.QueryResults = response.QueryResults;
                assistantMessage.HasQueryResults = true;
            }

            // Add insights if available
            if (response.Insights != null && response.Insights.Any())
            {
                assistantMessage.Insights = response.Insights;
                assistantMessage.HasInsights = true;
            }

            ConversationHistory.Add(assistantMessage);

            // Update suggested questions
            SuggestedQuestions.Clear();
            foreach (var suggestion in response.SuggestedQuestions)
            {
                SuggestedQuestions.Add(suggestion);
            }

            StatusMessage = $"Antwort erhalten (Vertrauen: {response.ConfidenceScore:F0}%, {response.ProcessingTimeMs:F0}ms)";
            HasConversation = true;
        }
        catch (Exception ex)
        {
            ConversationHistory.Add(new ConversationMessage
            {
                Role = "System",
                Content = $"❌ Fehler: {ex.Message}",
                Timestamp = DateTime.Now,
                IsUser = false,
                IsError = true
            });
            StatusMessage = "Fehler bei der Verarbeitung";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void UseSuggestedQuestion(string question)
    {
        if (!string.IsNullOrEmpty(question))
        {
            UserQuery = question;
        }
    }

    [RelayCommand]
    private async Task ClearConversationAsync()
    {
        var result = MessageBox.Show(
            "Möchten Sie die Konversation wirklich löschen?",
            "Konversation löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await _assistant.ClearSessionAsync(_sessionId);
            ConversationHistory.Clear();
            _sessionId = await _assistant.StartNewSessionAsync();
            StatusMessage = "Konversation gelöscht";
            HasConversation = false;

            // Re-initialize
            InitializeAsync();
        }
    }

    [RelayCommand]
    private void CopyMessageContent(string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            Clipboard.SetText(content);
            StatusMessage = "In Zwischenablage kopiert";
        }
    }

    [RelayCommand]
    private async Task ExportConversationAsync()
    {
        try
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("==============================================");
            content.AppendLine("Natural Language Query Assistant - Conversation");
            content.AppendLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            content.AppendLine($"Session: {_sessionId}");
            content.AppendLine("==============================================\n");

            foreach (var msg in ConversationHistory)
            {
                content.AppendLine($"[{msg.Timestamp:HH:mm:ss}] {msg.Role}:");
                content.AppendLine(msg.Content);

                if (!string.IsNullOrEmpty(msg.IntentDetected))
                {
                    content.AppendLine($"   Intent: {msg.IntentDetected} (Confidence: {msg.ConfidenceScore:F0}%)");
                }

                content.AppendLine();
            }

            var fileName = $"NL_Conversation_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            await System.IO.File.WriteAllTextAsync(filePath, content.ToString());

            MessageBox.Show(
                $"Konversation exportiert!\n\nDatei: {filePath}",
                "Export erfolgreich",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusMessage = "Konversation exportiert";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Export fehlgeschlagen: {ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

/// <summary>
/// Conversation message for UI display
/// </summary>
public class ConversationMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsUser { get; set; }
    public bool IsError { get; set; }

    // AI Analysis Data
    public string? IntentDetected { get; set; }
    public double ConfidenceScore { get; set; }
    public double ProcessingTimeMs { get; set; }

    // Rich content
    public List<SqlQueryMetric>? QueryResults { get; set; }
    public bool HasQueryResults { get; set; }

    public List<PerformanceInsight>? Insights { get; set; }
    public bool HasInsights { get; set; }
}
