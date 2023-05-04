using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AnySoftDesktop.UsersControls;

public partial class SearchBar : UserControl
{
    [Description("Searchbar Text"), Category("Text")]
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            "SearchText",
            typeof(string), 
            typeof(TextBoxWithPlaceholder),
            new UIPropertyMetadata(""));
    
    public SearchBar()
    {
        InitializeComponent();
    }
}