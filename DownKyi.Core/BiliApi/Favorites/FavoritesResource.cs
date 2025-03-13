using DownKyi.Core.BiliApi.Favorites.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Favorites;

public static class FavoritesResource
{
    /// <summary>
    /// 获取收藏夹内容明细列表
    /// </summary>
    /// <param name="mediaId">收藏夹ID</param>
    /// <param name="pn">页码</param>
    /// <param name="ps">每页项数</param>
    /// <returns></returns>
    public static List<FavoritesMedia>? GetFavoritesMedia(long mediaId, int pn, int ps)
    {
        var url = $"https://api.bilibili.com/x/v3/fav/resource/list?media_id={mediaId}&pn={pn}&ps={ps}&platform=web";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var resource = JsonConvert.DeserializeObject<FavoritesMediaResourceOrigin>(response);
            if (resource == null || resource.Data == null || resource.Data.Medias == null)
            {
                return null;
            }

            return resource.Data.Medias;
        }
        catch (Exception e)
        {
            Console.WriteLine("GetFavoritesMedia()发生异常: {0}", e);
            LogManager.Error("FavoritesResource", e);
            return null;
        }
    }

    /// <summary>
    /// 获取收藏夹内容明细列表（全部）
    /// </summary>
    /// <param name="mediaId">收藏夹ID</param>
    /// <returns></returns>
    public static List<FavoritesMedia> GetAllFavoritesMedia(long mediaId)
    {
        var result = new List<FavoritesMedia>();

        var i = 0;
        while (true)
        {
            i++;
            const int ps = 20;

            var data = GetFavoritesMedia(mediaId, i, ps);
            if (data == null || data.Count == 0)
            {
                break;
            }

            result.AddRange(data);
        }

        return result;
    }

    /// <summary>
    /// 获取收藏夹全部内容id
    /// </summary>
    /// <param name="mediaId"></param>
    /// <returns></returns>
    public static List<FavoritesMediaId> GetFavoritesMediaId(long mediaId)
    {
        var url = $"https://api.bilibili.com/x/v3/fav/resource/ids?media_id={mediaId}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var media = JsonConvert.DeserializeObject<FavoritesMediaIdOrigin>(response);
            if (media == null || media.Data == null)
            {
                return null;
            }

            return media.Data;
        }
        catch (Exception e)
        {
            Console.WriteLine("GetFavoritesMediaId()发生异常: {0}", e);
            LogManager.Error("FavoritesResource", e);
            return null;
        }
    }
}