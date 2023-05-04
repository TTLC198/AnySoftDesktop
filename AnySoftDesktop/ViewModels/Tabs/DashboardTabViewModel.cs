﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
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

public class DashboardTabViewModel : TabBaseViewModel, INotifyPropertyChanged
{
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

    public DashboardTabViewModel() : base(0, "Dashboard")
    {
    }

    public async void OnViewFullyLoaded()
    {
        await UpdateProducts();
    }

    /*
    public void ShiftLeft()
    {
        var index = Products.IndexOf(SelectedProduct);
        SelectedProduct = 
            index == 0 
                ? Products.Last() 
                : Products[index - 1];
    }

    public void ShiftRight()
    {
        var index = Products.IndexOf(SelectedProduct);
        SelectedProduct = 
            index == Products.Count 
                ? Products.First() 
                : Products[index + 1];
    }*/

    public async Task UpdateProducts()
    {
        var getProductsRequest = await WebApiService.GetCall("api/products");
        if (getProductsRequest.IsSuccessStatusCode)
        {
            var timeoutAfter = TimeSpan.FromMilliseconds(300);
            using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
            {
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Products = new ObservableCollection<ProductResponseDto>(products!);
                MainProduct = Products.First();
            }
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