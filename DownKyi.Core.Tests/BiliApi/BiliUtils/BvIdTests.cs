using DownKyi.Core.BiliApi.BiliUtils;
using Xunit;

namespace DownKyi.Core.Tests.BiliApi.BiliUtils;

public class BvIdTests
{
    [Theory]
    [InlineData(1, "BV1xx411c7mQ")]
    [InlineData(2, "BV1xx411c7mD")]
    [InlineData(170001, "BV17x411w7KC")]
    [InlineData(455017605, "BV1Q541167Qg")]
    [InlineData(882584971, "BV1mK4y1C7Bz")]
    public void Av2Bv_ReturnsKnownBvid(long aid, string expected)
    {
        Assert.Equal(expected, BvId.Av2Bv(aid));
    }

    [Theory]
    [InlineData("BV1xx411c7mQ", 1)]
    [InlineData("BV1xx411c7mD", 2)]
    [InlineData("BV17x411w7KC", 170001)]
    [InlineData("BV1Q541167Qg", 455017605)]
    [InlineData("BV1mK4y1C7Bz", 882584971)]
    public void Bv2Av_ReturnsKnownAid(string bvid, long expected)
    {
        Assert.Equal(expected, BvId.Bv2Av(bvid));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(114514)]
    [InlineData(170001)]
    [InlineData(455017605)]
    [InlineData(882584971)]
    [InlineData(1000000000)]
    public void AvAndBvConversions_RoundTrip(long aid)
    {
        var bvid = BvId.Av2Bv(aid);

        Assert.Equal(aid, BvId.Bv2Av(bvid));
    }
}
