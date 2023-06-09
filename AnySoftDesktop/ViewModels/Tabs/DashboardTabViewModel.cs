﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;

namespace AnySoftDesktop.ViewModels.Tabs;

public class DashboardTabViewModel : TabBaseViewModel, INotifyPropertyChanged
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

    private ProductResponseDto _mainProduct = new();

    public ProductResponseDto MainProduct
    {
        get => _mainProduct;
        set
        {
            _mainProduct = value;
            OnPropertyChanged();
        }
    }

    public DashboardTabViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(0, "Dashboard")
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }

    public async void OnViewFullyLoaded()
    {
        await UpdateProducts();
    }

    private async Task UpdateProducts()
    {
        try
        {
            var getProductsRequest = await WebApiService.GetCall("api/products");
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream =
                        await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(
                        responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    MainProduct = products.First();
                    Products = new ObservableCollection<ProductResponseDto>(products.Skip(1));
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