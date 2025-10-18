using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AX2012PerformanceOptimizer.WpfApp.Converters;

/// <summary>
/// Converts a collection count to Visibility.
/// Returns Visible if count > 0, otherwise Collapsed.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        // Handle int directly
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handle collections
        if (value is ICollection collection)
        {
            return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Handle IEnumerable
        if (value is IEnumerable enumerable)
        {
            return enumerable.GetEnumerator().MoveNext()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
