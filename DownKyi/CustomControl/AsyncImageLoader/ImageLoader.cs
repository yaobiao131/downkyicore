using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Threading;
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

        var bitmap = await Task.Run(async () =>
        {
            try
            {
                // A small delay allows to cancel early if the image goes out of screen too fast (eg. scrolling)
                // The Bitmap constructor is expensive and cannot be cancelled
                await Task.Delay(10, cts.Token);
                if (sender.DesiredSize.Width != 0 && sender.DesiredSize.Height != 0)
                {
                    var scale = Dispatcher.UIThread.Invoke(() => App.Current.MainWindow.DesktopScaling);
                    var actualWidth = Convert.ToInt32(sender.DesiredSize.Width * scale);
                    var actualHeight = Convert.ToInt32(sender.DesiredSize.Height * scale);
                    return (await AsyncImageLoader.ProvideImageAsync(url))?.CreateScaledBitmap(new PixelSize(actualWidth, actualHeight));
                }

                return await AsyncImageLoader.ProvideImageAsync(url);
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
        }, cts.Token);

        if (bitmap != null && !cts.Token.IsCancellationRequested)
            sender.Source = bitmap;

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

    public static bool GetIsLoading(Image element)
    {
        return element.GetValue(IsLoadingProperty);
    }

    private static void SetIsLoading(Image element, bool value)
    {
        element.SetValue(IsLoadingProperty, value);
    }

    public static readonly AttachedProperty<int> WidthProperty = AvaloniaProperty.RegisterAttached<Image, int>("Width", typeof(ImageLoader));

    public static int GetWidth(Image element)
    {
        return element.GetValue(WidthProperty);
    }

    public static void SetWidth(Image element, int value)
    {
        element.SetValue(WidthProperty, value);
    }

    public static readonly AttachedProperty<int> HeightProperty = AvaloniaProperty.RegisterAttached<Image, int>("Height", typeof(ImageLoader));

    public static int GetHeight(Image element)
    {
        return element.GetValue(HeightProperty);
    }

    public static void SetHeight(Image element, int value)
    {
        element.SetValue(HeightProperty, value);
    }
}