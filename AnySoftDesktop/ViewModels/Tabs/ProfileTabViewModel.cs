using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
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
    
    private ObservableCollection<Payment> _paymentMethods;

    public ObservableCollection<Payment> PaymentMethods
    {
        get => _paymentMethods;
        set
        {
            _paymentMethods = value;
            OnPropertyChanged();
        }
    }
    
    public TabBaseViewModel? PreviousTab { get; set; }
    
    public async void OnViewFullyLoaded()
    {
        try
        {
            var getPaymentsRequest = await WebApiService.GetCall("api/payment",  App.AuthorizationToken ?? "");
            if (getPaymentsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getPaymentsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                PaymentMethods = new ObservableCollection<Payment>(payments);
            }
            else
            {
                var msg = await getPaymentsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getPaymentsRequest.ReasonPhrase}\n{msg}");
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
    
    public async void AddPayment()
    {
        PaymentMethods.Add(new Payment());
        /*var postProductsToCartRequest = await WebApiService.PostCall($"api/payment", payment, App.AuthorizationToken);
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
        }*/
    }
    
    public async void EditPayment(Payment payment)
    {
        var paymentEditDto = new PaymentEditDto()
        {
            Id = payment.Id,
            Number = payment.Number,
            ExpirationDate = payment.ExpirationDate,
            Cvc = payment.Cvc
        };
        var putPaymentRequest = await WebApiService.PutCall($"api/payment", paymentEditDto, App.AuthorizationToken);
        try
        {
            if (putPaymentRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await putPaymentRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var editedPayment = await JsonSerializer.DeserializeAsync<Payment>(
                    responseStream,
                    CustomJsonSerializerOptions.Options,
                    cancellationToken: cancellationTokenSource.Token);
                var existedPayment = PaymentMethods.FirstOrDefault(p => p.Id == payment.Id);
                if (existedPayment is null)
                    throw new InvalidOperationException("Existed payment does not exist");
                existedPayment.Number = editedPayment.Number;
                existedPayment.ExpirationDate = editedPayment.ExpirationDate;
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
    
    public async void RemovePayment(int id)
    {
        var postProductsToCartRequest = await WebApiService.DeleteCall($"api/payment/{id}", App.AuthorizationToken);
        try
        {
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

    public ProfileTabViewModel(ApplicationUser applicationUser, IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(viewModelFactory, dialogManager)
    {
        ApplicationUser = applicationUser;
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }
}