using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Images;
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
        set
        {
            ResizeIcon = value == WindowState.Maximized ? SystemIcon.Instance().Restore : SystemIcon.Instance().Maximize;

            SetProperty(ref _winState, value);
        }
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

    private VectorImage _minimizeIcon;

    public VectorImage MinimizeIcon
    {
        get => _minimizeIcon;
        set => SetProperty(ref _minimizeIcon, value);
    }

    private VectorImage _resizeIcon;

    public VectorImage ResizeIcon
    {
        get => _resizeIcon;
        set => SetProperty(ref _resizeIcon, value);
    }


    private VectorImage _closeIcon;

    public VectorImage CloseIcon
    {
        get => _closeIcon;
        set => SetProperty(ref _closeIcon, value);
    }

    private bool _isOsx;

    public bool IsOsx
    {
        get => _isOsx;
        set => SetProperty(ref _isOsx, value);
    }

    private Thickness? _titleMargin;

    public Thickness? TitleMargin
    {
        get => _titleMargin;
        set => SetProperty(ref _titleMargin, value);
    }

    private DelegateCommand? LoadedCommand { get; }
    public DelegateCommand<PointerPressedEventArgs> DragMoveCommand { get; private set; }

    private DelegateCommand? _closeCommand;
    public DelegateCommand CloseCommand => _closeCommand ??= new DelegateCommand(ExecuteCloseCommand);

    private void ExecuteCloseCommand()
    {
        if (_clipboardListener != null)
        {
            _clipboardListener.Changed -= ClipboardListenerOnChanged;
            _clipboardListener.Dispose();
        }

        App.Current.MainWindow.Close();
    }

    private DelegateCommand? _resizeCommand;
    public DelegateCommand ResizeCommand => _resizeCommand ??= new DelegateCommand(ExecuteResizeCommand);

    private void ExecuteResizeCommand()
    {
        WinState = WinState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private DelegateCommand? _minimizeCommand;
    public DelegateCommand MinimizeCommand => _minimizeCommand ??= new DelegateCommand(ExecuteMinimizeCommand);

    private void ExecuteMinimizeCommand()
    {
        App.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private DelegateCommand? _closingCommand;

    public DelegateCommand ClosingCommand => _closingCommand ??= new DelegateCommand(ExecuteClosingCommand);

    private void ExecuteClosingCommand()
    {
        if (_clipboardListener == null) return;
        _clipboardListener.Changed -= ClipboardListenerOnChanged;
        _clipboardListener.Dispose();
    }


    public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        #region 属性初始化

        WinState = WindowState.Normal;

        MinimizeIcon = SystemIcon.Instance().Minimize;
        ResizeIcon = SystemIcon.Instance().Maximize;
        CloseIcon = SystemIcon.Instance().Close;
        if (OperatingSystem.IsMacOS())
        {
            TitleMargin = new Thickness(70, 0, 10, 0);
            IsOsx = true;
        }
        else
        {
            TitleMargin = new Thickness(10, 0);
            IsOsx = false;
        }

        #endregion


        #region 订阅

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

        var times = 0;

        DragMoveCommand = new DelegateCommand<PointerPressedEventArgs>(e =>
        {
            Window mainWindow = App.Current.MainWindow;
            // caption 双击事件
            times += 1;
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 300)
            };
            timer.Tick += (_, _) =>
            {
                timer.IsEnabled = false;
                times = 0;
            };
            timer.IsEnabled = true;

            if (times % 2 == 0)
            {
                timer.IsEnabled = false;
                times = 0;
                WinState = WinState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }

            // caption 拖动事件
            try
            {
                mainWindow.BeginMoveDrag(e);
            }
            catch
            {
                // ignored
            }
        });


        Dispatcher.UIThread.InvokeAsync(() => { LoadedCommand.Execute(); });
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

        var isListenClipboard = SettingsManager.GetInstance().IsListenClipboard();
        if (isListenClipboard != AllowStatus.YES)
        {
            return;
        }

        var searchService = new SearchService();
        Dispatcher.UIThread.InvokeAsync(() => { searchService.BiliInput(obj + AppConstant.ClipboardId, ViewIndexViewModel.Tag, _eventAggregator); });
    }

    #endregion
}