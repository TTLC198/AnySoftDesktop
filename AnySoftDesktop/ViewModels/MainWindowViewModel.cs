using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AnySoftDesktop.Models;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using RPM_Project_Backend.Domain;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class MainWindowViewModel : Screen, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    
    public List<ITabViewModel> Tabs { get; }
    public ITabViewModel? ActiveTab { get; private set; }
    
    public bool IsMenuExpanded { get; set; }
    public bool IsWindowMaximized { get; set; }
    public bool IsAuthorized { get; set; }
    
    public string? SearchString { get; set; }

    private ApplicationUser _currentUser = new ApplicationUser();

    public ApplicationUser CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel(IReadOnlyList<ITabViewModel> tabs, IViewModelFactory viewModelFactory, DialogManager dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        Tabs = tabs.OrderBy(t => t.Order).ToList();
        // Pre-select first tab
        var firstTab = Tabs.FirstOrDefault();
        if (firstTab is not null)
            ActivateTab(firstTab);
    }

    public void ActivateTab(ITabViewModel settingsTab)
    {
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
        ActiveTab = settingsTab;
        settingsTab.IsSelected = true;
    }

    public void ToggleMaximized() =>
        IsWindowMaximized = !IsWindowMaximized;

    public void ToggleMenu() => 
        IsMenuExpanded = !IsMenuExpanded;

    public async void OpenLoginPage()
    {
        if (IsAuthorized) return;
        var user = await _dialogManager.ShowDialogAsync(_viewModelFactory.CreateLoginViewModel());
        
        if (user is null) return;
        IsAuthorized = true;
        CurrentUser = user;
    }

    public async void OpenSearchPage()
    {
        var searchString = SearchString;
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
        var settingsTab = Tabs
            .FirstOrDefault(t => t.Name == "Dashboard");
        
        if (settingsTab is null)
            return;

        ActiveTab = settingsTab;
        settingsTab.IsSelected = true;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}