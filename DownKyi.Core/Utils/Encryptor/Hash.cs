using System.Security.Cryptography;
using System.Text;

namespace DownKyi.Core.Utils.Encryptor;

public static class Hash
{
    /// <summary>
    /// 计算字符串MD5值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string? GetMd5Hash(string? input)
    {
        if (input == null)
        {
            return null;
        }

        var md5Hash = MD5.Create();

        // 将输入字符串转换为字节数组并计算哈希数据
        var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // 创建一个 Stringbuilder 来收集字节并创建字符串
        var sBuilder = new StringBuilder();

        // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串
        foreach (var t in data)
        {
            sBuilder.Append(t.ToString("x2"));
        }

        // 返回十六进制字符串
        return sBuilder.ToString();
    }

    /// <summary>
    /// 计算文件MD5值
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            var file = new FileStream(fileName, FileMode.Open);
            var md5 = MD5.Create();
            var retVal = md5.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            foreach (var t in retVal)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
        catch (Exception e)
        {
            throw new Exception("GetMD5HashFromFile()发生异常: {0}" + e.Message);
        }
    }
}