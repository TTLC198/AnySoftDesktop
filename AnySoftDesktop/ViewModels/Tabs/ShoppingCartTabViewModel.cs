using System.Collections.Generic;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftDesktop.ViewModels.Framework;

namespace AnySoftDesktop.ViewModels.Tabs;

public class ShoppingCartTabViewModel : MultipleProductTabViewModel
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    public ShoppingCartTabViewModel(IEnumerable<ProductResponseDto> products, IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(products, viewModelFactory, dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }
    
    
}