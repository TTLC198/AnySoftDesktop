using System;
using System.ComponentModel;
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
    
    public TabBaseViewModel? PreviousTab { get; set; }
    
    public async void OnViewFullyLoaded()
    {
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