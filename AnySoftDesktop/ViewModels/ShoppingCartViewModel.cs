using System.Collections.Generic;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class ShoppingCartViewModel : MultipleProductViewModel
{
    
    
    public ShoppingCartViewModel(IEnumerable<ProductResponseDto> products, IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(products, viewModelFactory, dialogManager)
    {
    }
}