﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Web;
using AnySoftDesktop.Models;
using AnySoftDesktop.Services;
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
        
        var productRequestDto = new ProductRequestDto
        {
            Name = SearchString
        };
        var productRequestQueryJson = HttpUtility.UrlEncode(JsonSerializer.Serialize(productRequestDto, CustomJsonSerializerOptions.Options));
        var getProductsRequest = await WebApiService.GetCall($"api/products?Query={productRequestQueryJson}");
        try
        {
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(300);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                
                var tab = new MultipleProductViewModel(products, _viewModelFactory, _dialogManager);
                var baseTab = Tabs.First(t => t.GetType() == tab.GetType().BaseType);
                Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                baseTab.IsVisible = false;

                ActiveTab = tab;
                tab.OnTabSelected(EventArgs.Empty);
                tab.PreviousTab = baseTab;
                tab.IsSelected = true;
                tab.IsVisible = true;
            }
            else
            {
                var msg = await getProductsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getProductsRequest.ReasonPhrase}\n{msg}");
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
    
    public async void OpenShoppingCart()
    {
        // Deactivate previously selected tab
        if (ActiveTab is not null)
            ActiveTab.IsSelected = false;

        var getProductsRequest = await WebApiService.GetCall($"api/cart", App.AuthorizationToken ?? "");
        try
        {
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(300);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                
                var tab = new MultipleProductViewModel(products, _viewModelFactory, _dialogManager);
                var baseTab = Tabs.First(t => t.GetType() == tab.GetType().BaseType);
                Tabs.Insert(Tabs.IndexOf(baseTab), tab);
                baseTab.IsVisible = false;

                ActiveTab = tab;
                tab.OnTabSelected(EventArgs.Empty);
                tab.PreviousTab = baseTab;
                tab.IsSelected = true;
                tab.IsVisible = true;
            }
            else
            {
                var msg = await getProductsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getProductsRequest.ReasonPhrase}\n{msg}");
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