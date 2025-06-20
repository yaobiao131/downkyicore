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
    private ICommand? _checkUpdateCommand;

    public ICommand CheckUpdateCommand => _checkUpdateCommand ??= new AsyncDelegateCommand(ExecuteCheckUpdateCommand);


    /// <summary>
    /// 检查更新事件
    /// </summary>
    /// <param name="parameter"></param>
    private async Task ExecuteCheckUpdateCommand(object obj,CancellationToken token)
    {
        var service = new VersionCheckerService(App.RepoOwner, App.RepoName,_isReceiveBetaVersion);
        var  release = await service.GetLatestReleaseAsync();
        if(GitHubRelease.IsNullOrEmpty(release))
        {
            EventAggregator.GetEvent<MessageEvent>().Publish("检查失败，请稍后重试~");
            return;
        }
        
        if(service.IsNewVersionAvailable(release.TagName))
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

    // Brotli.NET许可证查看事件
    private DelegateCommand _brotliLicenseCommand;

    public DelegateCommand BrotliLicenseCommand => _brotliLicenseCommand ??= new DelegateCommand(ExecuteBrotliLicenseCommand);

    /// <summary>
    /// Brotli.NET许可证查看事件
    /// </summary>
    private void ExecuteBrotliLicenseCommand()
    {
        PlatformHelper.Open("https://licenses.nuget.org/MIT");
    }

    // Google.Protobuf许可证查看事件
    private DelegateCommand _protobufLicenseCommand;

    public DelegateCommand ProtobufLicenseCommand => _protobufLicenseCommand ??= new DelegateCommand(ExecuteProtobufLicenseCommand);

    /// <summary>
    /// Google.Protobuf许可证查看事件
    /// </summary>
    private void ExecuteProtobufLicenseCommand()
    {
        PlatformHelper.Open("https://github.com/protocolbuffers/protobuf/blob/master/LICENSE");
    }

    // Newtonsoft.Json许可证查看事件
    private DelegateCommand _newtonsoftLicenseCommand;

    public DelegateCommand NewtonsoftLicenseCommand => _newtonsoftLicenseCommand ??= new DelegateCommand(ExecuteNewtonsoftLicenseCommand);

    /// <summary>
    /// Newtonsoft.Json许可证查看事件
    /// </summary>
    private void ExecuteNewtonsoftLicenseCommand()
    {
        PlatformHelper.Open("https://licenses.nuget.org/MIT");
    }

    // Prism.DryIoc许可证查看事件
    private DelegateCommand _prismLicenseCommand;

    public DelegateCommand PrismLicenseCommand => _prismLicenseCommand ??= new DelegateCommand(ExecutePrismLicenseCommand);

    /// <summary>
    /// Prism.DryIoc许可证查看事件
    /// </summary>
    private void ExecutePrismLicenseCommand()
    {
        PlatformHelper.Open("https://www.nuget.org/packages/Prism.DryIoc/8.1.97/license");
    }

    // QRCoder许可证查看事件
    private DelegateCommand _qRCoderLicenseCommand;

    public DelegateCommand QRCoderLicenseCommand => _qRCoderLicenseCommand ??= new DelegateCommand(ExecuteQRCoderLicenseCommand);

    /// <summary>
    /// QRCoder许可证查看事件
    /// </summary>
    private void ExecuteQRCoderLicenseCommand()
    {
        PlatformHelper.Open("https://licenses.nuget.org/MIT");
    }

    // System.Data.SQLite.Core许可证查看事件
    private DelegateCommand _sQLiteLicenseCommand;

    public DelegateCommand SQLiteLicenseCommand => _sQLiteLicenseCommand ??= new DelegateCommand(ExecuteSQLiteLicenseCommand);

    /// <summary>
    /// System.Data.SQLite.Core许可证查看事件
    /// </summary>
    private void ExecuteSQLiteLicenseCommand()
    {
        PlatformHelper.Open("https://www.sqlite.org/copyright.html");
    }

    // Aria2c许可证查看事件
    private DelegateCommand _ariaLicenseCommand;

    public DelegateCommand AriaLicenseCommand => _ariaLicenseCommand ??= new DelegateCommand(ExecuteAriaLicenseCommand);

    /// <summary>
    /// Aria2c许可证查看事件
    /// </summary>
    private void ExecuteAriaLicenseCommand()
    {
        PlatformHelper.Open("aria2_COPYING.txt");
    }

    // FFmpeg许可证查看事件
    private DelegateCommand _fFmpegLicenseCommand;

    public DelegateCommand FFmpegLicenseCommand => _fFmpegLicenseCommand ??= new DelegateCommand(ExecuteFFmpegLicenseCommand);

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