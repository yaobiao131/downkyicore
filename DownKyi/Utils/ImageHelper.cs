using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DownKyi.Utils;

public static class ImageHelper
{
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        return resourceUri.Scheme switch
        {
            "avares" => new Bitmap(AssetLoader.Open(resourceUri)),
            "file" => LoadFromFile(resourceUri),
            _ => new Bitmap("")
        };
    }

    private static Bitmap LoadFromAvares(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }

    private static Bitmap LoadFromFile(Uri resourceUri)
    {
        return new Bitmap(resourceUri.AbsolutePath);
    }

    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
    }
}