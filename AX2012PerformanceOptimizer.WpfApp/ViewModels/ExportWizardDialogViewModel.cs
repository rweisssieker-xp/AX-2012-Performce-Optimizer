using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.WpfApp.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class ExportWizardDialogViewModel : ObservableObject
{
    private readonly IExportService _exportService;

    [ObservableProperty]
    private ObservableCollection<string> exportFormats = new() { "PDF", "Excel", "CSV", "JSON" };

    [ObservableProperty]
    private string selectedFormat = "Excel";

    [ObservableProperty]
    private ObservableCollection<string> roles = new() { "DBA", "Performance Engineer", "IT Manager", "Executive" };

    [ObservableProperty]
    private string? selectedRole;
    
    [ObservableProperty]
    private ObservableCollection<string> availableTemplates = new();
    
    [ObservableProperty]
    private string? selectedTemplate;

    [ObservableProperty]
    private DateTime? startDate;

    [ObservableProperty]
    private DateTime? endDate;

    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string previewText = "Select format and options to see preview...";

    public object? DataToExport { get; set; }
    public string ExportTitle { get; set; } = "Performance Data Export";

    public event EventHandler<string>? ExportCompleted;

    public ExportWizardDialogViewModel(IExportService exportService)
    {
        _exportService = exportService;
        
        // Set default file path
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        FilePath = Path.Combine(desktop, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(SelectedFormat)}");
        
        // Update preview when properties change
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedFormat) || 
                e.PropertyName == nameof(SelectedRole) ||
                e.PropertyName == nameof(SelectedTemplate) ||
                e.PropertyName == nameof(StartDate) ||
                e.PropertyName == nameof(EndDate) ||
                e.PropertyName == nameof(FilePath))
            {
                UpdatePreview();
                if (e.PropertyName == nameof(SelectedRole))
                {
                    UpdateTemplates();
                }
            }
        };
        
        UpdateTemplates();
    }

    [RelayCommand]
    private void Browse()
    {
        var dialog = new SaveFileDialog
        {
            Filter = GetFileFilter(),
            FileName = Path.GetFileName(FilePath),
            InitialDirectory = Path.GetDirectoryName(FilePath)
        };

        if (dialog.ShowDialog() == true)
        {
            FilePath = dialog.FileName;
            UpdatePreview();
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            MessageBox.Show("Please select a file location", "Export Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (DataToExport == null)
        {
            MessageBox.Show("No data to export", "Export Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var options = new ExportOptions
            {
                Role = SelectedRole,
                Template = SelectedTemplate,
                StartDate = StartDate,
                EndDate = EndDate
            };

            string exportedPath;
            
            if (DataToExport is IEnumerable<object> enumerable)
            {
                exportedPath = SelectedFormat switch
                {
                    "PDF" => await _exportService.ExportToPdfAsync(enumerable, ExportTitle, FilePath, options),
                    "Excel" => await _exportService.ExportToExcelAsync(enumerable, ExportTitle, FilePath, options),
                    "CSV" => await _exportService.ExportToCsvAsync(enumerable, ExportTitle, FilePath, options),
                    "JSON" => await _exportService.ExportToJsonAsync(enumerable, ExportTitle, FilePath, options),
                    _ => FilePath
                };
            }
            else
            {
                // Single object export
                var singleItem = new[] { DataToExport };
                exportedPath = SelectedFormat switch
                {
                    "PDF" => await _exportService.ExportToPdfAsync(singleItem, ExportTitle, FilePath, options),
                    "Excel" => await _exportService.ExportToExcelAsync(singleItem, ExportTitle, FilePath, options),
                    "CSV" => await _exportService.ExportToCsvAsync(singleItem, ExportTitle, FilePath, options),
                    "JSON" => await _exportService.ExportToJsonAsync(singleItem, ExportTitle, FilePath, options),
                    _ => FilePath
                };
            }

            MessageBox.Show($"Export completed successfully!\n\nFile: {exportedPath}", 
                "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            
            ExportCompleted?.Invoke(this, exportedPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed:\n\n{ex.Message}", 
                "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateTemplates()
    {
        if (!string.IsNullOrEmpty(SelectedRole))
        {
            var exportService = App.GetService<IExportService>();
            var templates = exportService.GetAvailableTemplates(SelectedRole);
            AvailableTemplates.Clear();
            foreach (var template in templates)
            {
                AvailableTemplates.Add(template);
            }
            if (AvailableTemplates.Any())
            {
                SelectedTemplate = AvailableTemplates.First();
            }
        }
        else
        {
            AvailableTemplates.Clear();
            SelectedTemplate = null;
        }
    }
    
    private void UpdatePreview()
    {
        var format = SelectedFormat;
        var role = SelectedRole ?? "None";
        var template = SelectedTemplate ?? "Default";
        var dateRange = StartDate.HasValue && EndDate.HasValue
            ? $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}"
            : "All dates";
        
        PreviewText = $"Format: {format}\nRole: {role}\nTemplate: {template}\nDate Range: {dateRange}\nFile: {Path.GetFileName(FilePath)}";
        
        // Update file extension
        if (!string.IsNullOrWhiteSpace(FilePath))
        {
            var dir = Path.GetDirectoryName(FilePath);
            var name = Path.GetFileNameWithoutExtension(FilePath);
            FilePath = Path.Combine(dir ?? "", $"{name}.{GetFileExtension(format)}");
        }
    }

    private string GetFileExtension(string format)
    {
        return format switch
        {
            "PDF" => "pdf",
            "Excel" => "xlsx",
            "CSV" => "csv",
            "JSON" => "json",
            _ => "txt"
        };
    }

    private string GetFileFilter()
    {
        return SelectedFormat switch
        {
            "PDF" => "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
            "Excel" => "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            "CSV" => "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            "JSON" => "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            _ => "All Files (*.*)|*.*"
        };
    }

}

