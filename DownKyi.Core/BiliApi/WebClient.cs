using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;

namespace DownKyi.Core.BiliApi;

internal static class WebClient
{
    internal class SpiOrigin
    {
        [JsonPropertyName("data")] public Spi? Data { get; init; }
        public int Code { get; init; }
        public string? Message { get; init; }
    }

    internal class Spi
    {
        [JsonPropertyName("b_3")] public string? Bvuid3 { get; set; }
        [JsonPropertyName("b_4")] public string? Bvuid4 { get; set; }
    }

    private static string? _bvuid3 = string.Empty;
    private static string? _bvuid4 = string.Empty;

    // private static string GetRandomBuvid3()
    // {
    //     // 随机生成10位字符串
    //     const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    //     var random = new Random();
    //     var result = new StringBuilder();
    //     for (var i = 0; i < 10; i++)
    //     {
    //         result.Append(str[random.Next(str.Length)]);
    //     }
    //
    //     return result.ToString();
    // }

    private static void GetBuvid()
    {
        const string url = "https://api.bilibili.com/x/frontend/finger/spi";
        var response = RequestWeb(url);
        var spi = JsonSerializer.Deserialize<SpiOrigin>(response);
        _bvuid3 = spi?.Data?.Bvuid3;
        _bvuid4 = spi?.Data?.Bvuid4;
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
    public static string RequestWeb(string url, string? referer = null, string method = "GET", Dictionary<string, string>? parameters = null, int retry = 3)
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
            if (string.IsNullOrEmpty(_bvuid3) && url != "https://api.bilibili.com/x/frontend/finger/spi")
            {
                GetBuvid();
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Timeout = 30 * 1000;

            request.UserAgent = SettingsManager.GetInstance().GetUserAgent();

            //request.ContentType = "application/json,text/html,application/xhtml+xml,application/xml;charset=UTF-8";
            request.Headers["accept-language"] = "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7";
            request.Headers["accept-encoding"] = "gzip, deflate, br";
            switch (SettingsManager.GetInstance().GetNetworkProxy())
            {
                case NetworkProxy.None:
                    request.Proxy = null;
                    break;
                case NetworkProxy.System:
                    request.Proxy = WebRequest.GetSystemWebProxy();
                    break;
                case NetworkProxy.Custom:
                {
                    try
                    {
                        request.Proxy = new WebProxy(SettingsManager.GetInstance().GetCustomProxy());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                    break;
            }

            // referer
            if (referer != null)
            {
                request.Referer = referer;
            }

            // 构造cookie
            if (!url.Contains("getLogin"))
            {
                request.Headers["origin"] = "https://www.bilibili.com";

                var cookies = LoginHelper.GetLoginInfoCookies();
                request.CookieContainer = cookies ?? new CookieContainer();

                if (!string.IsNullOrEmpty(_bvuid3))
                {
                    request.CookieContainer.Add(new Cookie("buvid3", _bvuid3, "/", ".bilibili.com"));
                }

                if (!string.IsNullOrEmpty(_bvuid4))
                {
                    request.CookieContainer.Add(new Cookie("buvid4", _bvuid4, "/", ".bilibili.com"));
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

    public static void DownloadFile(string url, string destFile, string? referer = null)
    {
        var handler = new HttpClientHandler();

        var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", SettingsManager.GetInstance().GetUserAgent());

        if (referer != null)
        {
            client.DefaultRequestHeaders.Add("Referer", referer);
        }

        if (!url.Contains("getLogin"))
        {
            client.DefaultRequestHeaders.Add("origin", "https://m.bilibili.com");
            var cookies = LoginHelper.GetLoginInfoCookies();
            if (cookies != null)
            {
                handler.CookieContainer = cookies;
            }
        }

        var responseMessage = client.GetAsync(url).Result;
        if (!responseMessage.IsSuccessStatusCode) return;
        using var fs = File.Create(destFile);
        responseMessage.Content.ReadAsStream().CopyTo(fs);
    }
}