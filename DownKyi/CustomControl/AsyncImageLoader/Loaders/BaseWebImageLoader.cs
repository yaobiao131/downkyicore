using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace DownKyi.CustomControl.AsyncImageLoader.Loaders;

public class BaseWebImageLoader : IAsyncImageLoader
{
    private readonly ParametrizedLogger? _logger;
    private readonly bool _shouldDisposeHttpClient;

    /// <summary>
    ///     Initializes a new instance with new <see cref="System.Net.Http.HttpClient" /> instance
    /// </summary>
    public BaseWebImageLoader() : this(new HttpClient(), true)
    {
    }

    /// <summary>
    ///     Initializes a new instance with the provided <see cref="System.Net.Http.HttpClient" />, and specifies whether that
    ///     <see cref="System.Net.Http.HttpClient" /> should be disposed when this instance is disposed.
    /// </summary>
    /// <param name="httpClient">The HttpMessageHandler responsible for processing the HTTP response messages.</param>
    /// <param name="disposeHttpClient">
    ///     true if the inner handler should be disposed of by Dispose; false if you intend to
    ///     reuse the HttpClient.
    /// </param>
    public BaseWebImageLoader(HttpClient httpClient, bool disposeHttpClient)
    {
        HttpClient = httpClient;
        _shouldDisposeHttpClient = disposeHttpClient;
        _logger = Logger.TryGet(LogEventLevel.Error, ImageLoader.AsyncImageLoaderLogArea);
    }


    protected  HttpClient HttpClient { get;}

    /// <inheritdoc />
    public virtual async Task<Bitmap?> ProvideImageAsync(string url, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            var bytes = await LoadBytesAsync(url);
            return ConvertToLowResolution(bytes,maxWidth,maxHeight, quality);
        }
        catch (Exception e)
        {
            _logger?.Log(this, "Failed to resolve image: {RequestUri}\nException: {Exception}", url, e);
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Attempts to load bitmap
    /// </summary>
    /// <param name="url">Target url</param>
    /// <returns>Bitmap</returns>
    protected virtual async Task<byte[]?> LoadBytesAsync(string url)
    {
        var loaders = new[] { LoadFromLocalAsync, LoadFromInternalAsync, LoadFromGlobalCache };
        var internalOrCachedBytes = await TryLoadBytesAsync(loaders, url);
        if (internalOrCachedBytes?.Length > 0)
        {
            return internalOrCachedBytes;
        }

        var externalBytes = await LoadDataFromExternalAsync(url);
        // if (externalBytes == null) return null;

        // using var memoryStream = new MemoryStream(externalBytes);
        // var bitmap = new Bitmap(memoryStream);
        // await SaveToGlobalCache(url, externalBytes).ConfigureAwait(false);
        return externalBytes;
    }

    private async Task<byte[]> TryLoadBytesAsync(Func<string, Task<byte[]>>[] loaders, string url)
    {
        foreach (var loader in loaders)
        {
            var result = await loader(url);
            if (result?.Length > 0)
            {
                return result;
            }
        }

        return Array.Empty<byte>();
    }


    public Bitmap ConvertToLowResolution(byte[] bytes, int maxWidth, int maxHeight, int quality = 75)
    {
        if (maxWidth <= 0 || maxHeight <= 0)
        {
            return new Bitmap(new MemoryStream(bytes));
        }
        using var skImage = SKImage.FromEncodedData(bytes);
        var (targetWidth, targetHeight) = CalculateScaling(skImage.Width, skImage.Height, maxWidth, maxHeight);

    
        var scaledInfo = new SKImageInfo(
            targetWidth,
            targetHeight,
            SKColorType.Rgba8888, 
            SKAlphaType.Premul);
        
        using var surface = SKSurface.Create(scaledInfo);
        using var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.DrawImage(skImage, new SKRect(0, 0, targetWidth, targetHeight));
        
        using var scaledImage = surface.Snapshot();
        using var skData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 75);
        
        using var stream = skData.AsStream();
        return new Bitmap(stream);
    }
    
    private (int width, int height) CalculateScaling(int origWidth, int origHeight, int maxW, int maxH)
    {
        double ratio = Math.Min((double)maxW / origWidth, (double)maxH / origHeight);
        return (
            (int)Math.Round(origWidth * ratio),
            (int)Math.Round(origHeight * ratio)
        );
    }

    /// <summary>
    /// the url maybe is local file url,so if file exists ,we got a Bitmap
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private Task<byte[]> LoadFromLocalAsync(string url)
    {
        return File.Exists(url) ? File.ReadAllBytesAsync(url) : Task.FromResult<byte[]>(Array.Empty<byte>());
    }

    /// <summary>
    ///     Receives image bytes from an internal source (for example, from the disk).
    ///     This data will be NOT cached globally (because it is assumed that it is already in internal source us and does not
    ///     require global caching)
    /// </summary>
    /// <param name="url">Target url</param>
    /// <returns>Bitmap</returns>
    protected virtual async Task<byte[]> LoadFromInternalAsync(string url)
    {
        var uri = url.StartsWith("/")
            ? new Uri(url, UriKind.Relative)
            : new Uri(url, UriKind.RelativeOrAbsolute);

        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            return Array.Empty<byte>();

        if (uri is { IsAbsoluteUri: true, IsFile: true })
            return await File.ReadAllBytesAsync(uri.LocalPath).ConfigureAwait(false);

        using var stream = AssetLoader.Open(uri);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Receives image bytes from an external source (for example, from the Internet).
    ///     This data will be cached globally (if required by the current implementation)
    /// </summary>
    /// <param name="url">Target url</param>
    /// <returns>Image bytes</returns>
    protected  virtual async Task<byte[]> LoadDataFromExternalAsync(string url)
    {
        return await HttpClient.GetByteArrayAsync(url);
    }

    /// <summary>
    ///     Attempts to load image from global cache (if it is stored before)
    /// </summary>
    /// <param name="url">Target url</param>
    /// <returns>Bitmap</returns>
    protected virtual Task<byte[]> LoadFromGlobalCache(string url)
    {
        // Current implementation does not provide global caching
        return Task.FromResult<byte[]>(Array.Empty<byte>());
    }

    /// <summary>
    ///     Attempts to load image from global cache (if it is stored before)
    /// </summary>
    /// <param name="url">Target url</param>
    /// <param name="imageBytes">Bytes to save</param>
    /// <returns>Bitmap</returns>
    protected virtual Task SaveToGlobalCache(string url, byte[] imageBytes)
    {
        // Current implementation does not provide global caching
        return Task.CompletedTask;
    }

    ~BaseWebImageLoader()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _shouldDisposeHttpClient) HttpClient.Dispose();
    }
}