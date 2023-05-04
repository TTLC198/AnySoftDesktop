using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnySoftDesktop.UsersControls;

public partial class TextBoxWithPlaceholder : UserControl
{
    [Description("Textbox corner radius"), Category("Appearance")]
    public int CornerRadius
    {
        get; set;
    }

    [Description("Textbox Text"), Category("Text")]
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string), 
            typeof(TextBoxWithPlaceholder),
            new UIPropertyMetadata(""));
    
    [Description("Textbox TextPlaceholder")]
    public string TextPlaceholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(TextPlaceholder),
            typeof(string), 
            typeof(TextBoxWithPlaceholder),
            new UIPropertyMetadata(""));

    public TextBoxWithPlaceholder()
    {
        InitializeComponent();
    }
}