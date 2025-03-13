using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Services;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;

    private ClipboardListener? _clipboardListener;

    private bool _messageVisibility;
    private string? _oldMessage;

    private WindowState _winState;

    public WindowState WinState
    {
        get => _winState;
        set => SetProperty(ref _winState, value);
    }

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

    private void ExecuteClosingCommand()
    {
        if (_clipboardListener == null) return;
        _clipboardListener.Changed -= ClipboardListenerOnChanged;
        _clipboardListener.Dispose();
    }


    public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        #region MyRegion

        _eventAggregator.GetEvent<NavigationEvent>().Subscribe(view =>
        {
            var param = new NavigationParameters
            {
                { "Parent", view.ParentViewName },
                { "Parameter", view.Parameter }
            };
            regionManager.RequestNavigate("ContentRegion", view.ViewName, param);
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
            _clipboardListener = new ClipboardListener(App.Current.MainWindow);
            _clipboardListener.Changed += ClipboardListenerOnChanged;
            var param = new NavigationParameters
            {
                { "Parent", "" },
                { "Parameter", "start" }
            };
            regionManager.RequestNavigate("ContentRegion", ViewIndexViewModel.Tag, param);
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
}