using System;
using System.Text.RegularExpressions;

namespace DownKyi.Models;

public class AppInfo
{
    public string Name { get; } = "哔哩下载姬";
    public int VersionCode { get; }
    public string VersionName { get; }

    private const int A = 1;
    private const int B = 0;
    private const int C = 20;

    public AppInfo()
    {
        VersionCode = A * 10000 + B * 100 + C;

#if DEBUG
        VersionName = $"{A}.{B}.{C}-debug";
#else
        VersionName = $"{A}.{B}.{C}";
#endif
    }

    public static int VersionNameToCode(string versionName)
    {
        var code = 0;

        var isMatch = Regex.IsMatch(versionName, @"^v?([1-9]\d|\d).([1-9]\d|\d).([1-9]\d|\d)$");
        if (!isMatch)
        {
            return 0;
        }

        var pattern = @"([1-9]\d|\d)";
        var m = Regex.Matches(versionName, pattern);
        if (m.Count == 3)
        {
            var i = 2;
            foreach (var item in m)
            {
                code += int.Parse(item.ToString()!) * (int)Math.Pow(100, i);
                i--;
            }
        }

        return code;
    }
}