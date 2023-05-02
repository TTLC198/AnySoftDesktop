using System.Windows;
using AnySoftDesktop.ViewModels;
using AnySoftDesktop.ViewModels.Framework;
using Stylet;
using StyletIoC;

namespace AnySoftDesktop.Utils;

public class Bootstrapper : Bootstrapper<MainWindowViewModel>
{
    private T GetInstance<T>() => (T) base.GetInstance(typeof(T));

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        base.ConfigureIoC(builder);
        
        builder.Bind<DialogManager>().ToSelf().InSingletonScope();
        
        builder.Bind<IViewModelFactory>().ToAbstractFactory();
        builder.Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
        builder.Bind<LoginViewModel>().ToSelf().InSingletonScope();
        builder.Bind<ITabViewModel>().ToAllImplementations().InSingletonScope();
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