using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Web;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using Microsoft.IdentityModel.Tokens;
using RPM_Project_Backend.Domain;
using RPM_Project_Backend.Helpers;
using RPM_Project_Backend.Models;

namespace AnySoftDesktop.ViewModels;

public class LoginViewModel : DialogScreen<ApplicationUser?>, INotifyPropertyChanged
{
    public LoginViewModel(DialogManager dialogManager, IViewModelFactory viewModelFactory)
    {
        _dialogManager = dialogManager;
        _viewModelFactory = viewModelFactory;
    }
    
    private readonly DialogManager _dialogManager;
    private readonly IViewModelFactory _viewModelFactory;

    public bool IsRegisterView { get; set; }

    public UserDto UserCredentials { get; set; } = new UserDto();
    
    private string _userImagePath;

    public string UserImagePath
    {
        get => _userImagePath;
        set
        {
            _userImagePath = value;
            OnPropertyChanged();
        }
    }
    
    public void CloseView()
    {
        Close(null);
    }
    
    public async void OpenFileDialog()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".png",
            Filter =
                "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif"
        };

        var result = dlg.ShowDialog();

        if (result != true) return;
        var filename = dlg.FileName;
        UserImagePath = filename;
    }

    public async void AccountLogin()
    {
        if (IsRegisterView)
        {
            IsRegisterView = false;
            return;
        }
        
        var validationContext = new ValidationContext(UserCredentials, null, null);
        var results = new List<ValidationResult>();
        var timeoutAfter = TimeSpan.FromMilliseconds(100);

        if (Validator.TryValidateObject(UserCredentials, validationContext, results, true))
        {
            var getTokenRequest = await WebApiService.PostCall("api/auth/login", UserCredentials);
            if (getTokenRequest.IsSuccessStatusCode)
            {
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
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getTokenRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var errorModel = await JsonSerializer.DeserializeAsync<ErrorModel>(responseStream, CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                
                var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                    title: "Some error has occurred",
                    message: $@"{(errorModel ?? new ErrorModel("An error occurred while making a request to the server")).Message}".Trim(),
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
        if (!IsRegisterView)
        {
            IsRegisterView = true;
            return;
        }
        
        var validationContext = new ValidationContext(UserCredentials, null, null);
        var results = new List<ValidationResult>();
        var timeoutAfter = TimeSpan.FromMilliseconds(100);

        try
        {
            if (Validator.TryValidateObject(UserCredentials, validationContext, results, true))
            {
                var postUserRequest = await WebApiService.PostCall("api/users", UserCredentials);
                if (postUserRequest.IsSuccessStatusCode)
                {
                    UserResponseDto? user;
                    using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                    {
                        var responseStream = await postUserRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                        user = await JsonSerializer.DeserializeAsync<UserResponseDto>(responseStream,
                            CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        if (user is null)
                            throw new InvalidOperationException("User is null");
                    }

                    var getTokenRequest = await WebApiService.PostCall("api/auth/login", UserCredentials);
                    if (getTokenRequest.IsSuccessStatusCode)
                    {
                        string? tokenString;
                        using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                        {
                            var responseStream =
                                await getTokenRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                            tokenString = await JsonSerializer.DeserializeAsync<string>(responseStream,
                                CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        }

                        var formContent = new MultipartFormDataContent();

                        var stream = File.OpenRead(UserImagePath);
                        formContent.Add(new StringContent(user.Id.ToString()), "userId");

                        var imageContent = new StreamContent(stream);
                        imageContent.Headers.ContentType =
                            MediaTypeHeaderValue.Parse($"image/{UserImagePath.Split('/').Last().Split('.').Last()}");
                        formContent.Add(imageContent, "image", $"{UserImagePath.Split('/').Last()}");
                    
                        var postImageRequest = await WebApiService.PostCall(
                            "resources/image/upload",
                            formContent,
                            tokenString!);

                        if (!postImageRequest.IsSuccessStatusCode) return;
                        {
                            using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                            var responseStream =
                                await postImageRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                            var image = await JsonSerializer.DeserializeAsync<RPM_Project_Backend.Domain.Image>(responseStream,
                                CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                            user.Image = HttpUtility.UrlPathEncode("/resources/image/" + string.Join(@"/", image!.ImagePath
                                .Split('\\')
                                .SkipWhile(s => s != "wwwroot")
                                .Skip(1)));
                            Close(new ApplicationUser
                            {
                                Id = user.Id,
                                Login = user.Login,
                                Email = user.Email,
                                JwtToken = tokenString,
                                Image = user.Image
                            });
                        }
                    }
                }
                else
                {
                    Close(null);
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream = await postUserRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var errorModel = await JsonSerializer.DeserializeAsync<ErrorModel>(responseStream, CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                
                    var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                        title: "Some error has occurred",
                        message: $@"{(errorModel ?? new ErrorModel("An error occurred while making a request to the server")).Message}".Trim(),
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
        catch (Exception e)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{e.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}