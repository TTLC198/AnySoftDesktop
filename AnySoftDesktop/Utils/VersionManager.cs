using System.Windows;
using AnySoftDesktop.Models;

namespace AnySoftDesktop.Utils;

public class VersionManager : DependencyObject
{
    private static readonly DependencyProperty ApiUrlProperty =
        DependencyProperty.Register( 
            nameof(ApiUrl), 
            typeof(string),
            typeof(VersionManager));

    public string? ApiUrl
    {
        get => (string)GetValue(ApiUrlProperty);
        set => SetValue(ApiUrlProperty, value );
    }
    
    private static readonly DependencyProperty CdnUrlProperty =
        DependencyProperty.Register( 
            nameof(CdnUrl), 
            typeof(string),
            typeof(VersionManager));
    
    public string? CdnUrl
    {
        get => (string)GetValue(CdnUrlProperty);
        set => SetValue(CdnUrlProperty, value );
    }

    public static VersionManager Instance { get; private set; }

    static VersionManager() {
        Instance = new VersionManager();
    }
}