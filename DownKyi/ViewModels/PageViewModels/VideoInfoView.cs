using Avalonia.Media.Imaging;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class VideoInfoView : BindableBase
{
    private string _coverUrl;

    public string CoverUrl
    {
        get => _coverUrl;
        set => SetProperty(ref _coverUrl, value);
    }

    public long UpperMid { get; set; }
    public int TypeId { get; set; }

    private string _title;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _videoZone;

    public string VideoZone
    {
        get => _videoZone;
        set => SetProperty(ref _videoZone, value);
    }

    private string _createTime;

    public string CreateTime
    {
        get => _createTime;
        set => SetProperty(ref _createTime, value);
    }

    private string _playNumber;

    public string PlayNumber
    {
        get => _playNumber;
        set => SetProperty(ref _playNumber, value);
    }

    private string _danmakuNumber;

    public string DanmakuNumber
    {
        get => _danmakuNumber;
        set => SetProperty(ref _danmakuNumber, value);
    }

    private string _likeNumber;

    public string LikeNumber
    {
        get => _likeNumber;
        set => SetProperty(ref _likeNumber, value);
    }

    private string _coinNumber;

    public string CoinNumber
    {
        get => _coinNumber;
        set => SetProperty(ref _coinNumber, value);
    }

    private string _favoriteNumber;

    public string FavoriteNumber
    {
        get => _favoriteNumber;
        set => SetProperty(ref _favoriteNumber, value);
    }

    private string _shareNumber;

    public string ShareNumber
    {
        get => _shareNumber;
        set => SetProperty(ref _shareNumber, value);
    }

    private string _replyNumber;

    public string ReplyNumber
    {
        get => _replyNumber;
        set => SetProperty(ref _replyNumber, value);
    }

    private string _description;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _upName;

    public string UpName
    {
        get => _upName;
        set => SetProperty(ref _upName, value);
    }

    private string _upHeader;

    public string UpHeader
    {
        get => _upHeader;
        set => SetProperty(ref _upHeader, value);
    }
}