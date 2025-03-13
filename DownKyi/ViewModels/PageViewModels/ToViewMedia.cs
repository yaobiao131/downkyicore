using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class ToViewMedia : BindableBase
{
    protected readonly IEventAggregator EventAggregator;

    public ToViewMedia(IEventAggregator eventAggregator)
    {
        EventAggregator = eventAggregator;
    }

    // aid
    public long Aid { get; set; }

    // bvid
    public string Bvid { get; set; }

    // UP主的mid
    public long UpMid { get; set; }

    #region 页面属性申明

    // 是否选中
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    // 封面
    private string _cover;

    public string Cover
    {
        get => _cover;
        set => SetProperty(ref _cover, value);
    }

    // 视频标题
    private string _title;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    // UP主的昵称
    private string _upName;

    public string UpName
    {
        get => _upName;
        set => SetProperty(ref _upName, value);
    }

    // UP主的头像
    private string _upHeader;

    public string UpHeader
    {
        get => _upHeader;
        set => SetProperty(ref _upHeader, value);
    }

    #endregion

    #region 命令申明

    // 视频标题点击事件
    private DelegateCommand<object> _titleCommand;

    public DelegateCommand<object> TitleCommand => _titleCommand ??= new DelegateCommand<object>(ExecuteTitleCommand);

    /// <summary>
    /// 视频标题点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteTitleCommand(object parameter)
    {
        if (parameter is not string tag)
        {
            return;
        }

        NavigateToView.NavigationView(EventAggregator, ViewVideoDetailViewModel.Tag, tag, $"{ParseEntrance.VideoUrl}{Bvid}");
    }

    // UP主头像点击事件
    private DelegateCommand<object> _upCommand;

    public DelegateCommand<object> UpCommand => _upCommand ??= new DelegateCommand<object>(ExecuteUpCommand);

    /// <summary>
    /// UP主头像点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteUpCommand(object parameter)
    {
        if (parameter is not string tag)
        {
            return;
        }

        NavigateToView.NavigateToViewUserSpace(EventAggregator, tag, UpMid);
    }

    #endregion
}