namespace AnySoftDesktop.ViewModels.Framework;

public interface IViewModelFactory
{
    LoginViewModel CreateLoginViewModel();
    RegisterViewModel CreateRegisterViewModel();
    
    MessageBoxViewModel CreateMessageBoxViewModel();
}