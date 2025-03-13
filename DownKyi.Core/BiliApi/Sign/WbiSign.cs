using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DownKyi.Core.Settings;

namespace DownKyi.Core.BiliApi.Sign;

public static class WbiSign
{
    /// <summary>
    /// 打乱重排实时口令
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    private static string GetMixinKey(string origin)
    {
        int[] mixinKeyEncTab =
        {
            46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49,
            33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40,
            61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11,
            36, 20, 34, 44, 52
        };

        var temp = new StringBuilder();
        foreach (var i in mixinKeyEncTab)
        {
            temp.Append(origin[i]);
        }

        return temp.ToString()[..32];
    }

    /// <summary>
    /// 将字典参数转为字符串
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string ParametersToQuery(Dictionary<string, string> parameters)
    {
        var keys = parameters.Keys.ToList();
        var queryList = (from item in keys let value = parameters[item] select $"{item}={value}").ToList();

        return string.Join("&", queryList);
    }

    /// <summary>
    /// Wbi签名，返回所有参数字典
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Dictionary<string, string> EncodeWbi(Dictionary<string, object?> parameters)
    {
        return EncWbi(parameters, GetKey().Item1, GetKey().Item2);
    }

    /// <summary>
    /// Wbi签名，返回所有参数字典
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="imgKey"></param>
    /// <param name="subKey"></param>
    /// <returns></returns>
    private static Dictionary<string, string> EncWbi(Dictionary<string, object?> parameters, string imgKey, string subKey)
    {
        var paraStr = new Dictionary<string, string>();
        foreach (var (key, value) in parameters)
        {
            var val = value?.ToString();
            if (val != null)
            {
                paraStr.Add(key, val);
            }
        }

        var mixinKey = GetMixinKey(imgKey + subKey);
        var currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        //添加 wts 字段
        paraStr["wts"] = currTime;
        // 按照 key 重排参数
        paraStr = paraStr.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        //过滤 value 中的 "!'()*" 字符
        paraStr = paraStr.ToDictionary(kvp => kvp.Key, kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray()));
        // 序列化参数
        var query = new FormUrlEncodedContent(paraStr).ReadAsStringAsync().Result;
        //计算 w_rid
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
        var wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        paraStr["w_rid"] = wbiSign;

        return paraStr;
    }

    private static Tuple<string, string> GetKey()
    {
        var user = SettingsManager.GetInstance().GetUserInfo();

        return new Tuple<string, string>(user.ImgKey, user.SubKey);
    }
}