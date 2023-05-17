using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibleConverter : IValueConverter
{
    public static BoolToVisibleConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility.Visible;
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBoolToVisibleConverter : IValueConverter
{
    public static InverseBoolToVisibleConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is false ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility.Collapsed;
}