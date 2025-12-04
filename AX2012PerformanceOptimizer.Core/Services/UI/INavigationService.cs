namespace AX2012PerformanceOptimizer.Core.Services.UI;

/// <summary>
/// Platform-agnostic navigation service for navigating between views/pages.
/// Implement this interface for WPF, Blazor, or any other UI framework.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to a view by name.
    /// </summary>
    Task NavigateToAsync(string viewName);

    /// <summary>
    /// Navigates to a view with parameters.
    /// </summary>
    Task NavigateToAsync(string viewName, IDictionary<string, object> parameters);

    /// <summary>
    /// Navigates back to the previous view.
    /// </summary>
    Task GoBackAsync();

    /// <summary>
    /// Returns true if navigation back is possible.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets the current view/page name.
    /// </summary>
    string CurrentView { get; }

    /// <summary>
    /// Event raised when navigation occurs.
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;
}

/// <summary>
/// Navigation event arguments.
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public string FromView { get; init; } = string.Empty;
    public string ToView { get; init; } = string.Empty;
    public IDictionary<string, object>? Parameters { get; init; }
}
