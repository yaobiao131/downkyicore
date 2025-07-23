using System.Numerics;

namespace DownKyi.Core.BiliApi.BiliUtils;

public static class BvId
{
    // 常量定义
    private static readonly BigInteger XorCode = BigInteger.Parse("23442827791579");
    private static readonly BigInteger MaskCode = BigInteger.Parse("2251799813685247");
    private static readonly BigInteger MaxAid = BigInteger.One << 51;
    private const long Base = 58;

    // Base58字符集
    private const string Data = "FcwAPNKTMug3GV5Lj7EJnHpWsx4tb8haYeviqBz6rkCy12mUSDQX9RdoZf";

    // 为了提高BvToAv的性能，预先构建字符到索引的映射
    private static readonly Dictionary<char, int> DataMap;

    static BvId()
    {
        DataMap = new Dictionary<char, int>();
        for (var i = 0; i < Data.Length; i++)
        {
            DataMap[Data[i]] = i;
        }
    }

    /// <summary>
    /// 将av号转换为bv号
    /// </summary>
    /// <param name="aid">av号 (aid)</param>
    /// <returns>bv号 (bvid)</returns>
    public static string Av2Bv(long aid)
    {
        var bytes = new[] { 'B', 'V', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
        var bvIndex = bytes.Length - 1;

        var tmp = (MaxAid | aid) ^ XorCode;

        while (tmp > 0)
        {
            var index = (int)(tmp % Base);
            bytes[bvIndex] = Data[index];
            tmp /= Base;
            bvIndex--;
        }

        (bytes[3], bytes[9]) = (bytes[9], bytes[3]);
        (bytes[4], bytes[7]) = (bytes[7], bytes[4]);

        return new string(bytes);
    }

    /// <summary>
    /// 将bv号转换为av号
    /// </summary>
    /// <param name="bvid">bv号 (bvid)</param>
    /// <returns>av号 (aid)</returns>
    public static long Bv2Av(string bvid)
    {
        var bvidArr = bvid.ToCharArray();

        (bvidArr[3], bvidArr[9]) = (bvidArr[9], bvidArr[3]);
        (bvidArr[4], bvidArr[7]) = (bvidArr[7], bvidArr[4]);

        BigInteger tmp = 0;

        for (var i = 3; i < bvidArr.Length; i++)
        {
            tmp = tmp * Base + DataMap[bvidArr[i]];
        }

        var result = (tmp & MaskCode) ^ XorCode;

        return (long)result;
    }
}