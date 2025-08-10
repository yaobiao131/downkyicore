using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DownKyi.Utils;

public static class ImageHelper
{
    public static Bitmap LoadFromResource(Uri? resourceUri)
    {
        return resourceUri?.Scheme switch
        {
            "avares" => LoadFromAvares(resourceUri),
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
        return new Bitmap(resourceUri.LocalPath);
    }
}