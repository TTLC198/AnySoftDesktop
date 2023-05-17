using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels.Tabs;

public class LibraryTabViewModel : TabBaseViewModel, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private ObservableCollection<ProductResponseDto> _products = new ObservableCollection<ProductResponseDto>();

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set
        {
            _products = value;
            OnPropertyChanged();
        }
    }

    public async void OnViewFullyLoaded()
    {
        
    }

    private async void UpdateOrders(object? sender, EventArgs e)
    {
        try
        {
            var getOrdersRequest = await WebApiService.GetCall("api/orders",  App.ApplicationUser?.JwtToken!);
            if (getOrdersRequest.IsSuccessStatusCode)
            {
                var orders = new List<OrderResponseDto>();
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream = await getOrdersRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var receivedOrders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    orders = receivedOrders.ToList();
                }
                var products = new List<ProductResponseDto>();
                foreach (var productId in orders
                             .Where(o => o.Status == "Paid")
                             .SelectMany(o => o.PurchasedProductsIds))
                {
                    var getProductRequest = await WebApiService.GetCall($"api/products/{productId}", App.ApplicationUser?.JwtToken!);
                    if (getProductRequest.IsSuccessStatusCode)
                    {
                        using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                        {
                            var responseStream =
                                await getProductRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                            var product = await JsonSerializer.DeserializeAsync<ProductResponseDto>(
                                responseStream,
                                CustomJsonSerializerOptions.Options,
                                cancellationToken: cancellationTokenSource.Token);
                            products.Add(product);
                        }
                    }
                }
                Products = new ObservableCollection<ProductResponseDto>(
                    products
                );
            }
            else
            {
                var msg = await getOrdersRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getOrdersRequest.ReasonPhrase}\n{msg}");
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

    public LibraryTabViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(1, "Library")
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        TabSelected += UpdateOrders;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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