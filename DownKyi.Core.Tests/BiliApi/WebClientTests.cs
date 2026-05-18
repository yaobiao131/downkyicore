using System.Net;
using Xunit;
using BiliWebClient = DownKyi.Core.BiliApi.WebClient;

namespace DownKyi.Core.Tests.BiliApi;

public class WebClientTests
{
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
