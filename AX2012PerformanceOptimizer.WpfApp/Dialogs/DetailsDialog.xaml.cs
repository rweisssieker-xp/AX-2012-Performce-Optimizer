using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.Dialogs;

public partial class DetailsDialog : Window, INotifyPropertyChanged
{
    private string _dialogTitle = string.Empty;
    private string _dialogContent = string.Empty;
    private string? _dialogCode;
    private string _dialogIcon = "ℹ️";

    public string DialogTitle
    {
        get => _dialogTitle;
        set { _dialogTitle = value; OnPropertyChanged(); }
    }

    public string DialogContent
    {
        get => _dialogContent;
        set { _dialogContent = value; OnPropertyChanged(); }
    }

    public string? DialogCode
    {
        get => _dialogCode;
        set { _dialogCode = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCode)); }
    }

    public string DialogIcon
    {
        get => _dialogIcon;
        set { _dialogIcon = value; OnPropertyChanged(); }
    }

    public bool HasCode => !string.IsNullOrWhiteSpace(DialogCode);

    public DetailsDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public DetailsDialog(string title, string content, string? code = null, string? icon = null) : this()
    {
        DialogTitle = title;
        DialogContent = content;
        DialogCode = code;
        if (!string.IsNullOrEmpty(icon))
            DialogIcon = icon;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CopyCodeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(DialogCode))
        {
            Clipboard.SetText(DialogCode);
            MessageBox.Show("Code kopiert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
