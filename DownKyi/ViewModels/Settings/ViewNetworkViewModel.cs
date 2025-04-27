using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DownKyi.Core.Aria2cNet.Server;
using DownKyi.Core.Settings;
using DownKyi.Core.Utils.Validator;
using DownKyi.Events;
using DownKyi.Services;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels.Settings;

public class ViewNetworkViewModel : ViewModelBase
{
    public const string Tag = "PageSettingsNetwork";

    private bool _isOnNavigatedTo;

    #region 页面属性申明

    private bool _useSsl;

    public bool UseSsl
    {
        get => _useSsl;
        set => SetProperty(ref _useSsl, value);
    }

    private string _userAgent;

    public string UserAgent
    {
        get => _userAgent;
        set => SetProperty(ref _userAgent, value);
    }

    private bool _builtin;

    public bool Builtin
    {
        get => _builtin;
        set => SetProperty(ref _builtin, value);
    }

    private bool _aria2C;

    public bool Aria2C
    {
        get => _aria2C;
        set => SetProperty(ref _aria2C, value);
    }

    private bool _customAria2C;

    public bool CustomAria2C
    {
        get => _customAria2C;
        set => SetProperty(ref _customAria2C, value);
    }

    private List<int> _maxCurrentDownloads;

    public List<int> MaxCurrentDownloads
    {
        get => _maxCurrentDownloads;
        set => SetProperty(ref _maxCurrentDownloads, value);
    }

    private int _selectedMaxCurrentDownload;

    public int SelectedMaxCurrentDownload
    {
        get => _selectedMaxCurrentDownload;
        set => SetProperty(ref _selectedMaxCurrentDownload, value);
    }

    private NetworkProxy _networkProxy;

    public NetworkProxy NetworkProxy
    {
        get => _networkProxy;
        set => SetProperty(ref _networkProxy, value);
    }

    private string? _customNetworkProxy;

    public string? CustomNetworkProxy
    {
        get => _customNetworkProxy;
        set => SetProperty(ref _customNetworkProxy, value);
    }

    private List<int> _splits;

    public List<int> Splits
    {
        get => _splits;
        set => SetProperty(ref _splits, value);
    }

    private int _selectedSplit;

    public int SelectedSplit
    {
        get => _selectedSplit;
        set => SetProperty(ref _selectedSplit, value);
    }

    private bool _isHttpProxy;

    public bool IsHttpProxy
    {
        get => _isHttpProxy;
        set => SetProperty(ref _isHttpProxy, value);
    }

    private string _httpProxy;

    public string HttpProxy
    {
        get => _httpProxy;
        set => SetProperty(ref _httpProxy, value);
    }

    private int _httpProxyPort;

    public int HttpProxyPort
    {
        get => _httpProxyPort;
        set => SetProperty(ref _httpProxyPort, value);
    }

    private string _ariaHost;

    public string AriaHost
    {
        get => _ariaHost;
        set => SetProperty(ref _ariaHost, value);
    }

    private int _ariaListenPort;

    public int AriaListenPort
    {
        get => _ariaListenPort;
        set => SetProperty(ref _ariaListenPort, value);
    }

    private string _ariaToken;

    public string AriaToken
    {
        get => _ariaToken;
        set => SetProperty(ref _ariaToken, value);
    }

    private List<string> _ariaLogLevels;

    public List<string> AriaLogLevels
    {
        get => _ariaLogLevels;
        set => SetProperty(ref _ariaLogLevels, value);
    }

    private string _selectedAriaLogLevel;

    public string SelectedAriaLogLevel
    {
        get => _selectedAriaLogLevel;
        set => SetProperty(ref _selectedAriaLogLevel, value);
    }

    private List<int> _ariaMaxConcurrentDownloads;

    public List<int> AriaMaxConcurrentDownloads
    {
        get => _ariaMaxConcurrentDownloads;
        set => SetProperty(ref _ariaMaxConcurrentDownloads, value);
    }

    private int _selectedAriaMaxConcurrentDownload;

    public int SelectedAriaMaxConcurrentDownload
    {
        get => _selectedAriaMaxConcurrentDownload;
        set => SetProperty(ref _selectedAriaMaxConcurrentDownload, value);
    }

