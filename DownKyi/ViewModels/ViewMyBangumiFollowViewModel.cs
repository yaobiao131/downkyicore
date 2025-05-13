using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Storage;
using DownKyi.CustomControl;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.Services;
using DownKyi.Services.Download;
using DownKyi.Utils;
using DownKyi.ViewModels.PageViewModels;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels;

public class ViewMyBangumiFollowViewModel : ViewModelBase
{
    public const string Tag = "PageMyBangumiFollow";

    private CancellationTokenSource _tokenSource;

    private long _mid = -1;

    // 每页视频数量，暂时在此写死，以后在设置中增加选项
    private const int VideoNumberInPage = 15;

    #region 页面属性申明

    private string _pageName = Tag;

    public string PageName
    {
        get => _pageName;
        set => SetProperty(ref _pageName, value);
    }

    private VectorImage _arrowBack;

    public VectorImage ArrowBack
    {
        get => _arrowBack;
        set => SetProperty(ref _arrowBack, value);
    }

    private VectorImage _downloadManage;

    public VectorImage DownloadManage
    {
        get => _downloadManage;
        set => SetProperty(ref _downloadManage, value);
    }

    private ObservableCollection<TabHeader> _tabHeaders;

    public ObservableCollection<TabHeader> TabHeaders
    {
        get => _tabHeaders;
        set => SetProperty(ref _tabHeaders, value);
    }

    private int _selectTabId;

    public int SelectTabId
    {
        get => _selectTabId;
        set => SetProperty(ref _selectTabId, value);
    }

    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    private bool _contentVisibility;

    public bool ContentVisibility
    {
        get => _contentVisibility;
        set => SetProperty(ref _contentVisibility, value);
    }

    private CustomPagerViewModel _pager;

    public CustomPagerViewModel Pager
    {
        get => _pager;
        set => SetProperty(ref _pager, value);
    }

    private ObservableCollection<BangumiFollowMedia> _medias;

    public ObservableCollection<BangumiFollowMedia> Medias
    {
        get => _medias;
        set => SetProperty(ref _medias, value);
    }

    private bool _isSelectAll;

    public bool IsSelectAll
    {
        get => _isSelectAll;
        set => SetProperty(ref _isSelectAll, value);
    }

    private bool _loading;

    public bool Loading
    {
        get => _loading;
        set => SetProperty(ref _loading, value);
    }

    private bool _loadingVisibility;

    public bool LoadingVisibility
    {
        get => _loadingVisibility;
        set => SetProperty(ref _loadingVisibility, value);
    }

    private bool _noDataVisibility;

    public bool NoDataVisibility
    {
        get => _noDataVisibility;
        set => SetProperty(ref _noDataVisibility, value);
    }

    #endregion

    public ViewMyBangumiFollowViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(
        eventAggregator)
    {
        DialogService = dialogService;

        #region 属性初始化

        // 初始化loading
        Loading = true;
        LoadingVisibility = false;
        NoDataVisibility = false;

        ArrowBack = NavigationIcon.Instance().ArrowBack;
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        // 下载管理按钮
        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");

        TabHeaders = new ObservableCollection<TabHeader>
        {
            new() { Id = (long)BangumiType.ANIME, Title = DictionaryResource.GetString("FollowAnime") },
            new() { Id = (long)BangumiType.EPISODE, Title = DictionaryResource.GetString("FollowMovie") }
        };

        Medias = new ObservableCollection<BangumiFollowMedia>();

        #endregion
    }

    #region 命令申明

    // 返回事件
    private DelegateCommand? _backSpaceCommand;

    public DelegateCommand BackSpaceCommand => _backSpaceCommand ??= new DelegateCommand(ExecuteBackSpace);

