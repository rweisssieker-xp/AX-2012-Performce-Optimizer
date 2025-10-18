using System.Linq;
using System.Text;
using System.Windows;
using AX2012PerformanceOptimizer.WpfApp.Dialogs;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for showing modern, styled dialogs instead of basic MessageBox
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// Show detailed information dialog (for Action Details, Insight Details, etc.)
    /// </summary>
    public Task ShowDetailsAsync(string title, string content, string? code = null, string? icon = null)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dialog = new DetailsDialog(title, content, code, icon)
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show summary/report dialog with sections (for Weekly Summary, Executive Summary)
    /// </summary>
    public Task ShowSummaryAsync(string title, Dictionary<string, string> sections)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dialog = new SummaryDialog(title, sections)
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show history dialog with timeline visualization
    /// </summary>
    public Task ShowHistoryAsync(string title, List<HistoryEntry> entries)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            // Convert history entries to sections for SummaryDialog
            var sections = new Dictionary<string, string>();
            foreach (var entry in entries.OrderByDescending(e => e.Timestamp))
            {
                var sectionTitle = $"{entry.Icon ?? "📅"} {entry.Timestamp:dd.MM.yyyy HH:mm} - {entry.Title}";
                var sectionContent = $"Score: {entry.Score}/100\n{entry.Description}";
                sections.Add(sectionTitle, sectionContent);
            }

            var dialog = new SummaryDialog(title, sections)
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show error dialog with stack trace
    /// </summary>
    public Task ShowErrorAsync(string title, string message, Exception? exception = null)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dialog = new ErrorDialog(title, message, exception)
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show success message dialog
    /// </summary>
    public Task ShowSuccessAsync(string title, string message, string? additionalInfo = null)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var content = message;
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                content += $"\n\n{additionalInfo}";
            }

            var dialog = new DetailsDialog(title, content, null, "✅")
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show information dialog
    /// </summary>
    public Task ShowInfoAsync(string title, string message)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dialog = new DetailsDialog(title, message, null, "ℹ️")
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show warning dialog
    /// </summary>
    public Task ShowWarningAsync(string title, string message)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dialog = new DetailsDialog(title, message, null, "⚠️")
            {
                Owner = GetActiveWindow()
            };
            dialog.ShowDialog();
        }).Task;
    }

    /// <summary>
    /// Show confirmation dialog with Yes/No buttons
    /// </summary>
    public Task<bool> ShowConfirmationAsync(string title, string message)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            return result == MessageBoxResult.Yes;
        }).Task;
    }

    /// <summary>
    /// Get the currently active window to set as owner
    /// </summary>
    private Window? GetActiveWindow()
    {
        return Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
               ?? Application.Current?.MainWindow;
    }
}
