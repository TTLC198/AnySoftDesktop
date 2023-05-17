using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(IEnumerable<object>), typeof(Visibility))]
public class EmptyToVisibilityConverter : IValueConverter
{
    public static EmptyToVisibilityConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<object> enumerable)
            return !enumerable.Any() ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

[ValueConversion(typeof(IEnumerable<object>), typeof(Visibility))]
public class InverseEmptyToVisibilityConverter : IValueConverter
{
    public static InverseEmptyToVisibilityConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<object> enumerable)
            return enumerable.Any() ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}