using Avalonia.Media.Imaging;
using DownKyi.Images;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class HistoryMedia : BindableBase
{
    protected readonly IEventAggregator EventAggregator;

    public HistoryMedia(IEventAggregator eventAggregator)
    {
        EventAggregator = eventAggregator;
    }

    // bvid
    public string Bvid { get; set; }

    // 播放url
    public string Url { get; set; }

    // UP主的mid
    public long UpMid { get; set; }

    // 类型
    public string Business { get; set; }

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

    // 分P的标题
    private string _subTitle;

    public string SubTitle
    {
        get => _subTitle;
        set => SetProperty(ref _subTitle, value);
    }

    // 时长
    private long _duration;

    public long Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    // tag标签
    private string _tagName;

    public string TagName
    {
        get => _tagName;
        set => SetProperty(ref _tagName, value);
    }

    // new_desc 剧集或分P描述
    private string _partdesc;

    public string Partdesc
    {
        get => _partdesc;
        set => SetProperty(ref _partdesc, value);
    }

    // 观看进度
    private string _progress;

    public string Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    // 观看平台
    private VectorImage _platform;

    public VectorImage Platform
    {
        get => _platform;
        set => SetProperty(ref _platform, value);
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

    // 是否显示Partdesc
    private bool _partdescVisibility;

    public bool PartdescVisibility
    {
        get => _partdescVisibility;
        set => SetProperty(ref _partdescVisibility, value);
    }

    // 是否显示UP主信息和分区信息
    private bool _upAndTagVisibility;

    public bool UpAndTagVisibility
    {
        get => _upAndTagVisibility;
        set => SetProperty(ref _upAndTagVisibility, value);
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

        NavigateToView.NavigationView(EventAggregator, ViewVideoDetailViewModel.Tag, tag, Url);
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