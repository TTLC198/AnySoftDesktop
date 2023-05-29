using System.Collections.Generic;
using System.ComponentModel;
using AnySoftBackend.Domain;
using AnySoftDesktop.ViewModels.Framework;

namespace AnySoftDesktop.ViewModels;

public class ProductPurchaseDialogViewModel : DialogScreen<Payment>, INotifyPropertyChanged
{
    private readonly DialogManager _dialogManager;
    private readonly IViewModelFactory _viewModelFactory;

    public List<Payment> PaymentMethods { get; set; }
    public Payment? SelectedPaymentMethod { get; set; }
    public bool Confirmation { get; set; }

    public ProductPurchaseDialogViewModel(DialogManager dialogManager, IViewModelFactory viewModelFactory, List<Payment> paymentMethods)
    {
        _dialogManager = dialogManager;
        _viewModelFactory = viewModelFactory;
        PaymentMethods = paymentMethods;
    }
    
    public async void OnViewFullyLoaded()
    {
        
    }
    
    public void OnConfirmation()
    {
        if (Confirmation && SelectedPaymentMethod is not null)
            Close(SelectedPaymentMethod);
    }

    public void CloseView()
    {
        Close(null!);
    }
}

public static class ProductPurchaseDialogViewModelExtensions
{
    public static ProductPurchaseDialogViewModel CreatePurchaseDialog(
        this IViewModelFactory factory,
        List<Payment> paymentMethods)
    {
        var viewModel = factory.CreatePurchaseDialog();
        viewModel.PaymentMethods = paymentMethods;

        return viewModel;
    }
}