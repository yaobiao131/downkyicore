using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.Services;
using DownKyi.Utils;
using DownKyi.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;

    private readonly IRegionManager _regionManager;

    private readonly IDialogService _dialogService;

    private const string ContentRegion = nameof(ContentRegion);

    private ClipboardListener? _clipboardListener;

    private bool _messageVisibility;
    private string? _oldMessage;

    public bool MessageVisibility
    {
        get => _messageVisibility;
        set => SetProperty(ref _messageVisibility, value);
    }

    private string? _message;

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public DelegateCommand? LoadedCommand { get; }

    private DelegateCommand? _closingCommand;

    public DelegateCommand ClosingCommand => _closingCommand ??= _closingCommand = new DelegateCommand(ExecuteClosingCommand);

    public DelegateCommand<PointerPressedEventArgs> PointerPressedCommand =>
        new(ExecutePointerPressed);

    private void ExecuteClosingCommand()
    {
        if (_clipboardListener == null) return;
        _clipboardListener.Changed -= ClipboardListenerOnChanged;
        _clipboardListener.Dispose();
    }

    private void ExecutePointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(null);
        var updateKind = point.Properties.PointerUpdateKind;
        if (updateKind == PointerUpdateKind.XButton1Pressed)
        {
            var v = GetCurrentUserControl()?.DataContext;
            if (v is ViewModelBase vm)
            {
                vm.ExecuteBackSpace();
                e.Handled = true;
            }
        }
    }

    private UserControl? GetCurrentUserControl() => _regionManager
        .Regions[ContentRegion].ActiveViews
        .FirstOrDefault() as UserControl;

    public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IDialogService dialogService)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        _dialogService = dialogService;

        #region MyRegion

        _eventAggregator.GetEvent<NavigationEvent>().Subscribe(view =>
        {
            var param = new NavigationParameters
            {
                { "Parent", view.ParentViewName },
                { "Parameter", view.Parameter }
            };
            regionManager.RequestNavigate(ContentRegion, view.ViewName, param);
        });

        // 订阅消息发送事件
        _eventAggregator.GetEvent<MessageEvent>().Subscribe(message =>
        {
            MessageVisibility = true;

            _oldMessage = Message;
            Message = message;
            var sleep = 2000;
            if (_oldMessage == Message)
            {
                sleep = 1500;
            }

            Thread.Sleep(sleep);

            MessageVisibility = false;
        }, ThreadOption.BackgroundThread);

        #endregion


        LoadedCommand = new DelegateCommand(() =>
        {
            Upgrade();
            CheckForUpdates();
            _clipboardListener = new ClipboardListener(App.Current.MainWindow);
            _clipboardListener.Changed += ClipboardListenerOnChanged;
            var param = new NavigationParameters
            {
                { "Parent", "" },
                { "Parameter", "start" }
            };
            _regionManager.RequestNavigate("ContentRegion", ViewIndexViewModel.Tag, param);
        });
    }

    #region 剪贴板

    private int _times;

    private void ClipboardListenerOnChanged(string obj)
    {
        #region 执行第二遍时跳过

        _times += 1;
        var timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 300)
        };
        timer.Tick += (_, _) =>
        {
            timer.IsEnabled = false;
            _times = 0;
        };
        timer.IsEnabled = true;

        if (_times % 2 == 0)
        {
            timer.IsEnabled = false;
            _times = 0;
            return;
        }

        #endregion

        var isListenClipboard = SettingsManager.GetInstance().GetIsListenClipboard();
        if (isListenClipboard != AllowStatus.Yes)
        {
            return;
        }

        var searchService = new SearchService();
        Dispatcher.UIThread.InvokeAsync(() => { searchService.BiliInput(obj + AppConstant.ClipboardId, ViewIndexViewModel.Tag, _eventAggregator); });
    }

    #endregion

    private void Upgrade()
    {
        _dialogService.ShowDialogAsync(ViewUpgradingDialogViewModel.Tag, new DialogParameters(), (result) => { });
    }
    
    private async void CheckForUpdates()
    {
        try
        {
            var isAutoUpdate = SettingsManager.GetInstance().GetAutoUpdateWhenLaunch() != AllowStatus.Yes;
            if(isAutoUpdate) return;
            var service = new VersionCheckerService(App.RepoOwner,App.RepoName,
                SettingsManager.GetInstance().GetIsReceiveBetaVersion() == AllowStatus.Yes);
            var release = await service.GetLatestReleaseAsync(SettingsManager.GetInstance().GetSkipVersionOnLaunch());
            if (release != null && service.IsNewVersionAvailable(release.TagName))
            {
                await _dialogService?.ShowDialogAsync(NewVersionAvailableDialogViewModel.Tag, new 
                    DialogParameters { { "release", release },{"enableSkipVersion",true} })!;
            }
        }
        catch (Exception ex)
        {
            /**/
        }
    }
}