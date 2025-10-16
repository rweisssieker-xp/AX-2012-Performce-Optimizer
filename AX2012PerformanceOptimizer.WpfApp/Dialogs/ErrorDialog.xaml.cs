using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AX2012PerformanceOptimizer.WpfApp.Dialogs;

public partial class ErrorDialog : Window, INotifyPropertyChanged
{
    private string _dialogTitle = "Error";
    private string _dialogMessage = string.Empty;
    private string? _stackTrace;

    public string DialogTitle
    {
        get => _dialogTitle;
        set { _dialogTitle = value; OnPropertyChanged(); }
    }

    public string DialogMessage
    {
        get => _dialogMessage;
        set { _dialogMessage = value; OnPropertyChanged(); }
    }

    public string? StackTrace
    {
        get => _stackTrace;
        set { _stackTrace = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStackTrace)); }
    }

    public bool HasStackTrace => !string.IsNullOrWhiteSpace(StackTrace);

    public ErrorDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ErrorDialog(string title, string message, Exception? exception = null) : this()
    {
        DialogTitle = title;
        DialogMessage = message;
        if (exception != null)
        {
            StackTrace = exception.ToString();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
    {
        var errorText = $"{DialogTitle}\n\n{DialogMessage}";
        if (HasStackTrace)
        {
            errorText += $"\n\n{StackTrace}";
        }

        Clipboard.SetText(errorText);
        MessageBox.Show("Fehler in Zwischenablage kopiert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
