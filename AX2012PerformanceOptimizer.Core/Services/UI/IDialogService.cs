namespace AX2012PerformanceOptimizer.Core.Services.UI;

/// <summary>
/// Platform-agnostic dialog service for showing messages and confirmations.
/// Implement this interface for WPF, Blazor, or any other UI framework.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an information message to the user.
    /// </summary>
    Task ShowInfoAsync(string title, string message);

    /// <summary>
    /// Shows a warning message to the user.
    /// </summary>
    Task ShowWarningAsync(string title, string message);

    /// <summary>
    /// Shows an error message to the user.
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog and returns the user's choice.
    /// </summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>
    /// Shows a message with custom severity level.
    /// </summary>
    Task ShowMessageAsync(string title, string message, MessageSeverity severity);
}

/// <summary>
/// Message severity levels for dialog display.
/// </summary>
public enum MessageSeverity
{
    Information,
    Warning,
    Error,
    Success
}
