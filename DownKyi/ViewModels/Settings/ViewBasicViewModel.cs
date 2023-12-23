using System.Collections.Generic;
using System.Linq;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.Settings;

public class ViewBasicViewModel : ViewModelBase
{
    public const string Tag = "PageSettingsBasic";

    private bool _isOnNavigatedTo;

    #region 页面属性申明

    private bool _none;

    public bool None
    {
        get => _none;
        set => SetProperty(ref _none, value);
    }

    private bool _closeApp;

    public bool CloseApp
    {
        get => _closeApp;
        set => SetProperty(ref _closeApp, value);
    }

    private bool _closeSystem;

    public bool CloseSystem
    {
        get => _closeSystem;
        set => SetProperty(ref _closeSystem, value);
    }

    private bool _listenClipboard;

    public bool ListenClipboard
    {
        get => _listenClipboard;
        set => SetProperty(ref _listenClipboard, value);
    }

    private bool _autoParseVideo;

    public bool AutoParseVideo
    {
        get => _autoParseVideo;
        set => SetProperty(ref _autoParseVideo, value);
    }

    private List<ParseScopeDisplay> _parseScopes;

    public List<ParseScopeDisplay> ParseScopes
    {
        get => _parseScopes;
        set => SetProperty(ref _parseScopes, value);
    }

    private ParseScopeDisplay _selectedParseScope;

    public ParseScopeDisplay SelectedParseScope
    {
        get => _selectedParseScope;
        set => SetProperty(ref _selectedParseScope, value);
    }

    private bool _autoDownloadAll;

    public bool AutoDownloadAll
    {
        get => _autoDownloadAll;
        set => SetProperty(ref _autoDownloadAll, value);
    }

    private List<RepeatDownloadStrategyDisplay> _repeatDownloadStrategy;

    public List<RepeatDownloadStrategyDisplay> RepeatDownloadStrategy
    {
        get => _repeatDownloadStrategy;
        set => SetProperty(ref _repeatDownloadStrategy, value);
    }

    private RepeatDownloadStrategyDisplay _selectedRepeatDownloadStrategy;

    public RepeatDownloadStrategyDisplay SelectedRepeatDownloadStrategy
    {
        get => _selectedRepeatDownloadStrategy;
        set => SetProperty(ref _selectedRepeatDownloadStrategy, value);
    }

    #endregion

    public ViewBasicViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        // 解析范围
        ParseScopes = new List<ParseScopeDisplay>
        {
            new() { Name = DictionaryResource.GetString("ParseNone"), ParseScope = ParseScope.NONE },
            new() { Name = DictionaryResource.GetString("ParseSelectedItem"), ParseScope = ParseScope.SELECTED_ITEM },
            new() { Name = DictionaryResource.GetString("ParseCurrentSection"), ParseScope = ParseScope.CURRENT_SECTION },
            new() { Name = DictionaryResource.GetString("ParseAll"), ParseScope = ParseScope.ALL }
        };

        RepeatDownloadStrategy = new List<RepeatDownloadStrategyDisplay>
        {
            new() { Name = DictionaryResource.GetString("RepeatDownloadAsk"), RepeatDownloadStrategy = Core.Settings.RepeatDownloadStrategy.Ask },
            new() { Name = DictionaryResource.GetString("RepeatDownloadReDownload"), RepeatDownloadStrategy = Core.Settings.RepeatDownloadStrategy.ReDownload },
            new() { Name = DictionaryResource.GetString("RepeatDownloadReJumpOver"), RepeatDownloadStrategy = Core.Settings.RepeatDownloadStrategy.JumpOver }
        };

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

        // 下载完成后的操作
        var afterDownload = SettingsManager.GetInstance().GetAfterDownloadOperation();
        SetAfterDownloadOperation(afterDownload);

        // 是否监听剪贴板
        var isListenClipboard = SettingsManager.GetInstance().IsListenClipboard();
        ListenClipboard = isListenClipboard == AllowStatus.YES;

        // 是否自动解析视频
        var isAutoParseVideo = SettingsManager.GetInstance().IsAutoParseVideo();
        AutoParseVideo = isAutoParseVideo == AllowStatus.YES;

        // 解析范围
        var parseScope = SettingsManager.GetInstance().GetParseScope();
        SelectedParseScope = ParseScopes.FirstOrDefault(t => { return t.ParseScope == parseScope; });

        // 解析后是否自动下载解析视频
        var isAutoDownloadAll = SettingsManager.GetInstance().IsAutoDownloadAll();
        AutoDownloadAll = isAutoDownloadAll == AllowStatus.YES;

        // 重复下载策略
        var repeatDownloadStrategy = SettingsManager.GetInstance().GetRepeatDownloadStrategy();
        SelectedRepeatDownloadStrategy = RepeatDownloadStrategy.FirstOrDefault(t => { return t.RepeatDownloadStrategy == repeatDownloadStrategy; });

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 下载完成后的操作事件
    private DelegateCommand<string>? _afterDownloadOperationCommand;

