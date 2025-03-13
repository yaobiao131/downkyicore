using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using DownKyi.Core.Settings;
using DownKyi.Core.Utils.Validator;
using DownKyi.Events;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.Settings;

public class ViewDanmakuViewModel : ViewModelBase
{
    public const string Tag = "PageSettingsDanmaku";

    private bool _isOnNavigatedTo;

    #region 页面属性申明

    private bool _topFilter;

    public bool TopFilter
    {
        get => _topFilter;
        set => SetProperty(ref _topFilter, value);
    }

    private bool _bottomFilter;

    public bool BottomFilter
    {
        get => _bottomFilter;
        set => SetProperty(ref _bottomFilter, value);
    }

    private bool _scrollFilter;

    public bool ScrollFilter
    {
        get => _scrollFilter;
        set => SetProperty(ref _scrollFilter, value);
    }

    private int _screenWidth;

    public int ScreenWidth
    {
        get => _screenWidth;
        set => SetProperty(ref _screenWidth, value);
    }

    private int _screenHeight;

    public int ScreenHeight
    {
        get => _screenHeight;
        set => SetProperty(ref _screenHeight, value);
    }

    private List<string> _fonts;

    public List<string> Fonts
    {
        get => _fonts;
        set => SetProperty(ref _fonts, value);
    }

    private string _selectedFont;

    public string SelectedFont
    {
        get => _selectedFont;
        set => SetProperty(ref _selectedFont, value);
    }

    private int _fontSize;

    public int FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    private int _lineCount;

    public int LineCount
    {
        get => _lineCount;
        set => SetProperty(ref _lineCount, value);
    }

    private bool _sync;

    public bool Sync
    {
        get => _sync;
        set => SetProperty(ref _sync, value);
    }

    private bool _async;

    public bool Async
    {
        get => _async;
        set => SetProperty(ref _async, value);
    }

    #endregion

    public ViewDanmakuViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        // 弹幕字体
        Fonts = new List<string>();
        var fontCollection = FontManager.Current.SystemFonts.Select(x => x.Name);
        foreach (var font in fontCollection)
        {
            Fonts.Add(font);
        }

