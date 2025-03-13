using DownKyi.Core.BiliApi.Favorites.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Favorites;

public static class FavoritesInfo
{
    /// <summary>
    /// 获取收藏夹元数据
    /// </summary>
    /// <param name="mediaId"></param>
    public static FavoritesMetaInfo? GetFavoritesInfo(long mediaId)
    {
        var url = $"https://api.bilibili.com/x/v3/fav/folder/info?media_id={mediaId}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var info = JsonConvert.DeserializeObject<FavoritesMetaInfoOrigin>(response);
            return info?.Data;
        }
        catch (Exception e)
        {
            Console.PrintLine("GetFavoritesInfo()发生异常: {0}", e);
            LogManager.Error("FavoritesInfo", e);
            return null;
        }
    }

    /// <summary>
    /// 查询用户创建的视频收藏夹
    /// </summary>
    /// <param name="mid">目标用户UID</param>
    /// <param name="pn">页码</param>
    /// <param name="ps">每页项数</param>
    /// <returns></returns>
    public static List<FavoritesMetaInfo>? GetCreatedFavorites(long mid, int pn, int ps)
    {
        var url = $"https://api.bilibili.com/x/v3/fav/folder/created/list?up_mid={mid}&pn={pn}&ps={ps}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var favorites = JsonConvert.DeserializeObject<FavoritesListOrigin>(response);

            return favorites?.Data.List;
        }
        catch (Exception e)
        {
            Console.PrintLine("GetCreatedFavorites()发生异常: {0}", e);
            LogManager.Error("FavoritesInfo", e);
            return null;
        }
    }

    /// <summary>
    /// 查询所有的用户创建的视频收藏夹
    /// </summary>
    /// <param name="mid">目标用户UID</param>
    /// <returns></returns>
    public static List<FavoritesMetaInfo> GetAllCreatedFavorites(long mid)
    {
        var result = new List<FavoritesMetaInfo>();

        var i = 0;
        while (true)
        {
            i++;
            const int ps = 50;

            var data = GetCreatedFavorites(mid, i, ps);
            if (data == null || data.Count == 0)
            {
                break;
            }

            result.AddRange(data);
        }

        return result;
    }

    /// <summary>
    /// 查询用户收藏的视频收藏夹
    /// </summary>
    /// <param name="mid">目标用户UID</param>
    /// <param name="pn">页码</param>
    /// <param name="ps">每页项数</param>
    /// <returns></returns>
    public static List<FavoritesMetaInfo>? GetCollectedFavorites(long mid, int pn, int ps)
    {
        var url = $"https://api.bilibili.com/x/v3/fav/folder/collected/list?up_mid={mid}&pn={pn}&ps={ps}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var favorites = JsonConvert.DeserializeObject<FavoritesListOrigin>(response);
            if (favorites == null || favorites.Data == null || favorites.Data.List == null)
            {
                return null;
            }

            return favorites.Data.List;
        }
        catch (Exception e)
        {
            Console.PrintLine("GetCollectedFavorites()发生异常: {0}", e);
            LogManager.Error("FavoritesInfo", e);
            return null;
        }
    }

    /// <summary>
    /// 查询所有的用户收藏的视频收藏夹
    /// </summary>
    /// <param name="mid">目标用户UID</param>
    /// <returns></returns>
    public static List<FavoritesMetaInfo> GetAllCollectedFavorites(long mid)
    {
        var result = new List<FavoritesMetaInfo>();

        var i = 0;
        while (true)
        {
            i++;
            const int ps = 50;

            var data = GetCollectedFavorites(mid, i, ps);
            if (data == null || data.Count == 0)
            {
                break;
            }

            result.AddRange(data);
        }

        return result;
    }
}