    private List<int> _ariaSplits;

    public List<int> AriaSplits
    {
        get => _ariaSplits;
        set => SetProperty(ref _ariaSplits, value);
    }

    private int _selectedAriaSplit;

    public int SelectedAriaSplit
    {
        get => _selectedAriaSplit;
        set => SetProperty(ref _selectedAriaSplit, value);
    }

    private int _ariaMaxOverallDownloadLimit;

    public int AriaMaxOverallDownloadLimit
    {
        get => _ariaMaxOverallDownloadLimit;
        set => SetProperty(ref _ariaMaxOverallDownloadLimit, value);
    }

    private int _ariaMaxDownloadLimit;

    public int AriaMaxDownloadLimit
    {
        get => _ariaMaxDownloadLimit;
        set => SetProperty(ref _ariaMaxDownloadLimit, value);
    }

    private bool _isAriaHttpProxy;

    public bool IsAriaHttpProxy
    {
        get => _isAriaHttpProxy;
        set => SetProperty(ref _isAriaHttpProxy, value);
    }

    private string _ariaHttpProxy;

    public string AriaHttpProxy
    {
        get => _ariaHttpProxy;
        set => SetProperty(ref _ariaHttpProxy, value);
    }

    private int _ariaHttpProxyPort;

    public int AriaHttpProxyPort
    {
        get => _ariaHttpProxyPort;
        set => SetProperty(ref _ariaHttpProxyPort, value);
    }

    private List<string> _ariaFileAllocations;

    public List<string> AriaFileAllocations
    {
        get => _ariaFileAllocations;
        set => SetProperty(ref _ariaFileAllocations, value);
    }

    private string _selectedAriaFileAllocation;

    public string SelectedAriaFileAllocation
    {
        get => _selectedAriaFileAllocation;
        set => SetProperty(ref _selectedAriaFileAllocation, value);
    }

    #endregion

