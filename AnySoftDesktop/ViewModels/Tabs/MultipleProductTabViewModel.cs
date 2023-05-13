using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AnySoftDesktop.Services;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;

namespace AnySoftDesktop.ViewModels;

public class MultipleProductTabViewModel : DashboardTabViewModel, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    private ObservableCollection<ProductResponseDto> _products = new();

    public new ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set
        {
            _products = value;
            OnPropertyChanged();
        }
    }

    public TabBaseViewModel? PreviousTab { get; set; }
    
    public MultipleProductTabViewModel(IEnumerable<ProductResponseDto> products, IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(viewModelFactory, dialogManager)
    {
        Products = new ObservableCollection<ProductResponseDto>(products);
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
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