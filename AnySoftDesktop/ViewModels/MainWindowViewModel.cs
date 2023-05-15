using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Web;
using System.Windows.Documents;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class MainWindowViewModel : Screen, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    
    public ObservableCollection<Genre> Genres { get; set; }
    public ObservableCollection<Property> Properties { get; set; }
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
        var getGenresRequest = await WebApiService.GetCall("api/genres");
        var getPropertiesRequest = await WebApiService.GetCall("api/properties");
        try
        {
            var timeoutAfter = TimeSpan.FromMilliseconds(300);
            if (getGenresRequest.IsSuccessStatusCode)
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getGenresRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var genres = await JsonSerializer.DeserializeAsync<IEnumerable<Genre>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Genres = new ObservableCollection<Genre>(genres.OrderBy(g => g.Name).Prepend(new Genre() {Id = -1, Name = "Empty"}));
            }
            if (getPropertiesRequest.IsSuccessStatusCode)
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getPropertiesRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var properties = await JsonSerializer.DeserializeAsync<IEnumerable<Property>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Properties = new ObservableCollection<Property>(properties.OrderBy(p => p.Name).Prepend(new Property() {Id = -1, Name = "Empty"}));
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
        var singleProductTab = Tabs.FirstOrDefault(t => t.GetType() == typeof(SingleProductTabViewModel));
        if (singleProductTab is not null)
        {
            var baseTab = Tabs.First(t => t.GetType() == singleProductTab.GetType().BaseType);
            baseTab.IsVisible = true;
            Tabs.Remove(singleProductTab);
        }

        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.IsSelected = true;
    }
    
    public void BackFromProfile() //костыль
    {
        var currentTab = Tabs.FirstOrDefault(t => t.GetType() == typeof(ProfileTabViewModel));
        if (currentTab is not ProfileTabViewModel)
            return;

        var baseTab = Tabs.First(t => t.GetType() == currentTab.GetType().BaseType);
        baseTab.IsVisible = true;
        Tabs.Remove(currentTab);
        
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
        ActiveTab = Tabs.First(t => t == (currentTab as ProfileTabViewModel).PreviousTab);
        ActiveTab.OnTabSelected(EventArgs.Empty);
        ActiveTab.IsSelected = true;
    }

    public void BackFromProduct() //костыль
    {
        var currentTab = Tabs.FirstOrDefault(t => t.GetType() == typeof(SingleProductTabViewModel));
        if (currentTab is not SingleProductTabViewModel)
            return;

        var baseTab = Tabs.First(t => t.GetType() == currentTab.GetType().BaseType);
        baseTab.IsVisible = true;
        Tabs.Remove(currentTab);
        
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
        ActiveTab = Tabs.First(t => t == (currentTab as SingleProductTabViewModel).PreviousTab);
        ActiveTab.OnTabSelected(EventArgs.Empty);
        ActiveTab.IsSelected = true;
    }
    
    public void BackFromProducts() //костыль
    {
        var currentTab = Tabs.FirstOrDefault(t => t is MultipleProductTabViewModel);
        if (currentTab is not MultipleProductTabViewModel)
            return;

        var baseTab = Tabs.First(t => t != currentTab && currentTab.GetType().IsSubclassOf(t.GetType()));
        baseTab.IsVisible = true;
        Tabs.Remove(currentTab);
        
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
        ActiveTab = Tabs.First(t => t == (currentTab as MultipleProductTabViewModel).PreviousTab);
        ActiveTab.OnTabSelected(EventArgs.Empty);
        ActiveTab.IsSelected = true;
    }

    public void OnProductButtonClick(int id)
    {
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        var tab = new SingleProductTabViewModel(id, _viewModelFactory, _dialogManager);

        var baseTab = Tabs.First(t => t.GetType() == tab.GetType().BaseType);
        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
        baseTab.IsVisible = false;

        tab.PreviousTab = ActiveTab;
        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.IsSelected = true;
        tab.IsVisible = true;
    }
    
    public void OnProfileButtonClick()
    {
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        if (Tabs.FirstOrDefault(t => t.GetType() == typeof(ProfileTabViewModel)) is ProfileTabViewModel tab)
        {
            tab.ApplicationUser = CurrentUser;
        }
        else
        {
            tab = new ProfileTabViewModel(CurrentUser, _viewModelFactory, _dialogManager);

            var baseTab = Tabs.First(t => t.GetType() == tab.GetType().BaseType);
            Tabs.Insert(Tabs.IndexOf(baseTab), tab);
            baseTab.IsVisible = false;

            tab.PreviousTab = ActiveTab;
        }
        
        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.IsSelected = true;
        tab.IsVisible = true;
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
        SelectedGenre = Genres.First(g => g.Id == id);
        OpenSearchPage();
    }

    public async void OpenSearchPage()
    {
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
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
        var productRequestQueryJson = HttpUtility.UrlEncode(JsonSerializer.Serialize(productRequestDto, CustomJsonSerializerOptions.Options));
        var getProductsRequest = await WebApiService.GetCall($"api/products?Query={productRequestQueryJson}");
        try
        {
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                
                if (Tabs.FirstOrDefault(t => t.GetType() == typeof(MultipleProductTabViewModel)) is MultipleProductTabViewModel tab)
                {
                    tab.Products = new ObservableCollection<ProductResponseDto>(products!);
                }
                else
                {
                    tab = new MultipleProductTabViewModel(products!, _viewModelFactory, _dialogManager);
                    var baseTab = Tabs.First(t => t != tab && tab.GetType().IsSubclassOf(t.GetType()));
                    Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                    baseTab.IsVisible = false;
                    tab.PreviousTab = ActiveTab;
                }
                
                ActiveTab = tab;
                tab.OnTabSelected(EventArgs.Empty);
                tab.IsSelected = true;
                tab.IsVisible = true;
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
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        var getProductsRequest = await WebApiService.GetCall($"api/cart", App.AuthorizationToken ?? "");
        try
        {
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                
                if (Tabs.FirstOrDefault(t => t.GetType() == typeof(ShoppingCartTabViewModel)) is ShoppingCartTabViewModel tab)
                {
                    tab.Products = new ObservableCollection<ProductResponseDto>(products!);
                }
                else
                {
                    tab = new ShoppingCartTabViewModel(products!, _viewModelFactory, _dialogManager);
                    var baseTab = Tabs.First(t => t != tab && tab.GetType().IsSubclassOf(t.GetType()));
                    Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                    baseTab.IsVisible = false;
                    tab.PreviousTab = ActiveTab;
                }
                
                ActiveTab = tab;
                tab.OnTabSelected(EventArgs.Empty);
                tab.IsSelected = true;
                tab.IsVisible = true;
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
    
    public async void OnProfileChanged(ApplicationUser applicationUser)
    {
        var userEdit = new UserEditDto()
        {
            Id = applicationUser.Id ?? 0,
            Login = applicationUser.Login,
            Email = applicationUser.Email,
            Password = applicationUser.Password
        };
        var putChangesOfUserRequest = await WebApiService.PutCall("api/users", userEdit, App.AuthorizationToken);
        try
        {
            if (putChangesOfUserRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await putChangesOfUserRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var editedUser = await JsonSerializer.DeserializeAsync<User>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                if (editedUser is null)
                    throw new InvalidOperationException("User is null");
                CurrentUser.Id = editedUser.Id;
                CurrentUser.Email = editedUser.Email;
                CurrentUser.Login = editedUser.Login;
                /*CurrentUser.Image = editedUser.Id;*/
                //TODO image edit
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
        var cartOrderRequest = await WebApiService.PostCall("api/cart/order", null!, App.AuthorizationToken);
        try
        {
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

                var getPaymentsRequest = await WebApiService.GetCall("api/payment",  App.AuthorizationToken ?? "");
                if (getPaymentsRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream = await getPaymentsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    
                    var paymentDialog = _viewModelFactory.CreatePurchaseDialog(payments.ToList());
                    var selectedPaymentMethod = await _dialogManager.ShowDialogAsync(paymentDialog);
                    if (selectedPaymentMethod is null)
                        throw new InvalidOperationException("Payment method not selected");
                    
                    var orderPurchase = new OrderPurchaseDto()
                    {
                        OrderId = createdOrder.Id,
                        PaymentId = selectedPaymentMethod.Id
                    };
                
                    var orderConfirmationRequest = await WebApiService.PostCall("api/orders/buy", orderPurchase, App.AuthorizationToken);
                    if (!orderConfirmationRequest.IsSuccessStatusCode) return;
                    var tab = Tabs.FirstOrDefault(t => t.GetType() == typeof(LibraryTabViewModel));
                    ActiveTab = tab;
                    tab.IsSelected = true;
                    tab.IsVisible = true;
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

    public void Logout()
    {
        IsAuthorized = false;
        CurrentUser = new ApplicationUser();
    }

    public void ToggleMaximized() =>
        IsWindowMaximized = !IsWindowMaximized;

    public void ToggleMenu() =>
        IsMenuExpanded = !IsMenuExpanded;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}