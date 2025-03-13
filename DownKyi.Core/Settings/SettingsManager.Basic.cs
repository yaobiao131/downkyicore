namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 默认下载完成后的操作
    private const AfterDownloadOperation AfterDownload = AfterDownloadOperation.None;

    // 是否监听剪贴板
    private const AllowStatus IsListenClipboard = AllowStatus.Yes;

    // 视频详情页面是否自动解析
    private const AllowStatus IsAutoParseVideo = AllowStatus.No;

    // 默认的视频解析项
    private const ParseScope ParseScope = Settings.ParseScope.None;

    // 解析后自动下载解析视频
    private const AllowStatus IsAutoDownloadAll = AllowStatus.No;

    // 下载完成列表排序
    private const DownloadFinishedSort FinishedSort = DownloadFinishedSort.DownloadAsc;

    // 重复下载策略
    private const RepeatDownloadStrategy RepeatDownloadStrategy = Settings.RepeatDownloadStrategy.Ask;

    // 重复文件自动添加数字后缀
    private const bool RepeatFileAutoAddNumberSuffix = false;

    public ThemeMode GetThemeMode()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.ThemeMode == ThemeMode.Default)
        {
            // 第一次获取，先设置默认值
            SetThemeMode(ThemeMode.Default);
            return ThemeMode.Default;
        }

        return _appSettings.Basic.ThemeMode;
    }

    public bool SetThemeMode(ThemeMode themeMode)
    {
        _appSettings.Basic.ThemeMode = themeMode;
        return SetSettings();
    }

    /// <summary>
    /// 获取下载完成后的操作
    /// </summary>
    /// <returns></returns>
    public AfterDownloadOperation GetAfterDownloadOperation()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.AfterDownload == AfterDownloadOperation.NotSet)
        {
            // 第一次获取，先设置默认值
            SetAfterDownloadOperation(AfterDownload);
            return AfterDownload;
        }

        return _appSettings.Basic.AfterDownload;
    }

    /// <summary>
    /// 设置下载完成后的操作
    /// </summary>
    /// <param name="afterDownload"></param>
    /// <returns></returns>
    public bool SetAfterDownloadOperation(AfterDownloadOperation afterDownload)
    {
        _appSettings.Basic.AfterDownload = afterDownload;
        return SetSettings();
    }

    /// <summary>
    /// 是否监听剪贴板
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsListenClipboard()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.IsListenClipboard == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsListenClipboard(IsListenClipboard);
            return IsListenClipboard;
        }

        return _appSettings.Basic.IsListenClipboard;
    }

    /// <summary>
    /// 是否监听剪贴板
    /// </summary>
    /// <param name="isListen"></param>
    /// <returns></returns>
    public bool SetIsListenClipboard(AllowStatus isListen)
    {
        _appSettings.Basic.IsListenClipboard = isListen;
        return SetSettings();
    }

    /// <summary>
    /// 视频详情页面是否自动解析
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsAutoParseVideo()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.IsAutoParseVideo == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsAutoParseVideo(IsAutoParseVideo);
            return IsAutoParseVideo;
        }

        return _appSettings.Basic.IsAutoParseVideo;
    }

    /// <summary>
    /// 视频详情页面是否自动解析
    /// </summary>
    /// <param name="isAuto"></param>
    /// <returns></returns>
    public bool SetIsAutoParseVideo(AllowStatus isAuto)
    {
        _appSettings.Basic.IsAutoParseVideo = isAuto;
        return SetSettings();
    }

    /// <summary>
    /// 获取视频解析项
    /// </summary>
    /// <returns></returns>
    public ParseScope GetParseScope()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.ParseScope == ParseScope.NotSet)
        {
            // 第一次获取，先设置默认值
            SetParseScope(ParseScope);
            return ParseScope;
        }

        return _appSettings.Basic.ParseScope;
    }

    /// <summary>
    /// 设置视频解析项
    /// </summary>
    /// <param name="parseScope"></param>
    /// <returns></returns>
    public bool SetParseScope(ParseScope parseScope)
    {
        _appSettings.Basic.ParseScope = parseScope;
        return SetSettings();
    }

    /// <summary>
    /// 解析后是否自动下载解析视频
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsAutoDownloadAll()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.IsAutoDownloadAll == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsAutoDownloadAll(IsAutoDownloadAll);
            return IsAutoDownloadAll;
        }

        return _appSettings.Basic.IsAutoDownloadAll;
    }

    /// <summary>
    /// 解析后是否自动下载解析视频
    /// </summary>
    /// <param name="isAutoDownloadAll"></param>
    /// <returns></returns>
    public bool SetIsAutoDownloadAll(AllowStatus isAutoDownloadAll)
    {
        _appSettings.Basic.IsAutoDownloadAll = isAutoDownloadAll;
        return SetSettings();
    }

    /// <summary>
    /// 获取下载完成列表排序
    /// </summary>
    /// <returns></returns>
    public DownloadFinishedSort GetDownloadFinishedSort()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.DownloadFinishedSort == DownloadFinishedSort.NotSet)
        {
            // 第一次获取，先设置默认值
            SetDownloadFinishedSort(FinishedSort);
            return FinishedSort;
        }

        return _appSettings.Basic.DownloadFinishedSort;
    }

    /// <summary>
    /// 设置下载完成列表排序
    /// </summary>
    /// <param name="finishedSort"></param>
    /// <returns></returns>
    public bool SetDownloadFinishedSort(DownloadFinishedSort finishedSort)
    {
        _appSettings.Basic.DownloadFinishedSort = finishedSort;
        return SetSettings();
    }

    /// <summary>
    /// 获取重复下载策略
    /// </summary>
    /// <returns></returns>
    public RepeatDownloadStrategy GetRepeatDownloadStrategy()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.RepeatDownloadStrategy == RepeatDownloadStrategy.Ask)
        {
            // 第一次获取，先设置默认值
            SetRepeatDownloadStrategy(RepeatDownloadStrategy);
            return RepeatDownloadStrategy;
        }

        return _appSettings.Basic.RepeatDownloadStrategy;
    }

    /// <summary>
    /// 设置重复下载策略
    /// </summary>
    /// <param name="repeatDownloadStrategy"></param>
    /// <returns></returns>
    public bool SetRepeatDownloadStrategy(RepeatDownloadStrategy repeatDownloadStrategy)
    {
        _appSettings.Basic.RepeatDownloadStrategy = repeatDownloadStrategy;
        return SetSettings();
    }

    /// <summary>
    /// 重复文件自动添加数字后缀
    /// </summary>
    /// <returns></returns>
    public bool IsRepeatFileAutoAddNumberSuffix()
    {
        _appSettings = GetSettings();
        if (_appSettings.Basic.RepeatFileAutoAddNumberSuffix == false)
        {
            // 第一次获取，先设置默认值
            IsRepeatFileAutoAddNumberSuffix(RepeatFileAutoAddNumberSuffix);
            return RepeatFileAutoAddNumberSuffix;
        }

        return _appSettings.Basic.RepeatFileAutoAddNumberSuffix;
    }

    /// <summary>
    /// 设置重复文件自动添加数字后缀
    /// </summary>
    /// <param name="repeatFileAutoAddNumberSuffix"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool IsRepeatFileAutoAddNumberSuffix(bool repeatFileAutoAddNumberSuffix)
    {
        _appSettings.Basic.RepeatFileAutoAddNumberSuffix = repeatFileAutoAddNumberSuffix;
        return SetSettings();
    }
}