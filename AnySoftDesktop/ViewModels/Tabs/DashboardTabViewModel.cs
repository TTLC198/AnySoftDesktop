using System;
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

    private ProductResponseDto _product = new ProductResponseDto();

    public ProductResponseDto Product
    {
        get => _product;
        set
        {
            _product = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ItemCarousel> _items;

    public ObservableCollection<ItemCarousel> Items
    {
        get => _items;
        set
        {
            _items = value;
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

    private void SelectFirstItem()
    {
        foreach (var item in Items)
        {
            item.IsSelected = item == Items[0];
        }
    }

    public void ShiftLeft()
    {
        // Moves the first game to the end
        var first = Items[0];
        Items.RemoveAt(0);
        Items.Add(first);
        SelectFirstItem();
    }

    public void ShiftRight()
    {
        // Moves the last game to the beginning
        var last = Items[^1];
        Items.RemoveAt(Items.Count - 1);
        Items.Insert(0, last);
        SelectFirstItem();
    }

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
                Product = Products.First();
                Items = new ObservableCollection<ItemCarousel>(
                    Product.Images
                        .Select(i => new ItemCarousel()
                        {
                            Image = i
                        }));
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