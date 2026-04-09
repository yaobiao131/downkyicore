using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.ViewModels;
using Prism.Events;

namespace DownKyi.Utils;

public static class NavigateToView
{
    public static string Tag = "NavigateToView";

    /// <summary>
    /// 导航到用户空间，
    /// 如果传入的mid与本地登录的mid一致，
    /// 则进入我的用户空间。
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="parentViewName"></param>
    /// <param name="mid"></param>
    /// <param name="title">标签页标题</param>
    public static void NavigateToViewUserSpace(IEventAggregator eventAggregator, string parentViewName, long mid, string? title = null)
    {
        var userInfo = SettingsManager.GetInstance().GetUserInfo();
        if (userInfo != null && userInfo.Mid == mid)
        {
            NavigationView(eventAggregator, ViewMySpaceViewModel.Tag, parentViewName, mid, title ?? "我的空间");
        }
        else
        {
            NavigationView(eventAggregator, ViewUserSpaceViewModel.Tag, parentViewName, mid, title);
        }
    }

    /// <summary>
    /// 导航到视频详情页
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="parentViewName"></param>
    /// <param name="videoId"></param>
    /// <param name="title">标签页标题（默认使用"视频详情"）</param>
    public static void NavigateToViewVideoDetail(IEventAggregator eventAggregator, string parentViewName, string videoId, string? title = null)
    {
        NavigationView(eventAggregator, ViewVideoDetailViewModel.Tag, parentViewName, videoId, title ?? "视频详情");
    }

    /// <summary>
    /// 导航到其他页面
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="viewName"></param>
    /// <param name="parentViewName"></param>
    /// <param name="param"></param>
    public static void NavigationView(IEventAggregator eventAggregator, string viewName, string parentViewName, object? param)
    {
        NavigationView(eventAggregator, viewName, parentViewName, param, null);
    }

    /// <summary>
    /// 导航到其他页面（带标题）
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="viewName"></param>
    /// <param name="parentViewName"></param>
    /// <param name="param"></param>
    /// <param name="title">标签页标题</param>
    public static void NavigationView(IEventAggregator eventAggregator, string viewName, string parentViewName, object? param, string? title)
    {
        var parameter = new NavigationParam
        {
            ViewName = viewName,
            ParentViewName = parentViewName,
            Parameter = param,
            Title = title
        };
        eventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }
}
