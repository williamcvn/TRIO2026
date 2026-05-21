using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TRIO2026.App.Converters;

/// <summary>
/// 布林值 → Visibility 轉換器
/// ConverterParameter="Invert" 可反轉結果
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is true;
        if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
