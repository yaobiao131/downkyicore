using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class ChannelMedia : BindableBase
{
    protected readonly IEventAggregator eventAggregator;

    public ChannelMedia(IEventAggregator eventAggregator)
    {
        this.eventAggregator = eventAggregator;
    }

    public long Avid { get; set; }
    public string Bvid { get; set; }

    #region 页面属性申明

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private string _cover;

    public string Cover
    {
        get => _cover;
        set => SetProperty(ref _cover, value);
    }

    private string _title;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _duration;

    public string Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    private string _playNumber;

    public string PlayNumber
    {
        get => _playNumber;
        set => SetProperty(ref _playNumber, value);
    }

    private string _createTime;

    public string CreateTime
    {
        get => _createTime;
        set => SetProperty(ref _createTime, value);
    }

    #endregion

    #region 命令申明

    // 视频标题点击事件
    private DelegateCommand<object> _titleCommand;

    public DelegateCommand<object> TitleCommand => _titleCommand ?? (_titleCommand = new DelegateCommand<object>(ExecuteTitleCommand));

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

        NavigateToView.NavigationView(eventAggregator, ViewVideoDetailViewModel.Tag, tag,
            $"{ParseEntrance.VideoUrl}{Bvid}");
        //string url = "https://www.bilibili.com/video/" + tag;
        //System.Diagnostics.Process.Start(url);
    }

    #endregion
}