namespace DownKyi.Events;

/// <summary>
/// 导航参数
/// </summary>
public class NavigationParam
{
    /// <summary>
    /// 目标视图名称
    /// </summary>
    public string? ViewName { get; set; }

    /// <summary>
    /// 父视图名称
    /// </summary>
    public string? ParentViewName { get; set; }

    /// <summary>
    /// 导航参数
    /// </summary>
    public object? Parameter { get; set; }

    /// <summary>
    /// 标签页标题（如未设置则使用视图名称）
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 是否是返回导航（返回时不应创建新Tab，而是切换到已有Tab）
    /// </summary>
    public bool IsBackNavigation { get; set; }

    /// <summary>
    /// 返回导航时用于精确匹配目标Tab的稳定标识
    /// </summary>
    public string? NavigationKey { get; set; }

    /// <summary>
    /// 返回导航时，需要关闭的源Tab的NavigationKey
    /// </summary>
    public string? CloseTabNavigationKey { get; set; }
}
