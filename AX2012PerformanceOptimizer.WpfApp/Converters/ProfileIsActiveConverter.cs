using System;
using System.Globalization;
using System.Windows.Data;
using AX2012PerformanceOptimizer.Data.Models;

namespace AX2012PerformanceOptimizer.WpfApp.Converters;

public class ProfileIsActiveConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return false;

        // Check for unset values
        if (values[0] == null || values[1] == null)
            return false;

        // values[0] = current profile's Id (string)
        // values[1] = ActiveConnectionProfile (ConnectionProfile object)
        if (values[0] is string profileId && values[1] is ConnectionProfile activeProfile)
        {
            return profileId == activeProfile.Id;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
