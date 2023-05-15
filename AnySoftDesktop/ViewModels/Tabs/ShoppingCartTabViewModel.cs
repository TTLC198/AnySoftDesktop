using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

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