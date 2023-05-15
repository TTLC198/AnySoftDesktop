using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class ProductPurchaseDialogViewModel : DialogScreen<Payment>, INotifyPropertyChanged
{
    private readonly DialogManager _dialogManager;
    private readonly IViewModelFactory _viewModelFactory;

    public IReadOnlyList<Payment> PaymentMethods { get; }

    public ProductPurchaseDialogViewModel(DialogManager dialogManager, IViewModelFactory viewModelFactory, IReadOnlyList<Payment> paymentMethods)
    {
        _dialogManager = dialogManager;
        _viewModelFactory = viewModelFactory;
        PaymentMethods = paymentMethods;
    }
    
    public async void OnViewFullyLoaded()
    {
        
    }

    public void CloseView()
    {
        Close(null!);
    }
}