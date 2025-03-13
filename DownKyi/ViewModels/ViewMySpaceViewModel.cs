using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.BiliApi.Users;
using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.Storage;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.Utils;
using DownKyi.ViewModels.PageViewModels;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class ViewMySpaceViewModel : ViewModelBase
{
    public const string Tag = "PageMySpace";

    private CancellationTokenSource _tokenSource;

    // mid
    private long _mid = -1;

    #region 页面属性申明

    private VectorImage _arrowBack;

    public VectorImage ArrowBack
    {
        get => _arrowBack;
        set => SetProperty(ref _arrowBack, value);
    }

    private VectorImage _logout;

    public VectorImage Logout
    {
        get => _logout;
        set => SetProperty(ref _logout, value);
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

    private VectorImage _coinIcon;

    public VectorImage CoinIcon
    {
        get => _coinIcon;
        set => SetProperty(ref _coinIcon, value);
    }

    private string _coin;

    public string Coin
    {
        get => _coin;
        set => SetProperty(ref _coin, value);
    }

    private VectorImage _moneyIcon;

    public VectorImage MoneyIcon
    {
        get => _moneyIcon;
        set => SetProperty(ref _moneyIcon, value);
    }

    private string _money;

    public string Money
    {
        get => _money;
        set => SetProperty(ref _money, value);
    }

    private VectorImage _bindingEmail;

    public VectorImage BindingEmail
    {
        get => _bindingEmail;
        set => SetProperty(ref _bindingEmail, value);
    }

    private bool _bindingEmailVisibility;

    public bool BindingEmailVisibility
    {
        get => _bindingEmailVisibility;
        set => SetProperty(ref _bindingEmailVisibility, value);
    }

    private VectorImage _bindingPhone;

    public VectorImage BindingPhone
    {
        get => _bindingPhone;
        set => SetProperty(ref _bindingPhone, value);
    }

    private bool _bindingPhoneVisibility;

    public bool BindingPhoneVisibility
    {
        get => _bindingPhoneVisibility;
        set => SetProperty(ref _bindingPhoneVisibility, value);
    }

    private string _levelText;

    public string LevelText
    {
        get => _levelText;
        set => SetProperty(ref _levelText, value);
    }

    private string _currentExp;

    public string CurrentExp
    {
        get => _currentExp;
        set => SetProperty(ref _currentExp, value);
    }

    private int _expProgress;

    public int ExpProgress
    {
        get => _expProgress;
        set => SetProperty(ref _expProgress, value);
    }

    private int _maxExp;

    public int MaxExp
    {
        get => _maxExp;
        set => SetProperty(ref _maxExp, value);
    }

    private ObservableCollection<SpaceItem> _statusList;

    public ObservableCollection<SpaceItem> StatusList
    {
        get => _statusList;
        set => SetProperty(ref _statusList, value);
    }

    private ObservableCollection<SpaceItem> _packageList;

    public ObservableCollection<SpaceItem> PackageList
    {
        get => _packageList;
        set => SetProperty(ref _packageList, value);
    }

    private int _selectedStatus = -1;

    public int SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    private int _selectedPackage = -1;

    public int SelectedPackage
    {
        get => _selectedPackage;
        set => SetProperty(ref _selectedPackage, value);
    }

    #endregion

    public ViewMySpaceViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        // 返回按钮
        ArrowBack = NavigationIcon.Instance().ArrowBack;
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        // 退出登录按钮
        Logout = NavigationIcon.Instance().Logout;
        Logout.Fill = DictionaryResource.GetColor("ColorTextDark");

        // 初始化loading
        Loading = true;

        TopNavigationBg = "#00FFFFFF"; // 透明

        // B站图标
        CoinIcon = NormalIcon.Instance().CoinIcon;
        CoinIcon.Fill = DictionaryResource.GetColor("ColorPrimary");
        MoneyIcon = NormalIcon.Instance().MoneyIcon;
        MoneyIcon.Fill = DictionaryResource.GetColor("ColorMoney");
        BindingEmail = NormalIcon.Instance().BindingEmail;
        BindingEmail.Fill = DictionaryResource.GetColor("ColorPrimary");
        BindingPhone = NormalIcon.Instance().BindingPhone;
        BindingPhone.Fill = DictionaryResource.GetColor("ColorPrimary");

        StatusList = new ObservableCollection<SpaceItem>();
        PackageList = new ObservableCollection<SpaceItem>();

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
        // 结束任务
        _tokenSource?.Cancel();

        var parameter = new NavigationParam
        {
            ViewName = ParentView,
            ParentViewName = null,
            Parameter = null
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    // 退出登录事件
    private DelegateCommand? _logoutCommand;

    public DelegateCommand LogoutCommand => _logoutCommand ??= new DelegateCommand(ExecuteLogoutCommand);

    /// <summary>
    /// 退出登录事件
    /// </summary>
    private void ExecuteLogoutCommand()
    {
        // 注销
        LoginHelper.Logout();

        // 返回上一页
        var parameter = new NavigationParam
        {
            ViewName = ParentView,
            ParentViewName = null,
            Parameter = "logout"
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    // 页面选择事件
    private DelegateCommand? _statusListCommand;

    public DelegateCommand StatusListCommand => _statusListCommand ??= new DelegateCommand(ExecuteStatusListCommand);

    /// <summary>
    /// 页面选择事件
    /// </summary>
    private void ExecuteStatusListCommand()
    {
        if (SelectedStatus == -1)
        {
            return;
        }

        var data = new Dictionary<string, object>
        {
            { "mid", _mid },
            { "friendId", 0 }
        };

        switch (SelectedStatus)
        {
            case 0:
                data["friendId"] = 0;
                NavigateToView.NavigationView(EventAggregator, ViewFriendsViewModel.Tag, Tag, data);
                break;
            case 1:
                data["friendId"] = 0;
                NavigateToView.NavigationView(EventAggregator, ViewFriendsViewModel.Tag, Tag, data);
                break;
            case 2:
                data["friendId"] = 1;
                NavigateToView.NavigationView(EventAggregator, ViewFriendsViewModel.Tag, Tag, data);
                break;
            default:
                break;
        }

        SelectedStatus = -1;
    }

    // 页面选择事件
    private DelegateCommand _packageListCommand;

    public DelegateCommand PackageListCommand => _packageListCommand ??= new DelegateCommand(ExecutePackageListCommand);

    /// <summary>
    /// 页面选择事件
    /// </summary>
    private void ExecutePackageListCommand()
    {
        if (SelectedPackage == -1)
        {
            return;
        }

        switch (SelectedPackage)
        {
            case 0:
                NavigateToView.NavigationView(EventAggregator, ViewMyFavoritesViewModel.Tag, Tag, _mid);
                break;
            case 1:
                NavigateToView.NavigationView(EventAggregator, ViewMyBangumiFollowViewModel.Tag, Tag, _mid);
                break;
            case 2:
                NavigateToView.NavigationView(EventAggregator, ViewMyToViewVideoViewModel.Tag, Tag, _mid);
                break;
            case 3:
                NavigateToView.NavigationView(EventAggregator, ViewMyHistoryViewModel.Tag, Tag, _mid);
                break;
            default:
                break;
        }

        SelectedPackage = -1;
    }

    #endregion

    /// <summary>
    /// 初始化页面
    /// </summary>
    private void InitView()
    {
        TopNavigationBg = "#00FFFFFF"; // 透明
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");
        Logout.Fill = DictionaryResource.GetColor("ColorTextDark");
        Background = null;

        Header = null;
        UserName = "";
        Sex = null;
        Level = null;
        VipTypeVisibility = false;
        VipType = "";
        Sign = "";

        Coin = "0.0";
        Money = "0.0";

        LevelText = "";
        CurrentExp = "--/--";

        StatusList.Clear();
        StatusList.Add(new SpaceItem { IsEnabled = true, Title = DictionaryResource.GetString("Following"), Subtitle = "--" });
        StatusList.Add(new SpaceItem { IsEnabled = true, Title = DictionaryResource.GetString("Whisper"), Subtitle = "--" });
        StatusList.Add(new SpaceItem { IsEnabled = true, Title = DictionaryResource.GetString("Follower"), Subtitle = "--" });
        StatusList.Add(new SpaceItem { IsEnabled = false, Title = DictionaryResource.GetString("Black"), Subtitle = "--" });
        StatusList.Add(new SpaceItem { IsEnabled = false, Title = DictionaryResource.GetString("Moral"), Subtitle = "--" });
        StatusList.Add(new SpaceItem { IsEnabled = false, Title = DictionaryResource.GetString("Silence"), Subtitle = "N/A" });

        PackageList.Clear();
        PackageList.Add(new SpaceItem
        {
            IsEnabled = true, Image = NormalIcon.Instance().FavoriteOutline,
            Title = DictionaryResource.GetString("Favorites")
        });
        PackageList.Add(new SpaceItem
        {
            IsEnabled = true, Image = NormalIcon.Instance().Subscription,
            Title = DictionaryResource.GetString("Subscription")
        });
        PackageList.Add(new SpaceItem
        {
            IsEnabled = true, Image = NormalIcon.Instance().ToView, Title = DictionaryResource.GetString("ToView")
        });
        PackageList.Add(new SpaceItem
        {
            IsEnabled = true, Image = NormalIcon.Instance().History, Title = DictionaryResource.GetString("History")
        });
        NormalIcon.Instance().FavoriteOutline.Fill = DictionaryResource.GetColor("ColorPrimary");
        NormalIcon.Instance().Subscription.Fill = DictionaryResource.GetColor("ColorPrimary");
        NormalIcon.Instance().ToView.Fill = DictionaryResource.GetColor("ColorPrimary");
        NormalIcon.Instance().History.Fill = DictionaryResource.GetColor("ColorPrimary");

        SelectedStatus = -1;
        SelectedPackage = -1;

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
        var isCancel = false;
        var isNoData = true;
        string? toutuUri = null;
        string headerUri = null;
        Uri? sexUri = null;
        Uri levelUri = null;

        await Task.Run(() =>
        {
            var cancellationToken = _tokenSource.Token;

            // 背景图片
            var spaceSettings = Core.BiliApi.Users.UserSpace.GetSpaceSettings(_mid);
            toutuUri = spaceSettings != null ? $"https://i0.hdslb.com/{spaceSettings.Toutu.Limg}" : "avares://DownKyi/Resources/backgound/9-绿荫秘境.png";

            // 我的用户信息
            var myInfo = UserInfo.GetMyInfo();
            if (myInfo != null)
            {
                isNoData = false;

                // 头像
                headerUri = myInfo.Face;
                // 用户名
                UserName = myInfo.Name;
                // 性别
                if (myInfo.Sex == "男")
                {
                    sexUri = new Uri("avares://DownKyi/Resources/sex/male.png");
                }
                else if (myInfo.Sex == "女")
                {
                    sexUri = new Uri("avares://DownKyi/Resources/sex/female.png");
                }

                // 显示vip信息
                if (myInfo.Vip.Label.Text == null || myInfo.Vip.Label.Text == "")
                {
                    VipTypeVisibility = false;
                }
                else
                {
                    VipTypeVisibility = true;
                    VipType = myInfo.Vip.Label.Text;
                }

                // 等级
                levelUri = new Uri($"avares://DownKyi/Resources/level/lv{myInfo.Level}.png");
                // 签名
                Sign = myInfo.Sign;
                // 绑定邮箱&手机
                if (myInfo.EmailStatus == 0)
                {
                    BindingEmailVisibility = false;
                }

                if (myInfo.TelStatus == 0)
                {
                    BindingPhoneVisibility = false;
                }

                // 等级
                PropertyChangeAsync(() => { LevelText = $"{DictionaryResource.GetString("Level")}{myInfo.LevelExp.CurrentLevel}"; });
                CurrentExp = myInfo.LevelExp.NextExp == -1 ? $"{myInfo.LevelExp.CurrentExp}/--" : $"{myInfo.LevelExp.CurrentExp}/{myInfo.LevelExp.NextExp}";

                // 经验
                MaxExp = myInfo.LevelExp.NextExp;
                ExpProgress = myInfo.LevelExp.CurrentExp;
                // 节操值
                StatusList[4].Subtitle = myInfo.Moral.ToString();
                // 封禁状态                   
                if (myInfo.Silence == 0)
                {
                    PropertyChangeAsync(() => { StatusList[5].Subtitle = DictionaryResource.GetString("Normal"); });
                }
                else if (myInfo.Silence == 1)
                {
                    PropertyChangeAsync(() => { StatusList[5].Subtitle = DictionaryResource.GetString("Ban"); });
                }
            }
            else
            {
                // 没有数据
                isNoData = true;
            }

            // 判断是否该结束线程
            if (cancellationToken.IsCancellationRequested)
            {
                isCancel = true;
            }
        }, (_tokenSource = new CancellationTokenSource()).Token);

        // 是否该结束线程
        if (isCancel)
        {
            return;
        }

        // 是否获取到数据
        if (isNoData)
        {
            TopNavigationBg = "#00FFFFFF"; // 透明
            ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");
            Logout.Fill = DictionaryResource.GetColor("ColorTextDark");
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
            Logout.Fill = DictionaryResource.GetColor("ColorText");
            TopNavigationBg = DictionaryResource.GetColor("ColorMask100");
            Background = toutuUri ?? "";

            ViewVisibility = true;
            LoadingVisibility = false;
            NoDataVisibility = false;
        }

        await Task.Run(() =>
        {
            // 导航栏信息
            var navData = UserInfo.GetUserInfoForNavigation();
            if (navData is { IsLogin: false }) return;
            if (navData != null)
            {
                ContentVisibility = true;

                // 硬币
                Coin = navData.Money == 0 ? "0.0" : navData.Money.ToString("F1");
                // B币
                Money = navData.Wallet.BcoinBalance == 0 ? "0.0" : navData.Wallet.BcoinBalance.ToString("F1");
            }

            //用户的关系状态数
            var relationStat = UserStatus.GetUserRelationStat(_mid);
            if (relationStat == null) return;
            // 关注数
            StatusList[0].Subtitle = relationStat.Following.ToString();
            // 悄悄关注数
            StatusList[1].Subtitle = relationStat.Whisper.ToString();
            // 粉丝数
            StatusList[2].Subtitle = relationStat.Follower.ToString();
            // 黑名单数
            StatusList[3].Subtitle = relationStat.Black.ToString();
        });
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

        _mid = parameter;

        InitView();
        UpdateSpaceInfo();
    }
}