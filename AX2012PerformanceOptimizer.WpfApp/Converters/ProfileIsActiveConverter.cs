using System;
using System.Globalization;
using System.Windows.Data;
using AX2012PerformanceOptimizer.Data.Models;

namespace AX2012PerformanceOptimizer.WpfApp.Converters;

public class ProfileIsActiveConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return false;

        if (values[0] is not Guid profileId || values[1] is not ConnectionProfile activeProfile)
            return false;

        return profileId == activeProfile.Id;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
