namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 默认下载完成后的操作
    private readonly AfterDownloadOperation _afterDownload = AfterDownloadOperation.NONE;

    // 是否监听剪贴板
    private readonly AllowStatus _isListenClipboard = AllowStatus.YES;

    // 视频详情页面是否自动解析
    private readonly AllowStatus _isAutoParseVideo = AllowStatus.NO;

    // 默认的视频解析项
    private readonly ParseScope _parseScope = ParseScope.NONE;

    // 解析后自动下载解析视频
    private readonly AllowStatus _isAutoDownloadAll = AllowStatus.NO;

    // 下载完成列表排序
    private readonly DownloadFinishedSort _finishedSort = DownloadFinishedSort.DownloadAsc;

    // 重复下载策略
    private readonly RepeatDownloadStrategy _repeatDownloadStrategy = RepeatDownloadStrategy.Ask;

    // 重复文件自动添加数字后缀
    private readonly bool _repeatFileAutoAddNumberSuffix = false;
    
    public ThemeMode GetThemeMode()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.ThemeMode == ThemeMode.Default)
        {
            // 第一次获取，先设置默认值
            SetThemeMode(ThemeMode.Default);
            return ThemeMode.Default;
        }

        return appSettings.Basic.ThemeMode;
    }
    
    public bool SetThemeMode(ThemeMode themeMode)
    {
        appSettings.Basic.ThemeMode = themeMode;
        return SetSettings();
    }

    /// <summary>
    /// 获取下载完成后的操作
    /// </summary>
    /// <returns></returns>
    public AfterDownloadOperation GetAfterDownloadOperation()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.AfterDownload == AfterDownloadOperation.NOT_SET)
        {
            // 第一次获取，先设置默认值
            SetAfterDownloadOperation(_afterDownload);
            return _afterDownload;
        }

        return appSettings.Basic.AfterDownload;
    }

    /// <summary>
    /// 设置下载完成后的操作
    /// </summary>
    /// <param name="afterDownload"></param>
    /// <returns></returns>
    public bool SetAfterDownloadOperation(AfterDownloadOperation afterDownload)
    {
        appSettings.Basic.AfterDownload = afterDownload;
        return SetSettings();
    }

    /// <summary>
    /// 是否监听剪贴板
    /// </summary>
    /// <returns></returns>
    public AllowStatus IsListenClipboard()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.IsListenClipboard == AllowStatus.NONE)
        {
            // 第一次获取，先设置默认值
            IsListenClipboard(_isListenClipboard);
            return _isListenClipboard;
        }

        return appSettings.Basic.IsListenClipboard;
    }

    /// <summary>
    /// 是否监听剪贴板
    /// </summary>
    /// <param name="isListen"></param>
    /// <returns></returns>
    public bool IsListenClipboard(AllowStatus isListen)
    {
        appSettings.Basic.IsListenClipboard = isListen;
        return SetSettings();
    }

    /// <summary>
    /// 视频详情页面是否自动解析
    /// </summary>
    /// <returns></returns>
    public AllowStatus IsAutoParseVideo()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.IsAutoParseVideo == AllowStatus.NONE)
        {
            // 第一次获取，先设置默认值
            IsAutoParseVideo(_isAutoParseVideo);
            return _isAutoParseVideo;
        }

        return appSettings.Basic.IsAutoParseVideo;
    }

    /// <summary>
    /// 视频详情页面是否自动解析
    /// </summary>
    /// <param name="IsAuto"></param>
    /// <returns></returns>
    public bool IsAutoParseVideo(AllowStatus IsAuto)
    {
        appSettings.Basic.IsAutoParseVideo = IsAuto;
        return SetSettings();
    }

    /// <summary>
    /// 获取视频解析项
    /// </summary>
    /// <returns></returns>
    public ParseScope GetParseScope()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.ParseScope == ParseScope.NOT_SET)
        {
            // 第一次获取，先设置默认值
            SetParseScope(_parseScope);
            return _parseScope;
        }

        return appSettings.Basic.ParseScope;
    }

    /// <summary>
    /// 设置视频解析项
    /// </summary>
    /// <param name="parseScope"></param>
    /// <returns></returns>
    public bool SetParseScope(ParseScope parseScope)
    {
        appSettings.Basic.ParseScope = parseScope;
        return SetSettings();
    }

    /// <summary>
    /// 解析后是否自动下载解析视频
    /// </summary>
    /// <returns></returns>
    public AllowStatus IsAutoDownloadAll()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.IsAutoDownloadAll == AllowStatus.NONE)
        {
            // 第一次获取，先设置默认值
            IsAutoDownloadAll(_isAutoDownloadAll);
            return _isAutoDownloadAll;
        }

        return appSettings.Basic.IsAutoDownloadAll;
    }

    /// <summary>
    /// 解析后是否自动下载解析视频
    /// </summary>
    /// <param name="isAutoDownloadAll"></param>
    /// <returns></returns>
    public bool IsAutoDownloadAll(AllowStatus isAutoDownloadAll)
    {
        appSettings.Basic.IsAutoDownloadAll = isAutoDownloadAll;
        return SetSettings();
    }

    /// <summary>
    /// 获取下载完成列表排序
    /// </summary>
    /// <returns></returns>
    public DownloadFinishedSort GetDownloadFinishedSort()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.DownloadFinishedSort == DownloadFinishedSort.NotSet)
        {
            // 第一次获取，先设置默认值
            SetDownloadFinishedSort(_finishedSort);
            return _finishedSort;
        }

        return appSettings.Basic.DownloadFinishedSort;
    }

    /// <summary>
    /// 设置下载完成列表排序
    /// </summary>
    /// <param name="finishedSort"></param>
    /// <returns></returns>
    public bool SetDownloadFinishedSort(DownloadFinishedSort finishedSort)
    {
        appSettings.Basic.DownloadFinishedSort = finishedSort;
        return SetSettings();
    }

    /// <summary>
    /// 获取重复下载策略
    /// </summary>
    /// <returns></returns>
    public RepeatDownloadStrategy GetRepeatDownloadStrategy()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.RepeatDownloadStrategy == RepeatDownloadStrategy.Ask)
        {
            // 第一次获取，先设置默认值
            SetRepeatDownloadStrategy(_repeatDownloadStrategy);
            return _repeatDownloadStrategy;
        }

        return appSettings.Basic.RepeatDownloadStrategy;
    }

    /// <summary>
    /// 设置重复下载策略
    /// </summary>
    /// <param name="repeatDownloadStrategy"></param>
    /// <returns></returns>
    public bool SetRepeatDownloadStrategy(RepeatDownloadStrategy repeatDownloadStrategy)
    {
        appSettings.Basic.RepeatDownloadStrategy = repeatDownloadStrategy;
        return SetSettings();
    }

    /// <summary>
    /// 重复文件自动添加数字后缀
    /// </summary>
    /// <returns></returns>
    public bool IsRepeatFileAutoAddNumberSuffix()
    {
        appSettings = GetSettings();
        if (appSettings.Basic.RepeatFileAutoAddNumberSuffix == false)
        {
            // 第一次获取，先设置默认值
            IsRepeatFileAutoAddNumberSuffix(_repeatFileAutoAddNumberSuffix);
            return _repeatFileAutoAddNumberSuffix;
        }

        return appSettings.Basic.RepeatFileAutoAddNumberSuffix;
    }

    /// <summary>
    /// 设置重复文件自动添加数字后缀
    /// </summary>
    /// <param name="repeatFileAutoAddNumberSuffix"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool IsRepeatFileAutoAddNumberSuffix(bool repeatFileAutoAddNumberSuffix)
    {
        appSettings.Basic.RepeatFileAutoAddNumberSuffix = repeatFileAutoAddNumberSuffix;
        return SetSettings();
    }
}