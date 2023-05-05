using AnySoftDesktop.Models;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class RegisterViewModel : DialogScreen<ApplicationUser?>
{
    private readonly DialogManager _dialogManager;
    private readonly IViewModelFactory _viewModelFactory;

    public RegisterViewModel(DialogManager dialogManager, IViewModelFactory viewModelFactory)
    {
        _dialogManager = dialogManager;
        _viewModelFactory = viewModelFactory;
    }

    public UserDto UserCredentials { get; set; } = new UserDto();
    
    public void CloseView()
    {
        Close(null);
    }
    
    public async void AccountLogin()
    {
        Close(null);
        var loginViewModel = _viewModelFactory.CreateLoginViewModel();
        await _dialogManager.ShowDialogAsync(loginViewModel);
    } 
    public async void AccountRegister()
    {
        //TODO доделать регистрацию
    }
}