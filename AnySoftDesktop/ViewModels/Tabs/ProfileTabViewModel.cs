using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels.Tabs;

public class ProfileTabViewModel : SettingsTabViewModel, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private ApplicationUser _applicationUser;

    public ApplicationUser ApplicationUser
    {
        get => _applicationUser;
        set
        {
            _applicationUser = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<CustomPayment> _paymentMethods = new ObservableCollection<CustomPayment>();

    
    public ObservableCollection<CustomPayment> PaymentMethods
    {
        get => _paymentMethods;
        set
        {
            _paymentMethods = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<OrderResponseDto> _orders = new ObservableCollection<OrderResponseDto>();

    
    public ObservableCollection<OrderResponseDto> Orders
    {
        get => _orders;
        set
        {
            _orders = value;
            OnPropertyChanged();
        }
    }
    
    private string _userImagePath;

    public string UserImagePath
    {
        get => _userImagePath;
        set
        {
            _userImagePath = value;
            OnPropertyChanged();
        }
    }

    public new async void OnViewFullyLoaded()
    {
        await UpdatePayments();
        await UpdateOrders();
    }

    public async Task UpdatePayments()
    {
        try
        {
            var getPaymentsRequest = await WebApiService.GetCall("api/payment", App.ApplicationUser?.JwtToken!);
            if (getPaymentsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getPaymentsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                PaymentMethods = new ObservableCollection<CustomPayment>(payments
                    .ToList()
                    .Select(p => new CustomPayment()
                    {
                        Id = p.Id,
                        CardName = p.CardName,
                        Number = p.Number,
                        Cvc = p.Cvc,
                        ExpirationDate =
                            new CustomDate() {Month = p.ExpirationDate.Month, Year = p.ExpirationDate.Year},
                        UserId = p.UserId,
                        IsActive = p.IsActive
                    }));
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
    
    public async Task UpdateOrders()
    {
        try
        {
            var getOrdersRequest = await WebApiService.GetCall("api/orders", App.ApplicationUser?.JwtToken!);
            if (getOrdersRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getOrdersRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var orders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Orders = new ObservableCollection<OrderResponseDto>(orders.ToList());
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
    
    public async void OpenFileDialog()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".png",
            Filter = "Picture files (*.jpeg, *.png, *.jpg, *.gif)|*.jpeg;*.png;*.jpg;*.gif"
        };

        var result = dlg.ShowDialog();

        if (result != true) return;
        var filename = dlg.FileName;
        UserImagePath = filename;
    }

    public async void AddPayment()
    {
        PaymentMethods.Add(new CustomPayment() { CardName = "", Number = "", Cvc = "", IsEditComplete = true });
    }

    public async void SavePayment(CustomPayment payment)
    {
        try
        {
            var paymentDto = new PaymentDto()
            {
                CardName = payment.CardName,
                Number = payment.Number,
                ExpirationDate = new DateTime(payment.ExpirationDate!.Year, payment.ExpirationDate.Month, 1),
                Cvc = payment.Cvc
            };
            var postPaymentMethodRequest =
                await WebApiService.PostCall("api/payment", paymentDto, App.ApplicationUser?.JwtToken!);
            if (postPaymentMethodRequest.IsSuccessStatusCode)
            {
                await UpdatePayments();
            }
            else
            {
                var msg = await postPaymentMethodRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{postPaymentMethodRequest.ReasonPhrase}\n{msg}");
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

    public async void EditPayment(CustomPayment payment)
    {
        if (payment.IsEditComplete)
        {
            try
            {
                var paymentEditDto = new PaymentEditDto()
                {
                    Id = payment.Id.Value,
                    Number = payment.Number,
                    CardName = payment.CardName,
                    ExpirationDate = new DateTime(payment.ExpirationDate!.Year, payment.ExpirationDate.Month, 1),
                    Cvc = payment.Cvc
                };
                var putPaymentRequest =
                    await WebApiService.PutCall($"api/payment", paymentEditDto, App.ApplicationUser?.JwtToken!);
                if (putPaymentRequest.IsSuccessStatusCode)
                {
                    var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream =
                        await putPaymentRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                    var editedPayment = await JsonSerializer.DeserializeAsync<Payment>(
                        responseStream,
                        CustomJsonSerializerOptions.Options,
                        cancellationToken: cancellationTokenSource.Token);
                    var existedPayment = PaymentMethods.FirstOrDefault(p => p.Id == payment.Id);
                    if (editedPayment is null)
                        throw new InvalidOperationException("Existed payment does not exist");
                    if (existedPayment is null)
                        throw new InvalidOperationException("Existed payment does not exist");
                    existedPayment.IsEditComplete = false;
                    existedPayment.Number = editedPayment.Number;
                    existedPayment.CardName = editedPayment.CardName;
                    existedPayment.ExpirationDate = new CustomDate()
                        {Month = editedPayment.ExpirationDate.Month, Year = editedPayment.ExpirationDate.Year};
                    existedPayment.Cvc = editedPayment.Cvc;
                }
                else
                {
                    var msg = await putPaymentRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{putPaymentRequest.ReasonPhrase}\n{msg}");
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
        else
        {
            var existedPayment = PaymentMethods.FirstOrDefault(pm => pm.Id == payment.Id);
            if (existedPayment is not null)
                existedPayment.IsEditComplete = true;
        }
    }

    public async void RemovePayment(int id)
    {
        try
        {
            if (id == 0)
            {
                await UpdatePayments();
                return;
            }

            var postProductsToCartRequest =
                await WebApiService.DeleteCall($"api/payment/{id}", App.ApplicationUser?.JwtToken!);
            if (postProductsToCartRequest.IsSuccessStatusCode)
            {
                var paymentToDelete = PaymentMethods.FirstOrDefault(p => p.Id == id);
                if (paymentToDelete is null)
                    throw new InvalidOperationException("Payment not found");
                PaymentMethods.Remove(paymentToDelete);
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

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ProfileTabViewModel(ApplicationUser applicationUser, IViewModelFactory viewModelFactory,
        DialogManager dialogManager) : base(viewModelFactory, dialogManager)
    {
        ApplicationUser = applicationUser;
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }
}