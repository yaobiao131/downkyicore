﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using DownKyi.Core.Storage;
using DownKyi.CustomControl.AsyncImageLoader.Loaders;

namespace DownKyi.CustomControl.AsyncImageLoader;

public static class ImageLoader
{
    private static readonly ParametrizedLogger? Logger;

    public const string AsyncImageLoaderLogArea = "AsyncImageLoader";

    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<Image, string?>("Source", typeof(ImageLoader));

    public static readonly AttachedProperty<bool> IsLoadingProperty =
        AvaloniaProperty.RegisterAttached<Image, bool>("IsLoading", typeof(ImageLoader));
    
    public static readonly AttachedProperty<int> MaxRenderWidthProperty =
        AvaloniaProperty.RegisterAttached<Image, int>("MaxRenderWidth", typeof(ImageLoader), 200);

    public static readonly AttachedProperty<int> MaxRenderHeightProperty =
        AvaloniaProperty.RegisterAttached<Image, int>("MaxRenderHeight", typeof(ImageLoader), 200);

    public static readonly AttachedProperty<int> QualityProperty = AvaloniaProperty.RegisterAttached<Image, int>(
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
    static ImageLoader()
    {
        SourceProperty.Changed.AddClassHandler<Image>(OnSourceChanged);
        Logger = Avalonia.Logging.Logger.TryGet(LogEventLevel.Error, AsyncImageLoaderLogArea);
    }

    public static IAsyncImageLoader AsyncImageLoader { get; set; } = new DiskCachedWebImageLoader(Path.Combine(StorageManager.GetCache(), "Images"));

    private static readonly ConcurrentDictionary<Image, CancellationTokenSource> PendingOperations = new();

    private static async void OnSourceChanged(Image sender, AvaloniaPropertyChangedEventArgs args)
    {
        var url = args.GetNewValue<string?>();

        var cts = PendingOperations.AddOrUpdate(sender, new CancellationTokenSource(), (x, y) =>
            {
                y.Cancel();
                return new CancellationTokenSource();
            }
        );

        if (url == null)
        {
            ((ICollection<KeyValuePair<Image, CancellationTokenSource>>)PendingOperations).Remove(new KeyValuePair<Image, CancellationTokenSource>(sender, cts));
            sender.Source = null;
            return;
        }

        SetIsLoading(sender, true);
        var  quality = GetQuality(sender);
        int width = (int)sender.Width;
        int height = (int)sender.Height;
        var bitmap = await Task.Run(async () =>
        {
            try
            {
                // A small delay allows to cancel early if the image goes out of screen too fast (eg. scrolling)
                // The Bitmap constructor is expensive and cannot be cancelled
                await Task.Delay(10, cts.Token);

                return await AsyncImageLoader.ProvideImageAsync(url,width,height,quality);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (Exception e)
            {
                Logger?.Log(LogEventLevel.Error, "ImageLoader image resolution failed: {0}", e);

                return null;
            }
        });

        if (bitmap != null && !cts.Token.IsCancellationRequested)
            sender.Source = bitmap!;

        // "It is not guaranteed to be thread safe by ICollection, but ConcurrentDictionary's implementation is. Additionally, we recently exposed this API for .NET 5 as a public ConcurrentDictionary.TryRemove"
        ((ICollection<KeyValuePair<Image, CancellationTokenSource>>)PendingOperations).Remove(new KeyValuePair<Image, CancellationTokenSource>(sender, cts));
        SetIsLoading(sender, false);
    }

    public static string? GetSource(Image element)
    {
        return element.GetValue(SourceProperty);
    }

    public static void SetSource(Image element, string? value)
    {
        element.SetValue(SourceProperty, value);
    }
    
    public static int GetQuality(Image element)
    {
        return element.GetValue(QualityProperty);
    }
    
    public static int GetMaxRenderHeight(Image element)
    {
        return element.GetValue(MaxRenderHeightProperty);
    }
    
    public static int GetMaxRenderWidth(Image element)
    {
        return element.GetValue(MaxRenderWidthProperty);
    }
    
    public static void SetMaxRenderHeight(Image element, int? value)
    {
        element.SetValue(MaxRenderHeightProperty, value);
    }
    
    public static void SetMaxRenderWidth(Image element, int? value)
    {
        element.SetValue(MaxRenderWidthProperty, value);
    }
    
    

    public static void SetQuality(Image element, int? value)
    {
        element.SetValue(QualityProperty, value);
    }

    public static bool GetIsLoading(Image element)
    {
        return element.GetValue(IsLoadingProperty);
    }
    
    

    private static void SetIsLoading(Image element, bool value)
    {
        element.SetValue(IsLoadingProperty, value);
    }
}