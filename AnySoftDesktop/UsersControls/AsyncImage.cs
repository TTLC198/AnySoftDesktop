using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AnySoftDesktop.UsersControls;

public class AsyncImage : Image, INotifyPropertyChanged
{
    public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.Register(
            nameof(ImagePath), typeof(string), typeof(AsyncImage),
            new PropertyMetadata(async (o, e) =>
                await ((AsyncImage)o).LoadImageAsync((string)e.NewValue)));

    public string ImagePath
    {
        get => (string)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    private bool _isLoaded;

    public bool IsLoaded
    {
        get => _isLoaded;
        set
        {
            _isLoaded = true;
            OnPropertyChanged();
        }
    }

    private async Task LoadImageAsync(string imagePath)
    {
        var httpClient = new HttpClient();
        var responseStream = await httpClient.GetStreamAsync(imagePath);
        var bitmapImage = new BitmapImage();

        using (var memoryStream = new MemoryStream())
        {
            await responseStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
        }

        Source = bitmapImage;
        IsLoaded = true;
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