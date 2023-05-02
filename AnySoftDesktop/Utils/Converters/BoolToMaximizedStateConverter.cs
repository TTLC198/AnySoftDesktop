using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(bool), typeof(WindowState))]
public class BoolToMaximizedStateConverter : IValueConverter
{
    public static BoolToMaximizedStateConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? WindowState.Maximized : WindowState.Normal;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is WindowState.Maximized;
}

[ValueConversion(typeof(bool), typeof(WindowState))]
public class InverseBoolToMaximizedStateConverter : IValueConverter
{
    public static InverseBoolToMaximizedStateConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is false ? WindowState.Maximized : WindowState.Normal;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is WindowState.Normal;
}