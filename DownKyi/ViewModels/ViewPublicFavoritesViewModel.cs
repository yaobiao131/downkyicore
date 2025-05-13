using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownKyi.Core.BiliApi.Favorites;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Logging;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.PrismExtension.Dialog;
using DownKyi.Services;
using DownKyi.Services.Download;
using DownKyi.Utils;
using DownKyi.ViewModels.PageViewModels;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class ViewPublicFavoritesViewModel : ViewModelBase
{
    public const string Tag = "PagePublicFavorites";

    private CancellationTokenSource _tokenSource;

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

    private Favorites _favorites;

    public Favorites Favorites
    {
        get => _favorites;
        set => SetProperty(ref _favorites, value);
    }

    private ObservableCollection<FavoritesMedia> _favoritesMedias;

    public ObservableCollection<FavoritesMedia> FavoritesMedias
    {
        get => _favoritesMedias;
        set => SetProperty(ref _favoritesMedias, value);
    }

    private bool _contentVisibility;

    public bool ContentVisibility
    {
        get => _contentVisibility;
        set => SetProperty(ref _contentVisibility, value);
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

    private bool _mediaLoading;

    public bool MediaLoading
    {
        get => _mediaLoading;
        set => SetProperty(ref _mediaLoading, value);
    }

    private bool _mediaLoadingVisibility;

    public bool MediaLoadingVisibility
    {
        get => _mediaLoadingVisibility;
        set => SetProperty(ref _mediaLoadingVisibility, value);
    }

    private bool _mediaNoDataVisibility;

    public bool MediaNoDataVisibility
    {
        get => _mediaNoDataVisibility;
        set => SetProperty(ref _mediaNoDataVisibility, value);
    }

    #endregion

    public ViewPublicFavoritesViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(
        eventAggregator)
    {
        DialogService = dialogService;

        #region 属性初始化

        // 初始化loading
        Loading = true;
        LoadingVisibility = false;
        NoDataVisibility = false;

        MediaLoading = true;
        MediaLoadingVisibility = false;
        MediaNoDataVisibility = false;

        ArrowBack = NavigationIcon.Instance().ArrowBack;
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        // 下载管理按钮
        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");

        FavoritesMedias = new ObservableCollection<FavoritesMedia>();

        #endregion
    }

    #region 命令申明

    // 返回
    private DelegateCommand? _backSpaceCommand;

    public DelegateCommand BackSpaceCommand => _backSpaceCommand ??= new DelegateCommand(ExecuteBackSpace);

    /// <summary>
    /// 返回
    /// </summary>
    protected internal override void ExecuteBackSpace()
    {
        // 结束任务
        _tokenSource?.Cancel();

        NavigationParam parameter = new NavigationParam
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
        NavigationParam parameter = new NavigationParam
        {
            ViewName = ViewDownloadManagerViewModel.Tag,
            ParentViewName = Tag,
            Parameter = null
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    // 复制封面事件
    private DelegateCommand? _copyCoverCommand;

    public DelegateCommand CopyCoverCommand => _copyCoverCommand ??= new DelegateCommand(ExecuteCopyCoverCommand);

    /// <summary>
    /// 复制封面事件
    /// </summary>
    private void ExecuteCopyCoverCommand()
    {
        // 复制封面图片到剪贴板
        // Clipboard.SetImage(Favorites.Cover);
        LogManager.Info(Tag, "复制封面图片到剪贴板");
    }

    // 复制封面URL事件
    private DelegateCommand? _copyCoverUrlCommand;

    public DelegateCommand CopyCoverUrlCommand => _copyCoverUrlCommand ??= new DelegateCommand(ExecuteCopyCoverUrlCommand);

    /// <summary>
    /// 复制封面URL事件
    /// </summary>
    private async void ExecuteCopyCoverUrlCommand()
    {
        // 复制封面url到剪贴板
        await ClipboardManager.SetText(Favorites.CoverUrl);
        LogManager.Info(Tag, "复制封面url到剪贴板");
    }

    // 前往UP主页事件
    private DelegateCommand? _upperCommand;
    public DelegateCommand UpperCommand => _upperCommand ??= new DelegateCommand(ExecuteUpperCommand);

    /// <summary>
    /// 前往UP主页事件
    /// </summary>
    private void ExecuteUpperCommand()
    {
        NavigateToView.NavigateToViewUserSpace(EventAggregator, Tag, Favorites.UpperMid);
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

    // 列表选择事件
    private DelegateCommand<object>? _favoritesMediasCommand;

    public DelegateCommand<object> FavoritesMediasCommand => _favoritesMediasCommand ??= new DelegateCommand<object>(ExecuteFavoritesMediasCommand);

    /// <summary>
    /// 列表选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteFavoritesMediasCommand(object parameter)
    {
    }

    #endregion

    private async void AddToDownload(bool isOnlySelected)
    {
        // 收藏夹里只有视频
        var addToDownloadService = new AddToDownloadService(PlayStreamType.Video);

        // 选择文件夹
        var directory = await addToDownloadService.SetDirectory(DialogService);

        // 视频计数
        var i = 0;
        await Task.Run(async () =>
        {
            // 为了避免执行其他操作时，
            // Medias变化导致的异常
            var list = FavoritesMedias.ToList();

            // 添加到下载
            foreach (var media in list)
            {
                // 只下载选中项，跳过未选中项
                if (isOnlySelected && !media.IsSelected)
                {
                    continue;
                }

                /// 有分P的就下载全部

                // 开启服务
                var videoInfoService = new VideoInfoService(media.Bvid);

                addToDownloadService.SetVideoInfoService(videoInfoService);
                addToDownloadService.GetVideo();
                addToDownloadService.ParseVideo(videoInfoService);
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

    /// <summary>
    /// 初始化页面元素
    /// </summary>
    private void InitView()
    {
        LogManager.Debug(Tag, "初始化页面元素");

        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");

        ContentVisibility = false;
        LoadingVisibility = false;
        NoDataVisibility = false;
        MediaLoadingVisibility = false;
        MediaNoDataVisibility = false;

        FavoritesMedias.Clear();
    }

    /// <summary>
    /// 更新页面
    /// </summary>
    private void UpdateView(IFavoritesService favoritesService, long favoritesId, CancellationToken cancellationToken)
    {
        LoadingVisibility = true;

        Favorites = favoritesService.GetFavorites(favoritesId);
        if (Favorites == null)
        {
            LogManager.Debug(Tag, "Favorites is null.");

            ContentVisibility = false;
            LoadingVisibility = false;
            NoDataVisibility = true;
            return;
        }
        else
        {
            ContentVisibility = true;
            LoadingVisibility = false;
            NoDataVisibility = false;
        }

        MediaLoadingVisibility = true;

        var medias = FavoritesResource.GetAllFavoritesMedia(favoritesId);
        if (medias == null || medias.Count == 0)
        {
            MediaLoadingVisibility = false;
            MediaNoDataVisibility = true;
            return;
        }
        else
        {
            MediaLoadingVisibility = false;
            MediaNoDataVisibility = false;
        }

        favoritesService.GetFavoritesMediaList(medias, FavoritesMedias, EventAggregator, cancellationToken);
    }

    /// <summary>
    /// 接收收藏夹id参数
    /// </summary>
    /// <param name="navigationContext"></param>
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        // 根据传入参数不同执行不同任务
        var parameter = navigationContext.Parameters.GetValue<long>("Parameter");
        if (parameter == 0)
        {
            return;
        }

        InitView();
        await Task.Run(() =>
        {
            var cancellationToken = _tokenSource.Token;

            UpdateView(new FavoritesService(), parameter, cancellationToken);
        }, (_tokenSource = new CancellationTokenSource()).Token);
    }
}