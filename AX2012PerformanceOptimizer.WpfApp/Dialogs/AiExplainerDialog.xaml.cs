using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Core.Services;

namespace AX2012PerformanceOptimizer.WpfApp.Dialogs;

public partial class AiExplainerDialog : Window, INotifyPropertyChanged
{
    private readonly IAiQueryExplainerService _explainerService;
    private readonly SqlQueryMetric _query;
    private QueryExplanation? _explanation;

    public AiExplainerDialog(IAiQueryExplainerService explainerService, SqlQueryMetric query)
    {
        InitializeComponent();
        DataContext = this;

        _explainerService = explainerService;
        _query = query;

        LoadExplanationAsync();
    }

    #region Properties

    private string _summary = "Analyzing query...";
    public string Summary
    {
        get => _summary;
        set
        {
            _summary = value;
            OnPropertyChanged();
        }
    }

    private int _performanceScore = 50;
    public int PerformanceScore
    {
        get => _performanceScore;
        set
        {
            _performanceScore = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreColor));
        }
    }

    private string _performanceRating = "Analyzing...";
    public string PerformanceRating
    {
        get => _performanceRating;
        set
        {
            _performanceRating = value;
            OnPropertyChanged();
        }
    }

    public SolidColorBrush ScoreColor
    {
        get
        {
            return PerformanceScore switch
            {
                >= 80 => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                >= 60 => new SolidColorBrush(Color.FromRgb(139, 195, 74)), // Light Green
                >= 40 => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                >= 20 => new SolidColorBrush(Color.FromRgb(255, 87, 34)), // Deep Orange
                _ => new SolidColorBrush(Color.FromRgb(244, 67, 54)) // Red
            };
        }
    }

    public ObservableCollection<PerformanceProblem> Problems { get; set; } = new();

    public ObservableCollection<UIRecommendation> Recommendations { get; set; } = new();

    public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new();

    public ObservableCollection<string> SuggestedQuestions { get; set; } = new();

    private bool _hasNoProblems;
    public bool HasNoProblems
    {
        get => _hasNoProblems;
        set
        {
            _hasNoProblems = value;
            OnPropertyChanged();
        }
    }

    private int _estimatedTotalImprovement;
    public int EstimatedTotalImprovement
    {
        get => _estimatedTotalImprovement;
        set
        {
            _estimatedTotalImprovement = value;
            OnPropertyChanged();
        }
    }

    private string _roiSummary = "";
    public string ROISummary
    {
        get => _roiSummary;
        set
        {
            _roiSummary = value;
            OnPropertyChanged();
        }
    }

    private string _userQuestion = "";
    public string UserQuestion
    {
        get => _userQuestion;
        set
        {
            _userQuestion = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAskQuestion));
        }
    }

    public bool CanAskQuestion => !string.IsNullOrWhiteSpace(UserQuestion);

    #endregion

    private async void LoadExplanationAsync()
    {
        try
        {
            _explanation = await _explainerService.ExplainQueryPerformanceAsync(_query);

            // Update UI
            Summary = _explanation.Summary;
            PerformanceScore = _explanation.PerformanceScore;
            PerformanceRating = _explanation.PerformanceRating;
            EstimatedTotalImprovement = _explanation.EstimatedTotalImprovement;
            ROISummary = _explanation.ROISummary;

            // Load problems
            Problems.Clear();
            foreach (var problem in _explanation.Problems)
            {
                Problems.Add(problem);
            }
            HasNoProblems = Problems.Count == 0;

            // Load recommendations
            Recommendations.Clear();
            foreach (var recommendation in _explanation.Recommendations)
            {
                Recommendations.Add(new UIRecommendation(recommendation));
            }

            // Load suggested questions
            var questions = await _explainerService.GetSuggestedQuestionsAsync(_query);
            foreach (var question in questions)
            {
                SuggestedQuestions.Add(question);
            }

            // Welcome message
            ChatMessages.Add(new ChatMessage
            {
                Sender = "AI Assistant",
                Message = "Hi! I've analyzed your query. Feel free to ask me any questions about it!",
                BackgroundColor = new SolidColorBrush(Color.FromRgb(227, 242, 253)), // Light blue
                SenderColor = new SolidColorBrush(Color.FromRgb(25, 118, 210))
            });
        }
        catch (Exception ex)
        {
            Summary = "Error analyzing query";
            MessageBox.Show($"Error generating explanation:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void AskQuestion_Click(object sender, RoutedEventArgs e)
    {
        await AskQuestionAsync(UserQuestion);
    }

    private async void SuggestedQuestion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is string question)
        {
            await AskQuestionAsync(question);
        }
    }

    private async Task AskQuestionAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return;

        // Add user message
        ChatMessages.Add(new ChatMessage
        {
            Sender = "You",
            Message = question,
            BackgroundColor = new SolidColorBrush(Color.FromRgb(232, 234, 237)), // Gray
            SenderColor = new SolidColorBrush(Color.FromRgb(97, 97, 97))
        });

        // Clear input
        UserQuestion = "";

        // Scroll to bottom
        ChatScrollViewer.ScrollToEnd();

        try
        {
            // Get AI response
            var response = await _explainerService.AskQuestionAsync(question, _query);

            // Add AI response
            ChatMessages.Add(new ChatMessage
            {
                Sender = "AI Assistant",
                Message = response.Answer,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(227, 242, 253)), // Light blue
                SenderColor = new SolidColorBrush(Color.FromRgb(25, 118, 210))
            });

            // Update suggested questions with follow-ups
            if (response.FollowUpSuggestions.Any())
            {
                SuggestedQuestions.Clear();
                foreach (var followUp in response.FollowUpSuggestions)
                {
                    SuggestedQuestions.Add(followUp);
                }
            }

            // Scroll to bottom
            ChatScrollViewer.ScrollToEnd();
        }
        catch (Exception ex)
        {
            ChatMessages.Add(new ChatMessage
            {
                Sender = "System",
                Message = $"Error: {ex.Message}",
                BackgroundColor = new SolidColorBrush(Color.FromRgb(255, 235, 238)), // Light red
                SenderColor = new SolidColorBrush(Color.FromRgb(198, 40, 40))
            });
        }
    }

    private void QuestionTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && CanAskQuestion)
        {
            AskQuestionAsync(UserQuestion);
            e.Handled = true;
        }
    }

    private void CopyScript_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string script)
        {
            try
            {
                Clipboard.SetText(script);
                MessageBox.Show("Script copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy script:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

// Helper class for chat messages
public class ChatMessage
{
    public string Sender { get; set; } = "";
    public string Message { get; set; } = "";
    public SolidColorBrush BackgroundColor { get; set; } = new(Colors.White);
    public SolidColorBrush SenderColor { get; set; } = new(Colors.Black);
}

// UI wrapper for ExplainerRecommendation to add UI-specific properties
public class UIRecommendation
{
    private readonly ExplainerRecommendation _recommendation;

    public UIRecommendation(ExplainerRecommendation recommendation)
    {
        _recommendation = recommendation;
        HasScript = !string.IsNullOrWhiteSpace(recommendation.Script);
    }

    // UI-specific properties
    public bool HasScript { get; set; }

    // Proxied properties from Core model
    public string Priority => _recommendation.Priority;
    public string Icon => _recommendation.Icon;
    public string Title => _recommendation.Title;
    public string Action => _recommendation.Action;
    public string Benefit => _recommendation.Benefit;
    public int EstimatedImprovement => _recommendation.EstimatedImprovement;
    public int EstimatedTimeMinutes => _recommendation.EstimatedTimeMinutes;
    public string RiskLevel => _recommendation.RiskLevel;
    public string? Script => _recommendation.Script;
    public List<string> Steps => _recommendation.Steps;
}
