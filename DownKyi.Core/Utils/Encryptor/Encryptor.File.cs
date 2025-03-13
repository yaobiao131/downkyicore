using System.Security.Cryptography;

namespace DownKyi.Core.Utils.Encryptor;

public static partial class Encryptor
{
    private const ulong FcTag = 0xFC010203040506CF;
    private const int BufferSize = 128 * 1024;

    /// <summary>
    /// 加密文件
    /// </summary>
    /// <param name="inFile">待加密文件</param>
    /// <param name="outFile">加密后输入文件</param>
    /// <param name="password">加密密码</param>
    public static void EncryptFile(string inFile, string outFile, string password)
    {
        using FileStream fin = File.OpenRead(inFile), fout = File.OpenWrite(outFile);
        var lSize = fin.Length; // 输入文件长度
        var size = (int)lSize;
        var bytes = new byte[BufferSize]; // 缓存
        var read = -1; // 输入文件读取数量
        var value = 0;

        // 获取IV和salt
        var iv = GenerateRandomBytes(16);
        var salt = GenerateRandomBytes(16);

        // 创建加密对象
        var sma = CreateRijndael(password, salt);
        sma.IV = iv;

        // 在输出文件开始部分写入IV和salt
        fout.Write(iv, 0, iv.Length);
        fout.Write(salt, 0, salt.Length);

        // 创建散列加密
        HashAlgorithm hasher = SHA256.Create();
        using CryptoStream cout = new(fout, sma.CreateEncryptor(), CryptoStreamMode.Write), chash = new(Stream.Null, hasher, CryptoStreamMode.Write);
        var bw = new BinaryWriter(cout);
        bw.Write(lSize);

        bw.Write(FcTag);

        // 读写字节块到加密流缓冲区
        while ((read = fin.Read(bytes, 0, bytes.Length)) != 0)
        {
            cout.Write(bytes, 0, read);
            chash.Write(bytes, 0, read);
            value += read;
        }

        // 关闭加密流
        chash.Flush();
        chash.Close();

        // 读取散列
        var hash = hasher.Hash;

        // 输入文件写入散列
        cout.Write(hash, 0, hash.Length);

        // 关闭文件流
        cout.Flush();
        cout.Close();
    }

    /// <summary>
    /// 解密文件
    /// </summary>
    /// <param name="inFile">待解密文件</param>
    /// <param name="outFile">解密后输出文件</param>
    /// <param name="password">解密密码</param>
    public static void DecryptFile(string inFile, string outFile, string password)
    {
        // 创建打开文件流
        using FileStream fin = File.OpenRead(inFile), fout = File.OpenWrite(outFile);
        var size = (int)fin.Length;
        var bytes = new byte[BufferSize];
        var read = -1;
        var value = 0;
        var outValue = 0;

        var iv = new byte[16];
        fin.Read(iv, 0, 16);
        var salt = new byte[16];
        fin.Read(salt, 0, 16);

        var sma = CreateRijndael(password, salt);
        sma.IV = iv;

        value = 32;
        long lSize = -1;

        // 创建散列对象, 校验文件
        HashAlgorithm hasher = SHA256.Create();

        try
        {
            using CryptoStream cin = new(fin, sma.CreateDecryptor(), CryptoStreamMode.Read),
                chash = new(Stream.Null, hasher, CryptoStreamMode.Write);
            // 读取文件长度
            var br = new BinaryReader(cin);
            lSize = br.ReadInt64();
            var tag = br.ReadUInt64();

            if (FcTag != tag) throw new CryptoHelpException("文件被破坏");

            var numReads = lSize / BufferSize;

            var slack = lSize % BufferSize;

            for (var i = 0; i < numReads; ++i)
            {
                read = cin.Read(bytes, 0, bytes.Length);
                fout.Write(bytes, 0, read);
                chash.Write(bytes, 0, read);
                value += read;
                outValue += read;
            }

            if (slack > 0)
            {
                read = cin.Read(bytes, 0, (int)slack);
                fout.Write(bytes, 0, read);
                chash.Write(bytes, 0, read);
                value += read;
                outValue += read;
            }

            chash.Flush();
            chash.Close();

            fout.Flush();
            fout.Close();

            var curHash = hasher.Hash;

            // 获取比较和旧的散列对象
            var oldHash = new byte[hasher.HashSize / 8];
            read = cin.Read(oldHash, 0, oldHash.Length);
            if (oldHash.Length != read || !CheckByteArrays(oldHash, curHash))
                throw new CryptoHelpException("文件被破坏");
        }
        catch (Exception e)
        {
            Console.WriteLine("DecryptFile()发生异常: {0}", e);
        }

        if (outValue != lSize)
            throw new CryptoHelpException("文件大小不匹配");
    }

    /// <summary>
    /// 检验两个Byte数组是否相同
    /// </summary>
    /// <param name="b1">Byte数组</param>
    /// <param name="b2">Byte数组</param>
    /// <returns>true－相等</returns>
    private static bool CheckByteArrays(byte[] b1, byte[] b2)
    {
        if (b1.Length == b2.Length)
        {
            for (var i = 0; i < b1.Length; ++i)
            {
                if (b1[i] != b2[i])
                    return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 创建DebugLZQ ,http://www.cnblogs.com/DebugLZQ
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="salt"></param>
    /// <returns>加密对象</returns>
    private static SymmetricAlgorithm CreateRijndael(string password, byte[] salt)
    {
        var pdb = new PasswordDeriveBytes(password, salt, "SHA256", 1000);

        SymmetricAlgorithm sma = Aes.Create();
        sma.KeySize = 256;
        sma.Key = pdb.GetBytes(32);
        sma.Padding = PaddingMode.Zeros;
        return sma;
    }

    /// <summary>
    /// 生成指定长度的随机Byte数组
    /// </summary>
    /// <param name="count">Byte数组长度</param>
    /// <returns>随机Byte数组</returns>
    private static byte[] GenerateRandomBytes(int count)
    {
        // 加密文件随机数生成
        var rand = RandomNumberGenerator.Create();

        var bytes = new byte[count];
        rand.GetBytes(bytes);
        return bytes;
    }
}

/// <summary>
/// 异常处理类
/// </summary>
public class CryptoHelpException : ApplicationException
{
    public CryptoHelpException(string msg) : base(msg)
    {
    }
}