using System;
using System.Diagnostics;

namespace DownKyi.Utils;

public static class PlatformHelper
{
    /// <summary>
    /// 打开文件夹
    /// </summary>
    /// <param name="folder">路径</param>
    public static void OpenFolder(string folder)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", $"{folder}");
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", $"\"{folder}\"");
        }
    }

    /// <summary>
    /// 打开各种 (文件、url)
    /// </summary>
    /// <param name="filename">文件名</param>
    public static void Open(string filename)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(filename);
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", $"\"{filename}\"");
        }
    }
}