using System;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using AnySoftDesktop.Services;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class LoginViewModel : DialogScreen
{
    private readonly DialogManager _dialogManager;
    private readonly IViewModelFactory _viewModelFactory;

    public LoginViewModel(DialogManager dialogManager, IViewModelFactory viewModelFactory)
    {
        _dialogManager = dialogManager;
        _viewModelFactory = viewModelFactory;
    }

    public UserDto UserCredentials { get; set; } = new UserDto();

    public void CloseView()
    {
        Close(false);
    }

    public async void AccountLogin()
    {
        var result = await WebApiService.PostCall("/auth/login", UserCredentials);
        if (result.IsSuccessStatusCode)
        {
            var timeoutAfter = TimeSpan.FromMilliseconds(100);
            using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
            
            var responseStream = await result.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
            var responseResult = await JsonSerializer.DeserializeAsync<JwtResponseModel>(responseStream, cancellationToken: cancellationTokenSource.Token);
            Close(true);
        }
        else
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"An error occurred while making a request to the server".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    } 
    public async void AccountRegister()
    {

    } 
}