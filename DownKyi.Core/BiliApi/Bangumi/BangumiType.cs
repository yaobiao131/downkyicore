namespace DownKyi.Core.BiliApi.Bangumi;

public static class BangumiType
{
    public static readonly Dictionary<int, string> Type = new()
    {
        { 1, "Anime" },
        { 2, "Movie" },
        { 3, "Documentary" },
        { 4, "Guochuang" },
        { 5, "TV" },
        { 6, "Unknown" },
        { 7, "Entertainment" },
        { 8, "Unknown" },
        { 9, "Unknown" },
        { 10, "Unknown" }
    };

    public static readonly Dictionary<int, int> TypeId = new()
    {
        { 1, 13 },
        { 2, 23 },
        { 3, 177 },
        { 4, 167 },
        { 5, 11 },
        { 6, -1 },
        { 7, -1 },
        { 8, -1 },
        { 9, -1 },
        { 10, -1 }
    };
}