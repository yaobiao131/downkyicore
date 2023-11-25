using System.Threading;
using Avalonia.Threading;
using DownKyi.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;

    private bool _messageVisibility = false;
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


    // 登录事件
    private DelegateCommand? _loginCommand;
    public DelegateCommand LoginCommand => _loginCommand ??= new DelegateCommand(ExecuteLogin);

    public DelegateCommand? LoadedCommand { get; }

    public void ExecuteLogin()
    {
        NavigationParam parameter = new NavigationParam
        {
            ViewName = ViewLoginViewModel.Tag,
            ParentViewName = null,
            Parameter = null
        };
        _eventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        _regionManager = regionManager;
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
            var param = new NavigationParameters
            {
                { "Parent", "" },
                { "Parameter", "start" }
            };
            regionManager.RequestNavigate("ContentRegion", ViewIndexViewModel.Tag, param);
        });

        Dispatcher.UIThread.InvokeAsync(() => { LoadedCommand.Execute(); });
    }
}