    public ViewNetworkViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator,
        dialogService)
    {
        #region 属性初始化

        // builtin同时下载数
        MaxCurrentDownloads = new List<int>();
        for (var i = 1; i <= 10; i++)
        {
            MaxCurrentDownloads.Add(i);
        }

        // builtin最大线程数
        Splits = new List<int>();
        for (var i = 1; i <= 10; i++)
        {
            Splits.Add(i);
        }

        // Aria的日志等级
        AriaLogLevels = new List<string>
        {
            "DEBUG",
            "INFO",
            "NOTICE",
            "WARN",
            "ERROR"
        };

        // Aria同时下载数
        AriaMaxConcurrentDownloads = new List<int>();
        for (var i = 1; i <= 10; i++)
        {
            AriaMaxConcurrentDownloads.Add(i);
        }

        // Aria最大线程数
        AriaSplits = new List<int>();
        for (var i = 1; i <= 10; i++)
        {
            AriaSplits.Add(i);
        }

        // Aria文件预分配
        AriaFileAllocations = new List<string>
        {
            "NONE",
            "PREALLOC",
            "FALLOC"
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

        // 启用https
        var useSsl = SettingsManager.GetInstance().GetUseSsl();
        UseSsl = useSsl == AllowStatus.Yes;

        // UserAgent
        UserAgent = SettingsManager.GetInstance().GetUserAgent();

        // 选择下载器
        var downloader = SettingsManager.GetInstance().GetDownloader();
        switch (downloader)
        {
            case Core.Settings.Downloader.NotSet:
                break;
            case Core.Settings.Downloader.BuiltIn:
                Builtin = true;
                break;
            case Core.Settings.Downloader.Aria:
                Aria2C = true;
                break;
            case Core.Settings.Downloader.CustomAria:
                CustomAria2C = true;
                break;
        }

        NetworkProxy = SettingsManager.GetInstance().GetNetworkProxy();

        CustomNetworkProxy = SettingsManager.GetInstance().GetCustomProxy();

        // builtin同时下载数
        SelectedMaxCurrentDownload = SettingsManager.GetInstance().GetMaxCurrentDownloads();

        // builtin最大线程数
        SelectedSplit = SettingsManager.GetInstance().GetSplit();

        // 是否开启builtin http代理
        var isHttpProxy = SettingsManager.GetInstance().GetIsHttpProxy();
        IsHttpProxy = isHttpProxy == AllowStatus.Yes;

        // builtin的http代理的地址
        HttpProxy = SettingsManager.GetInstance().GetHttpProxy();

        // builtin的http代理的端口
        HttpProxyPort = SettingsManager.GetInstance().GetHttpProxyListenPort();

        // Aria服务器host
        AriaHost = SettingsManager.GetInstance().GetAriaHost();

        // Aria服务器端口
        AriaListenPort = SettingsManager.GetInstance().GetAriaListenPort();

        // Aria服务器Token
        AriaToken = SettingsManager.GetInstance().GetAriaToken();

        // Aria的日志等级
        var ariaLogLevel = SettingsManager.GetInstance().GetAriaLogLevel();
        SelectedAriaLogLevel = ariaLogLevel.ToString("G");

        // Aria同时下载数
        SelectedAriaMaxConcurrentDownload = SettingsManager.GetInstance().GetMaxCurrentDownloads();

        // Aria最大线程数
        SelectedAriaSplit = SettingsManager.GetInstance().GetAriaSplit();

        // Aria下载速度限制
        AriaMaxOverallDownloadLimit = SettingsManager.GetInstance().GetAriaMaxOverallDownloadLimit();

        // Aria下载单文件速度限制
        AriaMaxDownloadLimit = SettingsManager.GetInstance().GetAriaMaxDownloadLimit();

        // 是否开启Aria http代理
        var isAriaHttpProxy = SettingsManager.GetInstance().GetIsAriaHttpProxy();
        IsAriaHttpProxy = isAriaHttpProxy == AllowStatus.Yes;

        // Aria的http代理的地址
        AriaHttpProxy = SettingsManager.GetInstance().GetAriaHttpProxy();

        // Aria的http代理的端口
        AriaHttpProxyPort = SettingsManager.GetInstance().GetAriaHttpProxyListenPort();

        // Aria文件预分配
        var ariaFileAllocation = SettingsManager.GetInstance().GetAriaFileAllocation();
        SelectedAriaFileAllocation = ariaFileAllocation.ToString("G");

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 是否启用https事件
    private DelegateCommand? _useSslCommand;

    public DelegateCommand UseSslCommand => _useSslCommand ??= new DelegateCommand(ExecuteUseSslCommand);

    /// <summary>
    /// 是否启用https事件
    /// </summary>
    private void ExecuteUseSslCommand()
    {
        var useSsl = UseSsl ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetUseSsl(useSsl);
        PublishTip(isSucceed);
    }

    // 设置UserAgent事件
    private DelegateCommand? _userAgentCommand;

    public DelegateCommand UserAgentCommand => _userAgentCommand ??= new DelegateCommand(ExecuteUserAgentCommand);

    /// <summary>
    /// 设置UserAgent事件
    /// </summary>
    private void ExecuteUserAgentCommand()
    {
        var isSucceed = SettingsManager.GetInstance().SetUserAgent(UserAgent);
        PublishTip(isSucceed);
    }

    // 下载器选择事件
    private DelegateCommand<string>? _selectDownloaderCommand;

    public DelegateCommand<string> SelectDownloaderCommand => _selectDownloaderCommand ??= new DelegateCommand<string>(ExecuteSelectDownloaderCommand);

    /// <summary>
    /// 下载器选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private async void ExecuteSelectDownloaderCommand(string parameter)
    {
        Core.Settings.Downloader downloader;
        switch (parameter)
        {
            case "Builtin":
                downloader = Core.Settings.Downloader.BuiltIn;
                break;
            case "Aria2c":
                downloader = Core.Settings.Downloader.Aria;
                break;
            case "CustomAria2c":
                downloader = Core.Settings.Downloader.CustomAria;
                break;
            default:
                downloader = SettingsManager.GetInstance().GetDownloader();
                break;
        }

        var isSucceed = SettingsManager.GetInstance().SetDownloader(downloader);
        PublishTip(isSucceed);

        var alertService = new AlertService(DialogService);
        var result = await alertService.ShowInfo(DictionaryResource.GetString("ConfirmReboot"));
        if (result == ButtonResult.OK)
        {
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            // var dir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            // todo 暂时去掉自动重启,多平台需要不同实现
            // if (dir != null)
            // {
            //     Process.Start($"{dir}/DownKyi");
            // }
        }
    }

    private DelegateCommand<object>? _networkProxyCommand;

    public DelegateCommand<object> NetworkProxyCommand => _networkProxyCommand ??= new DelegateCommand<object>(ExecuteNetworkProxyCommand);

    private void ExecuteNetworkProxyCommand(object obj)
    {
        if (obj is not NetworkProxy networkProxy) return;
        NetworkProxy = networkProxy;
        var isSucceed = SettingsManager.GetInstance().SetNetworkProxy(networkProxy);
        PublishTip(isSucceed);
    }
    
    // builtin的http代理的地址事件
    private DelegateCommand<string>? _customNetworkProxyCommand;

    public DelegateCommand<string> CustomNetworkProxyCommand => _customNetworkProxyCommand ??= new DelegateCommand<string>(ExecuteCustomNetworkProxyCommand);

    /// <summary>
    /// builtin的http代理的地址事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteCustomNetworkProxyCommand(string parameter)
    {
        var isSucceed = SettingsManager.GetInstance().SetCustomProxy(parameter);
        PublishTip(isSucceed);
    }
    

    // builtin同时下载数事件
    private DelegateCommand<object>? _maxCurrentDownloadsCommand;

    public DelegateCommand<object> MaxCurrentDownloadsCommand => _maxCurrentDownloadsCommand ??= new DelegateCommand<object>(ExecuteMaxCurrentDownloadsCommand);

    /// <summary>
    /// builtin同时下载数事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteMaxCurrentDownloadsCommand(object parameter)
    {
        // SelectedMaxCurrentDownload = (int)parameter;

        var isSucceed = SettingsManager.GetInstance().SetMaxCurrentDownloads(SelectedMaxCurrentDownload);
        PublishTip(isSucceed);
    }

    // builtin最大线程数事件
    private DelegateCommand<object>? _splitsCommand;

    public DelegateCommand<object> SplitsCommand => _splitsCommand ??= new DelegateCommand<object>(ExecuteSplitsCommand);

    /// <summary>
    /// builtin最大线程数事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteSplitsCommand(object parameter)
    {
        // SelectedSplit = (int)parameter;

        var isSucceed = SettingsManager.GetInstance().SetSplit(SelectedSplit);
        PublishTip(isSucceed);
    }

    // 是否开启builtin http代理事件
    private DelegateCommand? _isHttpProxyCommand;

    public DelegateCommand IsHttpProxyCommand => _isHttpProxyCommand ??= new DelegateCommand(ExecuteIsHttpProxyCommand);

    /// <summary>
    /// 是否开启builtin http代理事件
    /// </summary>
    private void ExecuteIsHttpProxyCommand()
    {
        var isHttpProxy = IsHttpProxy ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetIsHttpProxy(isHttpProxy);
        PublishTip(isSucceed);
    }

    // builtin的http代理的地址事件
    private DelegateCommand<string>? _httpProxyCommand;

    public DelegateCommand<string> HttpProxyCommand => _httpProxyCommand ??= new DelegateCommand<string>(ExecuteHttpProxyCommand);

    /// <summary>
    /// builtin的http代理的地址事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteHttpProxyCommand(string parameter)
    {
        var isSucceed = SettingsManager.GetInstance().SetHttpProxy(parameter);
        PublishTip(isSucceed);
    }

    // builtin的http代理的端口事件
    private DelegateCommand<string>? _httpProxyPortCommand;

    public DelegateCommand<string> HttpProxyPortCommand => _httpProxyPortCommand ??= new DelegateCommand<string>(ExecuteHttpProxyPortCommand);

    /// <summary>
    /// builtin的http代理的端口事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteHttpProxyPortCommand(string parameter)
    {
        var httpProxyPort = (int)Number.GetInt(parameter);
        HttpProxyPort = httpProxyPort;

        var isSucceed = SettingsManager.GetInstance().SetHttpProxyListenPort(HttpProxyPort);
        PublishTip(isSucceed);
    }

    // Aria服务器host事件
    private DelegateCommand<string>? _ariaHostCommand;

    public DelegateCommand<string> AriaHostCommand => _ariaHostCommand ??= new DelegateCommand<string>(ExecuteAriaHostCommand);

    /// <summary>
    /// Aria服务器host事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaHostCommand(string parameter)
    {
        AriaHost = parameter;
        var isSucceed = SettingsManager.GetInstance().SetAriaHost(AriaHost);
        PublishTip(isSucceed);
    }

    // Aria服务器端口事件
    private DelegateCommand<string>? _ariaListenPortCommand;

    public DelegateCommand<string> AriaListenPortCommand => _ariaListenPortCommand ??= new DelegateCommand<string>(ExecuteAriaListenPortCommand);

    /// <summary>
    /// Aria服务器端口事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaListenPortCommand(string parameter)
    {
        var listenPort = (int)Number.GetInt(parameter);
        AriaListenPort = listenPort;

        var isSucceed = SettingsManager.GetInstance().SetAriaListenPort(AriaListenPort);
        PublishTip(isSucceed);
    }

    // Aria服务器token事件
    private DelegateCommand<string>? _ariaTokenCommand;

    public DelegateCommand<string> AriaTokenCommand => _ariaTokenCommand ??= new DelegateCommand<string>(ExecuteAriaTokenCommand);

    /// <summary>
    /// Aria服务器token事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaTokenCommand(string parameter)
    {
        AriaToken = parameter;
        var isSucceed = SettingsManager.GetInstance().SetAriaToken(AriaToken);
        PublishTip(isSucceed);
    }

    // Aria的日志等级事件
    private DelegateCommand<string>? _ariaLogLevelsCommand;

    public DelegateCommand<string> AriaLogLevelsCommand => _ariaLogLevelsCommand ??= new DelegateCommand<string>(ExecuteAriaLogLevelsCommand);

    /// <summary>
    /// Aria的日志等级事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaLogLevelsCommand(string parameter)
    {
        var ariaLogLevel = parameter switch
        {
            "DEBUG" => AriaConfigLogLevel.DEBUG,
            "INFO" => AriaConfigLogLevel.INFO,
            "NOTICE" => AriaConfigLogLevel.NOTICE,
            "WARN" => AriaConfigLogLevel.WARN,
            "ERROR" => AriaConfigLogLevel.ERROR,
            _ => AriaConfigLogLevel.INFO
        };

        var isSucceed = SettingsManager.GetInstance().SetAriaLogLevel(ariaLogLevel);
        PublishTip(isSucceed);
    }

    // Aria同时下载数事件
    private DelegateCommand<object?>? _ariaMaxConcurrentDownloadsCommand;

    public DelegateCommand<object?> AriaMaxConcurrentDownloadsCommand =>
        _ariaMaxConcurrentDownloadsCommand ??= new DelegateCommand<object?>(ExecuteAriaMaxConcurrentDownloadsCommand);

    /// <summary>
    /// Aria同时下载数事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaMaxConcurrentDownloadsCommand(object? parameter)
    {
        if (parameter == null) return;
        SelectedAriaMaxConcurrentDownload = (int)parameter;

        var isSucceed = SettingsManager.GetInstance().SetMaxCurrentDownloads(SelectedAriaMaxConcurrentDownload);
        PublishTip(isSucceed);
    }

    // Aria最大线程数事件
    private DelegateCommand<object?>? _ariaSplitsCommand;

    public DelegateCommand<object?> AriaSplitsCommand => _ariaSplitsCommand ??= new DelegateCommand<object?>(ExecuteAriaSplitsCommand);

    /// <summary>
    /// Aria最大线程数事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaSplitsCommand(object? parameter)
    {
        if (parameter == null) return;
        SelectedAriaSplit = (int)parameter;

        var isSucceed = SettingsManager.GetInstance().SetAriaSplit(SelectedAriaSplit);
        PublishTip(isSucceed);
    }

    // Aria下载速度限制事件
    private DelegateCommand<string>? _ariaMaxOverallDownloadLimitCommand;

    public DelegateCommand<string> AriaMaxOverallDownloadLimitCommand => _ariaMaxOverallDownloadLimitCommand ??= new DelegateCommand<string>(
        ExecuteAriaMaxOverallDownloadLimitCommand);

    /// <summary>
    /// Aria下载速度限制事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaMaxOverallDownloadLimitCommand(string parameter)
    {
        var downloadLimit = (int)Number.GetInt(parameter);
        AriaMaxOverallDownloadLimit = downloadLimit;

        var isSucceed = SettingsManager.GetInstance().SetAriaMaxOverallDownloadLimit(AriaMaxOverallDownloadLimit);
        PublishTip(isSucceed);
    }

    // Aria下载单文件速度限制事件
    private DelegateCommand<string>? _ariaMaxDownloadLimitCommand;

    public DelegateCommand<string> AriaMaxDownloadLimitCommand => _ariaMaxDownloadLimitCommand ??= new DelegateCommand<string>(ExecuteAriaMaxDownloadLimitCommand);

    /// <summary>
    /// Aria下载单文件速度限制事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaMaxDownloadLimitCommand(string parameter)
    {
        var downloadLimit = (int)Number.GetInt(parameter);
        AriaMaxDownloadLimit = downloadLimit;

        var isSucceed = SettingsManager.GetInstance().SetAriaMaxDownloadLimit(AriaMaxDownloadLimit);
        PublishTip(isSucceed);
    }

    // 是否开启Aria http代理事件
    private DelegateCommand? _isAriaHttpProxyCommand;

    public DelegateCommand IsAriaHttpProxyCommand => _isAriaHttpProxyCommand ??= new DelegateCommand(ExecuteIsAriaHttpProxyCommand);

    /// <summary>
    /// 是否开启Aria http代理事件
    /// </summary>
    private void ExecuteIsAriaHttpProxyCommand()
    {
        var isAriaHttpProxy = IsAriaHttpProxy ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetIsAriaHttpProxy(isAriaHttpProxy);
        PublishTip(isSucceed);
    }

    // Aria的http代理的地址事件
    private DelegateCommand<string>? _ariaHttpProxyCommand;

    public DelegateCommand<string> AriaHttpProxyCommand => _ariaHttpProxyCommand ??= new DelegateCommand<string>(ExecuteAriaHttpProxyCommand);

    /// <summary>
    /// Aria的http代理的地址事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaHttpProxyCommand(string parameter)
    {
        var isSucceed = SettingsManager.GetInstance().SetAriaHttpProxy(parameter);
        PublishTip(isSucceed);
    }

    // Aria的http代理的端口事件
    private DelegateCommand<string>? _ariaHttpProxyPortCommand;

    public DelegateCommand<string> AriaHttpProxyPortCommand => _ariaHttpProxyPortCommand ??= new DelegateCommand<string>(ExecuteAriaHttpProxyPortCommand);

    /// <summary>
    /// Aria的http代理的端口事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaHttpProxyPortCommand(string parameter)
    {
        var httpProxyPort = (int)Number.GetInt(parameter);
        AriaHttpProxyPort = httpProxyPort;

        var isSucceed = SettingsManager.GetInstance().SetAriaHttpProxyListenPort(AriaHttpProxyPort);
        PublishTip(isSucceed);
    }

    // Aria文件预分配事件
    private DelegateCommand<string>? _ariaFileAllocationsCommand;

    public DelegateCommand<string> AriaFileAllocationsCommand => _ariaFileAllocationsCommand ??= new DelegateCommand<string>(ExecuteAriaFileAllocationsCommand);

    /// <summary>
    /// Aria文件预分配事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAriaFileAllocationsCommand(string parameter)
    {
        var ariaFileAllocation = parameter switch
        {
            "NONE" => AriaConfigFileAllocation.NONE,
            "PREALLOC" => AriaConfigFileAllocation.PREALLOC,
            "FALLOC" => AriaConfigFileAllocation.FALLOC,
            _ => AriaConfigFileAllocation.PREALLOC
        };

        var isSucceed = SettingsManager.GetInstance().SetAriaFileAllocation(ariaFileAllocation);
        PublishTip(isSucceed);
    }

    #endregion

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