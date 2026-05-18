using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using Console = DownKyi.Core.Utils.Debugging.Console;

[assembly: InternalsVisibleTo("DownKyi.Core.Tests")]

namespace DownKyi.Core.BiliApi;

public static class WebClient
{
    private const string Tag = "WebClient";
    private const string SpiUrl = "https://api.bilibili.com/x/frontend/finger/spi";
    private const string BilibiliOrigin = "https://www.bilibili.com";
    private const string AcceptLanguage = "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7";

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
                    Console.PrintLine("WebClient初始化自定义代理发生异常: {0}", e);
                    LogManager.Error("WebClient", e);
                }
            }
                break;
        }

        HttpClient = new HttpClient(socketsHandler);
        EnsureDefaultHeaders(HttpClient);
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

    private static void GetBuvid(HttpClient httpClient)
    {
        try
        {
            var response = RequestWeb(httpClient, SpiUrl);
            if (string.IsNullOrWhiteSpace(response))
            {
                Console.PrintLine("GetBuvid()返回空响应");
                LogManager.Error(Tag, "GetBuvid() returned an empty response.");
                return;
            }

            var spi = JsonSerializer.Deserialize<SpiOrigin>(response);
            _bvuid3 = spi?.Data?.Bvuid3;
            _bvuid4 = spi?.Data?.Bvuid4;
        }
        catch (Exception e)
        {
            Console.PrintLine("GetBuvid()发生异常: {0}", e);
            LogManager.Error(Tag, e);
        }
    }

    public static string RequestWeb(string url, string? referer = null, string method = "GET", Dictionary<string, object?>? parameters = null, int retry = 2, bool json = false)
    {
        return RequestWeb(HttpClient, url, referer, method, parameters, retry, json);
    }

    internal static string RequestWeb(HttpClient httpClient, string url, string? referer = null, string method = "GET", Dictionary<string, object?>? parameters = null, int retry = 2, bool json = false)
    {
        if (retry <= 0)
        {
            return "";
        }

        var retryUrl = url;

        try
        {
            PrepareHttpClientForRequest(httpClient);
            EnsureBuvidForRequest(httpClient, url);

            using var request = CreateRequestMessage(url, referer, method, parameters, json);
            retryUrl = CreateRequestUrl(url, method, parameters);
            ApplyBiliRequestHeaders(request, url);

            return SendRequestWeb(httpClient, request);
        }
        catch (HttpRequestException e)
        {
            Console.PrintLine("RequestWeb()发生HTTP请求异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(httpClient, retryUrl, referer, method, parameters, retry - 1, json);
        }
        catch (Exception e)
        {
            Console.PrintLine("RequestWeb()发生其他异常: {0}", e);
            LogManager.Error(e);
            return RequestWeb(httpClient, retryUrl, referer, method, parameters, retry - 1, json);
        }
    }

    private static void PrepareHttpClientForRequest(HttpClient httpClient)
    {
        EnsureDefaultHeaders(httpClient);
    }

    private static void EnsureBuvidForRequest(HttpClient httpClient, string url)
    {
        if (string.IsNullOrEmpty(_bvuid3) && !IsSpiUrl(url) && !IsGetLoginUrl(url))
        {
            GetBuvid(httpClient);
        }
    }

    internal static HttpRequestMessage CreateRequestMessage(string url, string? referer = null, string method = "GET", Dictionary<string, object?>? parameters = null, bool json = false)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), CreateRequestUri(url, method, parameters));
        ApplyReferer(request, referer);

        if (method == "POST" && parameters != null)
        {
            request.Content = CreatePostContent(parameters, json);
        }

        return request;
    }

    private static Uri CreateRequestUri(string url, string method, Dictionary<string, object?>? parameters)
    {
        return new Uri(CreateRequestUrl(url, method, parameters));
    }

    private static string CreateRequestUrl(string url, string method, Dictionary<string, object?>? parameters)
    {
        if (method == "POST" || parameters == null)
        {
            return url;
        }

        var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{url}?{query}";
    }

    private static HttpContent CreatePostContent(Dictionary<string, object?> parameters, bool json)
    {
        if (json)
        {
            return new StringContent(JsonSerializer.Serialize(parameters), System.Text.Encoding.UTF8, "application/json");
        }

        return new FormUrlEncodedContent(parameters.Select(item => new KeyValuePair<string, string>(item.Key, item.Value?.ToString() ?? "")));
    }

    private static void ApplyReferer(HttpRequestMessage request, string? referer)
    {
        if (referer != null)
        {
            request.Headers.Referrer = new Uri(referer);
        }
    }

    internal static void ApplyBiliRequestHeaders(HttpRequestMessage request, string url)
    {
        if (IsGetLoginUrl(url))
        {
            return;
        }

        request.Headers.Add("origin", BilibiliOrigin);

        var cookies = LoginHelper.GetLoginInfoCookies()
            .Select(item => new DownKyiCookie(item.Name, item.Value, item.Domain))
            .ToList();

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

    internal static string SendRequestWeb(HttpClient httpClient, HttpRequestMessage request)
    {
        using var response = httpClient.Send(request);
        response.EnsureSuccessStatusCode();

        using var reader = new StreamReader(response.Content.ReadAsStream());
        return reader.ReadToEnd();
    }

    private static bool IsSpiUrl(string url)
    {
        return string.Equals(url, SpiUrl, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGetLoginUrl(string url)
    {
        return url.Contains("getLogin", StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureDefaultHeaders(HttpClient httpClient)
    {
        if (!httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", SettingsManager.GetInstance().GetUserAgent());
        }

        if (!httpClient.DefaultRequestHeaders.AcceptLanguage.Any())
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", AcceptLanguage);
        }
    }

    internal static void ResetBuvidForTests()
    {
        _bvuid3 = string.Empty;
        _bvuid4 = string.Empty;
    }

    public static void DownloadFile(string url, string destFile, string? referer = null)
    {
        DownloadFile(HttpClient, url, destFile, referer);
    }

    internal static void DownloadFile(HttpClient httpClient, string url, string destFile, string? referer = null)
    {
        using var fs = File.Create(destFile);
        using var stream = RequestStream(httpClient, url, referer);
        stream.CopyTo(fs);
    }

    public static Stream RequestStream(string url, string? referer = null, string method = "GET")
    {
        return RequestStream(HttpClient, url, referer, method);
    }

    internal static Stream RequestStream(HttpClient httpClient, string url, string? referer = null, string method = "GET")
    {
        using var request = CreateStreamRequestMessage(url, referer, method);
        var response = SendRequestStream(httpClient, request);

        try
        {
            response.EnsureSuccessStatusCode();
            return new HttpResponseStream(response.Content.ReadAsStream(), response);
        }
        catch (Exception e)
        {
            Console.PrintLine("RequestStream()发生异常: {0}", e);
            LogManager.Error("RequestStream()", e);
            response.Dispose();
            throw;
        }
    }

    internal static HttpRequestMessage CreateStreamRequestMessage(string url, string? referer = null, string method = "GET")
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        ApplyReferer(request, referer);
        ApplyStreamRequestHeaders(request, url);
        return request;
    }

    internal static void ApplyStreamRequestHeaders(HttpRequestMessage request, string url)
    {
        if (url.Contains("getLogin"))
        {
            return;
        }

        request.Headers.Add("origin", "https://m.bilibili.com");
        var cookies = LoginHelper.GetLoginInfoCookiesString();
        if (cookies is not "")
        {
            request.Headers.Add("cookie", cookies);
        }
    }

    internal static HttpResponseMessage SendRequestStream(HttpClient httpClient, HttpRequestMessage request)
    {
        return httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
    }

    private sealed class HttpResponseStream : Stream
    {
        private readonly Stream _inner;
        private readonly HttpResponseMessage _response;

        public HttpResponseStream(Stream inner, HttpResponseMessage response)
        {
            _inner = inner;
            _response = response;
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _inner.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            return _inner.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _inner.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _inner.ReadAsync(buffer, cancellationToken);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _inner.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await _inner.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                _response.Dispose();
            }

            await base.DisposeAsync().ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
