﻿using System;
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

    private ObservableCollection<CustomPayment> _paymentMethods;

    public ObservableCollection<CustomPayment> PaymentMethods
    {
        get => _paymentMethods;
        set
        {
            _paymentMethods = value;
            OnPropertyChanged();
        }
    }

    public async void OnViewFullyLoaded()
    {
        await UpdatePayments();
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
                        Number = p.Number,
                        Cvc = p.Cvc,
                        ExpirationDate =
                            new CustomDate() {Month = p.ExpirationDate.Month, Year = p.ExpirationDate.Year},
                        UserId = p.UserId,
                        IsActive = p.IsActive
                    }));
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
        PaymentMethods.Add(new CustomPayment());
    }

    public async void SavePayment(CustomPayment payment)
    {
        try
        {
            var paymentDto = new PaymentDto()
            {
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