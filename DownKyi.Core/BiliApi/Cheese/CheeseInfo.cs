using DownKyi.Core.BiliApi.Cheese.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Cheese;

public static class CheeseInfo
{
    /// <summary>
    /// 获取课程基本信息
    /// </summary>
    /// <param name="seasonId"></param>
    /// <param name="episodeId"></param>
    /// <returns></returns>
    public static CheeseView? CheeseViewInfo(long seasonId = -1, long episodeId = -1)
    {
        const string baseUrl = "https://api.bilibili.com/pugv/view/web/season";
        const string referer = "https://www.bilibili.com";
        string url;
        if (seasonId > -1)
        {
            url = $"{baseUrl}?season_id={seasonId}";
        }
        else if (episodeId > -1)
        {
            url = $"{baseUrl}?ep_id={episodeId}";
        }
        else
        {
            return null;
        }

        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var cheese = JsonConvert.DeserializeObject<CheeseViewOrigin>(response);
            return cheese?.Data;
        }
        catch (Exception e)
        {
            Console.PrintLine("CheeseViewInfo()发生异常: {0}", e);
            LogManager.Error("CheeseInfo", e);
            return null;
        }
    }

    /// <summary>
    /// 获取课程分集列表
    /// </summary>
    /// <param name="seasonId"></param>
    /// <param name="ps"></param>
    /// <param name="pn"></param>
    /// <returns></returns>
    public static CheeseEpisodeList? CheeseEpisodeList(long seasonId, int ps = 50, int pn = 1)
    {
        var url = $"https://api.bilibili.com/pugv/view/web/ep/list?season_id={seasonId}&pn={pn}&ps={ps}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var cheese = JsonConvert.DeserializeObject<CheeseEpisodeListOrigin>(response);
            return cheese?.Data;
        }
        catch (Exception e)
        {
            Console.PrintLine("CheeseEpisodeList()发生异常: {0}", e);
            LogManager.Error("CheeseInfo", e);
            return null;
        }
    }
}