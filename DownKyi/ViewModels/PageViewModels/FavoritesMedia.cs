using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class FavoritesMedia : BindableBase
{
    protected readonly IEventAggregator EventAggregator;

    public FavoritesMedia(IEventAggregator eventAggregator)
    {
        EventAggregator = eventAggregator;
    }

    public long Avid { get; set; }
    public string Bvid { get; set; }
    public long UpMid { get; set; }

    #region 页面属性申明

    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    private int order;

    public int Order
    {
        get => order;
        set => SetProperty(ref order, value);
    }

    private string cover;

    public string Cover
    {
        get => cover;
        set => SetProperty(ref cover, value);
    }

    private string title;

    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    private string playNumber;

    public string PlayNumber
    {
        get => playNumber;
        set => SetProperty(ref playNumber, value);
    }

    private string danmakuNumber;

    public string DanmakuNumber
    {
        get => danmakuNumber;
        set => SetProperty(ref danmakuNumber, value);
    }

    private string favoriteNumber;

    public string FavoriteNumber
    {
        get => favoriteNumber;
        set => SetProperty(ref favoriteNumber, value);
    }

    private string duration;

    public string Duration
    {
        get => duration;
        set => SetProperty(ref duration, value);
    }

    private string upName;

    public string UpName
    {
        get => upName;
        set => SetProperty(ref upName, value);
    }

    private string createTime;

    public string CreateTime
    {
        get => createTime;
        set => SetProperty(ref createTime, value);
    }

    private string favTime;

    public string FavTime
    {
        get => favTime;
        set => SetProperty(ref favTime, value);
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

        NavigateToView.NavigationView(EventAggregator, ViewVideoDetailViewModel.Tag, tag,
            $"{ParseEntrance.VideoUrl}{Bvid}");
    }

    // 视频的UP主点击事件
    private DelegateCommand<object> _videoUpperCommand;

    public DelegateCommand<object> VideoUpperCommand => _videoUpperCommand ??= new DelegateCommand<object>(ExecuteVideoUpperCommand);

    /// <summary>
    /// 视频的UP主点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoUpperCommand(object parameter)
    {
        if (parameter is not string tag)
        {
            return;
        }

        NavigateToView.NavigateToViewUserSpace(EventAggregator, tag, UpMid);
    }

    #endregion
}