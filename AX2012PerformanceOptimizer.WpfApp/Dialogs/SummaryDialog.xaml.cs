using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.Dialogs;

public partial class SummaryDialog : Window, INotifyPropertyChanged
{
    private string _dialogTitle = string.Empty;
    private ObservableCollection<KeyValuePair<string, string>> _sections = new();

    public string DialogTitle
    {
        get => _dialogTitle;
        set { _dialogTitle = value; OnPropertyChanged(); }
    }

    public ObservableCollection<KeyValuePair<string, string>> Sections
    {
        get => _sections;
        set { _sections = value; OnPropertyChanged(); }
    }

    public SummaryDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public SummaryDialog(string title, Dictionary<string, string> sections) : this()
    {
        DialogTitle = title;
        Sections = new ObservableCollection<KeyValuePair<string, string>>(sections);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var content = new StringBuilder();
            content.AppendLine("===========================================");
            content.AppendLine($"{DialogTitle}");
            content.AppendLine($"Generated: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            content.AppendLine("===========================================\n");

            foreach (var section in Sections)
            {
                content.AppendLine($"## {section.Key}");
                content.AppendLine(section.Value);
                content.AppendLine();
            }

            var fileName = $"Summary_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = System.IO.Path.Combine(desktopPath, fileName);

            System.IO.File.WriteAllText(filePath, content.ToString());

            MessageBox.Show(
                $"Report exportiert!\n\nDatei: {filePath}",
                "Export erfolgreich",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fehler beim Exportieren:\n\n{ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
