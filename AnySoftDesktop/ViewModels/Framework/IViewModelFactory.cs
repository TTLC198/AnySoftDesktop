namespace AnySoftDesktop.ViewModels.Framework;

public interface IViewModelFactory
{
    LoginViewModel CreateLoginViewModel();
    
    MessageBoxViewModel CreateMessageBoxViewModel();
}