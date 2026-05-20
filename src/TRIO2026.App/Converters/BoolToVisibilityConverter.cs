using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TRIO2026.App.Converters;

/// <summary>
/// 布林值 → Visibility 轉換器
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
