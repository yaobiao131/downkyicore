using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownKyi.Commands;
using DownKyi.Core.BiliApi.History;
using DownKyi.Core.BiliApi.History.Models;
using DownKyi.Core.BiliApi.VideoStream;
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
    private const int VideoNumberInPage = 30;

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
    
    public AsyncDelegateCommand LoadMoreCommand => new (ExecuteLoadMoreCommand);
    
    private long _nextMax = 0;

    private long _nextViewAt = 0;

    public async Task ExecuteLoadMoreCommand(object obj,CancellationToken token)
    {
        if(NoDataVisibility) return;
        LoadingVisibility = true;
        var result = await Task<HistoryData>.Run(() =>
        {
            return History.GetHistory(_nextMax, _nextViewAt, VideoNumberInPage);
        });
        if (result?.List?.Count > 0)
        {
            Medias.AddRange(result.List.Select(x => Convert(x,EventAggregator))
                .Where(v => v != null && !string.IsNullOrEmpty(v.Title)).ToList());
            _nextMax = result.Cursor.Max;
            _nextViewAt = result.Cursor.ViewAt;
        }
        LoadingVisibility = false;
    }
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
        var addToDownloadService = new AddToDownloadService(PlayStreamType.Video);

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
                IInfoService? service = media.Business switch
                {
                    "archive" => new VideoInfoService(media.Url),
                    "pgc" => new BangumiInfoService(media.Url),
                    _ => null
                };

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
        EventAggregator.GetEvent<MessageEvent>().Publish(i <= 0
            ? DictionaryResource.GetString("TipAddDownloadingZero")
            : $"{DictionaryResource.GetString("TipAddDownloadingFinished1")}{i}{DictionaryResource.GetString("TipAddDownloadingFinished2")}");
    }

    private async void UpdateHistoryMediaList()
    {
        LoadingVisibility = true;
        NoDataVisibility = false;
        Medias.Clear();

        await Task.Run(() =>
        {
            var historyList = History.GetHistory(0, 0, VideoNumberInPage);
            if (historyList?.List?.Count > 0)
            {
                App.PropertyChangeAsync(() =>
                {
                    ContentVisibility = true;
                    LoadingVisibility = false;
                    NoDataVisibility = false;
                });
            }
            else
            {
               App.PropertyChangeAsync(() =>
               {
                   LoadingVisibility = false;
                   NoDataVisibility = true;
               });
            }
        });
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

        _nextMax = 0;
        _nextViewAt = 0;
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

    private static bool IsValidBusiness(string business)
        => business is "archive" or "pgc";

    private static string BuildMediaUrl(HistoryList history) =>
        history.History.Business switch
        {
            "archive" => $"https://www.bilibili.com/video/{history.History.Bvid}",
            "pgc" => history.Uri,
            _ => "https://www.bilibili.com"
        };

    private static string ProcessCoverUrl(string originalUrl) =>
        !string.IsNullOrEmpty(originalUrl) && !originalUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? $"https:{originalUrl}"
            : originalUrl;

    private static VectorImage? GetPlatformIcon(int dt) =>
        dt switch
        {
            1 or 3 or 5 or 7 => NormalIcon.Instance().PlatformMobile,
            2 => NormalIcon.Instance().PlatformPC,
            4 or 6 => NormalIcon.Instance().PlatformIpad,
            33 => NormalIcon.Instance().PlatformTV,
            _ => null
        };

    private static string BuildProgressText(long progress) =>
        progress switch
        {
            -1 => DictionaryResource.GetString("HistoryFinished"),
            0 => DictionaryResource.GetString("HistoryStarted"),
            _ => $"{DictionaryResource.GetString("HistoryWatch")} {Format.FormatDuration3(progress)}"
        };

    public static HistoryMedia? Convert(HistoryList history, IEventAggregator eventAggregator)
    {
        if (history?.History == null || !IsValidBusiness(history.History.Business))
            return null;

        var url = BuildMediaUrl(history);
        var coverUrl = ProcessCoverUrl(history.Cover);
        var platform = GetPlatformIcon(history.History.Dt);

        return new HistoryMedia(eventAggregator)
        {
            Business = history.History.Business,
            Bvid = history.History.Bvid,
            Url = url,
            UpMid = history.AuthorMid,
            Cover = coverUrl ?? "avares://DownKyi/Resources/video-placeholder.png",
            Title = history.Title,
            SubTitle = history.ShowTitle,
            Duration = history.Duration,
            TagName = history.TagName,
            Partdesc = history.NewDesc,
            Progress = BuildProgressText(history.Progress),
            Platform = platform,
            UpName = history.AuthorFace != null ? history.AuthorName : "",
            UpHeader = history.AuthorFace ?? "",
            PartdescVisibility = !string.IsNullOrEmpty(history.NewDesc),
            UpAndTagVisibility = history.History.Business == "archive"
        };
    }
}