using System.Collections.Generic;
using System.Linq;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public class MainWindowViewModel : Screen
{
    public IReadOnlyList<ITabViewModel> Tabs { get; }

    public ITabViewModel? ActiveTab { get; private set; }
    
    public bool IsMenuExpanded { get; set; }
    public bool IsWindowMaximized { get; set; }

    public MainWindowViewModel(IReadOnlyList<ITabViewModel> tabs)
    {
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

    public void ToggleMenu() => 
        IsMenuExpanded = !IsMenuExpanded;
}