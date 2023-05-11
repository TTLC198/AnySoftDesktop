using System;
using System.Globalization;
using System.Windows.Data;
using AnySoftDesktop.ViewModels;
using AnySoftDesktop.ViewModels.Tabs;
using MaterialDesignThemes.Wpf;

namespace AnySoftDesktop.Utils.Converters;

[ValueConversion(typeof(TabBaseViewModel), typeof(PackIconKind))]
public class TabsToIconConverter : IValueConverter
{
    public static TabsToIconConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        DashboardTabViewModel => PackIconKind.ViewDashboard,
        LibraryTabViewModel => PackIconKind.Package,
        SettingsTabViewModel => PackIconKind.Settings,
        _ => PackIconKind.QuestionMark
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}