using System.Text;
using System.Text.Json;

namespace AX2012PerformanceOptimizer.WpfApp.Services;

/// <summary>
/// Service for managing role-specific export templates
/// </summary>
public class ExportTemplateService
{
    public ExportTemplate GetTemplateForRole(string role)
    {
        return role switch
        {
            "DBA" => GetDbaTemplate(),
            "Performance Engineer" => GetPerformanceEngineerTemplate(),
            "IT Manager" => GetItManagerTemplate(),
            "Executive" => GetExecutiveTemplate(),
            _ => GetTemplate()
        };
    }

    private ExportTemplate GetDbaTemplate()
    {
        return new ExportTemplate
        {
            Name = "DBA Technical Report",
            Role = "DBA",
            IncludeMetrics = new[] { "QueryPerformance", "DatabaseHealth", "IndexMaintenance", "StatisticsFreshness", "BackupRecovery" },
            IncludeVisualizations = true,
            Language = "Technical",
            Sections = new[]
            {
                "Executive Summary",
                "Query Performance Analysis",
                "Database Health Metrics",
                "Index Recommendations",
                "Statistics Status",
                "Backup Status",
                "Action Items"
            }
        };
    }

    private ExportTemplate GetPerformanceEngineerTemplate()
    {
        return new ExportTemplate
        {
            Name = "Performance Analysis Report",
            Role = "Performance Engineer",
            IncludeMetrics = new[] { "QueryPerformance", "AosPerformance", "ResourceUtilization", "PerformanceAnomalies" },
            IncludeVisualizations = true,
            Language = "Technical",
            Sections = new[]
            {
                "Performance Overview",
                "Top Performance Issues",
                "Resource Utilization",
                "AOS Performance",
                "Anomaly Detection",
                "Optimization Recommendations"
            }
        };
    }

    private ExportTemplate GetItManagerTemplate()
    {
        return new ExportTemplate
        {
            Name = "IT Management Report",
            Role = "IT Manager",
            IncludeMetrics = new[] { "OverallScore", "SystemHealth", "ResourceUtilization", "Compliance" },
            IncludeVisualizations = true,
            Language = "Business",
            Sections = new[]
            {
                "Executive Summary",
                "System Health Score",
                "Resource Utilization",
                "Compliance Status",
                "Key Metrics",
                "Recommendations"
            }
        };
    }

    private ExportTemplate GetExecutiveTemplate()
    {
        return new ExportTemplate
        {
            Name = "Executive Dashboard",
            Role = "Executive",
            IncludeMetrics = new[] { "OverallScore", "CostImpact", "BusinessImpact", "ROI" },
            IncludeVisualizations = true,
            Language = "Plain",
            Sections = new[]
            {
                "Performance at a Glance",
                "Cost Impact",
                "Business Impact",
                "Key Achievements",
                "Investment Recommendations"
            }
        };
    }

    private ExportTemplate GetTemplate()
    {
        return new ExportTemplate
        {
            Name = "Standard Report",
            Role = "General",
            IncludeMetrics = Array.Empty<string>(),
            IncludeVisualizations = true,
            Language = "Technical"
        };
    }

    public string FormatDataForTemplate<T>(IEnumerable<T> data, ExportTemplate template, string format)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"# {template.Name}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Role: {template.Role}");
        sb.AppendLine();
        
        foreach (var section in template.Sections)
        {
            sb.AppendLine($"## {section}");
            sb.AppendLine();
            
            // Format data based on template language
            foreach (var item in data)
            {
                var formatted = FormatItemForLanguage(item, template.Language);
                sb.AppendLine(formatted);
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    private string FormatItemForLanguage<T>(T item, string language)
    {
        if (language == "Plain" || language == "Business")
        {
            // Simplified, business-friendly format
            return JsonSerializer.Serialize(item, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        
        // Technical format
        return JsonSerializer.Serialize(item, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}

public class ExportTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string[] IncludeMetrics { get; set; } = Array.Empty<string>();
    public bool IncludeVisualizations { get; set; }
    public string Language { get; set; } = "Technical";
    public string[] Sections { get; set; } = Array.Empty<string>();
}

