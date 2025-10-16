namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for showing modern, styled dialogs instead of basic MessageBox
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show detailed information dialog (for Action Details, Insight Details, etc.)
    /// </summary>
    Task ShowDetailsAsync(string title, string content, string? code = null, string? icon = null);

    /// <summary>
    /// Show summary/report dialog with sections (for Weekly Summary, Executive Summary)
    /// </summary>
    Task ShowSummaryAsync(string title, Dictionary<string, string> sections);

    /// <summary>
    /// Show history dialog with timeline visualization
    /// </summary>
    Task ShowHistoryAsync(string title, List<HistoryEntry> entries);

    /// <summary>
    /// Show error dialog with stack trace
    /// </summary>
    Task ShowErrorAsync(string title, string message, Exception? exception = null);

    /// <summary>
    /// Show success message dialog
    /// </summary>
    Task ShowSuccessAsync(string title, string message, string? additionalInfo = null);

    /// <summary>
    /// Show information dialog
    /// </summary>
    Task ShowInfoAsync(string title, string message);

    /// <summary>
    /// Show warning dialog
    /// </summary>
    Task ShowWarningAsync(string title, string message);

    /// <summary>
    /// Show confirmation dialog with Yes/No buttons
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);
}

/// <summary>
/// Represents a history entry for timeline visualization
/// </summary>
public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? StatusColor { get; set; }
    public string? Icon { get; set; }
}
