using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class NaturalLanguageAssistantView : UserControl
{
    public NaturalLanguageAssistantView()
    {
        InitializeComponent();
        DataContext = App.GetService<NaturalLanguageAssistantViewModel>();
    }
}
