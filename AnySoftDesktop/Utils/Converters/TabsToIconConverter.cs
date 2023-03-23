using System;
using System.Globalization;
using System.Windows.Data;
using AnySoftDesktop.ViewModels;
using AnySoftDesktop.ViewModels.Tabs;
using MaterialDesignThemes.Wpf;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(ITabViewModel), typeof(PackIconKind))]
public class TabsToIconConverter : IValueConverter
{
    public static TabsToIconConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        LibraryTabViewModel => PackIconKind.Library,
        _ => PackIconKind.QuestionMark
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}