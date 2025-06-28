namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 是否屏蔽顶部弹幕
    private const AllowStatus DanmakuTopFilter = AllowStatus.No;

    // 是否屏蔽底部弹幕
    private const AllowStatus DanmakuBottomFilter = AllowStatus.No;

    // 是否屏蔽滚动弹幕
    private const AllowStatus DanmakuScrollFilter = AllowStatus.No;

    // 是否自定义分辨率
    private const AllowStatus IsCustomDanmakuResolution = AllowStatus.No;

    // 分辨率-宽
    private const int DanmakuScreenWidth = 1920;

    // 分辨率-高
    private const int DanmakuScreenHeight = 1080;

    // 弹幕字体
    private const string DanmakuFontName = "黑体";

    // 弹幕字体大小
    private const int DanmakuFontSize = 50;

    // 弹幕限制行数
    private const int DanmakuLineCount = 0;

    // 弹幕布局算法
    private const DanmakuLayoutAlgorithm DanmakuLayoutAlgorithm = Settings.DanmakuLayoutAlgorithm.Sync;


    /// <summary>
    /// 获取是否屏蔽顶部弹幕
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetDanmakuTopFilter()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuTopFilter == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetDanmakuTopFilter(DanmakuTopFilter);
            return DanmakuTopFilter;
        }

        return _appSettings.Danmaku.DanmakuTopFilter;
    }

    /// <summary>
    /// 设置是否屏蔽顶部弹幕
    /// </summary>
    /// <param name="danmakuFilter"></param>
    /// <returns></returns>
    public bool SetDanmakuTopFilter(AllowStatus danmakuFilter)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuTopFilter,
            danmakuFilter,
            v => _appSettings.Danmaku.DanmakuTopFilter = v);
    }

    /// <summary>
    /// 获取是否屏蔽底部弹幕
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetDanmakuBottomFilter()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuBottomFilter == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetDanmakuBottomFilter(DanmakuBottomFilter);
            return DanmakuBottomFilter;
        }

        return _appSettings.Danmaku.DanmakuBottomFilter;
    }

    /// <summary>
    /// 设置是否屏蔽底部弹幕
    /// </summary>
    /// <param name="danmakuFilter"></param>
    /// <returns></returns>
    public bool SetDanmakuBottomFilter(AllowStatus danmakuFilter)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuBottomFilter,
            danmakuFilter,
            v => _appSettings.Danmaku.DanmakuBottomFilter = v);
    }

    /// <summary>
    /// 获取是否屏蔽滚动弹幕
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetDanmakuScrollFilter()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuScrollFilter == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetDanmakuScrollFilter(DanmakuScrollFilter);
            return DanmakuScrollFilter;
        }

        return _appSettings.Danmaku.DanmakuScrollFilter;
    }

    /// <summary>
    /// 设置是否屏蔽滚动弹幕
    /// </summary>
    /// <param name="danmakuFilter"></param>
    /// <returns></returns>
    public bool SetDanmakuScrollFilter(AllowStatus danmakuFilter)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuScrollFilter,
            danmakuFilter,
            v => _appSettings.Danmaku.DanmakuScrollFilter = v);
    }

    /// <summary>
    /// 获取是否自定义分辨率
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsCustomDanmakuResolution()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.IsCustomDanmakuResolution == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsCustomDanmakuResolution(IsCustomDanmakuResolution);
            return IsCustomDanmakuResolution;
        }

        return _appSettings.Danmaku.IsCustomDanmakuResolution;
    }

    /// <summary>
    /// 设置是否自定义分辨率
    /// </summary>
    /// <param name="isCustomResolution"></param>
    /// <returns></returns>
    public bool SetIsCustomDanmakuResolution(AllowStatus isCustomResolution)
    {
        return SetProperty(
            _appSettings.Danmaku.IsCustomDanmakuResolution,
            isCustomResolution,
            v => _appSettings.Danmaku.IsCustomDanmakuResolution = v);
    }

    /// <summary>
    /// 获取分辨率-宽
    /// </summary>
    /// <returns></returns>
    public int GetDanmakuScreenWidth()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuScreenWidth == -1)
        {
            // 第一次获取，先设置默认值
            SetDanmakuScreenWidth(DanmakuScreenWidth);
            return DanmakuScreenWidth;
        }

        return _appSettings.Danmaku.DanmakuScreenWidth;
    }

    /// <summary>
    /// 设置分辨率-宽
    /// </summary>
    /// <param name="screenWidth"></param>
    /// <returns></returns>
    public bool SetDanmakuScreenWidth(int screenWidth)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuScreenWidth,
            screenWidth,
            v => _appSettings.Danmaku.DanmakuScreenWidth = v);
    }

    /// <summary>
    /// 获取分辨率-高
    /// </summary>
    /// <returns></returns>
    public int GetDanmakuScreenHeight()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuScreenHeight == -1)
        {
            // 第一次获取，先设置默认值
            SetDanmakuScreenHeight(DanmakuScreenHeight);
            return DanmakuScreenHeight;
        }

        return _appSettings.Danmaku.DanmakuScreenHeight;
    }

    /// <summary>
    /// 设置分辨率-高
    /// </summary>
    /// <param name="screenHeight"></param>
    /// <returns></returns>
    public bool SetDanmakuScreenHeight(int screenHeight)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuScreenHeight,
            screenHeight,
            v => _appSettings.Danmaku.DanmakuScreenHeight = v);
    }

    /// <summary>
    /// 获取弹幕字体
    /// </summary>
    /// <returns></returns>
    public string GetDanmakuFontName()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuFontName == null)
        {
            // 第一次获取，先设置默认值
            SetDanmakuFontName(DanmakuFontName);
            return DanmakuFontName;
        }

        return _appSettings.Danmaku.DanmakuFontName;
    }

    /// <summary>
    /// 设置弹幕字体
    /// </summary>
    /// <param name="danmakuFontName"></param>
    /// <returns></returns>
    public bool SetDanmakuFontName(string danmakuFontName)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuFontName,
            danmakuFontName,
            v => _appSettings.Danmaku.DanmakuFontName = v);
    }

    /// <summary>
    /// 获取弹幕字体大小
    /// </summary>
    /// <returns></returns>
    public int GetDanmakuFontSize()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuFontSize == -1)
        {
            // 第一次获取，先设置默认值
            SetDanmakuFontSize(DanmakuFontSize);
            return DanmakuFontSize;
        }

        return _appSettings.Danmaku.DanmakuFontSize;
    }

    /// <summary>
    /// 设置弹幕字体大小
    /// </summary>
    /// <param name="danmakuFontSize"></param>
    /// <returns></returns>
    public bool SetDanmakuFontSize(int danmakuFontSize)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuFontSize,
            danmakuFontSize,
            v => _appSettings.Danmaku.DanmakuFontSize = v);
    }

    /// <summary>
    /// 获取弹幕限制行数
    /// </summary>
    /// <returns></returns>
    public int GetDanmakuLineCount()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuLineCount == -1)
        {
            // 第一次获取，先设置默认值
            SetDanmakuLineCount(DanmakuLineCount);
            return DanmakuLineCount;
        }

        return _appSettings.Danmaku.DanmakuLineCount;
    }

    /// <summary>
    /// 设置弹幕限制行数
    /// </summary>
    /// <param name="danmakuLineCount"></param>
    /// <returns></returns>
    public bool SetDanmakuLineCount(int danmakuLineCount)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuLineCount,
            danmakuLineCount,
            v => _appSettings.Danmaku.DanmakuLineCount = v);
    }

    /// <summary>
    /// 获取弹幕布局算法
    /// </summary>
    /// <returns></returns>
    public DanmakuLayoutAlgorithm GetDanmakuLayoutAlgorithm()
    {
        _appSettings = GetSettings();
        if (_appSettings.Danmaku.DanmakuLayoutAlgorithm == DanmakuLayoutAlgorithm.None)
        {
            // 第一次获取，先设置默认值
            SetDanmakuLayoutAlgorithm(DanmakuLayoutAlgorithm);
            return DanmakuLayoutAlgorithm;
        }

        return _appSettings.Danmaku.DanmakuLayoutAlgorithm;
    }

    /// <summary>
    /// 设置弹幕布局算法
    /// </summary>
    /// <param name="danmakuLayoutAlgorithm"></param>
    /// <returns></returns>
    public bool SetDanmakuLayoutAlgorithm(DanmakuLayoutAlgorithm danmakuLayoutAlgorithm)
    {
        return SetProperty(
            _appSettings.Danmaku.DanmakuLayoutAlgorithm,
            danmakuLayoutAlgorithm,
            v => _appSettings.Danmaku.DanmakuLayoutAlgorithm = v);
        
    }
}