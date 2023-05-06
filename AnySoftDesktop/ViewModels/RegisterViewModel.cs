using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Web;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;
using RPM_Project_Backend.Models;
using Image = System.Windows.Controls.Image;

namespace AnySoftDesktop.ViewModels;

public class RegisterViewModel : DialogScreen<ApplicationUser?>, INotifyPropertyChanged
{
    private readonly DialogManager _dialogManager;
    private readonly IViewModelFactory _viewModelFactory;

    public RegisterViewModel(DialogManager dialogManager, IViewModelFactory viewModelFactory)
    {
        _dialogManager = dialogManager;
        _viewModelFactory = viewModelFactory;
    }

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
        Close(null);
        var loginViewModel = _viewModelFactory.CreateLoginViewModel();
        await _dialogManager.ShowDialogAsync(loginViewModel);
    }

    public async void AccountRegister()
    {
        var validationContext = new ValidationContext(UserCredentials, null, null);
        var results = new List<ValidationResult>();
        var timeoutAfter = TimeSpan.FromMilliseconds(100);

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}