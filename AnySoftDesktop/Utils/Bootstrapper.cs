using System.Windows;
using AnySoftDesktop.ViewModels;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
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
        
        builder.Bind<TabBaseViewModel>().To<DashboardTabViewModel>().InSingletonScope();
        builder.Bind<TabBaseViewModel>().To<LibraryTabViewModel>().InSingletonScope();
        builder.Bind<TabBaseViewModel>().To<SettingsTabViewModel>().InSingletonScope();
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