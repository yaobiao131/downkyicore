using System;
using System.Diagnostics;
using DownKyi.Events;
using Prism.Events;

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

        if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", $"\"{folder}\"");
        }
    }

    /// <summary>
    /// 打开各种 (文件、url)
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <param name="eventAggregator"></param>
    public static void Open(string filename, IEventAggregator? eventAggregator = null)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", filename);
            }

            if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", $"\"{filename}\"");
            }

            if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", $"\"{filename}\"");
            }
        }
        catch (Exception e)
        {
            eventAggregator?.GetEvent<MessageEvent>().Publish("没有可以打开网址的默认程序");
        }
    }
}