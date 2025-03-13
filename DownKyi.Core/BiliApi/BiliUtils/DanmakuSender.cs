namespace DownKyi.Core.BiliApi.BiliUtils;

public static class DanmakuSender
{
    private const uint Crcpolynomial = 0xEDB88320;
    private static readonly uint[] Crctable = new uint[256];

    static DanmakuSender()
    {
        CreateTable();
    }

    private static void CreateTable()
    {
        for (var i = 0; i < 256; i++)
        {
            var crcreg = (uint)i;

            for (var j = 0; j < 8; j++)
            {
                if ((crcreg & 1) != 0)
                {
                    crcreg = Crcpolynomial ^ (crcreg >> 1);
                }
                else
                {
                    crcreg >>= 1;
                }
            }

            Crctable[i] = crcreg;
        }
    }

    private static uint Crc32(string userId)
    {
        var crcstart = 0xFFFFFFFF;
        foreach (var index in userId.Select(t => (uint)(crcstart ^ (int)t) & 255))
        {
            crcstart = (crcstart >> 8) ^ Crctable[index];
        }

        return crcstart;
    }

    private static uint Crc32LastIndex(string userId)
    {
        uint index = 0;
        var crcstart = 0xFFFFFFFF;
        foreach (var t in userId)
        {
            index = (uint)((crcstart ^ (int)t) & 255);
            crcstart = (crcstart >> 8) ^ Crctable[index];
        }

        return index;
    }

    private static int GetCrcIndex(long t)
    {
        for (var i = 0; i < 256; i++)
        {
            if ((Crctable[i] >> 24) == t)
            {
                return i;
            }
        }

        return -1;
    }

    private static object[] DeepCheck(int i, int[] index)
    {
        var resultArray = new object[2];

        var result = "";
        uint tc; // = 0x00;
        var hashcode = Crc32(i.ToString());
        tc = (uint)(hashcode & 0xff ^ index[2]);

        if (!(tc <= 57 && tc >= 48))
        {
            resultArray[0] = 0;
            return resultArray;
        }

        result += (tc - 48).ToString();
        hashcode = Crctable[index[2]] ^ (hashcode >> 8);
        tc = (uint)(hashcode & 0xff ^ index[1]);

        if (!(tc <= 57 && tc >= 48))
        {
            resultArray[0] = 0;
            return resultArray;
        }

        result += (tc - 48).ToString();
        hashcode = Crctable[index[1]] ^ (hashcode >> 8);
        tc = (uint)(hashcode & 0xff ^ index[0]);

        if (!(tc <= 57 && tc >= 48))
        {
            resultArray[0] = 0;
            return resultArray;
        }

        result += (tc - 48).ToString();
        //hashcode = crctable[index[0]] ^ (hashcode >> 8);

        resultArray[0] = 1;
        resultArray[1] = result;
        return resultArray;
    }

    /// <summary>
    /// 查询弹幕发送者
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string FindDanmakuSender(string userId)
    {
        var deepCheckData = new object[2];

        var index = new int[4];
        var ht = (uint)Convert.ToInt32($"0x{userId}", 16);
        ht ^= 0xffffffff;

        int i;
        for (i = 3; i > -1; i--)
        {
            index[3 - i] = GetCrcIndex(ht >> (i * 8));
            var snum = Crctable[index[3 - i]];
            ht ^= snum >> ((3 - i) * 8);
        }

        for (i = 0; i < 100000000; i++)
        {
            var lastindex = Crc32LastIndex(i.ToString());
            if (lastindex != index[3]) continue;
            deepCheckData = DeepCheck(i, index);
            if ((int)deepCheckData[0] != 0)
            {
                break;
            }
        }

        return i == 100000000 ? "-1" : $"{i}{deepCheckData[1]}";
    }
}