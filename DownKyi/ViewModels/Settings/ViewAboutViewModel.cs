using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.PrismExtension.Dialog;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.Settings;

public class ViewAboutViewModel : ViewModelBase
{
    public const string Tag = "PageSettingsAbout";

    private bool _isOnNavigatedTo;

    #region 页面属性申明

    private string _appName;

    public string AppName
    {
        get => _appName;
        set => SetProperty(ref _appName, value);
    }

    private string _appVersion;

    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }

    private bool _isReceiveBetaVersion;

    public bool IsReceiveBetaVersion
    {
        get => _isReceiveBetaVersion;
        set => SetProperty(ref _isReceiveBetaVersion, value);
    }

    private bool _autoUpdateWhenLaunch;

    public bool AutoUpdateWhenLaunch
    {
        get => _autoUpdateWhenLaunch;
        set => SetProperty(ref _autoUpdateWhenLaunch, value);
    }

    #endregion

    public ViewAboutViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator,
        dialogService)
    {
        #region 属性初始化

        AppInfo app = new AppInfo();
        AppName = app.Name;
        AppVersion = app.VersionName;

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

        // 是否接收测试版更新
        var isReceiveBetaVersion = SettingsManager.GetInstance().IsReceiveBetaVersion();
        IsReceiveBetaVersion = isReceiveBetaVersion == AllowStatus.YES;

        // 是否在启动时自动检查更新
        var isAutoUpdateWhenLaunch = SettingsManager.GetInstance().GetAutoUpdateWhenLaunch();
        AutoUpdateWhenLaunch = isAutoUpdateWhenLaunch == AllowStatus.YES;

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 访问主页事件
    private DelegateCommand? _appNameCommand;

    public DelegateCommand AppNameCommand => _appNameCommand ??= new DelegateCommand(ExecuteAppNameCommand);

    /// <summary>
    /// 访问主页事件
    /// </summary>
    private void ExecuteAppNameCommand()
    {
        PlatformHelper.Open("https://github.com/yaobiao131/downkyicore/releases", EventAggregator);
    }

    // 检查更新事件
    private DelegateCommand? _checkUpdateCommand;

    public DelegateCommand CheckUpdateCommand => _checkUpdateCommand ??= new DelegateCommand(ExecuteCheckUpdateCommand);

    /// <summary>
    /// 检查更新事件
    /// </summary>
    private void ExecuteCheckUpdateCommand()
    {
        //eventAggregator.GetEvent<MessageEvent>().Publish("开始查找更新，请稍后~");
        EventAggregator.GetEvent<MessageEvent>().Publish("请前往主页下载最新版~");
    }

    // 意见反馈事件
    private DelegateCommand? _feedbackCommand;

    public DelegateCommand FeedbackCommand => _feedbackCommand ??= new DelegateCommand(ExecuteFeedbackCommand);

    /// <summary>
    /// 意见反馈事件
    /// </summary>
    private void ExecuteFeedbackCommand()
    {
        PlatformHelper.Open("https://github.com/yaobiao131/downkyicore/issues", EventAggregator);
    }

    // 是否接收测试版更新事件
    private DelegateCommand? _receiveBetaVersionCommand;

    public DelegateCommand ReceiveBetaVersionCommand =>
        _receiveBetaVersionCommand ??= new DelegateCommand(ExecuteReceiveBetaVersionCommand);

    /// <summary>
    /// 是否接收测试版更新事件
    /// </summary>
    private void ExecuteReceiveBetaVersionCommand()
    {
        AllowStatus isReceiveBetaVersion = IsReceiveBetaVersion ? AllowStatus.YES : AllowStatus.NO;

        bool isSucceed = SettingsManager.GetInstance().IsReceiveBetaVersion(isReceiveBetaVersion);
        PublishTip(isSucceed);
    }

    // 是否在启动时自动检查更新事件
    private DelegateCommand? _autoUpdateWhenLaunchCommand;

    public DelegateCommand AutoUpdateWhenLaunchCommand =>
        _autoUpdateWhenLaunchCommand ??= new DelegateCommand(ExecuteAutoUpdateWhenLaunchCommand);

    /// <summary>
    /// 是否在启动时自动检查更新事件
    /// </summary>
    private void ExecuteAutoUpdateWhenLaunchCommand()
    {
        AllowStatus isAutoUpdateWhenLaunch = AutoUpdateWhenLaunch ? AllowStatus.YES : AllowStatus.NO;

        bool isSucceed = SettingsManager.GetInstance().SetAutoUpdateWhenLaunch(isAutoUpdateWhenLaunch);
        PublishTip(isSucceed);
    }

    // Brotli.NET许可证查看事件
    private DelegateCommand brotliLicenseCommand;

    public DelegateCommand BrotliLicenseCommand => brotliLicenseCommand ??
                                                   (brotliLicenseCommand =
                                                       new DelegateCommand(ExecuteBrotliLicenseCommand));

    /// <summary>
    /// Brotli.NET许可证查看事件
    /// </summary>
    private void ExecuteBrotliLicenseCommand()
    {
        PlatformHelper.Open("https://licenses.nuget.org/MIT");
    }

    // Google.Protobuf许可证查看事件
    private DelegateCommand protobufLicenseCommand;

    public DelegateCommand ProtobufLicenseCommand => protobufLicenseCommand ??
                                                     (protobufLicenseCommand =
                                                         new DelegateCommand(ExecuteProtobufLicenseCommand));

    /// <summary>
    /// Google.Protobuf许可证查看事件
    /// </summary>
    private void ExecuteProtobufLicenseCommand()
    {
        PlatformHelper.Open("https://github.com/protocolbuffers/protobuf/blob/master/LICENSE");
    }

    // Newtonsoft.Json许可证查看事件
    private DelegateCommand newtonsoftLicenseCommand;

    public DelegateCommand NewtonsoftLicenseCommand => newtonsoftLicenseCommand ??
                                                       (newtonsoftLicenseCommand =
                                                           new DelegateCommand(ExecuteNewtonsoftLicenseCommand));

    /// <summary>
    /// Newtonsoft.Json许可证查看事件
    /// </summary>
    private void ExecuteNewtonsoftLicenseCommand()
    {
        PlatformHelper.Open("https://licenses.nuget.org/MIT");
    }

    // Prism.DryIoc许可证查看事件
    private DelegateCommand prismLicenseCommand;

    public DelegateCommand PrismLicenseCommand => prismLicenseCommand ??
                                                  (prismLicenseCommand =
                                                      new DelegateCommand(ExecutePrismLicenseCommand));

    /// <summary>
    /// Prism.DryIoc许可证查看事件
    /// </summary>
    private void ExecutePrismLicenseCommand()
    {
        PlatformHelper.Open("https://www.nuget.org/packages/Prism.DryIoc/8.1.97/license");
    }

    // QRCoder许可证查看事件
    private DelegateCommand qRCoderLicenseCommand;

    public DelegateCommand QRCoderLicenseCommand => qRCoderLicenseCommand ??
                                                    (qRCoderLicenseCommand =
                                                        new DelegateCommand(ExecuteQRCoderLicenseCommand));

    /// <summary>
    /// QRCoder许可证查看事件
    /// </summary>
    private void ExecuteQRCoderLicenseCommand()
    {
        PlatformHelper.Open("https://licenses.nuget.org/MIT");
    }

    // System.Data.SQLite.Core许可证查看事件
    private DelegateCommand sQLiteLicenseCommand;

    public DelegateCommand SQLiteLicenseCommand => sQLiteLicenseCommand ??
                                                   (sQLiteLicenseCommand =
                                                       new DelegateCommand(ExecuteSQLiteLicenseCommand));

    /// <summary>
    /// System.Data.SQLite.Core许可证查看事件
    /// </summary>
    private void ExecuteSQLiteLicenseCommand()
    {
        PlatformHelper.Open("https://www.sqlite.org/copyright.html");
    }

    // Aria2c许可证查看事件
    private DelegateCommand ariaLicenseCommand;

    public DelegateCommand AriaLicenseCommand =>
        ariaLicenseCommand ?? (ariaLicenseCommand = new DelegateCommand(ExecuteAriaLicenseCommand));

    /// <summary>
    /// Aria2c许可证查看事件
    /// </summary>
    private void ExecuteAriaLicenseCommand()
    {
        PlatformHelper.Open("aria2_COPYING.txt");
    }

    // FFmpeg许可证查看事件
    private DelegateCommand fFmpegLicenseCommand;

    public DelegateCommand FFmpegLicenseCommand => fFmpegLicenseCommand ??
                                                   (fFmpegLicenseCommand =
                                                       new DelegateCommand(ExecuteFFmpegLicenseCommand));

    /// <summary>
    /// FFmpeg许可证查看事件
    /// </summary>
    private void ExecuteFFmpegLicenseCommand()
    {
        PlatformHelper.Open("FFmpeg_LICENSE.txt");
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

        EventAggregator.GetEvent<MessageEvent>().Publish(isSucceed
            ? DictionaryResource.GetString("TipSettingUpdated")
            : DictionaryResource.GetString("TipSettingFailed"));
    }
}