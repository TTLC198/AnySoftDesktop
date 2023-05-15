namespace AnySoftDesktop.ViewModels.Framework;

public interface IViewModelFactory
{
    LoginViewModel CreateLoginViewModel();

    ProductPurchaseDialogViewModel CreatePurchaseDialog();
    
    MessageBoxViewModel CreateMessageBoxViewModel();
}