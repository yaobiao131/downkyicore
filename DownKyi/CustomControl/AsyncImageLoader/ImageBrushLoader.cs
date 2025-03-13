using System;
using System.IO;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DownKyi.Core.Storage;
using DownKyi.CustomControl.AsyncImageLoader.Loaders;

namespace DownKyi.CustomControl.AsyncImageLoader;

public static class ImageBrushLoader
{
    private static readonly ParametrizedLogger? Logger;
    public static IAsyncImageLoader AsyncImageLoader { get; set; } = new DiskCachedWebImageLoader(Path.Combine(StorageManager.GetCache(), "Images"));

    static ImageBrushLoader()
    {
        SourceProperty.Changed.AddClassHandler<ImageBrush>(OnSourceChanged);
        Logger = Avalonia.Logging.Logger.TryGet(LogEventLevel.Error, ImageLoader.AsyncImageLoaderLogArea);
    }

    private static async void OnSourceChanged(ImageBrush imageBrush, AvaloniaPropertyChangedEventArgs args)
    {
        var (oldValue, newValue) = args.GetOldAndNewValue<string?>();
        if (oldValue == newValue)
            return;

        SetIsLoading(imageBrush, true);

        Bitmap? bitmap = null;
        try
        {
            if (newValue is not null)
            {
                bitmap = await AsyncImageLoader.ProvideImageAsync(newValue);
            }
        }
        catch (Exception e)
        {
            Logger?.Log("ImageBrushLoader", "ImageBrushLoader image resolution failed: {0}", e);
        }

        if (GetSource(imageBrush) != newValue) return;
        imageBrush.Source = bitmap;

        SetIsLoading(imageBrush, false);
    }

    public static readonly AttachedProperty<string?> SourceProperty = AvaloniaProperty.RegisterAttached<ImageBrush, string?>("Source", typeof(ImageLoader));

    public static string? GetSource(ImageBrush element)
    {
        return element.GetValue(SourceProperty);
    }

    public static void SetSource(ImageBrush element, string? value)
    {
        element.SetValue(SourceProperty, value);
    }

    public static readonly AttachedProperty<bool> IsLoadingProperty = AvaloniaProperty.RegisterAttached<ImageBrush, bool>("IsLoading", typeof(ImageLoader));

    public static bool GetIsLoading(ImageBrush element)
    {
        return element.GetValue(IsLoadingProperty);
    }

    private static void SetIsLoading(ImageBrush element, bool value)
    {
        element.SetValue(IsLoadingProperty, value);
    }
}