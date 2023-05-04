using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using Microsoft.IdentityModel.Tokens;
using RPM_Project_Backend.Domain;
using RPM_Project_Backend.Helpers;

namespace AnySoftDesktop.ViewModels;

public class LoginViewModel : DialogScreen<ApplicationUser?>
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
        Close(null);
    }

    public async void AccountLogin()
    {
        var validationContext = new ValidationContext(UserCredentials, null, null);
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(UserCredentials, validationContext, results, true))
        {
            var getTokenRequest = await WebApiService.PostCall("api/auth/login", UserCredentials);
            if (getTokenRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(100);
                string? userId, tokenString;
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream = await getTokenRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    tokenString = await JsonSerializer.DeserializeAsync<string>(responseStream, CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(tokenString);
                    userId = token.Payload.Claims.First(cl => cl.Type == "id").Value;
                }

                var getUserRequest = await WebApiService.GetCall($"api/users/{userId}", tokenString!);
                if (getTokenRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream = await getUserRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var responseUserResult = await JsonSerializer.DeserializeAsync<UserResponseDto>(responseStream, CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (responseUserResult != null)
                        Close(new ApplicationUser
                        {
                            Id = responseUserResult.Id,
                            Login = responseUserResult.Login,
                            Email = responseUserResult.Email,
                            JwtToken = tokenString,
                            Image = responseUserResult.Image
                        });
                }
            }
            else
            {
                Close(null);
                var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                    title: "Some error has occurred",
                    message: $@"An error occurred while making a request to the server".Trim(),
                    okButtonText: "OK",
                    cancelButtonText: null
                );
                await _dialogManager.ShowDialogAsync(messageBoxDialog);
            }
        }
        else
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"Please enter valid values!".Trim(),
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