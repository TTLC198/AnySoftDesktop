using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolToBoolConverter : IValueConverter
{
    public static InverseBoolToBoolConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not true;
}