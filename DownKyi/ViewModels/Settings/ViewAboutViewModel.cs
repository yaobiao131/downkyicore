using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DownKyi.Commands;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.Services;
using DownKyi.Utils;
using DownKyi.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

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

        var app = new AppInfo();
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
        var isReceiveBetaVersion = SettingsManager.GetInstance().GetIsReceiveBetaVersion();
        IsReceiveBetaVersion = isReceiveBetaVersion == AllowStatus.Yes;

        // 是否在启动时自动检查更新
        var isAutoUpdateWhenLaunch = SettingsManager.GetInstance().GetAutoUpdateWhenLaunch();
        AutoUpdateWhenLaunch = isAutoUpdateWhenLaunch == AllowStatus.Yes;

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 访问主页事件
    private AsyncDelegateCommand? _appNameCommand;

    public AsyncDelegateCommand AppNameCommand => _appNameCommand ??= new AsyncDelegateCommand(ExecuteAppNameCommand);

    /// <summary>
    /// 访问主页事件
    /// </summary>
    private async Task ExecuteAppNameCommand()
    {
        await PlatformHelper.OpenUrl("https://github.com/yaobiao131/downkyicore/releases", EventAggregator);
    }

    // 检查更新事件
    private ICommand? _checkUpdateCommand;

    public ICommand CheckUpdateCommand => _checkUpdateCommand ??= new AsyncDelegateCommand(ExecuteCheckUpdateCommand);


    /// <summary>
    /// 检查更新事件
    /// </summary>
    private async Task ExecuteCheckUpdateCommand()
    {
        var service = new VersionCheckerService(App.RepoOwner, App.RepoName, _isReceiveBetaVersion);
        var release = await service.GetLatestReleaseAsync();
        if (GitHubRelease.IsNullOrEmpty(release))
        {
            EventAggregator.GetEvent<MessageEvent>().Publish("检查失败，请稍后重试~");
            return;
        }

        if (service.IsNewVersionAvailable(release!.TagName))
        {
            await DialogService?.ShowDialogAsync(NewVersionAvailableDialogViewModel.Tag, new
                DialogParameters { { "release", release } })!;
        }
        else
        {
            EventAggregator.GetEvent<MessageEvent>().Publish("已是最新版~");
        }
    }

    // 意见反馈事件
    private AsyncDelegateCommand? _feedbackCommand;

    public AsyncDelegateCommand FeedbackCommand => _feedbackCommand ??= new AsyncDelegateCommand(ExecuteFeedbackCommand);

    /// <summary>
    /// 意见反馈事件
    /// </summary>
    private async Task ExecuteFeedbackCommand()
    {
        await PlatformHelper.OpenUrl("https://github.com/yaobiao131/downkyicore/issues", EventAggregator);
    }

    // 是否接收测试版更新事件
    private DelegateCommand? _receiveBetaVersionCommand;

    public DelegateCommand ReceiveBetaVersionCommand => _receiveBetaVersionCommand ??= new DelegateCommand(ExecuteReceiveBetaVersionCommand);

    /// <summary>
    /// 是否接收测试版更新事件
    /// </summary>
    private void ExecuteReceiveBetaVersionCommand()
    {
        var isReceiveBetaVersion = IsReceiveBetaVersion ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetIsReceiveBetaVersion(isReceiveBetaVersion);
        PublishTip(isSucceed);
    }

    // 是否在启动时自动检查更新事件
    private DelegateCommand? _autoUpdateWhenLaunchCommand;

    public DelegateCommand AutoUpdateWhenLaunchCommand => _autoUpdateWhenLaunchCommand ??= new DelegateCommand(ExecuteAutoUpdateWhenLaunchCommand);

    /// <summary>
    /// 是否在启动时自动检查更新事件
    /// </summary>
    private void ExecuteAutoUpdateWhenLaunchCommand()
    {
        var isAutoUpdateWhenLaunch = AutoUpdateWhenLaunch ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetAutoUpdateWhenLaunch(isAutoUpdateWhenLaunch);
        PublishTip(isSucceed);
    }

    // Google.Protobuf许可证查看事件
    private AsyncDelegateCommand? _protobufLicenseCommand;

    public AsyncDelegateCommand ProtobufLicenseCommand => _protobufLicenseCommand ??= new AsyncDelegateCommand(ExecuteProtobufLicenseCommand);

    /// <summary>
    /// Google.Protobuf许可证查看事件
    /// </summary>
    private async Task ExecuteProtobufLicenseCommand()
    {
        await PlatformHelper.OpenUrl("https://github.com/protocolbuffers/protobuf/blob/master/LICENSE");
    }

    // Newtonsoft.Json许可证查看事件
    private AsyncDelegateCommand? _newtonsoftLicenseCommand;

    public AsyncDelegateCommand NewtonsoftLicenseCommand => _newtonsoftLicenseCommand ??= new AsyncDelegateCommand(ExecuteNewtonsoftLicenseCommand);

    /// <summary>
    /// Newtonsoft.Json许可证查看事件
    /// </summary>
    private async Task ExecuteNewtonsoftLicenseCommand()
    {
        await PlatformHelper.OpenUrl("https://licenses.nuget.org/MIT");
    }

    // Prism.DryIoc许可证查看事件
    private AsyncDelegateCommand? _prismLicenseCommand;

    public AsyncDelegateCommand PrismLicenseCommand => _prismLicenseCommand ??= new AsyncDelegateCommand(ExecutePrismLicenseCommand);

    /// <summary>
    /// Prism.DryIoc许可证查看事件
    /// </summary>
    private async Task ExecutePrismLicenseCommand()
    {
        await PlatformHelper.OpenUrl("https://www.nuget.org/packages/Prism.DryIoc/8.1.97/license");
    }

    // QRCoder许可证查看事件
    private AsyncDelegateCommand? _qRCoderLicenseCommand;

    public AsyncDelegateCommand QRCoderLicenseCommand => _qRCoderLicenseCommand ??= new AsyncDelegateCommand(ExecuteQRCoderLicenseCommand);

    /// <summary>
    /// QRCoder许可证查看事件
    /// </summary>
    private async Task ExecuteQRCoderLicenseCommand()
    {
        await PlatformHelper.OpenUrl("https://licenses.nuget.org/MIT");
    }

    // System.Data.SQLite.Core许可证查看事件
    private AsyncDelegateCommand? _sQLiteLicenseCommand;

    public AsyncDelegateCommand SQLiteLicenseCommand => _sQLiteLicenseCommand ??= new AsyncDelegateCommand(ExecuteSQLiteLicenseCommand);

    /// <summary>
    /// System.Data.SQLite.Core许可证查看事件
    /// </summary>
    private async Task ExecuteSQLiteLicenseCommand()
    {
        await PlatformHelper.OpenUrl("https://www.sqlite.org/copyright.html");
    }

    // Aria2c许可证查看事件
    private AsyncDelegateCommand? _ariaLicenseCommand;

    public AsyncDelegateCommand AriaLicenseCommand => _ariaLicenseCommand ??= new AsyncDelegateCommand(ExecuteAriaLicenseCommand);

    /// <summary>
    /// Aria2c许可证查看事件
    /// </summary>
    private async Task ExecuteAriaLicenseCommand()
    {
        await PlatformHelper.Open("aria2_COPYING.txt");
    }

    // FFmpeg许可证查看事件
    private AsyncDelegateCommand? _fFmpegLicenseCommand;

    public AsyncDelegateCommand FFmpegLicenseCommand => _fFmpegLicenseCommand ??= new AsyncDelegateCommand(ExecuteFFmpegLicenseCommand);

    /// <summary>
    /// FFmpeg许可证查看事件
    /// </summary>
    private async Task ExecuteFFmpegLicenseCommand()
    {
        await PlatformHelper.Open("FFmpeg_LICENSE.txt");
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