using System.Globalization;
using System.Windows.Data;

namespace AX2012PerformanceOptimizer.WpfApp.Converters;

public class BoolToActiveInactiveConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Active" : "Inactive";
        }
        return "Inactive";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
