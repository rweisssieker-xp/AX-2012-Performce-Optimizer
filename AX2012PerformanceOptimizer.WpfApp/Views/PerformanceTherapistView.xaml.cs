using System;
using System.Windows;
using System.Windows.Controls;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;

namespace AX2012PerformanceOptimizer.WpfApp.Views;

public partial class PerformanceTherapistView : UserControl
{
    private bool _isInitialized = false;

    public PerformanceTherapistView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized) return;

        try
        {
            DataContext = App.GetService<PerformanceTherapistViewModel>();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing Performance Therapist:\n\n{ex.Message}\n\nInner: {ex.InnerException?.Message}",
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
