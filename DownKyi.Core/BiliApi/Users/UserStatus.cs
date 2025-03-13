using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Users;

/// <summary>
/// 用户状态数
/// </summary>
public static class UserStatus
{
    /// <summary>
    /// 关系状态数
    /// </summary>
    /// <param name="mid"></param>
    /// <returns></returns>
    public static UserRelationStat? GetUserRelationStat(long mid)
    {
        var url = $"https://api.bilibili.com/x/relation/stat?vmid={mid}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var userRelationStat = JsonConvert.DeserializeObject<UserRelationStatOrigin>(response);
            if (userRelationStat == null || userRelationStat.Data == null)
            {
                return null;
            }

            return userRelationStat.Data;
        }
        catch (Exception e)
        {
            Console.PrintLine("GetUserRelationStat()发生异常: {0}", e);
            LogManager.Error("UserStatus", e);
            return null;
        }
    }

    /// <summary>
    /// UP主状态数
    /// 
    /// 注：该接口需要任意用户登录，否则不会返回任何数据
    /// </summary>
    /// <param name="mid"></param>
    /// <returns></returns>
    public static UpStat? GetUpStat(long mid)
    {
        var url = $"https://api.bilibili.com/x/space/upstat?mid={mid}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var upStat = JsonConvert.DeserializeObject<UpStatOrigin>(response);
            if (upStat == null || upStat.Data == null)
            {
                return null;
            }

            return upStat.Data;
        }
        catch (Exception e)
        {
            Console.PrintLine("GetUpStat()发生异常: {0}", e);
            LogManager.Error("UserStatus", e);
            return null;
        }
    }
}