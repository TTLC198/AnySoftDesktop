using Stylet;

namespace AnySoftDesktop.ViewModels;

public abstract class TabBaseViewModel : PropertyChangedBase, ITabViewModel
{
    public int Order { get; }

    public string Name { get; }

    public bool IsSelected { get; set; }

    public bool IsVisible { get; set; } = true;

    protected TabBaseViewModel(int order, string name)
    {
        Order = order;
        Name = name;
    }
}