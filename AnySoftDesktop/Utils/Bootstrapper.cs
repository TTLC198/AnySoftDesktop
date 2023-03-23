using System.Windows;
using AnySoftDesktop.ViewModels;
using Stylet;
using StyletIoC;

namespace AnySoftDesktop.Utils;

public class Bootstrapper : Bootstrapper<MainWindowViewModel>
{
    private T GetInstance<T>() => (T) base.GetInstance(typeof(T));

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        base.ConfigureIoC(builder);
    }
    
    protected override void Launch()
    {
        base.Launch();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}