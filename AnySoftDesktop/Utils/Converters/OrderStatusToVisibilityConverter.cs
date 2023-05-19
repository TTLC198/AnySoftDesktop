using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(string), typeof(Visibility))]
public class OrderStatusToVisibilityConverter : IValueConverter
{
    public static OrderStatusToVisibilityConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return 
            value is string and "Paid" 
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}