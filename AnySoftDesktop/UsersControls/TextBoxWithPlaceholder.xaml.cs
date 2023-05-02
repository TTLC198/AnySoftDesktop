using System.ComponentModel;
using System.Windows.Controls;

namespace AnySoftDesktop.UsersControls;

public partial class TextBoxWithPlaceholder : UserControl
{
    [Description("Textbox corner radius"), Category("Appearance")]
    public int CornerRadius
    {
        get; set;
    }
    
    [Description("Textbox placeholder"), Category("Appearance")]
    public string? Placeholder
    {
        get; set;
    }

    public TextBoxWithPlaceholder()
    {
        InitializeComponent();
        this.DataContext = this;
    }
}