using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnySoftDesktop.Models;

public class CustomPayment : INotifyPropertyChanged
{
    public int? Id { get; set; }
    public int UserId { get; set; }
    public string? Number { get; set; }
    public CustomDate ExpirationDate { get; set; } = new CustomDate();
    public string? CardName { get; set; }
    public string? Cvc { get; set; }
    public bool IsActive { get; set; }
    private bool _isEditComplete;

    public bool IsEditComplete
    {
        get => _isEditComplete;
        set
        {
            _isEditComplete = value;
            OnPropertyChanged();
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}