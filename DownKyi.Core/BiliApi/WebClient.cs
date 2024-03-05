using System.IO.Compression;
using System.Net;
using System.Text;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;

namespace DownKyi.Core.BiliApi;

internal static class WebClient
{
    private static string GetRandomBuvid3()
    {
        // 随机生成10位字符串
        const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var result = new StringBuilder();
        for (var i = 0; i < 10; i++)
        {
            result.Append(str[random.Next(str.Length)]);
        }

        return result.ToString();
    }

    /// <summary>
    /// 发送get或post请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="referer"></param>
    /// <param name="method"></param>
    /// <param name="parameters"></param>
    /// <param name="retry"></param>
    /// <returns></returns>
    public static string RequestWeb(string url, string? referer = null, string method = "GET",
        Dictionary<string, string>? parameters = null, int retry = 3, bool needRandomBvuid3 = false)
    {
        // 重试次数
        if (retry <= 0)
        {
            return "";
        }

        // post请求，发送参数
        if (method == "POST" && parameters != null)
        {
            var builder = new StringBuilder();
            var i = 0;
            foreach (var item in parameters)
            {
                if (i > 0)
                {
                    builder.Append('&');
                }

                builder.Append($"{item.Key}={item.Value}");
                i++;
            }

            url += "?" + builder;
        }

        try
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Timeout = 30 * 1000;

            request.UserAgent = SettingsManager.GetInstance().GetUserAgent();

            //request.ContentType = "application/json,text/html,application/xhtml+xml,application/xml;charset=UTF-8";
            request.Headers["accept-language"] = "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7";
            request.Headers["accept-encoding"] = "gzip, deflate, br";

            // referer
            if (referer != null)
            {
                request.Referer = referer;
            }

            // 构造cookie
            if (!url.Contains("getLogin"))
            {
                request.Headers["origin"] = "https://m.bilibili.com";

                var cookies = LoginHelper.GetLoginInfoCookies();
                if (cookies != null)
                {
                    request.CookieContainer = cookies;
                }
                else
                {
                    request.CookieContainer = new CookieContainer();
                    if (needRandomBvuid3)
                    {
                        request.CookieContainer.Add(new Cookie("buvid3", GetRandomBuvid3(), "/", ".bilibili.com"));
                    }
                }
            }

            var html = string.Empty;
            using var response = (HttpWebResponse)request.GetResponse();
            if (response.ContentEncoding.ToLower().Contains("gzip"))
            {
                using var stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                html = reader.ReadToEnd();
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
            {
                using var stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                html = reader.ReadToEnd();
            }
            else if (response.ContentEncoding.ToLower().Contains("br"))
            {
                using var stream = new BrotliStream(response.GetResponseStream(), CompressionMode.Decompress);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                html = reader.ReadToEnd();
            }
            else
            {
                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                html = reader.ReadToEnd();
            }

            return html;
        }
        catch (WebException e)
        {
            Console.WriteLine("RequestWeb()发生Web异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(url, referer, method, parameters, retry - 1);
        }
        catch (IOException e)
        {
            Console.WriteLine("RequestWeb()发生IO异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(url, referer, method, parameters, retry - 1);
        }
        catch (Exception e)
        {
            Console.WriteLine("RequestWeb()发生其他异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(url, referer, method, parameters, retry - 1);
        }
    }
}