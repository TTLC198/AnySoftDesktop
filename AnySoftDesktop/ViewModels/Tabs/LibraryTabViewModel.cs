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
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels.Tabs;

public class LibraryTabViewModel : TabBaseViewModel, INotifyPropertyChanged
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
    
    public async void OnViewFullyLoaded()
    {
        //await UpdateOrders();
    }

    private async void UpdateOrders(object? sender, EventArgs e)
    {
        var getOrdersRequest = await WebApiService.GetCall("api/orders",/* App.AuthorizationToken)*/ "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQiLCJyb2xlIjoidXNlciIsImp0aSI6ImVmYmRiMjgxLTY4N2YtNDEzZS1hYjZlLTFjMGRmYzUyM2Y2ZiIsImV4cCI6MTY4MzgzOTU5NywiaXNzIjoieW91ciBlbnZpcm9ubWVudCIsImF1ZCI6InlvdXIgYXVkaWVuY2UifQ.eFlEdthzSXldF9pM8HSC11MGiigCzQ_WobWwOHbDQXc");
        if (getOrdersRequest.IsSuccessStatusCode)
        {
            var timeoutAfter = TimeSpan.FromMilliseconds(300);
            using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
            {
                var responseStream = await getOrdersRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var orders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Products = new ObservableCollection<ProductResponseDto>(
                    orders
                        .SelectMany(o => o.PurchasedProducts)
                    );
            }
        }
    }
    
    public LibraryTabViewModel() : base(1, "Library")
    {
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