        #endregion
    }

    /// <summary>
    /// 导航到页面时执行
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        _isOnNavigatedTo = true;

        // 屏蔽顶部弹幕
        var danmakuTopFilter = SettingsManager.GetInstance().GetDanmakuTopFilter();
        TopFilter = danmakuTopFilter == AllowStatus.Yes;

        // 屏蔽底部弹幕
        var danmakuBottomFilter = SettingsManager.GetInstance().GetDanmakuBottomFilter();
        BottomFilter = danmakuBottomFilter == AllowStatus.Yes;

        // 屏蔽滚动弹幕
        var danmakuScrollFilter = SettingsManager.GetInstance().GetDanmakuScrollFilter();
        ScrollFilter = danmakuScrollFilter == AllowStatus.Yes;

        // 分辨率-宽
        ScreenWidth = SettingsManager.GetInstance().GetDanmakuScreenWidth();

        // 分辨率-高
        ScreenHeight = SettingsManager.GetInstance().GetDanmakuScreenHeight();

        // 弹幕字体
        var danmakuFont = SettingsManager.GetInstance().GetDanmakuFontName();
        if (danmakuFont != null && Fonts.Contains(danmakuFont))
        {
            // 只有系统中存在当前设置的字体，才能显示
            SelectedFont = danmakuFont;
        }

        // 弹幕字体大小
        FontSize = SettingsManager.GetInstance().GetDanmakuFontSize();

        // 弹幕限制行数
        LineCount = SettingsManager.GetInstance().GetDanmakuLineCount();

        // 弹幕布局算法
        var layoutAlgorithm = SettingsManager.GetInstance().GetDanmakuLayoutAlgorithm();
        SetLayoutAlgorithm(layoutAlgorithm);

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 屏蔽顶部弹幕事件
    private DelegateCommand _topFilterCommand;

    public DelegateCommand TopFilterCommand => _topFilterCommand ??= new DelegateCommand(ExecuteTopFilterCommand);

    /// <summary>
    /// 屏蔽顶部弹幕事件
    /// </summary>
    private void ExecuteTopFilterCommand()
    {
        var isTopFilter = TopFilter ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuTopFilter(isTopFilter);
        PublishTip(isSucceed);
    }

    // 屏蔽底部弹幕事件
    private DelegateCommand _bottomFilterCommand;

    public DelegateCommand BottomFilterCommand => _bottomFilterCommand ??= new DelegateCommand(ExecuteBottomFilterCommand);

    /// <summary>
    /// 屏蔽底部弹幕事件
    /// </summary>
    private void ExecuteBottomFilterCommand()
    {
        var isBottomFilter = BottomFilter ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuBottomFilter(isBottomFilter);
        PublishTip(isSucceed);
    }

    // 屏蔽滚动弹幕事件
    private DelegateCommand _scrollFilterCommand;

    public DelegateCommand ScrollFilterCommand => _scrollFilterCommand ??= new DelegateCommand(ExecuteScrollFilterCommand);

    /// <summary>
    /// 屏蔽滚动弹幕事件
    /// </summary>
    private void ExecuteScrollFilterCommand()
    {
        var isScrollFilter = ScrollFilter ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuScrollFilter(isScrollFilter);
        PublishTip(isSucceed);
    }

    // 设置分辨率-宽事件
    private DelegateCommand<string> _screenWidthCommand;

    public DelegateCommand<string> ScreenWidthCommand => _screenWidthCommand ??= new DelegateCommand<string>(ExecuteScreenWidthCommand);

    /// <summary>
    /// 设置分辨率-宽事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteScreenWidthCommand(string parameter)
    {
        var width = (int)Number.GetInt(parameter);
        ScreenWidth = width;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuScreenWidth(ScreenWidth);
        PublishTip(isSucceed);
    }

    // 设置分辨率-高事件
    private DelegateCommand<string> _screenHeightCommand;

    public DelegateCommand<string> ScreenHeightCommand => _screenHeightCommand ??= new DelegateCommand<string>(ExecuteScreenHeightCommand);

    /// <summary>
    /// 设置分辨率-高事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteScreenHeightCommand(string parameter)
    {
        var height = (int)Number.GetInt(parameter);
        ScreenHeight = height;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuScreenHeight(ScreenHeight);
        PublishTip(isSucceed);
    }

    // 弹幕字体选择事件
    private DelegateCommand<string> _fontSelectCommand;

    public DelegateCommand<string> FontSelectCommand => _fontSelectCommand ??= new DelegateCommand<string>(ExecuteFontSelectCommand);

    /// <summary>
    /// 弹幕字体选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteFontSelectCommand(string parameter)
    {
        var isSucceed = SettingsManager.GetInstance().SetDanmakuFontName(parameter);
        PublishTip(isSucceed);
    }

    // 弹幕字体大小事件
    private DelegateCommand<string> _fontSizeCommand;

    public DelegateCommand<string> FontSizeCommand => _fontSizeCommand ??= new DelegateCommand<string>(ExecuteFontSizeCommand);

    /// <summary>
    /// 弹幕字体大小事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteFontSizeCommand(string parameter)
    {
        var fontSize = (int)Number.GetInt(parameter);
        FontSize = fontSize;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuFontSize(FontSize);
        PublishTip(isSucceed);
    }

    // 弹幕限制行数事件
    private DelegateCommand<string> _lineCountCommand;

    public DelegateCommand<string> LineCountCommand => _lineCountCommand ??= new DelegateCommand<string>(ExecuteLineCountCommand);

    /// <summary>
    /// 弹幕限制行数事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteLineCountCommand(string parameter)
    {
        var lineCount = (int)Number.GetInt(parameter);
        LineCount = lineCount;

        var isSucceed = SettingsManager.GetInstance().SetDanmakuLineCount(LineCount);
        PublishTip(isSucceed);
    }

    // 弹幕布局算法事件
    private DelegateCommand<string> _layoutAlgorithmCommand;

    public DelegateCommand<string> LayoutAlgorithmCommand => _layoutAlgorithmCommand ??= new DelegateCommand<string>(ExecuteLayoutAlgorithmCommand);

    /// <summary>
    /// 弹幕布局算法事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteLayoutAlgorithmCommand(string parameter)
    {
        var layoutAlgorithm = parameter switch
        {
            "Sync" => DanmakuLayoutAlgorithm.Sync,
            "Async" => DanmakuLayoutAlgorithm.Async,
            _ => DanmakuLayoutAlgorithm.Sync
        };

        var isSucceed = SettingsManager.GetInstance().SetDanmakuLayoutAlgorithm(layoutAlgorithm);
        PublishTip(isSucceed);

        if (isSucceed)
        {
            SetLayoutAlgorithm(layoutAlgorithm);
        }
    }

    #endregion

    /// <summary>
    /// 设置弹幕同步算法
    /// </summary>
    /// <param name="layoutAlgorithm"></param>
    private void SetLayoutAlgorithm(DanmakuLayoutAlgorithm layoutAlgorithm)
    {
        switch (layoutAlgorithm)
        {
            case DanmakuLayoutAlgorithm.Sync:
                Sync = true;
                Async = false;
                break;
            case DanmakuLayoutAlgorithm.Async:
                Sync = false;
                Async = true;
                break;
            case DanmakuLayoutAlgorithm.None:
                Sync = false;
                Async = false;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 发送需要显示的tip
    /// </summary>
    /// <param name="isSucceed"></param>
    private void PublishTip(bool isSucceed)
    {
        if (_isOnNavigatedTo)
        {
            return;
        }

        EventAggregator.GetEvent<MessageEvent>().Publish(isSucceed ? DictionaryResource.GetString("TipSettingUpdated") : DictionaryResource.GetString("TipSettingFailed"));
    }
}