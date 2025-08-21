using System;
using Avalonia.Controls;
using DownKyi.Core.Logging;
using DownKyi.Events;
using Prism.Events;

namespace DownKyi.Utils;

public static class PlatformHelper
{
    /// <summary>
    /// 打开文件夹
    /// </summary>
    /// <param name="folder">路径</param>
    /// <param name="eventAggregator"></param>
    public static void OpenFolder(string folder, IEventAggregator? eventAggregator = null)
    {
        var topLevel = TopLevel.GetTopLevel(App.Current.MainWindow);
        if (topLevel == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取顶层窗口，无法打开文件夹");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取顶层窗口，无法打开文件夹");
            return;
        }

        var openFolder = topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(folder)).Result;
        if (openFolder == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取文件夹路径");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取文件夹路径");
            return;
        }

        _ = topLevel.Launcher.LaunchFileAsync(openFolder).Result;
    }

    /// <summary>
    /// 打开各种 (文件、url)
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <param name="eventAggregator"></param>
    public static void Open(string filename, IEventAggregator? eventAggregator = null)
    {
        var topLevel = TopLevel.GetTopLevel(App.Current.MainWindow);
        if (topLevel == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取顶层窗口，无法打开文件");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取顶层窗口，无法打开文件");
            return;
        }

        var openFolder = topLevel.StorageProvider.TryGetFileFromPathAsync(new Uri(filename)).Result;
        if (openFolder == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取文件路径");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取文件路径");
            return;
        }

        _ = topLevel.Launcher.LaunchFileAsync(openFolder).Result;
    }

    public static void OpenUrl(string url, IEventAggregator? eventAggregator = null)
    {
        var topLevel = TopLevel.GetTopLevel(App.Current.MainWindow);
        if (topLevel == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取顶层窗口");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取顶层窗口");
            return;
        }

        _ = topLevel.Launcher.LaunchUriAsync(new Uri(url)).Result;
    }
}