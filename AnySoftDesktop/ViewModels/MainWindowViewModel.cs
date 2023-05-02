using System.Collections.Generic;
using System.Linq;
using AnySoftDesktop.Utils;
using AnySoftDesktop.ViewModels.Framework;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class MainWindowViewModel : Screen
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    
    public IReadOnlyList<ITabViewModel> Tabs { get; }
    public ITabViewModel? ActiveTab { get; private set; }
    
    public bool IsMenuExpanded { get; set; }
    public bool IsWindowMaximized { get; set; }
    public bool IsAuthorized { get; set; }

    public MainWindowViewModel(IReadOnlyList<ITabViewModel> tabs, IViewModelFactory viewModelFactory, DialogManager dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        Tabs = tabs.OrderBy(t => t.Order).ToArray();
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
        if (!IsAuthorized)
        {
            await _dialogManager.ShowDialogAsync(_viewModelFactory.CreateLoginViewModel());
        }
    }
}