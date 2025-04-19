using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace DownKyi.CustomControl.AsyncImageLoader;

public interface IAsyncImageLoader : IDisposable
{
    /// <summary>
    ///     Loads image
    /// </summary>
    /// <param name="url">Target url</param>
    /// <param name="quality">quality</param>
    /// <returns>Bitmap</returns>
    public Task<Bitmap?> ProvideImageAsync(string url,int maxWidth, int maxHeight,int quality);
}