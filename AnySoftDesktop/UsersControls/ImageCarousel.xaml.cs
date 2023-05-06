using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Stylet;

namespace AnySoftDesktop.UsersControls;

public partial class ImageCarousel : UserControl, INotifyPropertyChanged
{
    public string SelectedImage
    {
        get => (string)GetValue(SelectedImageProperty);
        set => SetValue(SelectedImageProperty, value);
    }
    
    public static readonly DependencyProperty SelectedImageProperty =
        DependencyProperty.Register(
            nameof(SelectedImage),
            typeof(string), 
            typeof(ImageCarousel),
            new UIPropertyMetadata(""));

    [Description("Images")]
    public List<string> Images
    {
        get => (List<string>)GetValue(ImagesProperty);
        set => SetValue(ImagesProperty, value);
    }

    public static readonly DependencyProperty ImagesProperty =
        DependencyProperty.Register(
            nameof(Images),
            typeof(List<string>), 
            typeof(ImageCarousel),
            new UIPropertyMetadata(new List<string>()));
    
    public ImageCarousel()
    {
        InitializeComponent();
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

    private void LeftButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Images is {Count: 0}) return;
        if (SelectedImage == string.Empty)
            SelectedImage = Images.First();
        var index = Images.IndexOf(SelectedImage);
        SelectedImage = 
            index == 0 
                ? Images.Last() 
                : Images[index - 1];
    }

    private void RightButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Images is {Count: 0}) return;
        if (SelectedImage == string.Empty)
            SelectedImage = Images.First();
        var index = Images.IndexOf(SelectedImage);
        SelectedImage = 
            index == Images.Count - 1
                ? Images.First() 
                : Images[index + 1];
    }
}