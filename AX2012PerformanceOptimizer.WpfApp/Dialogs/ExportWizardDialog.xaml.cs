using System.Windows;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Dialogs;

public partial class ExportWizardDialog : Window
{
    public ExportWizardDialogViewModel ViewModel { get; }

    public ExportWizardDialog()
    {
        ViewModel = App.GetService<ExportWizardDialogViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
        
        ViewModel.ExportCompleted += (sender, filePath) =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

