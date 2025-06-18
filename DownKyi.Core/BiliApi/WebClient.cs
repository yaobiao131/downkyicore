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
    private static readonly HttpClient _httpClient;
    private static string? _bvuid3 = string.Empty;
    private static string? _bvuid4 = string.Empty;

    static WebClient()
    {
        var socketsHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10), 
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(10) 
        };
        switch (SettingsManager.GetInstance().GetNetworkProxy())
        {
            case NetworkProxy.None:
                socketsHandler.UseProxy = false;
                socketsHandler.Proxy = null;
                break;
            case NetworkProxy.System:
                socketsHandler.UseProxy = true;
                socketsHandler.Proxy = WebRequest.GetSystemWebProxy();
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
                    socketsHandler.UseProxy = true;
                    Console.WriteLine(e);
                }
            }
                break;
        }
        
        _httpClient = new HttpClient(socketsHandler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", SettingsManager.GetInstance().GetUserAgent());
        _httpClient.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
        
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

    public static string RequestWeb(string url, string? referer = null, string method = "GET",
        Dictionary<string, string>? parameters = null, int retry = 3)
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

                var cookies = LoginHelper.GetLoginInfoCookiesString();

                if (!string.IsNullOrEmpty(_bvuid3))
                {
                    cookies += $"; buvid3={_bvuid3}";
                }

                if (!string.IsNullOrEmpty(_bvuid4))
                {
                    cookies += $"; buvid4={_bvuid4}";
                }

                if (cookies is not "")
                {
                    request.Headers.Add("cookie", cookies);
                }
            }

            if (method == "POST" && parameters != null)
            {
                request.Content = new FormUrlEncodedContent(parameters);
            }
            else if (parameters != null)
            {
                var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                url = $"{url}?{query}";
                request.RequestUri = new Uri(url);
            }

            var response = _httpClient.Send(request);
            response.EnsureSuccessStatusCode();
            
            using var reader = new StreamReader(response.Content.ReadAsStream());
            return reader.ReadToEnd();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("RequestWeb()发生HTTP请求异常: {0}", e);
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
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

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

            var response = _httpClient.Send(request);
            response.EnsureSuccessStatusCode();

            using var fs = File.Create(destFile);
            response.Content.ReadAsStream().CopyTo(fs);
        }
        catch (Exception e)
        {
            Console.WriteLine("DownloadFile()发生异常: {0}", e);
            LogManager.Error(e);
            throw;
        }
    }
}