using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AnySoftDesktop.Utils.Converters;

public class UrlToImageConverter : IValueConverter
{
    public static UrlToImageConverter Instance { get; } = new();
    
    public object? Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not (not null and string path)) return null;
        if (path.StartsWith('/'))
            path = path[1..];
        var bi = new BitmapImage(new Uri($"{App.CdnUrl}{path}", UriKind.Absolute));
        return bi;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}