    public DelegateCommand<string> AfterDownloadOperationCommand => _afterDownloadOperationCommand ??= new DelegateCommand<string>(ExecuteAfterDownloadOperationCommand);

    /// <summary>
    /// 下载完成后的操作事件
    /// </summary>
    private void ExecuteAfterDownloadOperationCommand(string parameter)
    {
        AfterDownloadOperation afterDownload;
        switch (parameter)
        {
            case "None":
                afterDownload = AfterDownloadOperation.NONE;
                break;
            case "CloseApp":
                afterDownload = AfterDownloadOperation.CLOSE_APP;
                break;
            case "CloseSystem":
                afterDownload = AfterDownloadOperation.CLOSE_SYSTEM;
                break;
            default:
                afterDownload = AfterDownloadOperation.NONE;
                break;
        }

        var isSucceed = SettingsManager.GetInstance().SetAfterDownloadOperation(afterDownload);
        PublishTip(isSucceed);
    }

    // 是否监听剪贴板事件
    private DelegateCommand? _listenClipboardCommand;

    public DelegateCommand ListenClipboardCommand => _listenClipboardCommand ??= new DelegateCommand(ExecuteListenClipboardCommand);

    /// <summary>
    /// 是否监听剪贴板事件
    /// </summary>
    private void ExecuteListenClipboardCommand()
    {
        var isListenClipboard = ListenClipboard ? AllowStatus.YES : AllowStatus.NO;

        var isSucceed = SettingsManager.GetInstance().IsListenClipboard(isListenClipboard);
        PublishTip(isSucceed);
    }

    private DelegateCommand? _autoParseVideoCommand;

    public DelegateCommand AutoParseVideoCommand => _autoParseVideoCommand ??= new DelegateCommand(ExecuteAutoParseVideoCommand);

    /// <summary>
    /// 是否自动解析视频
    /// </summary>
    private void ExecuteAutoParseVideoCommand()
    {
        var isAutoParseVideo = AutoParseVideo ? AllowStatus.YES : AllowStatus.NO;

        var isSucceed = SettingsManager.GetInstance().IsAutoParseVideo(isAutoParseVideo);
        PublishTip(isSucceed);
    }

    // 解析范围事件
    private DelegateCommand<object>? _parseScopesCommand;

    public DelegateCommand<object> ParseScopesCommand => _parseScopesCommand ??= new DelegateCommand<object>(ExecuteParseScopesCommand);

    /// <summary>
    /// 解析范围事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteParseScopesCommand(object parameter)
    {
        if (parameter is not ParseScopeDisplay parseScope)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetParseScope(parseScope.ParseScope);
        PublishTip(isSucceed);
    }

    // 解析后是否自动下载解析视频
    private DelegateCommand? _autoDownloadAllCommand;

    public DelegateCommand AutoDownloadAllCommand => _autoDownloadAllCommand ??= new DelegateCommand(ExecuteAutoDownloadAllCommand);

    /// <summary>
    /// 解析后是否自动下载解析视频
    /// </summary>
    private void ExecuteAutoDownloadAllCommand()
    {
        var isAutoDownloadAll = AutoDownloadAll ? AllowStatus.YES : AllowStatus.NO;

        var isSucceed = SettingsManager.GetInstance().IsAutoDownloadAll(isAutoDownloadAll);
        PublishTip(isSucceed);
    }

    // 解析范围事件
    private DelegateCommand<object>? _repeatDownloadStrategyCommand;

    public DelegateCommand<object> RepeatDownloadStrategyCommand => _repeatDownloadStrategyCommand ??= new DelegateCommand<object>(ExecuteRepeatDownloadStrategyCommand);

    /// <summary>
    /// 解析范围事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteRepeatDownloadStrategyCommand(object parameter)
    {
        if (parameter is not RepeatDownloadStrategyDisplay repeatDownloadStrategy)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetRepeatDownloadStrategy(repeatDownloadStrategy.RepeatDownloadStrategy);
        PublishTip(isSucceed);
    }

    #endregion

    /// <summary>
    /// 设置下载完成后的操作
    /// </summary>
    /// <param name="afterDownload"></param>
    private void SetAfterDownloadOperation(AfterDownloadOperation afterDownload)
    {
        switch (afterDownload)
        {
            case AfterDownloadOperation.NONE:
                None = true;
                break;
            case AfterDownloadOperation.OPEN_FOLDER:
                break;
            case AfterDownloadOperation.CLOSE_APP:
                CloseApp = true;
                break;
            case AfterDownloadOperation.CLOSE_SYSTEM:
                CloseSystem = true;
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

        if (isSucceed)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipSettingUpdated"));
        }
        else
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipSettingFailed"));
        }
    }
}