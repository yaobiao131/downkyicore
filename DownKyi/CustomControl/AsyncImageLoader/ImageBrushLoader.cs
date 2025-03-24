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

    public static IAsyncImageLoader AsyncImageLoader { get; set; } =
        new DiskCachedWebImageLoader(Path.Combine(StorageManager.GetCache(), "Images"));

    
    public static readonly AttachedProperty<int> MaxRenderWidthProperty =
        AvaloniaProperty.RegisterAttached<ImageBrush, int>("MaxRenderWidth", typeof(ImageLoader), 200);

    public static readonly AttachedProperty<int> MaxRenderHeightProperty =
        AvaloniaProperty.RegisterAttached<ImageBrush, int>("MaxRenderHeight", typeof(ImageLoader), 200);
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
                bitmap = await AsyncImageLoader.ProvideImageAsync(newValue,GetMaxRenderWidth(imageBrush) ,
                    GetMaxRenderHeight(imageBrush),GetQuality(imageBrush));
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

    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<ImageBrush, string?>("Source", typeof(ImageLoader));

    public static readonly AttachedProperty<int> QualityProperty = AvaloniaProperty.RegisterAttached<ImageBrush, int>(
        "Quality", typeof(ImageLoader), defaultValue: 65, 
        coerce:(s, e) =>
        {
            if (e > 100 || e < 0)
            {
                return 65;
            }
            return e;
        }
    );


    public static string? GetSource(ImageBrush element)
    {
        return element.GetValue(SourceProperty);
    }

    public static void SetSource(ImageBrush element, string? value)
    {
        element.SetValue(SourceProperty, value);
    }

    public static int GetQuality(ImageBrush element)
    {
        return element.GetValue(QualityProperty);
    }
    
    public static int GetMaxRenderHeight(ImageBrush element)
    {
        return element.GetValue(MaxRenderHeightProperty);
    }
    
    public static int GetMaxRenderWidth(ImageBrush element)
    {
        return element.GetValue(MaxRenderWidthProperty);
    }
    
    public static void SetMaxRenderHeight(ImageBrush element, int? value)
    {
        element.SetValue(MaxRenderHeightProperty, value);
    }
    
    public static void SetMaxRenderWidth(ImageBrush element, int? value)
    {
        element.SetValue(MaxRenderWidthProperty, value);
    }

    public static void SetQuality(ImageBrush element, int? value)
    {
        element.SetValue(QualityProperty, value);
    }

    public static readonly AttachedProperty<bool> IsLoadingProperty =
        AvaloniaProperty.RegisterAttached<ImageBrush, bool>("IsLoading", typeof(ImageLoader));

    public static bool GetIsLoading(ImageBrush element)
    {
        return element.GetValue(IsLoadingProperty);
    }

    private static void SetIsLoading(ImageBrush element, bool value)
    {
        element.SetValue(IsLoadingProperty, value);
    }
}