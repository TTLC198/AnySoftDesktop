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
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class SingleProductViewModel : DashboardTabViewModel, INotifyPropertyChanged
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
    
    public TabBaseViewModel PreviousTab { get; set; }

    public SingleProductViewModel(int productId, IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(viewModelFactory, dialogManager)
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
                var timeoutAfter = TimeSpan.FromMilliseconds(300);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var product = await JsonSerializer.DeserializeAsync<ProductResponseDto>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Product = product ?? throw new InvalidOperationException("Product is null");
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