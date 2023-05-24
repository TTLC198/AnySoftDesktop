using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;
using RPM_Project_Backend.Models;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class MainWindowViewModel : Screen, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    public ObservableCollection<Genre>? Genres { get; private set; }
    public ObservableCollection<Property>? Properties { get; private set; }
    public ObservableCollection<TabBaseViewModel> Tabs { get; }
    public TabBaseViewModel? ActiveTab { get; private set; }

    public bool IsMenuExpanded { get; set; }
    public bool IsWindowMaximized { get; set; }
    public bool IsAuthorized { get; set; }

    public string? SearchString { get; set; }
    public Genre? SelectedGenre { get; set; }
    public Property? SelectedProperty { get; set; }

    private ApplicationUser _currentUser = new ApplicationUser();

    public ApplicationUser CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel(List<TabBaseViewModel> tabs, IViewModelFactory viewModelFactory,
        DialogManager dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        Tabs = new BindableCollection<TabBaseViewModel>(tabs
            .OrderBy(t => t.Order)
            .ToList());
        // Pre-select first tab
        var firstTab = Tabs.FirstOrDefault();
        if (firstTab is not null)
            ActivateTab(firstTab);
    }

    public async void OnViewFullyLoaded()
    {
        try
        {
            var getGenresRequest = await WebApiService.GetCall("api/genres");
            var getPropertiesRequest = await WebApiService.GetCall("api/properties");
            var timeoutAfter = TimeSpan.FromMilliseconds(3000);
            if (getGenresRequest.IsSuccessStatusCode)
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getGenresRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var genres = await JsonSerializer.DeserializeAsync<IEnumerable<Genre>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Genres = new ObservableCollection<Genre>(genres?.OrderBy(g => g.Name)!);
            }

            if (getPropertiesRequest.IsSuccessStatusCode)
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream =
                    await getPropertiesRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var properties = await JsonSerializer.DeserializeAsync<IEnumerable<Property>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Properties = new ObservableCollection<Property>(properties?.OrderBy(p => p.Name)!);
            }
        }
        catch (Exception exception)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{exception.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }

    public void ActivateTab(TabBaseViewModel tab)
    {
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.IsSelected = true;
    }

    public void BackFromTab(TabBaseViewModel currentTab)
    {
        if (currentTab.BaseTab is null)
            return;

        Tabs.Insert(Tabs.IndexOf(currentTab), currentTab.BaseTab);
        Tabs.Remove(currentTab);

        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        ActiveTab = currentTab.PreviousTab ?? currentTab.BaseTab;
        ActiveTab.OnTabSelected(EventArgs.Empty);
        ActiveTab.IsSelected = true;
    }

    public void OnProductButtonClick(int id)
    {
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        var tab = new SingleProductTabViewModel(id, _viewModelFactory, _dialogManager);

        var baseTab = Tabs.First(t => t != tab && tab.GetType().IsSubclassOf(t.GetType().BaseType!));
        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
        Tabs.Remove(baseTab);

        tab.BaseTab = baseTab;
        tab.PreviousTab = ActiveTab;
        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.IsSelected = true;
    }

    public void OnProfileButtonClick()
    {
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        if (Tabs.FirstOrDefault(t => t.GetType() == typeof(ProfileTabViewModel)) is not null and ProfileTabViewModel tab)
        {
            tab.ApplicationUser = CurrentUser;
        }
        else
        {
            tab = new ProfileTabViewModel(CurrentUser, _viewModelFactory, _dialogManager);

            var baseTab = Tabs.First(t => t != tab && tab.GetType().IsSubclassOf(t.GetType()));
            Tabs.Insert(Tabs.IndexOf(baseTab), tab);
            Tabs.Remove(baseTab);

            tab.PreviousTab = ActiveTab;
            tab.BaseTab = baseTab;
            ActiveTab = tab;
            tab.OnTabSelected(EventArgs.Empty);
            tab.IsSelected = true;
        }
    }

    public async void OpenLoginPage()
    {
        if (IsAuthorized) return;
        var user = await _dialogManager.ShowDialogAsync(_viewModelFactory.CreateLoginViewModel());

        if (user is null) return;
        IsAuthorized = true;
        CurrentUser = user;
    }

    public async void OpenProductsByGenre(int id)
    {
        SelectedGenre = Genres?.First(g => g.Id == id);
        OpenSearchPage();
    }

    public async void OpenSearchPage()
    {
        try
        {
            var productRequestDto = new ProductRequestDto
            {
                Name = string.IsNullOrEmpty(SearchString)
                    ? null
                    : SearchString,
                Genres = SelectedGenre is {Id: > 0}
                    ? new List<int>() {SelectedGenre.Id}
                    : null,
                Properties = SelectedProperty is {Id: > 0}
                    ? new List<int>() {SelectedProperty.Id}
                    : null,
            };
            var productRequestQueryJson =
                HttpUtility.UrlEncode(JsonSerializer.Serialize(productRequestDto, CustomJsonSerializerOptions.Options));
            var getProductsRequest = await WebApiService.GetCall($"api/products?Query={productRequestQueryJson}");
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);

                if (ActiveTab is not null)
                    ActiveTab.IsSelected = false;
                var tab = Tabs.FirstOrDefault(t => t.GetType() == typeof(MultipleProductTabViewModel));
                if (tab is not null and MultipleProductTabViewModel multipleProductTabViewModel)
                {
                    multipleProductTabViewModel.Products = new ObservableCollection<ProductResponseDto>(products!);
                }
                else
                {
                    tab = new MultipleProductTabViewModel(products!, _viewModelFactory, _dialogManager);

                    var baseTab = Tabs.FirstOrDefault(t => t != tab && tab.GetType().IsSubclassOf(t.GetType()));
                    if (baseTab != null)
                    {
                        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                        Tabs.Remove(baseTab);
                        tab.BaseTab = baseTab;
                    }
                    
                    tab.PreviousTab = ActiveTab;
                    ActiveTab = tab;
                    tab.OnTabSelected(EventArgs.Empty);
                    tab.IsSelected = true;
                }
            }
            else
            {
                var msg = await getProductsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getProductsRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{exception.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }

    public async void OpenShoppingCart()
    {
        try
        {
            var getProductsRequest = await WebApiService.GetCall($"api/cart", App.ApplicationUser?.JwtToken!);
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);

                if (ActiveTab is not null)
                    ActiveTab.IsSelected = false;
                var tab = Tabs.FirstOrDefault(t => t.GetType() == typeof(ShoppingCartTabViewModel));
                if (tab is not null and ShoppingCartTabViewModel shoppingCartTabViewModel)
                {
                    shoppingCartTabViewModel.Products = new ObservableCollection<ProductResponseDto>(products!);
                }
                else
                {
                    tab = new ShoppingCartTabViewModel(products!, _viewModelFactory, _dialogManager);

                    var baseTab = Tabs.FirstOrDefault(t => t != tab && tab.GetType().IsSubclassOf(t.GetType()));
                    if (baseTab != null)
                    {
                        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                        Tabs.Remove(baseTab);
                        tab.BaseTab = baseTab;
                    }
                    
                    tab.PreviousTab = ActiveTab;
                    ActiveTab = tab;
                    tab.OnTabSelected(EventArgs.Empty);
                    tab.IsSelected = true;
                }
            }
            else
            {
                var msg = await getProductsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getProductsRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{exception.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }

    public async void OnProfileChanged(string imagePath)
    {
        try
        {
            var userEdit = new UserEditDto()
            {
                Id = CurrentUser.Id ?? 0,
                Login = CurrentUser.Login,
                Email = CurrentUser.Email,
                Password = CurrentUser.Password
            };
            var putChangesOfUserRequest =
                await WebApiService.PutCall("api/users", userEdit, App.ApplicationUser?.JwtToken!);
            if (putChangesOfUserRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream =
                        await putChangesOfUserRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var editedUser = await JsonSerializer.DeserializeAsync<User>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (editedUser is null)
                        throw new InvalidOperationException("User is null");
                }

                if (string.IsNullOrEmpty(imagePath))
                    return;

                var deletePreviousImageRequest = await WebApiService.DeleteCall(
                    $"resources/image/delete/{CurrentUser.Image?.Split('/').Last().Split('.').First()}",
                    App.ApplicationUser?.JwtToken!);
                if (!deletePreviousImageRequest.IsSuccessStatusCode) return;

                var formContent = new MultipartFormDataContent();

                var stream = File.OpenRead(imagePath);
                formContent.Add(new StringContent(CurrentUser.Id.ToString()!), "userId");

                var imageContent = new StreamContent(stream);
                imageContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse($"image/{imagePath.Split('/').Last().Split('.').Last()}");
                formContent.Add(imageContent, "image", $"{imagePath.Split('/').Last()}");

                var postImageRequest = await WebApiService.PostCall(
                    "resources/image/upload",
                    formContent,
                    App.ApplicationUser?.JwtToken!);

                if (postImageRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream =
                        await postImageRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var newImagePath = await JsonSerializer.DeserializeAsync<string>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    CurrentUser.Image = newImagePath;
                    Refresh();
                }
            }
            else
            {
                var msg = await putChangesOfUserRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{putChangesOfUserRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{exception.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }

    public async void OnOrderRequest()
    {
        try
        {
            var cartOrderRequest =
                await WebApiService.PostCall("api/cart/order", null!, App.ApplicationUser?.JwtToken!);
            if (cartOrderRequest.IsSuccessStatusCode)
            {
                var createdOrder = new OrderResponseDto();
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream =
                        await cartOrderRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    createdOrder = await JsonSerializer.DeserializeAsync<OrderResponseDto>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (createdOrder is null)
                        throw new InvalidOperationException("Order is null");
                }

                var getPaymentsRequest = await WebApiService.GetCall("api/payment", App.ApplicationUser?.JwtToken!);
                if (getPaymentsRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream =
                        await getPaymentsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);

                    var paymentDialog = _viewModelFactory.CreatePurchaseDialog(payments?.ToList()!);
                    var selectedPaymentMethod = await _dialogManager.ShowDialogAsync(paymentDialog);
                    if (selectedPaymentMethod is null)
                        throw new InvalidOperationException("Payment method not selected");

                    var orderPurchase = new OrderPurchaseDto()
                    {
                        OrderId = createdOrder.Id,
                        PaymentId = selectedPaymentMethod.Id ?? -1
                    };

                    var orderConfirmationRequest = await WebApiService.PostCall("api/orders/buy", orderPurchase,
                        App.ApplicationUser?.JwtToken!);
                    if (!orderConfirmationRequest.IsSuccessStatusCode) return;
                    var tab = Tabs.FirstOrDefault(t => t.GetType() == typeof(LibraryTabViewModel));
                    if (tab is null)
                        return;

                    var baseTab = Tabs.FirstOrDefault(t => t != tab && tab.GetType().IsSubclassOf(t.GetType()));
                    if (baseTab != null)
                    {
                        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                        Tabs.Remove(baseTab);
                        tab.BaseTab = baseTab;
                    }
                    
                    tab.PreviousTab = ActiveTab;
                    ActiveTab = tab;
                    tab.OnTabSelected(EventArgs.Empty);
                    tab.IsSelected = true;
                }
                else
                {
                    var msg = await getPaymentsRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{getPaymentsRequest.ReasonPhrase}\n{msg}");
                }
            }
            else
            {
                var msg = await cartOrderRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{cartOrderRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{exception.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }

    public async void OnOrderPayment(int orderId)
    {
        try
        {
            var getPaymentsRequest = await WebApiService.GetCall("api/payment", App.ApplicationUser?.JwtToken!);
            var timeoutAfter = TimeSpan.FromMilliseconds(3000);
            if (getPaymentsRequest.IsSuccessStatusCode)
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream =
                    await getPaymentsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);

                var paymentDialog = _viewModelFactory.CreatePurchaseDialog(payments?.ToList()!);
                var selectedPaymentMethod = await _dialogManager.ShowDialogAsync(paymentDialog);
                if (selectedPaymentMethod is null)
                    throw new InvalidOperationException("Payment method not selected");

                var orderPurchase = new OrderPurchaseDto()
                {
                    OrderId = orderId,
                    PaymentId = selectedPaymentMethod.Id ?? -1
                };

                var orderConfirmationRequest = await WebApiService.PostCall("api/orders/buy", orderPurchase,
                    App.ApplicationUser?.JwtToken!);
                if (!orderConfirmationRequest.IsSuccessStatusCode)
                {
                    // ReSharper disable once MethodSupportsCancellation
                    var msg = await orderConfirmationRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{orderConfirmationRequest.ReasonPhrase}\n{msg}");
                }
            }
            else
            {
                var msg = await getPaymentsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getPaymentsRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            var messageBoxDialog = _viewModelFactory.CreateMessageBoxViewModel(
                title: "Some error has occurred",
                message: $@"{exception.Message}".Trim(),
                okButtonText: "OK",
                cancelButtonText: null
            );
            await _dialogManager.ShowDialogAsync(messageBoxDialog);
        }
    }

    public void Logout()
    {
        IsAuthorized = false;
        CurrentUser = new ApplicationUser();
        Refresh();
    }

    public void ToggleMaximized() =>
        IsWindowMaximized = !IsWindowMaximized;

    public void ToggleMenu() =>
        IsMenuExpanded = !IsMenuExpanded;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}