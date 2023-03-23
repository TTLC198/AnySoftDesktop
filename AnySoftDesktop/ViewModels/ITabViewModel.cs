namespace AnySoftDesktop.ViewModels;

public interface ITabViewModel
{
    int Order { get; }
    string Name { get; }
    bool IsSelected { get; set; }
}