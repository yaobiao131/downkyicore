using System.Net;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.Storage;
using Xunit;
using BiliWebClient = DownKyi.Core.BiliApi.WebClient;

namespace DownKyi.Core.Tests.BiliApi;

public class WebClientTests
{
    [Fact]
    public void RequestWeb_SpiBuvidRequestIncludesUserAgentAndAcceptLanguage()
    {
        BiliWebClient.ResetBuvidForTests();
        var handler = new CapturingHandler(_ => JsonResponse("{\"code\":0,\"data\":{\"b_3\":\"test-buvid3\",\"b_4\":\"test-buvid4\"}}"));
        using var httpClient = new HttpClient(handler);

        BiliWebClient.RequestWeb(httpClient, "https://api.bilibili.com/x/frontend/finger/spi");

        var request = Assert.Single(handler.Requests);
        Assert.True(request.Headers.UserAgent.Any());
        Assert.Contains(request.Headers.GetValues("accept-language"), value => value.Contains("zh-CN"));
    }

    [Fact]
    public void RequestWeb_NormalApiRequestIncludesBuvidCookiesAfterSpiResponse()
    {
        BiliWebClient.ResetBuvidForTests();
        LoginHelper.SetLoginInfoCookiesForTests(null);
        var handler = new CapturingHandler(request =>
            request.RequestUri?.AbsoluteUri == "https://api.bilibili.com/x/frontend/finger/spi"
                ? JsonResponse("{\"code\":0,\"data\":{\"b_3\":\"test-buvid3\",\"b_4\":\"test-buvid4\"}}")
                : JsonResponse("{}"));
        using var httpClient = new HttpClient(handler);

        BiliWebClient.RequestWeb(httpClient, "https://api.bilibili.com/x/web-interface/nav");
        Assert.Equal(2, handler.Requests.Count);
        var cookie = Assert.Single(handler.Requests[1].Headers.GetValues("cookie"));
        Assert.Contains("buvid3=test-buvid3", cookie);
        Assert.Contains("buvid4=test-buvid4", cookie);
        Assert.Equal("https://www.bilibili.com", Assert.Single(handler.Requests[1].Headers.GetValues("origin")));
    }

    [Fact]
    public void RequestWeb_NormalApiRequestKeepsLoginCookiesWithBuvidCookies()
    {
        BiliWebClient.ResetBuvidForTests();
        LoginHelper.SetLoginInfoCookiesForTests(null);

        try
        {
            LoginHelper.SetLoginInfoCookiesForTests(new List<DownKyiCookie>
            {
                new("SESSDATA", "fake-session", ".bilibili.com"),
                new("bili_jct", "fake-jct", ".bilibili.com")
            });

            var handler = new CapturingHandler(request =>
                request.RequestUri?.AbsoluteUri == "https://api.bilibili.com/x/frontend/finger/spi"
                    ? JsonResponse("{\"code\":0,\"data\":{\"b_3\":\"test-buvid3\",\"b_4\":\"test-buvid4\"}}")
                    : JsonResponse("{}"));
            using var httpClient = new HttpClient(handler);

            BiliWebClient.RequestWeb(httpClient, "https://api.bilibili.com/x/web-interface/nav");
            var cookie = Assert.Single(handler.Requests[1].Headers.GetValues("cookie"));
            Assert.Contains("SESSDATA=fake-session", cookie);
            Assert.Contains("bili_jct=fake-jct", cookie);
            Assert.Contains("buvid3=test-buvid3", cookie);
            Assert.Contains("buvid4=test-buvid4", cookie);
        }
        finally
        {
            LoginHelper.SetLoginInfoCookiesForTests(null);
        }
    }

    [Fact]
    public void RequestWeb_GetLoginUrlDoesNotReceiveCookieOrOriginInjection()
    {
        BiliWebClient.ResetBuvidForTests();
        LoginHelper.SetLoginInfoCookiesForTests(null);

        try
        {
            LoginHelper.SetLoginInfoCookiesForTests(new List<DownKyiCookie>
            {
                new("SESSDATA", "fake-session", ".bilibili.com")
            });

            var handler = new CapturingHandler(_ => JsonResponse("{}"));
            using var httpClient = new HttpClient(handler);

            BiliWebClient.RequestWeb(httpClient, "https://example.test/getLogin");

            var request = Assert.Single(handler.Requests);
            Assert.False(request.Headers.Contains("cookie"));
            Assert.False(request.Headers.Contains("origin"));
        }
        finally
        {
            LoginHelper.SetLoginInfoCookiesForTests(null);
        }
    }

    [Fact]
    public void RequestWeb_SpiFailureDoesNotPreventNormalRequestFallback()
    {
        BiliWebClient.ResetBuvidForTests();
        LoginHelper.SetLoginInfoCookiesForTests(null);
        var handler = new CapturingHandler(request =>
            request.RequestUri?.AbsoluteUri == "https://api.bilibili.com/x/frontend/finger/spi"
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("failure") }
                : JsonResponse("normal-response"));
        using var httpClient = new HttpClient(handler);

