namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for exporting data to various formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data to PDF format
    /// </summary>
    Task<string> ExportToPdfAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null);

    /// <summary>
    /// Export data to Excel format
    /// </summary>
    Task<string> ExportToExcelAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null);

    /// <summary>
    /// Export data to CSV format
    /// </summary>
    Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null);

    /// <summary>
    /// Export data to JSON format
    /// </summary>
    Task<string> ExportToJsonAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null);
    
    /// <summary>
    /// Get available export templates for a role
    /// </summary>
    List<string> GetAvailableTemplates(string role);
}

public class ExportOptions
{
    public string? Role { get; set; }
    public string? Template { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeCharts { get; set; } = true;
    public Dictionary<string, string>? CustomFields { get; set; }
}

