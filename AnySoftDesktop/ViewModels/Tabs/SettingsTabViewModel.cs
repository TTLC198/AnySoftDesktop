using AnySoftDesktop.ViewModels.Framework;

namespace AnySoftDesktop.ViewModels.Tabs;

public class SettingsTabViewModel : TabBaseViewModel
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    
    public async void OnViewFullyLoaded()
    {
    }
    
    public SettingsTabViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager) : base(2, "Settings")
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }
}