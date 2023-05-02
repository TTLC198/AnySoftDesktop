using AnySoftDesktop.ViewModels.Framework;

namespace AnySoftDesktop.ViewModels;

public class LoginViewModel : DialogScreen
{
    public string Login
    {
        get;
        set;
    }
    
    public string Email
    {
        get;
        set;
    }
    
    public string Password
    {
        get;
        set;
    }
}