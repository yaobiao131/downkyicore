using Avalonia.Media.Imaging;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class FriendInfo : BindableBase
{
    protected readonly IEventAggregator EventAggregator;

    public FriendInfo(IEventAggregator eventAggregator)
    {
        this.EventAggregator = eventAggregator;
    }

    public long Mid { get; set; }

    #region 页面属性申明

    private string _header;

    public string Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }

    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _sign;

    public string Sign
    {
        get => _sign;
        set => SetProperty(ref _sign, value);
    }

    #endregion

    #region 命令申明

    // 视频标题点击事件
    private DelegateCommand<object> _userCommand;

    public DelegateCommand<object> UserCommand => _userCommand ??= new DelegateCommand<object>(ExecuteUserCommand);

    /// <summary>
    /// 视频标题点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteUserCommand(object parameter)
    {
        if (parameter is not string tag)
        {
            return;
        }

        NavigateToView.NavigationView(EventAggregator, ViewUserSpaceViewModel.Tag, tag, Mid);
    }

    #endregion
}