using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace DownKyi.CustomControl.AsyncImageLoader.Loaders;

public class RamCachedWebImageLoader : BaseWebImageLoader
{
    private readonly ConcurrentDictionary<string, Task<byte[]?>> _memoryCache = new();

    /// <inheritdoc />
    public RamCachedWebImageLoader()
    {
    }

    /// <inheritdoc />
    public RamCachedWebImageLoader(HttpClient httpClient, bool disposeHttpClient) : base(httpClient, disposeHttpClient)
    {
    }

    /// <inheritdoc />
    public override async Task<Bitmap?> ProvideImageAsync(string url, int maxWidth, int maxHeight,int quality)
    {
        var bytes = await _memoryCache.GetOrAdd(url, LoadBytesAsync).ConfigureAwait(false);
        // If load failed - remove from cache and return
        // Next load attempt will try to load image again
        if (bytes == null) _memoryCache.TryRemove(url, out _);
        return ConvertToLowResolution(bytes,maxWidth,maxHeight,quality);
    }
}