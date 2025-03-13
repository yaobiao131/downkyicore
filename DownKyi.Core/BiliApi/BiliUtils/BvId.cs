namespace DownKyi.Core.BiliApi.BiliUtils;

public static class BvId
{
    private const string TableStr = "fZodR9XQDSUm21yCkr6zBqiveYah8bt4xsWpHnJE7jL5VG3guMTKNPAwcF"; //码表
    private static readonly char[] Table = TableStr.ToCharArray();

    private static readonly char[] Tr = new char[124]; //反查码表
    private const ulong Xor = 177451812; //固定异或值
    private const ulong Add = 8728348608; //固定加法值
    private static readonly int[] S = { 11, 10, 3, 8, 4, 6 }; //位置编码表

    static BvId()
    {
        Tr_init();
    }

    //初始化反查码表
    private static void Tr_init()
    {
        for (var i = 0; i < 58; i++)
            Tr[Table[i]] = (char)i;
    }

    /// <summary>
    /// bvid转avid
    /// </summary>
    /// <param name="bvid"></param>
    /// <returns></returns>
    public static ulong Bv2Av(string bvid)
    {
        var bv = bvid.ToCharArray();

        ulong r = 0;
        ulong av;
        for (var i = 0; i < 6; i++)
            r += Tr[bv[S[i]]] * (ulong)Math.Pow(58, i);
        av = (r - Add) ^ Xor;
        return av;
    }

    /// <summary>
    /// avid转bvid
    /// </summary>
    /// <param name="av"></param>
    /// <returns></returns>
    public static string Av2Bv(ulong av)
    {
        //编码结果
        const string res = "BV1  4 1 7  ";
        var result = res.ToCharArray();

        av = (av ^ Xor) + Add;
        for (var i = 0; i < 6; i++)
            result[S[i]] = Table[av / (ulong)Math.Pow(58, i) % 58];
        var bv = new string(result);
        return bv;
    }
}