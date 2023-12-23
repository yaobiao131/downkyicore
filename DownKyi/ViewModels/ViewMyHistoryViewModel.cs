using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.History;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
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

public class ViewMyHistoryViewModel : ViewModelBase
{
    public const string Tag = "PageMyHistory";

    private CancellationTokenSource? _tokenSource;

    // 每页视频数量，暂时在此写死，以后在设置中增加选项
    private readonly int VideoNumberInPage = 30;

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

    private bool _contentVisibility;

    public bool ContentVisibility
    {
        get => _contentVisibility;
        set => SetProperty(ref _contentVisibility, value);
    }

    private ObservableCollection<HistoryMedia> _medias;

    public ObservableCollection<HistoryMedia> Medias
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

    public ViewMyHistoryViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(
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

        Medias = new ObservableCollection<HistoryMedia>();

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

    public DelegateCommand DownloadManagerCommand =>
        _downloadManagerCommand ??= new DelegateCommand(ExecuteDownloadManagerCommand);

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

    // 全选按钮点击事件
    private DelegateCommand<object>? _selectAllCommand;

    public DelegateCommand<object> SelectAllCommand =>
        _selectAllCommand ??= new DelegateCommand<object>(ExecuteSelectAllCommand);

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

    public DelegateCommand<object> MediasCommand =>
        _mediasCommand ??= new DelegateCommand<object>(ExecuteMediasCommand);

    /// <summary>
    /// 列表选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteMediasCommand(object parameter)
    {
        if (parameter is not IList selectedMedia)
        {
            return;
        }

        IsSelectAll = selectedMedia.Count == Medias.Count;
    }

    // 添加选中项到下载列表事件
    private DelegateCommand? _addToDownloadCommand;

    public DelegateCommand AddToDownloadCommand =>
        _addToDownloadCommand ??= new DelegateCommand(ExecuteAddToDownloadCommand);

    /// <summary>
    /// 添加选中项到下载列表事件
    /// </summary>
    private void ExecuteAddToDownloadCommand()
    {
        AddToDownload(true);
    }

    // 添加所有视频到下载列表事件
    private DelegateCommand? _addAllToDownloadCommand;

    public DelegateCommand AddAllToDownloadCommand =>
        _addAllToDownloadCommand ??= new DelegateCommand(ExecuteAddAllToDownloadCommand);

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
        // BANGUMI类型
        var addToDownloadService = new AddToDownloadService(PlayStreamType.VIDEO);

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

                // 有分P的就下载全部

                // 开启服务
                IInfoService? service = null;
                switch (media.Business)
                {
                    case "archive":
                        service = new VideoInfoService(media.Url);
                        break;
                    case "pgc":
                        service = new BangumiInfoService(media.Url);
                        break;
                }

                if (service == null)
                {
                    return;
                }

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
        if (i <= 0)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipAddDownloadingZero"));
        }
        else
        {
            EventAggregator.GetEvent<MessageEvent>()
                .Publish(
                    $"{DictionaryResource.GetString("TipAddDownloadingFinished1")}{i}{DictionaryResource.GetString("TipAddDownloadingFinished2")}");
        }
    }

    private async void UpdateHistoryMediaList()
    {
        LoadingVisibility = true;
        NoDataVisibility = false;
        Medias.Clear();

        await Task.Run(() =>
        {
            var cancellationToken = _tokenSource.Token;

            var historyList = History.GetHistory(0, 0, VideoNumberInPage);
            if (historyList == null || historyList.List == null || historyList.List.Count == 0)
            {
                LoadingVisibility = false;
                NoDataVisibility = true;
                return;
            }

            foreach (var history in historyList.List)
            {
                if (history.History == null)
                {
                    continue;
                }

                if (history.History.Business != "archive" && history.History.Business != "pgc")
                {
                    continue;
                }

                // 播放url
                var url = "https://www.bilibili.com";
                switch (history.History.Business)
                {
                    case "archive":
                        url = "https://www.bilibili.com/video/" + history.History.Bvid;
                        break;
                    case "pgc":
                        url = history.Uri;
                        break;
                }

                // 查询、保存封面
                var coverUrl = history.Cover;
                Bitmap cover;
                if (coverUrl == null || coverUrl == "")
                {
                    cover = null;
                }
                else
                {
                    if (!coverUrl.ToLower().StartsWith("http"))
                    {
                        coverUrl = $"https:{history.Cover}";
                    }

                    var storageCover = new StorageCover();
                    cover = storageCover.GetCoverThumbnail(history.History.Oid, history.History.Bvid,
                        history.History.Cid, coverUrl, 160, 100);
                }

                // 获取用户头像
                string upName;
                Bitmap upHeader;
                if (history.AuthorFace != null)
                {
                    upName = history.AuthorName;
                    StorageHeader storageHeader = new StorageHeader();
                    upHeader = storageHeader.GetHeaderThumbnail(history.AuthorMid, upName, history.AuthorFace, 24, 24);
                }
                else
                {
                    upName = "";
                    upHeader = null;
                }


                // 观看平台
                VectorImage platform;
                switch (history.History.Dt)
                {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                        // 手机端
                        platform = NormalIcon.Instance().PlatformMobile;
                        break;
                    case 2:
                        // web端
                        platform = NormalIcon.Instance().PlatformPC;
                        break;
                    case 4:
                    case 6:
                        // pad端
                        platform = NormalIcon.Instance().PlatformIpad;
                        break;
                    case 33:
                        // TV端
                        platform = NormalIcon.Instance().PlatformTV;
                        break;
                    default:
                        // 其他
                        platform = null;
                        break;
                }

                // 是否显示Partdesc
                bool partdescVisibility;
                if (history.NewDesc == "")
                {
                    partdescVisibility = false;
                }
                else
                {
                    partdescVisibility = true;
                }

                // 是否显示UP主信息和分区信息
                bool upAndTagVisibility;
                if (history.History.Business == "archive")
                {
                    upAndTagVisibility = true;
                }
                else
                {
                    upAndTagVisibility = false;
                }

                App.PropertyChangeAsync(() =>
                {
                    // 观看进度
                    // -1 已看完
                    // 0 刚开始
                    // >0 看到 progress
                    string progress;
                    if (history.Progress == -1)
                    {
                        progress = DictionaryResource.GetString("HistoryFinished");
                    }
                    else if (history.Progress == 0)
                    {
                        progress = DictionaryResource.GetString("HistoryStarted");
                    }
                    else
                    {
                        progress = DictionaryResource.GetString("HistoryWatch") + " " +
                                   Format.FormatDuration3(history.Progress);
                    }

                    var media = new HistoryMedia(EventAggregator)
                    {
                        Business = history.History.Business,
                        Bvid = history.History.Bvid,
                        Url = url,
                        UpMid = history.AuthorMid,
                        Cover = cover ??
                                ImageHelper.LoadFromResource(
                                    new Uri($"avares://DownKyi/Resources/video-placeholder.png")),
                        Title = history.Title,
                        SubTitle = history.ShowTitle,
                        Duration = history.Duration,
                        TagName = history.TagName,
                        Partdesc = history.NewDesc,
                        Progress = progress,
                        Platform = platform,
                        UpName = upName,
                        UpHeader = upHeader,

                        PartdescVisibility = partdescVisibility,
                        UpAndTagVisibility = upAndTagVisibility,
                    };

                    Medias.Add(media);

                    ContentVisibility = true;
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
    }

    /// <summary>
    /// 初始化页面数据
    /// </summary>
    private void InitView()
    {
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");

        ContentVisibility = false;
        LoadingVisibility = false;
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
        var mid = navigationContext.Parameters.GetValue<long>("Parameter");
        if (mid == 0)
        {
            IsSelectAll = false;
            foreach (var media in Medias)
            {
                media.IsSelected = false;
            }

            return;
        }

        InitView();

        UpdateHistoryMediaList();
    }
}