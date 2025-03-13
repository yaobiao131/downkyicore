using DownKyi.Core.BiliApi.Video.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Video;

public static class Ranking
{
    /// <summary>
    /// 获取分区视频排行榜列表
    /// </summary>
    /// <param name="rid">目标分区tid</param>
    /// <param name="day">3日榜或周榜（3/7）</param>
    /// <param name="original"></param>
    /// <returns></returns>
    public static List<RankingVideoView>? RegionRankingList(int rid, int day = 3, int original = 0)
    {
        var url = $"https://api.bilibili.com/x/web-interface/ranking/region?rid={rid}&day={day}&ps={original}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var ranking = JsonConvert.DeserializeObject<RegionRanking>(response);
            if (ranking != null)
            {
                return ranking.Data;
            }

            return null;
        }
        catch (Exception e)
        {
            Console.PrintLine("RegionRankingList()发生异常: {0}", e);
            LogManager.Error("Ranking", e);
            return null;
        }
    }
}