using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.BiliApi.Sign;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;

namespace DownKyi.Core.BiliApi;

public static class WebClient
{
    private static readonly HttpClient HttpClient;
    private static string? _bvuid3 = string.Empty;
    private static string? _bvuid4 = string.Empty;

    static WebClient()
    {
        var socketsHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(3)
        };
        switch (SettingsManager.GetInstance().GetNetworkProxy())
        {
            case NetworkProxy.None:
                socketsHandler.UseProxy = false;
                socketsHandler.Proxy = null;
                break;
            case NetworkProxy.System:
                socketsHandler.UseProxy = true;
                socketsHandler.Proxy = HttpClient.DefaultProxy;
                break;
            case NetworkProxy.Custom:
            {
                try
                {
                    socketsHandler.UseProxy = true;
                    socketsHandler.Proxy = new WebProxy(SettingsManager.GetInstance().GetCustomProxy());
                }
                catch (Exception e)
                {
                    socketsHandler.UseProxy = false;
                    socketsHandler.Proxy = null;
                    Console.WriteLine(e);
                }
            } 
                break;
        }

        HttpClient = new HttpClient(socketsHandler);
        HttpClient.DefaultRequestHeaders.Add("User-Agent", SettingsManager.GetInstance().GetUserAgent());
        HttpClient.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
    }

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

    private static void GetBuvid()
    {
        const string url = "https://api.bilibili.com/x/frontend/finger/spi";
        var response = RequestWeb(url);
        var spi = JsonSerializer.Deserialize<SpiOrigin>(response);
        _bvuid3 = spi?.Data?.Bvuid3;
        _bvuid4 = spi?.Data?.Bvuid4;
    }

    public static string RequestWeb(string url, string? referer = null, string method = "GET", Dictionary<string, object?>? parameters = null, int retry = 2, bool json = false, bool isRetryAfterKeyRefresh = false)
    {
        if (retry <= 0)
        {
            return "";
        }

        try
        {
            if (string.IsNullOrEmpty(_bvuid3) && url != "https://api.bilibili.com/x/frontend/finger/spi")
            {
                GetBuvid();
            }

            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (referer != null)
            {
                request.Headers.Referrer = new Uri(referer);
            }

            if (!url.Contains("getLogin"))
            {
                request.Headers.Add("origin", "https://www.bilibili.com");

                // 创建Cookie列表的副本以避免修改全局集合，并清理已有的相关键
                var cookies = LoginHelper.GetLoginInfoCookies().ToList();
                cookies.RemoveAll(c => c.Name == "buvid3" || c.Name == "buvid4");

                if (!string.IsNullOrEmpty(_bvuid3))
                {
                    cookies.Add(new DownKyiCookie("buvid3", HttpUtility.UrlEncode(_bvuid3)));
                }

                if (!string.IsNullOrEmpty(_bvuid4))
                {
                    cookies.Add(new DownKyiCookie("buvid4", HttpUtility.UrlEncode(_bvuid4)));
                }

                if (cookies.Count > 0)
                {
                    request.Headers.Add("cookie", string.Join("; ", cookies.Select(item => $"{item.Name}={item.Value}")));
                }
            }

            if (method == "POST" && parameters != null)
            {
                if (json)
                {
                    request.Content = new StringContent(JsonSerializer.Serialize(parameters), System.Text.Encoding.UTF8, "application/json");
                }
                else
                {
                    request.Content = new FormUrlEncodedContent(parameters.Select(item => new KeyValuePair<string, string>(item.Key, item.Value?.ToString() ?? "")));
                }
            }
            else if (parameters != null)
            {
                var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                url = $"{url}?{query}";
                request.RequestUri = new Uri(url);
            }

            var response = HttpClient.Send(request);

            // 读取响应内容，无论状态码如何
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var content = reader.ReadToEnd();

            // 如果不是成功状态码，记录错误并返回空字符串
            // 让调用方通过检查返回值是否为空来判断是否成功
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("RequestWeb()收到非成功状态码: {0}, 响应内容: {1}", response.StatusCode, content.Substring(0, Math.Min(content.Length, 200)));
                LogManager.Info("WebClient", $"Received non-success status code: {response.StatusCode}, URL: {url}");
                return string.Empty;
            }

            return content;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("RequestWeb()发生HTTP请求异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(url, referer, method, parameters, retry - 1, json, isRetryAfterKeyRefresh);
        }
        catch (Exception e)
        {
            Console.WriteLine("RequestWeb()发生其他异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(url, referer, method, parameters, retry - 1, json, isRetryAfterKeyRefresh);
        }
    }

    public static void DownloadFile(string url, string destFile, string? referer = null)
    {
        using var fs = File.Create(destFile);
        using var stream = RequestStream(url, referer);
        stream.CopyTo(fs);
    }

    public static Stream RequestStream(string url, string? referer = null, string method = "GET")
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (referer != null)
        {
            request.Headers.Referrer = new Uri(referer);
        }

        if (!url.Contains("getLogin"))
        {
            request.Headers.Add("origin", "https://m.bilibili.com");
            var cookies = LoginHelper.GetLoginInfoCookiesString();
            if (cookies is not "")
            {
                request.Headers.Add("cookie", cookies);
            }
        }

        var response = HttpClient.Send(request);
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStream();
    }
}