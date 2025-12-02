using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for exporting data to various formats
/// </summary>
public class ExportService : IExportService
{
    private readonly ExportTemplateService _templateService = new();
    
    public async Task<string> ExportToPdfAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null)
    {
        var sb = new StringBuilder();
        
        // Apply role-specific template if provided
        if (!string.IsNullOrEmpty(options?.Role) && !string.IsNullOrEmpty(options?.Template))
        {
            var template = _templateService.GetTemplateForRole(options.Role);
            var formatted = _templateService.FormatDataForTemplate(data, template, "PDF");
            sb.Append(formatted);
        }
        else
        {
            sb.AppendLine($"# {title}");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            if (options?.StartDate.HasValue == true && options.EndDate.HasValue == true)
            {
                sb.AppendLine($"Date Range: {options.StartDate:yyyy-MM-dd} to {options.EndDate:yyyy-MM-dd}");
                sb.AppendLine();
            }
            
            sb.AppendLine("## Data");
            foreach (var item in data)
            {
                sb.AppendLine(JsonSerializer.Serialize(item, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                }));
                sb.AppendLine();
            }
        }
        
        // Save as text file (PDF generation requires additional library)
        var txtPath = filePath.Replace(".pdf", ".txt");
        await File.WriteAllTextAsync(txtPath, sb.ToString());
        
        return txtPath;
    }

    public async Task<string> ExportToExcelAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null)
    {
        // For now, create CSV format (Excel-compatible)
        // In production, use EPPlus or ClosedXML
        var csvPath = filePath.Replace(".xlsx", ".csv");
        return await ExportToCsvAsync(data, title, csvPath, options);
    }

    public async Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null)
    {
        var sb = new StringBuilder();
        
        // Get property names as headers
        var properties = typeof(T).GetProperties();
        var headers = properties.Select(p => p.Name);
        sb.AppendLine(string.Join(",", headers));
        
        // Add data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                if (value == null) return "";
                var str = value.ToString() ?? "";
                // Escape commas and (str.Contains(",") || str.Contains("\"") || str.Contains("\n")))
                {
                    return $"\"{str.Replace("\"", "\"\"")}\"";
                }
                return str;
            });
            sb.AppendLine(string.Join(",", values));
        }
        
        await File.WriteAllTextAsync(filePath, sb.ToString());
        return filePath;
    }

    public async Task<string> ExportToJsonAsync<T>(IEnumerable<T> data, string title, string filePath, ExportOptions? options = null)
    {
        var exportData = new
        {
            Title = title,
            Generated = DateTime.Now,
            DateRange = options != null && options.StartDate.HasValue && options.EndDate.HasValue
                ? new { Start = options.StartDate, End = options.EndDate }
                : null,
            Role = options?.Role,
            Template = options?.Template,
            Data = data.ToList()
        };
        
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }
    
    public List<string> GetAvailableTemplates(string role)
    {
        var template = _templateService.GetTemplateForRole(role);
        return template.Sections.ToList();
    }
}

