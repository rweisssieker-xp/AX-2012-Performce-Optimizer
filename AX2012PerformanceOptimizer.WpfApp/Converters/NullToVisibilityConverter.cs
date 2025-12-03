using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AX2012PerformanceOptimizer.WpfApp.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If value is null, return Collapsed; otherwise Visible
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
