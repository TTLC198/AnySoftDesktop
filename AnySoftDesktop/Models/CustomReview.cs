using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AnySoftBackend.Library.DataTransferObjects.Review;

namespace AnySoftMobile.Models;

public class CustomReview : ReviewResponseDto, INotifyPropertyChanged
{
    private bool _isOwn;

    /// <summary>
    /// Review check is own
    /// </summary>
    public bool IsOwn
    {
        get => _isOwn;
        set
        {
            _isOwn = value;
            OnPropertyChanged();
        }
    }

    private bool _isModified;

    /// <summary>
    /// Review check is modified
    /// </summary>
    public bool IsModified
    {
        get => _isModified;
        set
        {
            _isModified = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

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