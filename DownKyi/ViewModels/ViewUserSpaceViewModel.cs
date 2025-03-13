using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.Users;
using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.Utils;
using DownKyi.ViewModels.UserSpace;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class ViewUserSpaceViewModel : ViewModelBase
{
    public const string Tag = "PageUserSpace";

    private readonly IRegionManager _regionManager;

    // mid
    private long mid = -1;

    #region 页面属性申明

    private VectorImage _arrowBack;

    public VectorImage ArrowBack
    {
        get => _arrowBack;
        set => SetProperty(ref _arrowBack, value);
    }

    private bool _loading;

    public bool Loading
    {
        get => _loading;
        set => SetProperty(ref _loading, value);
    }

    private bool _noDataVisibility;

    public bool NoDataVisibility
    {
        get => _noDataVisibility;
        set => SetProperty(ref _noDataVisibility, value);
    }

    private bool _loadingVisibility;

    public bool LoadingVisibility
    {
        get => _loadingVisibility;
        set => SetProperty(ref _loadingVisibility, value);
    }

    private bool _viewVisibility;

    public bool ViewVisibility
    {
        get => _viewVisibility;
        set => SetProperty(ref _viewVisibility, value);
    }

    private bool _contentVisibility;

    public bool ContentVisibility
    {
        get => _contentVisibility;
        set => SetProperty(ref _contentVisibility, value);
    }

    private string _topNavigationBg;

    public string TopNavigationBg
    {
        get => _topNavigationBg;
        set => SetProperty(ref _topNavigationBg, value);
    }

    private string _background;

    public string Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }

    private string _header;

    public string Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }

    private string _userName;

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private Bitmap _sex;

    public Bitmap Sex
    {
        get => _sex;
        set => SetProperty(ref _sex, value);
    }

    private Bitmap _level;

    public Bitmap Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    private bool _vipTypeVisibility;

    public bool VipTypeVisibility
    {
        get => _vipTypeVisibility;
        set => SetProperty(ref _vipTypeVisibility, value);
    }

    private string _vipType;

    public string VipType
    {
        get => _vipType;
        set => SetProperty(ref _vipType, value);
    }

    private string _sign;

    public string Sign
    {
        get => _sign;
        set => SetProperty(ref _sign, value);
    }

    private string _isFollowed;

    public string IsFollowed
    {
        get => _isFollowed;
        set => SetProperty(ref _isFollowed, value);
    }

    private ObservableCollection<TabLeftBanner> _tabLeftBanners;

    public ObservableCollection<TabLeftBanner> TabLeftBanners
    {
        get => _tabLeftBanners;
        set => SetProperty(ref _tabLeftBanners, value);
    }

    private ObservableCollection<TabRightBanner> _tabRightBanners;

    public ObservableCollection<TabRightBanner> TabRightBanners
    {
        get => _tabRightBanners;
        set => SetProperty(ref _tabRightBanners, value);
    }

    private int _selectedRightBanner;

    public int SelectedRightBanner
    {
        get => _selectedRightBanner;
        set => SetProperty(ref _selectedRightBanner, value);
    }

    #endregion

    public ViewUserSpaceViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(
        eventAggregator)
    {
        _regionManager = regionManager;

        #region 属性初始化

        // 返回按钮
        ArrowBack = NavigationIcon.Instance().ArrowBack;
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        // 初始化loading
        Loading = true;

        TopNavigationBg = "#00FFFFFF"; // 透明

        TabLeftBanners = new ObservableCollection<TabLeftBanner>();
        TabRightBanners = new ObservableCollection<TabRightBanner>();

        #endregion
    }

    #region 命令申明

    // 返回事件
    private DelegateCommand? _backSpaceCommand;

    public DelegateCommand BackSpaceCommand => _backSpaceCommand ??= new DelegateCommand(ExecuteBackSpace);

    /// <summary>
    /// 返回事件
    /// </summary>
    private void ExecuteBackSpace()
    {
        var parameter = new NavigationParam
        {
            ViewName = ParentView,
            ParentViewName = null,
            Parameter = null
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    // 左侧tab点击事件
    private DelegateCommand<object>? _tabLeftBannersCommand;

    public DelegateCommand<object> TabLeftBannersCommand => _tabLeftBannersCommand ??= new DelegateCommand<object>(ExecuteTabLeftBannersCommand);

    /// <summary>
    /// 左侧tab点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteTabLeftBannersCommand(object parameter)
    {
        if (parameter is not TabLeftBanner banner)
        {
            return;
        }

        var param = new NavigationParameters
        {
            { "object", banner.Object },
            { "mid", mid },
        };

        switch (banner.Id)
        {
            case 0: // 投稿
                _regionManager.RequestNavigate("UserSpaceContentRegion", ViewArchiveViewModel.Tag, param);
                break;
            case 1: // 频道（弃用）
                _regionManager.RequestNavigate("UserSpaceContentRegion", ViewChannelViewModel.Tag, param);
                break;
            case 2: // 合集和列表
                _regionManager.RequestNavigate("UserSpaceContentRegion", UserSpace.ViewSeasonsSeriesViewModel.Tag,
                    param);
                break;
        }
    }

    // 右侧tab点击事件
    private DelegateCommand<object>? _tabRightBannersCommand;

    public DelegateCommand<object> TabRightBannersCommand => _tabRightBannersCommand ??= new DelegateCommand<object>(ExecuteTabRightBannersCommand);

    /// <summary>
    /// 右侧tab点击事件
    /// </summary>
    private void ExecuteTabRightBannersCommand(object parameter)
    {
        if (!(parameter is TabRightBanner banner))
        {
            return;
        }

        var data = new Dictionary<string, object>
        {
            { "mid", mid },
            { "friendId", 0 }
        };

        var parentViewName = ParentView == ViewFriendsViewModel.Tag ? ViewIndexViewModel.Tag : Tag;

        switch (banner.Id)
        {
            case 0:
                data["friendId"] = 0;
                NavigateToView.NavigationView(EventAggregator, ViewFriendsViewModel.Tag, parentViewName, data);
                break;
            case 1:
                data["friendId"] = 1;
                NavigateToView.NavigationView(EventAggregator, ViewFriendsViewModel.Tag, parentViewName, data);
                break;
        }

        SelectedRightBanner = -1;
    }

    #endregion

    /// <summary>
    /// 初始化页面
    /// </summary>
    private void InitView()
    {
        TopNavigationBg = "#00FFFFFF"; // 透明
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");
        Background = null;

        Header = null;
        UserName = "";
        Sex = null;
        Level = null;
        VipTypeVisibility = false;
        VipType = "";
        Sign = "";

        TabLeftBanners.Clear();
        TabRightBanners.Clear();

        SelectedRightBanner = -1;

        // 将内容置空，使其不指向任何页面
        _regionManager.RequestNavigate("UserSpaceContentRegion", "");

        ContentVisibility = false;
        ViewVisibility = false;
        LoadingVisibility = true;
        NoDataVisibility = false;
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    private async void UpdateSpaceInfo()
    {
        var isNoData = true;
        string? toutuUri = null;
        string? headerUri = null;
        Uri? sexUri = null;
        Uri? levelUri = null;

        await Task.Run(() =>
        {
            // 背景图片
            var spaceSettings = Core.BiliApi.Users.UserSpace.GetSpaceSettings(mid);
            toutuUri = spaceSettings != null ? $"https://i0.hdslb.com/{spaceSettings.Toutu.Limg}" : "avares://DownKyi/Resources/backgound/9-绿荫秘境.png";

            // 用户信息
            var userInfo = UserInfo.GetUserInfoForSpace(mid);
            if (userInfo != null)
            {
                isNoData = false;

                // 头像
                headerUri = userInfo.Face;
                // 用户名
                UserName = userInfo.Name;
                sexUri = userInfo.Sex switch
                {
                    // 性别
                    "男" => new Uri("avares://DownKyi/Resources/sex/male.png"),
                    "女" => new Uri("avares://DownKyi/Resources/sex/female.png"),
                    _ => sexUri
                };

                // 显示vip信息
                if (userInfo.Vip?.Label?.Text is null or "")
                {
                    VipTypeVisibility = false;
                }
                else
                {
                    VipTypeVisibility = true;
                    VipType = userInfo.Vip.Label.Text;
                }

                // 等级
                levelUri = new Uri($"avares://DownKyi/Resources/level/lv{userInfo.Level}.png");
                // 签名
                Sign = userInfo.Sign;

                // 是否关注此UP
                PropertyChangeAsync(() =>
                {
                    IsFollowed = userInfo.IsFollowed
                        ? DictionaryResource.GetString("Followed")
                        : DictionaryResource.GetString("NotFollowed");
                });
            }
            else
            {
                // 没有数据
                isNoData = true;
            }
        });

        // 是否获取到数据
        if (isNoData)
        {
            TopNavigationBg = "#00FFFFFF"; // 透明
            ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");
            Background = null;

            ViewVisibility = false;
            LoadingVisibility = false;
            NoDataVisibility = true;
            return;
        }
        else
        {
            // 头像
            Header = headerUri;
            // 性别
            Sex = sexUri == null ? null : ImageHelper.LoadFromResource(sexUri);
            // 等级
            Level = levelUri == null ? null : ImageHelper.LoadFromResource(levelUri);

            ArrowBack.Fill = DictionaryResource.GetColor("ColorText");
            TopNavigationBg = DictionaryResource.GetColor("ColorMask100");
            Background = toutuUri;

            ViewVisibility = true;
            LoadingVisibility = false;
            NoDataVisibility = false;
        }

        ContentVisibility = true;

        // 投稿视频
        List<SpacePublicationListTypeVideoZone>? publicationTypes = null;
        await Task.Run(() => { publicationTypes = Core.BiliApi.Users.UserSpace.GetPublicationType(mid); });
        if (publicationTypes is { Count: > 0 })
        {
            TabLeftBanners.Add(new TabLeftBanner
            {
                Object = publicationTypes,
                Id = 0,
                Icon = NormalIcon.Instance().VideoUp,
                IconColor = "#FF02B5DA",
                Title = DictionaryResource.GetString("Publication"),
                IsSelected = true
            });
        }

        // 频道
        //List<SpaceChannelList> channelList = null;
        //await Task.Run(() =>
        //{
        //    channelList = Core.BiliApi.Users.UserSpace.GetChannelList(mid);
        //});
        //if (channelList != null && channelList.Count > 0)
        //{
        //    TabLeftBanners.Add(new TabLeftBanner
        //    {
        //        Object = channelList,
        //        Id = 1,
        //        Icon = NormalIcon.Instance().Channel,
        //        IconColor = "#FF23C9ED",
        //        Title = DictionaryResource.GetString("Channel")
        //    });
        //}

        // 合集和列表
        SpaceSeasonsSeries? seasonsSeries = null;
        await Task.Run(() => { seasonsSeries = Core.BiliApi.Users.UserSpace.GetSeasonsSeries(mid, 1, 20); });
        if (seasonsSeries is { Page.Total: > 0 })
        {
            TabLeftBanners.Add(new TabLeftBanner
            {
                Object = seasonsSeries,
                Id = 2,
                Icon = NormalIcon.Instance().Channel,
                IconColor = "#FF23C9ED",
                Title = DictionaryResource.GetString("SeasonsSeries")
            });
        }

        // 收藏夹
        // 订阅

        // 关系状态数
        UserRelationStat? relationStat = null;
        await Task.Run(() => { relationStat = UserStatus.GetUserRelationStat(mid); });
        if (relationStat != null)
        {
            TabRightBanners.Add(new TabRightBanner
            {
                Id = 0,
                IsEnabled = true,
                LabelColor = DictionaryResource.GetColor("ColorPrimary"),
                CountColor = DictionaryResource.GetColor("ColorPrimary"),
                Label = DictionaryResource.GetString("FollowingCount"),
                Count = Format.FormatNumber(relationStat.Following)
            });
            TabRightBanners.Add(new TabRightBanner
            {
                Id = 1,
                IsEnabled = true,
                LabelColor = DictionaryResource.GetColor("ColorPrimary"),
                CountColor = DictionaryResource.GetColor("ColorPrimary"),
                Label = DictionaryResource.GetString("FollowerCount"),
                Count = Format.FormatNumber(relationStat.Follower)
            });
        }

        // UP主状态数，需要任意用户登录，否则不会返回任何数据
        UpStat? upStat = null;
        await Task.Run(() => { upStat = UserStatus.GetUpStat(mid); });
        if (upStat is { Archive: not null, Article: not null })
        {
            TabRightBanners.Add(new TabRightBanner
            {
                Id = 2,
                IsEnabled = false,
                LabelColor = DictionaryResource.GetColor("ColorTextGrey"),
                CountColor = DictionaryResource.GetColor("ColorTextDark"),
                Label = DictionaryResource.GetString("LikesCount"),
                Count = Format.FormatNumber(upStat.Likes)
            });

            long archiveView = 0;
            if (upStat?.Archive != null)
            {
                archiveView = upStat.Archive.View;
            }

            TabRightBanners.Add(new TabRightBanner
            {
                Id = 3,
                IsEnabled = false,
                LabelColor = DictionaryResource.GetColor("ColorTextGrey"),
                CountColor = DictionaryResource.GetColor("ColorTextDark"),
                Label = DictionaryResource.GetString("ArchiveViewCount"),
                Count = Format.FormatNumber(archiveView)
            });

            long articleView = 0;
            if (upStat?.Article != null)
            {
                articleView = upStat.Article.View;
            }

            TabRightBanners.Add(new TabRightBanner
            {
                Id = 4,
                IsEnabled = false,
                LabelColor = DictionaryResource.GetColor("ColorTextGrey"),
                CountColor = DictionaryResource.GetColor("ColorTextDark"),
                Label = DictionaryResource.GetString("ArticleViewCount"),
                Count = Format.FormatNumber(articleView)
            });
        }
    }

    /// <summary>
    /// 接收mid参数
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        // 根据传入参数不同执行不同任务
        var parameter = navigationContext.Parameters.GetValue<long>("Parameter");
        if (parameter == 0)
        {
            return;
        }

        mid = parameter;

        InitView();
        UpdateSpaceInfo();
    }
}