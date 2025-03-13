using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.Logging;
using DownKyi.Events;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.ViewModels;

public class ViewLoginViewModel : ViewModelBase
{
    public const string Tag = "PageLogin";

    private CancellationTokenSource? _tokenSource;

    #region 页面属性申明

    private Bitmap? _loginQrCode;

    public Bitmap? LoginQrCode
    {
        get => _loginQrCode;
        set => SetProperty(ref _loginQrCode, value);
    }

    private double _loginQrCodeOpacity;

    public double LoginQrCodeOpacity
    {
        get => _loginQrCodeOpacity;
        set => SetProperty(ref _loginQrCodeOpacity, value);
    }

    private bool _loginQrCodeStatus;

    public bool LoginQrCodeStatus
    {
        get => _loginQrCodeStatus;
        set => SetProperty(ref _loginQrCodeStatus, value);
    }

    #endregion

    public ViewLoginViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
    }

    private DelegateCommand? _backSpaceCommand;

    public DelegateCommand BackSpaceCommand => _backSpaceCommand ??= new DelegateCommand(ExecuteBackSpace);

    private void ExecuteBackSpace()
    {
        // 初始化状态
        InitStatus();

        // 结束任务
        _tokenSource?.Cancel();
        var parameter = new NavigationParam
        {
            ViewName = ParentView,
            ParentViewName = null,
            Parameter = "login"
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    /// <summary>
    /// 登录
    /// </summary>
    private void Login()
    {
        try
        {
            var loginUrl = LoginQr.GetLoginUrl();
            if (loginUrl == null)
            {
                return;
            }

            if (loginUrl.Code != 0)
            {
                ExecuteBackSpace();
                return;
            }

            if (loginUrl.Data?.Url == null || loginUrl.Data?.QrcodeKey == null)
            {
                EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("GetLoginUrlFailed"));
                return;
            }

            PropertyChangeAsync(() => { LoginQrCode = LoginQr.GetLoginQrCode(loginUrl.Data.Url); });
            Console.PrintLine(loginUrl.Data.Url + "\n");
            LogManager.Debug(Tag, loginUrl.Data.Url);

            GetLoginStatus(loginUrl.Data.QrcodeKey);
        }
        catch (Exception e)
        {
            Console.PrintLine("Login()发生异常: {0}", e);
            LogManager.Error(Tag, e);
        }
    }

    /// <summary>
    /// 循环查询登录状态
    /// </summary>
    /// <param name="oauthKey"></param>
    private void GetLoginStatus(string oauthKey)
    {
        var cancellationToken = _tokenSource?.Token;
        while (true)
        {
            Thread.Sleep(1000);
            var loginStatus = LoginQr.GetLoginStatus(oauthKey);
            if (loginStatus == null)
            {
                continue;
            }

            switch (loginStatus.Data.Code)
            {
                case 86038:
                    // 二维码已失效
                    // 发送通知
                    EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("LoginTimeOut"));
                    LogManager.Info(Tag, DictionaryResource.GetString("LoginTimeOut"));

                    // 取消任务
                    _tokenSource?.Cancel();

                    // 创建新任务
                    PropertyChangeAsync(() => { Task.Run(Login, (_tokenSource = new CancellationTokenSource()).Token); });
                    break;
                case 86101:
                    // 未扫码
                    break;
                case 86090:
                    // 已扫码，未确认
                    PropertyChangeAsync(() =>
                    {
                        LoginQrCodeStatus = true;
                        LoginQrCodeOpacity = 0.3;
                    });
                    break;
                case 0:
                    // 确认登录

                    // 发送通知
                    EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("LoginSuccessful"));
                    LogManager.Info(Tag, DictionaryResource.GetString("LoginSuccessful"));

                    // 保存登录信息
                    try
                    {
                        var isSucceed = LoginHelper.SaveLoginInfoCookies(loginStatus.Data.Url);
                        if (!isSucceed)
                        {
                            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("LoginFailed"));
                            LogManager.Error(Tag, DictionaryResource.GetString("LoginFailed"));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.PrintLine("PageLogin 保存登录信息发生异常: {0}", e);
                        LogManager.Error(e);
                        EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("LoginFailed"));
                    }
                    
                    // 取消任务
                    Thread.Sleep(3000);
                    PropertyChange(ExecuteBackSpace);
                    break;
            }

            // 判断是否该结束线程，若为true，跳出while循环
            if (cancellationToken?.IsCancellationRequested != true) continue;
            Console.PrintLine("停止Login线程，跳出while循环");
            LogManager.Debug(Tag, "登录操作结束");
            break;
        }
    }


    /// <summary>
    /// 初始化状态
    /// </summary>
    private void InitStatus()
    {
        LoginQrCode = null;
        LoginQrCodeOpacity = 1;
        LoginQrCodeStatus = false;
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        InitStatus();

        Task.Run(Login, (_tokenSource = new CancellationTokenSource()).Token);
    }
}