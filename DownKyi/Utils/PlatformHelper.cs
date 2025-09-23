using System;
using System.Threading.Tasks;
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
    public static async Task OpenFolder(string folder, IEventAggregator? eventAggregator = null)
    {
        var topLevel = TopLevel.GetTopLevel(App.Current.MainWindow);
        if (topLevel == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取顶层窗口，无法打开文件夹");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取顶层窗口，无法打开文件夹");
            return;
        }

        var openFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(folder));
        if (openFolder == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取文件夹路径");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取文件夹路径");
            return;
        }

        _ = await topLevel.Launcher.LaunchFileAsync(openFolder);
    }

    /// <summary>
    /// 打开各种 (文件、url)
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <param name="eventAggregator"></param>
    public static async Task Open(string filename, IEventAggregator? eventAggregator = null)
    {
        var topLevel = TopLevel.GetTopLevel(App.Current.MainWindow);
        if (topLevel == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取顶层窗口，无法打开文件");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取顶层窗口，无法打开文件");
            return;
        }

        var openFolder = await topLevel.StorageProvider.TryGetFileFromPathAsync(new Uri(filename));
        if (openFolder == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取文件路径");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取文件路径");
            return;
        }

        _ = await topLevel.Launcher.LaunchFileAsync(openFolder);
    }

    public static async Task OpenUrl(string url, IEventAggregator? eventAggregator = null)
    {
        var topLevel = TopLevel.GetTopLevel(App.Current.MainWindow);
        if (topLevel == null)
        {
            LogManager.Error(nameof(PlatformHelper), "无法获取顶层窗口");
            eventAggregator?.GetEvent<MessageEvent>().Publish("无法获取顶层窗口");
            return;
        }

        _ = await topLevel.Launcher.LaunchUriAsync(new Uri(url));
    }
}