    /// <summary>
    /// 返回事件
    /// </summary>
    protected internal override void ExecuteBackSpace()
    {
        InitView();

        ArrowBack.Fill = DictionaryResource.GetColor("ColorText");

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

    // 前往下载管理页面
    private DelegateCommand? _downloadManagerCommand;

    public DelegateCommand DownloadManagerCommand => _downloadManagerCommand ??= new DelegateCommand(ExecuteDownloadManagerCommand);

    /// <summary>
    /// 前往下载管理页面
    /// </summary>
    private void ExecuteDownloadManagerCommand()
    {
        var parameter = new NavigationParam
        {
            ViewName = ViewDownloadManagerViewModel.Tag,
            ParentViewName = Tag,
            Parameter = null
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    // 顶部tab点击事件
    private DelegateCommand<object>? _tabHeadersCommand;

    public DelegateCommand<object> TabHeadersCommand => _tabHeadersCommand ??= new DelegateCommand<object>(ExecuteTabHeadersCommand, CanExecuteTabHeadersCommand);

    /// <summary>
    /// 顶部tab点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteTabHeadersCommand(object parameter)
    {
        if (parameter is not TabHeader tabHeader)
        {
            return;
        }

        // 顶部tab点击后，隐藏Content
        ContentVisibility = false;

        // 页面选择
        Pager = new CustomPagerViewModel(1, 1);
        Pager.CurrentChanged += OnCurrentChanged_Pager;
        Pager.CountChanged += OnCountChanged_Pager;
        Pager.Current = 1;
    }

    /// <summary>
    /// 顶部tab点击事件是否允许执行
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    private bool CanExecuteTabHeadersCommand(object parameter)
    {
        return IsEnabled;
    }

    // 全选按钮点击事件
    private DelegateCommand<object>? _selectAllCommand;

    public DelegateCommand<object> SelectAllCommand => _selectAllCommand ??= new DelegateCommand<object>(ExecuteSelectAllCommand);

    /// <summary>
    /// 全选按钮点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteSelectAllCommand(object parameter)
    {
        if (IsSelectAll)
        {
            foreach (var item in Medias)
            {
                item.IsSelected = true;
            }
        }
        else
        {
            foreach (var item in Medias)
            {
                item.IsSelected = false;
            }
        }
    }

    // 列表选择事件
    private DelegateCommand<object>? _mediasCommand;

    public DelegateCommand<object> MediasCommand => _mediasCommand ??= new DelegateCommand<object>(ExecuteMediasCommand);

    /// <summary>
    /// 列表选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteMediasCommand(object parameter)
    {
        if (!(parameter is IList selectedMedia))
        {
            return;
        }

        if (selectedMedia.Count == Medias.Count)
        {
            IsSelectAll = true;
        }
        else
        {
            IsSelectAll = false;
        }
    }

    // 添加选中项到下载列表事件
    private DelegateCommand? _addToDownloadCommand;

    public DelegateCommand AddToDownloadCommand => _addToDownloadCommand ??= new DelegateCommand(ExecuteAddToDownloadCommand);

    /// <summary>
    /// 添加选中项到下载列表事件
    /// </summary>
    private void ExecuteAddToDownloadCommand()
    {
        AddToDownload(true);
    }

    // 添加所有视频到下载列表事件
    private DelegateCommand? _addAllToDownloadCommand;

    public DelegateCommand AddAllToDownloadCommand => _addAllToDownloadCommand ??= new DelegateCommand(ExecuteAddAllToDownloadCommand);

    /// <summary>
    /// 添加所有视频到下载列表事件
    /// </summary>
    private void ExecuteAddAllToDownloadCommand()
    {
        AddToDownload(false);
    }

    #endregion

    /// <summary>
    /// 添加到下载
    /// </summary>
    /// <param name="isOnlySelected"></param>
    private async void AddToDownload(bool isOnlySelected)
    {
        // 订阅里只有BANGUMI类型
        var addToDownloadService = new AddToDownloadService(PlayStreamType.Bangumi);

        // 选择文件夹
        var directory = await addToDownloadService.SetDirectory(DialogService);

        // 视频计数
        var i = 0;
        await Task.Run(async () =>
        {
            // 为了避免执行其他操作时，
            // Medias变化导致的异常
            var list = Medias.ToList();

            // 添加到下载
            foreach (var media in list)
            {
                // 只下载选中项，跳过未选中项
                if (isOnlySelected && !media.IsSelected)
                {
                    continue;
                }

                // 开启服务
                var service = new BangumiInfoService($"{ParseEntrance.BangumiMediaUrl}md{media.MediaId}");

                addToDownloadService.SetVideoInfoService(service);
                addToDownloadService.GetVideo();
                addToDownloadService.ParseVideo(service);
                // 下载
                i += await addToDownloadService.AddToDownload(EventAggregator, DialogService, directory);
            }
        });

        if (directory == null)
        {
            return;
        }

        // 通知用户添加到下载列表的结果
        EventAggregator.GetEvent<MessageEvent>().Publish(i <= 0
            ? DictionaryResource.GetString("TipAddDownloadingZero")
            : $"{DictionaryResource.GetString("TipAddDownloadingFinished1")}{i}{DictionaryResource.GetString("TipAddDownloadingFinished2")}");
    }

    private void OnCountChanged_Pager(int count)
    {
    }

    private bool OnCurrentChanged_Pager(int old, int current)
    {
        if (!IsEnabled)
        {
            //Pager.Current = old;
            return false;
        }

        UpdateBangumiMediaList(current);

        return true;
    }

    private async void UpdateBangumiMediaList(int current)
    {
        Medias.Clear();
        IsSelectAll = false;

        LoadingVisibility = true;
        NoDataVisibility = false;

        // 是否正在获取数据
        // 在所有的退出分支中都需要设为true
        IsEnabled = false;

        var tab = TabHeaders[SelectTabId];
        var type = (BangumiType)tab.Id;

        await Task.Run(() =>
        {
            var cancellationToken = _tokenSource.Token;

            var bangumiFollows = Core.BiliApi.Users.UserSpace.GetBangumiFollow(_mid, type, current, VideoNumberInPage);
            if (bangumiFollows?.List == null || bangumiFollows.List.Count == 0)
            {
                LoadingVisibility = false;
                NoDataVisibility = true;
                return;
            }

            // 更新总页码
            Pager.Count = (int)Math.Ceiling((double)bangumiFollows.Total / VideoNumberInPage);
            // 更新内容
            ContentVisibility = true;

            foreach (var bangumiFollow in bangumiFollows.List)
            {
                // 查询、保存封面
                var coverUrl = bangumiFollow.Cover;
                if (!coverUrl.ToLower().StartsWith("http"))
                {
                    coverUrl = $"https:{bangumiFollow.Cover}";
                }

                // 地区
                var area = string.Empty;
                if (bangumiFollow.Areas != null && bangumiFollow.Areas.Count > 0)
                {
                    area = bangumiFollow.Areas[0].Name;
                }

                // 视频更新进度
                var indexShow = string.Empty;
                if (bangumiFollow.NewEp != null)
                {
                    indexShow = bangumiFollow.NewEp.IndexShow;
                }

                // 观看进度
                string progress;
                if (bangumiFollow.Progress == null || bangumiFollow.Progress == "")
                {
                    progress = DictionaryResource.GetString("BangumiNotWatched");
                }
                else
                {
                    progress = bangumiFollow.Progress;
                }

                App.PropertyChangeAsync(() =>
                {
                    var media = new BangumiFollowMedia(EventAggregator)
                    {
                        MediaId = bangumiFollow.MediaId,
                        SeasonId = bangumiFollow.SeasonId,
                        Title = bangumiFollow.Title,
                        SeasonTypeName = bangumiFollow.SeasonTypeName,
                        Area = area,
                        Badge = bangumiFollow.Badge,
                        Cover = coverUrl ?? "avares://DownKyi/Resources/video-placeholder.png",
                        Evaluate = bangumiFollow.Evaluate,
                        IndexShow = indexShow,
                        Progress = progress
                    };

                    Medias.Add(media);

                    LoadingVisibility = false;
                    NoDataVisibility = false;
                });

                // 判断是否该结束线程，若为true，跳出循环
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }, (_tokenSource = new CancellationTokenSource()).Token);

        IsEnabled = true;
    }

    /// <summary>
    /// 初始化页面数据
    /// </summary>
    private void InitView()
    {
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        ContentVisibility = false;
        LoadingVisibility = true;
        NoDataVisibility = false;

        Medias.Clear();
        IsSelectAll = false;
    }

    /// <summary>
    /// 导航到页面时执行
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");

        // 根据传入参数不同执行不同任务
        _mid = navigationContext.Parameters.GetValue<long>("Parameter");
        if (_mid == 0)
        {
            return;
        }

        InitView();

        // 初始选中项
        SelectTabId = 0;

        // 页面选择
        Pager = new CustomPagerViewModel(1, 1);
        Pager.CurrentChanged += OnCurrentChanged_Pager;
        Pager.CountChanged += OnCountChanged_Pager;
        Pager.Current = 1;
    }
}