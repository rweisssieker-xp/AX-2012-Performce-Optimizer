namespace AX2012PerformanceOptimizer.Core.Services.UI;

/// <summary>
/// Platform-agnostic clipboard service.
/// Implement this interface for WPF, Blazor, or any other UI framework.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies text to the clipboard.
    /// </summary>
    Task CopyToClipboardAsync(string text);

    /// <summary>
    /// Gets text from the clipboard.
    /// </summary>
    Task<string?> GetFromClipboardAsync();

    /// <summary>
    /// Returns true if the clipboard contains text.
    /// </summary>
    Task<bool> ContainsTextAsync();
}
