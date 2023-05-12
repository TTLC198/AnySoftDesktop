using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class MultipleProductViewModel : DashboardTabViewModel, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    private ObservableCollection<ProductResponseDto> _products = new();

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set
        {
            _products = value;
            OnPropertyChanged();
        }
    }
    
    private ProductRequestDto _productRequestDto = new();

    public ProductRequestDto ProductRequestDto
    {
        get => _productRequestDto;
        set
        {
            _productRequestDto = value;
            OnPropertyChanged();
        }
    }
    
    public TabBaseViewModel PreviousTab { get; set; }
    
    public MultipleProductViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(viewModelFactory, dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }
    
    public new async void OnViewFullyLoaded()
    {
        await UpdateProducts();
    }

    private async Task UpdateProducts()
    {
        var productRequestQueryJson = HttpUtility.UrlEncode(JsonSerializer.Serialize(_productRequestDto, CustomJsonSerializerOptions.Options));
        var getProductsRequest = await WebApiService.GetCall($"api/products?Query={productRequestQueryJson}");
        try
        {
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(300);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Products = new ObservableCollection<ProductResponseDto>(products);
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