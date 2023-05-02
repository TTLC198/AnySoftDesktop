using System.Windows;
using System.Windows.Input;
using AnySoftDesktop.ViewModels;

namespace AnySoftDesktop.Views;

public partial class MainWindowView : Window
{
    public MainWindowView()
    {
        InitializeComponent();
    }
    
    private void HeaderBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void HideButton_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}