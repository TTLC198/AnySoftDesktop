using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnySoftDesktop.Controls;

public class AlternativeSourceImage : Image
{
    private bool _tryAlternativeSource = true;

    public ImageSource AlternativeSource
    {
        get => (ImageSource)GetValue(AlternativeSourceProperty);
        set => SetValue(AlternativeSourceProperty, value);
    }

    // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty AlternativeSourceProperty =
        DependencyProperty.Register(nameof(AlternativeSource), typeof(ImageSource), typeof(AlternativeSourceImage), new PropertyMetadata(null));

    public AlternativeSourceImage()
    {
        Initialized += OnInitialized;
    }

    private void OnInitialized(object sender, EventArgs eventArgs)
    {
        //Note , ths need to be unregistered 
        ImageFailed += OnImageFailed;
    }

    private void OnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
    {
        if (!_tryAlternativeSource)
            return;

        _tryAlternativeSource = false;
        
        Source = AlternativeSource;
    }
}