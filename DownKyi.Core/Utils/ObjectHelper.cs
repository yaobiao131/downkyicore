using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using DownKyi.Core.Logging;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Utils;

public static class ObjectHelper
{

    private const string EncryptKey = "k5F9#pL@";
    /// <summary>
    /// 解析二维码登录返回的url，用于设置cookie
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static CookieContainer ParseCookie(string url)
    {
        var cookieContainer = new CookieContainer();

        if (url is null or "")
        {
            return cookieContainer;
        }

        var strList = url.Split('?');
        if (strList.Length < 2)
        {
            return cookieContainer;
        }

        var strList2 = strList[1].Split('&');
        if (strList2.Length == 0)
        {
            return cookieContainer;
        }

        // 获取expires
        var expires = strList2.FirstOrDefault(it => it.Contains("Expires"))?.Split('=')[1];
        var dateTime = DateTime.Now;
        dateTime = dateTime.AddSeconds(int.Parse(expires));

        foreach (var item in strList2)
        {
            var strList3 = item.Split('=');
            if (strList3.Length < 2)
            {
                continue;
            }

            var name = strList3[0];
            var value = strList3[1];

            // 不需要
            if (name is "Expires" or "gourl")
            {
                continue;
            }

            // 添加cookie
            cookieContainer.Add(new Cookie(name, value.Replace(",", "%2c"), "/", ".bilibili.com") { Expires = dateTime });
            Console.PrintLine(name + ": " + value + "\t" + cookieContainer.Count);
        }

        return cookieContainer;
    }
    
    /// <summary>
    /// 写入cookies到磁盘
    /// </summary>
    /// <param name="file"></param>
    /// <param name="cookieJar"></param>
    /// <returns></returns>
    public static bool WriteCookiesToDisk(string file,CookieContainer cookieJar)
    {
        try
        {
           var str = CookieJsonSerializer.Serialize(cookieJar);
           var es = Encryptor.Encryptor.EncryptString(str, EncryptKey);
           File.WriteAllText(file, es);
        }
        catch (Exception e)
        {
           LogManager.Error(e);
           return false;
        }
        return true;
    }

    /// <summary>
    /// 从磁盘读取cookie
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static CookieContainer? ReadCookiesFromDisk(string file)
    {
        try
        {
            var es = File.ReadAllText(file);
            var ds = Encryptor.Encryptor.DecryptString(es, EncryptKey);
            return CookieJsonSerializer.Deserialize(ds);
        }
        catch (Exception e)
        {
            LogManager.Error(e);
        }
        return null;
    }
    
}