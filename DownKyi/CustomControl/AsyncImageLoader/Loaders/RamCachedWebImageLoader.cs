using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace DownKyi.CustomControl.AsyncImageLoader.Loaders;

public class RamCachedWebImageLoader : BaseWebImageLoader
{
    private readonly ConcurrentDictionary<string, Task<Bitmap?>> _memoryCache = new();

    /// <inheritdoc />
    public RamCachedWebImageLoader()
    {
    }

    /// <inheritdoc />
    public RamCachedWebImageLoader(HttpClient httpClient, bool disposeHttpClient) : base(httpClient, disposeHttpClient)
    {
    }

    /// <inheritdoc />
    public override async Task<Bitmap?> ProvideImageAsync(string url)
    {
        var bitmap = await _memoryCache.GetOrAdd(url, LoadAsync).ConfigureAwait(false);
        // If load failed - remove from cache and return
        // Next load attempt will try to load image again
        if (bitmap == null) _memoryCache.TryRemove(url, out _);
        return bitmap;
    }
}