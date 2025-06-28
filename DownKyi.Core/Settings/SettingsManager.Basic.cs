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
        return SetProperty(
            _appSettings.Basic.ThemeMode,
            themeMode,
            v => _appSettings.Basic.ThemeMode = v);
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
        return SetProperty(
            _appSettings.Basic.AfterDownload,
            afterDownload,
            v => _appSettings.Basic.AfterDownload = v);
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
        return SetProperty(
            _appSettings.Basic.IsListenClipboard,
            isListen,
            v => _appSettings.Basic.IsListenClipboard = v);
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
        return SetProperty(
            _appSettings.Basic.IsAutoParseVideo,
            isAuto,
            v => _appSettings.Basic.IsAutoParseVideo = v);
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
        return SetProperty(
            _appSettings.Basic.ParseScope,
            parseScope,
            v => _appSettings.Basic.ParseScope = v);
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
        return SetProperty(
            _appSettings.Basic.IsAutoDownloadAll,
            isAutoDownloadAll,
            v => _appSettings.Basic.IsAutoDownloadAll = v);
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
        return SetProperty(
            _appSettings.Basic.DownloadFinishedSort,
            finishedSort,
            v => _appSettings.Basic.DownloadFinishedSort = v);
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
        return SetProperty(
            _appSettings.Basic.RepeatDownloadStrategy,
            repeatDownloadStrategy,
            v => _appSettings.Basic.RepeatDownloadStrategy = v);
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
        return SetProperty(
            _appSettings.Basic.RepeatFileAutoAddNumberSuffix,
            repeatFileAutoAddNumberSuffix,
            v => _appSettings.Basic.RepeatFileAutoAddNumberSuffix = v);
    }
}