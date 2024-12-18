using System.Collections;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using DownKyi.Core.Logging;
using DownKyi.Core.Utils.Encryptor;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Utils;

public static class ObjectHelper
{
    private const string EncryptionKey = "FgjsEEqc";
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
        var expires = strList2.FirstOrDefault(it => it.Contains("Expires")).Split('=')[1];
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
            if (name == "Expires" || name == "gourl")
            {
                continue;
            }

            // 添加cookie
            cookieContainer.Add(
                new Cookie(name, value.Replace(",", "%2c"), "/", ".bilibili.com") { Expires = dateTime });
            Console.PrintLine(name + ": " + value + "\t" + cookieContainer.Count);
        }

        return cookieContainer;
    }

    /// <summary>
    /// 将CookieContainer中的所有的Cookie读出来
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    public static List<Cookie> GetAllCookies(CookieContainer cc)
    {
        var lstCookies = new List<Cookie>();

        var table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
            BindingFlags.NonPublic | BindingFlags.GetField |
            BindingFlags.Instance, null, cc, new object[] { });

        foreach (var pathList in table.Values)
        {
            var lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                BindingFlags.NonPublic | BindingFlags.GetField
                                       | BindingFlags.Instance, null, pathList,
                new object[] { });
            foreach (CookieCollection colCookies in lstCookieCol.Values)
            {
                foreach (Cookie c in colCookies)
                {
                    lstCookies.Add(c);
                }
            }
        }

        return lstCookies;
    }

    /// <summary>
    /// 写入cookies到磁盘
    /// </summary>
    /// <param name="file"></param>
    /// <param name="cookieJar"></param>
    /// <returns></returns>
    public static bool WriteCookiesToDisk(string file, CookieContainer cookieJar)
    {
        try
        {
            var jsonString = CookieContainerSerializer.SerializeCookieContainer(cookieJar);
            var encryptedString = Encryptor.Encryptor.EncryptString(jsonString, EncryptionKey);
            File.WriteAllText(file, encryptedString);
        }
        catch (IOException e)
        {
            Console.PrintLine("WriteObjectToDisk()发生IO异常: {0}", e);
            LogManager.Error(e);
            return false;
        }
        catch (Exception e)
        {
            Console.PrintLine("WriteObjectToDisk()发生异常: {0}", e);
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
    public static CookieContainer ReadCookiesFromDisk(string file)
    {
        try
        {
            string jsonString = File.ReadAllText(file);
            var decryptedString = Encryptor.Encryptor.DecryptString(jsonString, EncryptionKey);
            CookieContainer obj = CookieContainerSerializer.DeserializeCookieContainer(decryptedString);
            return obj;
        }
        catch (IOException e)
        {
            Console.PrintLine("ReadObjectFromDisk()发生IO异常: {0}", e);
            LogManager.Error(e);
            return null;
        }
        catch (Exception e)
        {
            Console.PrintLine("ReadObjectFromDisk()发生异常: {0}", e);
            LogManager.Error(e);
            return null;
        }
    }

 
    /// <summary>
    /// 从磁盘读取序列化对象
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static T ReadObjectFromDisk<T>(string file)
    {
        try
        {
            string jsonString = File.ReadAllText(file);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (IOException e)
        {
            Console.PrintLine("ReadObjectFromDisk()发生IO异常: {0}", e);
            LogManager.Error(e);
            return default(T);
        }
        catch (Exception e)
        {
            Console.PrintLine("ReadObjectFromDisk()发生异常: {0}", e);
            LogManager.Error(e);
            return default(T);
        }
    }
}