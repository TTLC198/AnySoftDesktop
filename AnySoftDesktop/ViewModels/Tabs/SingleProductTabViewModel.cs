using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class SingleProductTabViewModel : DashboardTabViewModel, INotifyPropertyChanged
{
    private readonly int _productId;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    private ProductResponseDto _product = new();

    public ProductResponseDto Product
    {
        get => _product;
        set
        {
            _product = value;
            OnPropertyChanged();
        }
    }
    
    public TabBaseViewModel? PreviousTab { get; set; }

    private bool _isInCart;

    public bool IsInCart
    {
        get => _isInCart;
        set
        {
            _isInCart = value;
            OnPropertyChanged();
        }
    }
    
    private bool _isBought;

    public bool IsBought
    {
        get => _isBought;
        set
        {
            _isBought = value;
            OnPropertyChanged();
        }
    }

    public SingleProductTabViewModel(int productId, IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(viewModelFactory, dialogManager)
    {
        _productId = productId;
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }

    public new async void OnViewFullyLoaded()
    {
        await UpdateProduct();
    }

    private async Task UpdateProduct()
    {
        var getProductsRequest = await WebApiService.GetCall($"api/products/{_productId}");

        try
        {
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var product = await JsonSerializer.DeserializeAsync<ProductResponseDto>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    Product = product ?? throw new InvalidOperationException("Product is null");
                }
                var getOrdersRequest = await WebApiService.GetCall("api/orders",  App.AuthorizationToken ?? "");
                if (getOrdersRequest.IsSuccessStatusCode)
                {
                    using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                    {
                        var responseStream = await getOrdersRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                        var orders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(responseStream,
                            CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        if (orders
                            .SelectMany(o => o.PurchasedProducts)
                            .Any(p => p.Id == Product.Id))
                            IsBought = true;
                    }
                }
                
                var getProductsInCartRequest = await WebApiService.GetCall($"api/cart", App.AuthorizationToken ?? "");
                if (getProductsInCartRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream = await getProductsInCartRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (products
                        .Any(p => p.Id == Product.Id))
                        IsInCart = true;
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
    
    public async void OnCartButtonClick(int id)
    {
        try
        {
            if (IsBought)
                return;
            if (IsInCart)
                OnRemoveFromCartButtonClick(id);
            else 
                OnAddToCartButtonClick(id);
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
    public async void OnAddToCartButtonClick(int id)
    {
        var jsonObject = new
        {
            productIds = new List<int>()
            {
                id
            }
        };
        var postProductsToCartRequest = await WebApiService.PostCall("api/cart", jsonObject, App.AuthorizationToken);
        try
        {
            if (postProductsToCartRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await postProductsToCartRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var cart = await JsonSerializer.DeserializeAsync<ShoppingCartResponseDto>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                if (cart.Products.Any(p => p.Id == Product.Id))
                {
                    IsInCart = true;
                }
            }
            else
            {
                var msg = await postProductsToCartRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{postProductsToCartRequest.ReasonPhrase}\n{msg}");
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
    
    public async void OnRemoveFromCartButtonClick(int id)
    {
        var postProductsToCartRequest = await WebApiService.DeleteCall($"api/cart?productId={id}", App.AuthorizationToken);
        try
        {
            if (postProductsToCartRequest.IsSuccessStatusCode)
            {
                IsInCart = false;
            }
            else
            {
                var msg = await postProductsToCartRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{postProductsToCartRequest.ReasonPhrase}\n{msg}");
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}