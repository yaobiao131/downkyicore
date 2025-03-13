using DownKyi.Core.BiliApi.History.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.History
{
    /// <summary>
    /// 稍后再看
    /// </summary>
    public static class ToView
    {
        /// <summary>
        /// 获取稍后再看视频列表
        /// </summary>
        /// <returns></returns>
        public static List<ToViewList>? GetToView()
        {
            const string url = "https://api.bilibili.com/x/v2/history/toview";
            const string referer = "https://www.bilibili.com";
            var response = WebClient.RequestWeb(url, referer);

            try
            {
                var toView = JsonConvert.DeserializeObject<ToViewOrigin>(response);
                if (toView == null || toView.Data == null) { return null; }
                return toView.Data.List;
            }
            catch (Exception e)
            {
                Console.PrintLine("GetToView()发生异常: {0}", e);
                LogManager.Error("ToView", e);
                return null;
            }
        }
    }
}