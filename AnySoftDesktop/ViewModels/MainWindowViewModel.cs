using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AnySoftDesktop.Models;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using AnySoftDesktop.ViewModels.Tabs;
using RPM_Project_Backend.Domain;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class MainWindowViewModel : Screen, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    public IObservableCollection<TabBaseViewModel> Tabs { get; }
    public TabBaseViewModel? ActiveTab { get; private set; }

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

    public MainWindowViewModel(List<TabBaseViewModel> tabs, IViewModelFactory viewModelFactory,
        DialogManager dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        Tabs = new BindableCollection<TabBaseViewModel>(tabs
            .OrderBy(t => t.Order)
            .ToList());
        // Pre-select first tab
        var firstTab = Tabs.FirstOrDefault();
        if (firstTab is not null)
            ActivateTab(firstTab);
    }

    public void ActivateTab(TabBaseViewModel tab)
    {
        var singleProductTab = Tabs.FirstOrDefault(t => t.GetType() == typeof(SingleProductViewModel));
        if (singleProductTab is not null)
        {
            var baseTab = Tabs.First(t => t.GetType() == singleProductTab.GetType().BaseType);
            baseTab.IsVisible = true;
            Tabs.Remove(singleProductTab);
        }

        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.IsSelected = true;
    }

    public void BackFromProduct()
    {
        var singleProductTab = Tabs.FirstOrDefault(t => t.GetType() == typeof(SingleProductViewModel));
        if (singleProductTab is not SingleProductViewModel)
            return;

        var baseTab = Tabs.First(t => t.GetType() == singleProductTab.GetType().BaseType);
        baseTab.IsVisible = true;
        Tabs.Remove(singleProductTab);
        
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;
        
        ActiveTab = Tabs.First(t => t == (singleProductTab as SingleProductViewModel).PreviousTab);
        ActiveTab.OnTabSelected(EventArgs.Empty);
        ActiveTab.IsSelected = true;
    }

    public void OnProductButtonClick(int id)
    {
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        var tab = new SingleProductViewModel(id, _viewModelFactory, _dialogManager);

        var baseTab = Tabs.First(t => t.GetType() == tab.GetType().BaseType);
        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
        baseTab.IsVisible = false;

        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.PreviousTab = baseTab;
        tab.IsSelected = true;
        tab.IsVisible = true;
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
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        var tab = new MultipleProductViewModel(_viewModelFactory, _dialogManager);

        var baseTab = Tabs.First(t => t.GetType() == tab.GetType().BaseType);
        Tabs.Insert(Tabs.IndexOf(baseTab), tab);
        baseTab.IsVisible = false;

        ActiveTab = tab;
        tab.OnTabSelected(EventArgs.Empty);
        tab.PreviousTab = baseTab;
        tab.ProductRequestDto = new()
        {
            Name = SearchString
        };
        tab.IsSelected = true;
        tab.IsVisible = true;
    }

    public void Logout()
    {
        IsAuthorized = false;
        CurrentUser = new ApplicationUser();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}