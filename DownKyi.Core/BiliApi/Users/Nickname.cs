using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.Logging;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Users;

/// <summary>
/// 用户昵称
/// </summary>
public class Nickname
{
    /// <summary>
    /// 检查昵称
    /// </summary>
    /// <param name="nickName"></param>
    /// <returns></returns>
    public static NicknameStatus? CheckNickname(string nickName)
    {
        var url = $"https://api.bilibili.com/x/relation/stat?nickName={nickName}";
        const string referer = "https://www.bilibili.com";
        var response = WebClient.RequestWeb(url, referer);

        try
        {
            var nickname = JsonConvert.DeserializeObject<NicknameStatus>(response);
            return nickname;
        }
        catch (Exception e)
        {
            Console.PrintLine("CheckNickname()发生异常: {0}", e);
            LogManager.Error("Nickname", e);
            return null;
        }
    }
}