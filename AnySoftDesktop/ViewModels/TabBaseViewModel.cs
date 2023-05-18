using System;
using Stylet;

namespace AnySoftDesktop.ViewModels;

public abstract class TabBaseViewModel : PropertyChangedBase
{
    public int Order { get; }

    public string Name { get; }

    public bool IsSelected { get; set; }
    
    public TabBaseViewModel? BaseTab { get; set; }
    
    public TabBaseViewModel? PreviousTab { get; set; }
    
    public event EventHandler? TabSelected;
    
    public virtual void OnTabSelected(EventArgs e)
    {
        var handler = TabSelected;
        handler?.Invoke(this, e);
    }

    protected TabBaseViewModel(int order, string name)
    {
        Order = order;
        Name = name;
    }
}