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
    
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        //clean up notifyicon (would otherwise stay open until application finishes)
        MainTaskbarIcon.Dispose();
        base.OnClosing(e);
    }
    
    private void HeaderBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
    
    private void MainTaskbarIcon_OnTrayBalloonTipClicked(object sender, RoutedEventArgs e)
    {
        Visibility = 
            Visibility == Visibility.Visible
                ? Visibility.Hidden 
                : Visibility.Visible;
    }

    private void Show_OnClick(object sender, RoutedEventArgs e) => Show();
    private void Hide_OnClick(object sender, RoutedEventArgs e) => Hide();
    private void Close_OnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}