        var response = BiliWebClient.RequestWeb(httpClient, "https://api.bilibili.com/x/web-interface/nav");
        Assert.Equal("normal-response", response);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal("https://api.bilibili.com/x/web-interface/nav", handler.Requests[2].RequestUri?.AbsoluteUri);
        Assert.False(handler.Requests[2].Headers.Contains("cookie"));
        Assert.Equal("https://www.bilibili.com", Assert.Single(handler.Requests[2].Headers.GetValues("origin")));
    }

    [Fact]
    public void RequestStream_ReturnsReadableStreamThatRemainsValidAfterReturn()
    {
        var contentStream = new TrackingStream(CreatePayload(1024 * 1024));
        var response = new TrackingResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(contentStream)
        };
        using var httpClient = new HttpClient(new StaticResponseHandler(response));

        using var stream = BiliWebClient.RequestStream(httpClient, "https://example.test/getLogin");

        Assert.True(stream.CanRead);
        Assert.Equal(0, contentStream.TotalBytesRead);

        var buffer = new byte[8192];
        var read = stream.Read(buffer, 0, buffer.Length);

        Assert.Equal(buffer.Length, read);
        Assert.Equal(buffer.Length, contentStream.TotalBytesRead);
        Assert.Equal(CreatePayload(buffer.Length), buffer);
    }

    [Fact]
    public void RequestStream_DisposeDisposesOwnedHttpResponse()
    {
        var contentStream = new TrackingStream(CreatePayload(128));
        var response = new TrackingResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(contentStream)
        };
        using var httpClient = new HttpClient(new StaticResponseHandler(response));

        var stream = BiliWebClient.RequestStream(httpClient, "https://example.test/getLogin");
        stream.Dispose();

        Assert.True(contentStream.IsDisposed);
        Assert.True(response.IsDisposed);
    }

    [Fact]
    public async Task RequestStream_CopyToAsyncCopiesContentAndDisposeAsyncDisposesOwnedHttpResponse()
    {
        var payload = CreatePayload(32 * 1024);
        var contentStream = new TrackingStream(payload);
        var response = new TrackingResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(contentStream)
        };
        using var httpClient = new HttpClient(new StaticResponseHandler(response));

        var stream = BiliWebClient.RequestStream(httpClient, "https://example.test/getLogin");
        await using var destination = new MemoryStream();

        await stream.CopyToAsync(destination);
        await stream.DisposeAsync();

        Assert.Equal(payload, destination.ToArray());
        Assert.Equal(payload.Length, contentStream.TotalBytesRead);
        Assert.True(contentStream.IsDisposed);
        Assert.True(response.IsDisposed);
    }

    [Fact]
    public void DownloadFile_WritesResponseContentToDisk()
    {
        var payload = CreatePayload(64 * 1024);
        var response = new TrackingResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new TrackingStream(payload))
        };
        using var httpClient = new HttpClient(new StaticResponseHandler(response));
        var destFile = Path.Combine(Path.GetTempPath(), $"downkyi-webclient-{Guid.NewGuid():N}.bin");

        try
        {
            BiliWebClient.DownloadFile(httpClient, "https://example.test/getLogin", destFile);

            Assert.Equal(payload, File.ReadAllBytes(destFile));
            Assert.True(response.IsDisposed);
        }
        finally
        {
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
        }
    }

    [Fact]
    public void RequestStream_DisposesResponseWhenStatusIsUnsuccessful()
    {
        var response = new TrackingResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StreamContent(new TrackingStream(CreatePayload(128)))
        };
        using var httpClient = new HttpClient(new StaticResponseHandler(response));

        Assert.Throws<HttpRequestException>(() => BiliWebClient.RequestStream(httpClient, "https://example.test/getLogin"));
        Assert.True(response.IsDisposed);
    }

    private static byte[] CreatePayload(int length)
    {
        var payload = new byte[length];
        for (var i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)(i % 251);
        }

        return payload;
    }

    private sealed class StaticResponseHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StaticResponseHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }


    private static HttpResponseMessage JsonResponse(string content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));
            return _responseFactory(request);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));
            return Task.FromResult(_responseFactory(request));
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }

    private sealed class TrackingResponseMessage : HttpResponseMessage
    {
        public TrackingResponseMessage(HttpStatusCode statusCode) : base(statusCode)
        {
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class TrackingStream : Stream
    {
        private readonly MemoryStream _inner;

        public TrackingStream(byte[] payload)
        {
            _inner = new MemoryStream(payload, writable: false);
        }

        public long TotalBytesRead { get; private set; }

        public bool IsDisposed { get; private set; }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => false;

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

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, count);
            TotalBytesRead += read;
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            var read = _inner.Read(buffer);
            TotalBytesRead += read;
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken);
            TotalBytesRead += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await _inner.ReadAsync(buffer, cancellationToken);
            TotalBytesRead += read;
            return read;
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
            IsDisposed = true;
            await _inner.DisposeAsync();
            await base.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
