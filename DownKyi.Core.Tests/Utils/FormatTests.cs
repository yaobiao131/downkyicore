using DownKyi.Core.Utils;
using Xunit;

namespace DownKyi.Core.Tests.Utils;

public class FormatTests
{
    [Theory]
    [InlineData(0, "0s")]
    [InlineData(59, "59s")]
    [InlineData(60, "1m0s")]
    [InlineData(3661, "1h1m1s")]
    public void FormatDuration_ReturnsCompactDuration(long duration, string expected)
    {
        Assert.Equal(expected, Format.FormatDuration(duration));
    }

    [Theory]
    [InlineData(0, "00:00:00")]
    [InlineData(59, "00:00:59")]
    [InlineData(60, "00:01:00")]
    [InlineData(3661, "01:01:01")]
    public void FormatDuration2_ReturnsClockDuration(long duration, string expected)
    {
        Assert.Equal(expected, Format.FormatDuration2(duration));
    }

    [Theory]
    [InlineData(9999, "9999")]
    [InlineData(10000, "1.0万")]
    [InlineData(100000000, "1.0亿")]
    public void FormatNumber_ReturnsLocalizedLargeNumber(long number, string expected)
    {
        Assert.Equal(expected, Format.FormatNumber(number));
    }

    [Theory]
    [InlineData(0, "0B")]
    [InlineData(1023, "1023B")]
    [InlineData(1024, "1KB")]
    [InlineData(1048576, "1MB")]
    [InlineData(1073741824, "1GB")]
    public void FormatFileSize_ReturnsReadableFileSize(long fileSize, string expected)
    {
        Assert.Equal(expected, Format.FormatFileSize(fileSize));
    }

    [Theory]
    [InlineData("  .video title.  ", "video title")]
    [InlineData("normal-name", "normal-name")]
    public void FormatFileName_TrimsLeadingAndTrailingSpacesOrDots(string originName, string expected)
    {
        Assert.Equal(expected, Format.FormatFileName(originName));
    }
}
