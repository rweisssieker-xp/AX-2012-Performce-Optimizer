using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace AX2012PerformanceOptimizer.WpfApp.Converters;

/// <summary>
/// Converts an object to a list of key-value pairs for display
/// </summary>
public class ObjectToKeyValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return new List<KeyValuePair<string, string>> { new("Status", "No data available") };

        var result = new List<KeyValuePair<string, string>>();

        // Handle collections/lists
        if (value is IEnumerable enumerable && !(value is string))
        {
            var list = enumerable.Cast<object>().ToList();
            if (list.Count == 0)
            {
                result.Add(new KeyValuePair<string, string>("Items", "No items"));
                return result;
            }

            // If it's a list of complex objects, show count and first few items
            result.Add(new KeyValuePair<string, string>("Count", list.Count.ToString()));
            
            int itemIndex = 0;
            foreach (var item in list.Take(5))
            {
                itemIndex++;
                if (item == null) continue;
                
                var itemType = item.GetType();
                if (itemType.IsPrimitive || item is string || item is decimal)
                {
                    result.Add(new KeyValuePair<string, string>($"Item {itemIndex}", item.ToString() ?? ""));
                }
                else
                {
                    // Extract key properties from the item
                    var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var summary = string.Join(", ", props.Take(3).Select(p => 
                    {
                        var val = p.GetValue(item);
                        return $"{p.Name}: {FormatValue(val)}";
                    }));
                    result.Add(new KeyValuePair<string, string>($"Item {itemIndex}", summary));
                }
            }
            
            if (list.Count > 5)
            {
                result.Add(new KeyValuePair<string, string>("...", $"and {list.Count - 5} more items"));
            }
            
            return result;
        }

        // Handle anonymous types and regular objects
        var type = value.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            try
            {
                var propValue = prop.GetValue(value);
                var displayValue = FormatValue(propValue);
                result.Add(new KeyValuePair<string, string>(FormatPropertyName(prop.Name), displayValue));
            }
            catch
            {
                // Skip properties that can't be read
            }
        }

        if (result.Count == 0)
        {
            result.Add(new KeyValuePair<string, string>("Value", value.ToString() ?? ""));
        }

        return result;
    }

    private string FormatValue(object? value)
    {
        if (value == null) return "N/A";
        
        if (value is double d)
            return d.ToString("N2");
        if (value is decimal dec)
            return dec.ToString("N2");
        if (value is float f)
            return f.ToString("N2");
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd HH:mm");
        if (value is bool b)
            return b ? "Yes" : "No";
        if (value is IEnumerable enumerable && !(value is string))
        {
            var list = enumerable.Cast<object>().ToList();
            return $"{list.Count} items";
        }
        
        return value.ToString() ?? "";
    }

    private string FormatPropertyName(string name)
    {
        // Convert camelCase/PascalCase to readable format
        var result = new System.Text.StringBuilder();
        foreach (char c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result.Append(' ');
            result.Append(c);
        }
        return result.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
