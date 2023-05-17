using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    public static NullToVisibilityConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

[ValueConversion(typeof(object), typeof(Visibility))]
public class InverseNullToVisibilityConverter : IValueConverter
{
    public static InverseNullToVisibilityConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}