using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.Services;
using DownKyi.Services.Download;
using DownKyi.Utils;
using DownKyi.ViewModels.Dialogs;
using DownKyi.ViewModels.PageViewModels;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.ViewModels;

public class ViewVideoDetailViewModel : ViewModelBase
{
    public const string Tag = "PageVideoDetail";

    // 保存输入字符串，避免被用户修改
    private string input = null;

    private IInfoService? infoService;

    #region 页面属性申明

    private VectorImage arrowBack;

    public VectorImage ArrowBack
    {
        get => arrowBack;
        set => SetProperty(ref arrowBack, value);
    }

    private string inputText;

    public string InputText
    {
        get => inputText;
        set => SetProperty(ref inputText, value);
    }

    private string inputSearchText;

    public string InputSearchText
    {
        get => inputSearchText;
        set => SetProperty(ref inputSearchText, value);
    }

    private bool loading;

    public bool Loading
    {
        get => loading;
        set => SetProperty(ref loading, value);
    }


    private bool loadingVisibility;

    public bool LoadingVisibility
    {
        get => loadingVisibility;
        set => SetProperty(ref loadingVisibility, value);
    }

    private VectorImage downloadManage;

    public VectorImage DownloadManage
    {
        get => downloadManage;
        set => SetProperty(ref downloadManage, value);
    }

    private VideoInfoView videoInfoView;

    public VideoInfoView VideoInfoView
    {
        get => videoInfoView;
        set => SetProperty(ref videoInfoView, value);
    }

    private ObservableCollection<VideoSection> videoSections;

    public ObservableCollection<VideoSection> VideoSections
    {
        get => videoSections;
        set => SetProperty(ref videoSections, value);
    }

    public ObservableCollection<VideoSection> CaCheVideoSections { get; set; }

    public List<VideoPage> selectedVideoPages { get; set; } = new();

    private bool isSelectAll;

    public bool IsSelectAll
    {
        get => isSelectAll;
        set => SetProperty(ref isSelectAll, value);
    }

    private bool contentVisibility;

    public bool ContentVisibility
    {
        get => contentVisibility;
        set => SetProperty(ref contentVisibility, value);
    }

    private bool noDataVisibility;

    public bool NoDataVisibility
    {
        get => noDataVisibility;
        set => SetProperty(ref noDataVisibility, value);
    }

    #endregion

    public ViewVideoDetailViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(
        eventAggregator, dialogService)
    {
        // 初始化loading
        Loading = true;
        LoadingVisibility = false;

        // 返回按钮
        ArrowBack = NavigationIcon.Instance().ArrowBack;
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        // 下载管理按钮
        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");

        VideoSections = new ObservableCollection<VideoSection>();
        CaCheVideoSections = new ObservableCollection<VideoSection>();
    }

    #region 命令申明

// 返回
    private DelegateCommand backSpaceCommand;

    public DelegateCommand BackSpaceCommand =>
        backSpaceCommand ?? (backSpaceCommand = new DelegateCommand(ExecuteBackSpace));

    /// <summary>
    /// 返回
    /// </summary>
    private void ExecuteBackSpace()
    {
        NavigationParam parameter = new NavigationParam
        {
            ViewName = ParentView,
            ParentViewName = null,
            Parameter = null
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
    }

    // 前往下载管理页面
    private DelegateCommand downloadManagerCommand;

    public DelegateCommand DownloadManagerCommand => downloadManagerCommand ??
                                                     (downloadManagerCommand =
                                                         new DelegateCommand(ExecuteDownloadManagerCommand));

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

    // 输入确认事件
    private DelegateCommand inputCommand;

    public DelegateCommand InputCommand =>
        inputCommand ?? (inputCommand = new DelegateCommand(ExecuteInputCommand, CanExecuteInputCommand));


    private DelegateCommand inputSearchCommand;

    public DelegateCommand InputSearchCommand =>
        inputSearchCommand ?? (inputSearchCommand = new DelegateCommand(ExecuteInputSearchCommand));

    /// <summary>
    /// 搜索视频输入事件
    /// </summary>
    private async void ExecuteInputSearchCommand()
    {
        await Task.Run(() =>
        {
            if (InputSearchText == null || InputSearchText == string.Empty)
            {
                foreach (VideoSection section in VideoSections)
                {
                    var cache = CaCheVideoSections.FirstOrDefault(e => e.Id == section.Id);
                    if (cache != null)
                    {
                        section.VideoPages = cache.VideoPages;
                    }
                }
            }
            else
            {
                foreach (VideoSection section in VideoSections)
                {
                    var cache = CaCheVideoSections.FirstOrDefault(e => e.Id == section.Id);
                    if (cache != null)
                    {
                        var pages = cache.VideoPages.Where(e => e.Name.Contains(InputSearchText)).ToList();
                        section.VideoPages = pages;
                    }
                }
            }
        });
    }

    /// <summary>
    /// 处理输入事件
    /// </summary>
    private async void ExecuteInputCommand()
    {
        InitView();
        try
        {
            await Task.Run(() =>
            {
                if (InputText == null || InputText == string.Empty)
                {
                    return;
                }

                LogManager.Debug(Tag, $"InputText: {InputText}");
                InputText = Regex.Replace(InputText, @"[【]*[^【]*[^】]*[】 ]", "");
                input = InputText;

                // 更新页面
                UnityUpdateView(UpdateView, input, null, true);

                // 是否自动解析视频
                if (SettingsManager.GetInstance().IsAutoParseVideo() == AllowStatus.YES)
                {
                    PropertyChangeAsync(ExecuteParseAllVideoCommand);
                }
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("InputCommand()发生异常: {0}", e);
            LogManager.Error(Tag, e);

            LoadingVisibility = false;
            ContentVisibility = false;
            NoDataVisibility = true;
        }
    }

    /// <summary>
    /// 输入事件是否允许执行
    /// </summary>
    /// <returns></returns>
    private bool CanExecuteInputCommand()
    {
        return LoadingVisibility != true;
    }

    // 复制封面事件
    private DelegateCommand? copyCoverCommand;

    public DelegateCommand CopyCoverCommand =>
        copyCoverCommand ??= new DelegateCommand(ExecuteCopyCoverCommand);

    /// <summary>
    /// 复制封面事件
    /// </summary>
    private void ExecuteCopyCoverCommand()
    {
        // 复制封面图片到剪贴板
        // Clipboard.SetImage(VideoInfoView.Cover);
        LogManager.Info(Tag, "复制封面图片到剪贴板");
    }

    // 复制封面URL事件
    private DelegateCommand? copyCoverUrlCommand;

    public DelegateCommand CopyCoverUrlCommand =>
        copyCoverUrlCommand ??= new DelegateCommand(ExecuteCopyCoverUrlCommand);

    /// <summary>
    /// 复制封面URL事件
    /// </summary>
    private void ExecuteCopyCoverUrlCommand()
    {
        // 复制封面url到剪贴板
        // Clipboard.SetText(VideoInfoView.CoverUrl);
        LogManager.Info(Tag, "复制封面url到剪贴板");
    }

    // 前往UP主页事件
    private DelegateCommand? upperCommand;
    public DelegateCommand UpperCommand => upperCommand ??= new DelegateCommand(ExecuteUpperCommand);

    /// <summary>
    /// 前往UP主页事件
    /// </summary>
    private void ExecuteUpperCommand()
    {
        NavigateToView.NavigateToViewUserSpace(EventAggregator, Tag, VideoInfoView.UpperMid);
    }

// 视频章节选择事件
    private DelegateCommand<object>? videoSectionsCommand;

    public DelegateCommand<object> VideoSectionsCommand =>
        videoSectionsCommand ??= new DelegateCommand<object>(ExecuteVideoSectionsCommand);

    /// <summary>
    /// 视频章节选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoSectionsCommand(object parameter)
    {
        if (!(parameter is VideoSection section))
        {
            return;
        }

        bool isSelectAll = true;
        foreach (VideoPage page in section.VideoPages)
        {
            if (!page.IsSelected)
            {
                isSelectAll = false;
                break;
            }
        }

        IsSelectAll = section.VideoPages.Count != 0 && isSelectAll;
    }

// 视频page选择事件
    private DelegateCommand<IList> videoPagesCommand;

    public DelegateCommand<IList> VideoPagesCommand => videoPagesCommand ??
                                                       (videoPagesCommand =
                                                           new DelegateCommand<IList>(ExecuteVideoPagesCommand));

    /// <summary>
    /// 视频page选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoPagesCommand(IList parameter)
    {
        if (!(parameter is IList videoPages))
        {
            return;
        }

        VideoSection section = VideoSections.FirstOrDefault(item => item.IsSelected);

        if (section == null)
        {
            return;
        }

        selectedVideoPages.Clear();
        foreach (var page in videoPages)
        {
            selectedVideoPages.Add((VideoPage)page);
        }

        IsSelectAll = section.VideoPages.Count == videoPages.Count && section.VideoPages.Count != 0;
    }

    // Ctrl+A 全选事件
    private DelegateCommand<object> keySelectAllCommand;

    public DelegateCommand<object> KeySelectAllCommand => keySelectAllCommand ??
                                                          (keySelectAllCommand =
                                                              new DelegateCommand<object>(ExecuteKeySelectAllCommand));

    /// <summary>
    /// Ctrl+A 全选事件
    /// </summary>
    private void ExecuteKeySelectAllCommand(object parameter)
    {
        if (!(parameter is VideoSection section))
        {
            return;
        }

        foreach (VideoPage page in section.VideoPages)
        {
            page.IsSelected = true;
        }
    }

    // 解析视频流事件
    private DelegateCommand<object> parseCommand;

    public DelegateCommand<object> ParseCommand => parseCommand ??
                                                   (parseCommand = new DelegateCommand<object>(ExecuteParseCommand,
                                                       CanExecuteParseCommand));

    /// <summary>
    /// 解析视频流事件
    /// </summary>
    /// <param name="parameter"></param>
    private async void ExecuteParseCommand(object parameter)
    {
        if (!(parameter is VideoPage videoPage))
        {
            return;
        }

        LoadingVisibility = true;

        try
        {
            await Task.Run(() =>
            {
                LogManager.Debug(Tag, $"Video Page: {videoPage.Cid}");

                UnityUpdateView(ParseVideo, input, videoPage, true);
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("ParseCommand()发生异常: {0}", e);
            LogManager.Error(Tag, e);

            LoadingVisibility = false;
        }

        LoadingVisibility = false;
    }

    /// <summary>
    /// 解析视频流事件是否允许执行
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    private bool CanExecuteParseCommand(object parameter)
    {
        return LoadingVisibility != true;
    }


    // 解析所有视频流事件
    private DelegateCommand parseAllVideoCommand;

    public DelegateCommand ParseAllVideoCommand => parseAllVideoCommand ?? (parseAllVideoCommand =
        new DelegateCommand(ExecuteParseAllVideoCommand, CanExecuteParseAllVideoCommand));

    /// <summary>
    /// 解析所有视频流事件
    /// </summary>
    private async void ExecuteParseAllVideoCommand()
    {
        // 解析范围
        ParseScope parseScope = SettingsManager.GetInstance().GetParseScope();

        // 是否选择了解析范围
        if (parseScope == ParseScope.NONE)
        {
            //打开解析选择器
            DialogService.ShowDialog(ViewParsingSelectorViewModel.Tag, null, async result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    // 选择的解析范围
                    parseScope = result.Parameters.GetValue<ParseScope>("parseScope");
                    await ExecuteParse(parseScope);
                }
            });
        }
        else
        {
            await ExecuteParse(parseScope);
        }
    }

    /// <summary>
    /// 解析所有视频流事件是否允许执行
    /// </summary>
    /// <returns></returns>
    private bool CanExecuteParseAllVideoCommand()
    {
        return LoadingVisibility != true;
    }

    private async Task ExecuteParse(ParseScope parseScope)
    {
        try
        {
            LoadingVisibility = true;
            await Task.Run(() =>
            {
                LogManager.Debug(Tag, "Parse video");

                switch (parseScope)
                {
                    case ParseScope.NONE:
                        break;
                    case ParseScope.SELECTED_ITEM:
                        foreach (VideoSection section in VideoSections)
                        {
                            foreach (VideoPage page in section.VideoPages)
                            {
                                if (selectedVideoPages.Find(v => v.Order == page.Order) != null)
                                {
                                    UnityUpdateView(ParseVideo, input, page);
                                }
                                /*if (page.IsSelected)
                                {
                                    // 执行解析任务
                                    UnityUpdateView(ParseVideo, input, page);
                                }*/
                            }
                        }

                        break;
                    case ParseScope.CURRENT_SECTION:
                        foreach (VideoSection section in VideoSections)
                        {
                            if (section.IsSelected)
                            {
                                foreach (VideoPage page in section.VideoPages)
                                {
                                    // 执行解析任务
                                    UnityUpdateView(ParseVideo, input, page);
                                }
                            }
                        }

                        break;
                    case ParseScope.ALL:
                        foreach (VideoSection section in VideoSections)
                        {
                            foreach (VideoPage page in section.VideoPages)
                            {
                                // 执行解析任务
                                UnityUpdateView(ParseVideo, input, page);
                            }
                        }

                        break;
                    default:
                        break;
                }
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("ParseCommand()发生异常: {0}", e);
            LogManager.Error(Tag, e);

            LoadingVisibility = false;
        }

        LoadingVisibility = false;

        // 解析后是否自动下载解析视频
        AllowStatus isAutoDownloadAll = SettingsManager.GetInstance().IsAutoDownloadAll();
        if (parseScope != ParseScope.NONE && isAutoDownloadAll == AllowStatus.YES)
        {
            AddToDownload(true);
        }

        LogManager.Debug(Tag, $"ParseScope: {parseScope:G}");
    }

    // 添加到下载列表事件
    private DelegateCommand addToDownloadCommand;

    public DelegateCommand AddToDownloadCommand => addToDownloadCommand ?? (addToDownloadCommand =
        new DelegateCommand(ExecuteAddToDownloadCommand, CanExecuteAddToDownloadCommand));

    /// <summary>
    /// 添加到下载列表事件
    /// </summary>
    private void ExecuteAddToDownloadCommand()
    {
        AddToDownload(false);
    }

    private bool CanExecuteAddToDownloadCommand()
    {
        return LoadingVisibility != true;
    }

    #endregion

    #region 业务逻辑

    /// <summary>
    /// 初始化页面元素
    /// </summary>
    private void InitView()
    {
        LogManager.Debug(Tag, "初始化页面元素");

        LoadingVisibility = true;
        ContentVisibility = false;
        NoDataVisibility = false;

        VideoSections.Clear();
        CaCheVideoSections.Clear();
    }

    /// <summary>
    /// 更新页面的统一方法
    /// </summary>
    /// <param name="action"></param>
    /// <param name="input"></param>
    /// <param name="page"></param>
    /// <param name="refresh"></param>
    private void UnityUpdateView(Action<IInfoService, VideoPage> action, string input, VideoPage page,
        bool refresh = false)
    {
        if (infoService == null || refresh)
        {
            // 视频
            if (ParseEntrance.IsAvUrl(input) || ParseEntrance.IsBvUrl(input))
            {
                infoService = new VideoInfoService(input);
            }

            // 番剧（电影、电视剧）
            if (ParseEntrance.IsBangumiSeasonUrl(input) || ParseEntrance.IsBangumiEpisodeUrl(input) ||
                ParseEntrance.IsBangumiMediaUrl(input))
            {
                infoService = new BangumiInfoService(input);
            }

            // 课程
            if (ParseEntrance.IsCheeseSeasonUrl(input) || ParseEntrance.IsCheeseEpisodeUrl(input))
            {
                infoService = new CheeseInfoService(input);
            }
        }

        if (infoService == null)
        {
            return;
        }

        action(infoService, page);
    }

    /// <summary>
    /// 更新页面
    /// </summary>
    /// <param name="videoInfoService"></param>
    private void UpdateView(IInfoService videoInfoService, VideoPage param)
    {
        // 获取视频详情
        VideoInfoView = videoInfoService.GetVideoView();
        if (VideoInfoView == null)
        {
            LogManager.Debug(Tag, "VideoInfoView is null.");

            LoadingVisibility = false;
            ContentVisibility = false;
            NoDataVisibility = true;
            return;
        }
        else
        {
            LoadingVisibility = false;
            ContentVisibility = true;
            NoDataVisibility = false;
        }

        // 获取视频列表
        List<VideoSection> videoSections = videoInfoService.GetVideoSections(false);

        // 清空以前的数据
        PropertyChangeAsync(() =>
        {
            VideoSections.Clear();
            CaCheVideoSections.Clear();
        });

        // 添加新数据
        if (videoSections == null)
        {
            LogManager.Debug(Tag, "videoSections is not exist.");

            List<VideoPage> pages = videoInfoService.GetVideoPages();

            PropertyChangeAsync(() =>
            {
                VideoSections.Add(new VideoSection
                {
                    Id = 0,
                    Title = "default",
                    IsSelected = true,
                    VideoPages = pages
                });
                CaCheVideoSections.Add(new VideoSection
                {
                    Id = 0,
                    Title = "default",
                    IsSelected = true,
                    VideoPages = pages
                });
            });
        }
        else
        {
            //这里如果浅拷贝会导致用于查询的CaCheVideoSections数据变化，所以这样处理
            var videoSectionsStr = JsonConvert.SerializeObject(videoSections);
            var videoSectionsData = JsonConvert.DeserializeObject<List<VideoSection>>(videoSectionsStr);
            PropertyChangeAsync(() =>
            {
                VideoSections.AddRange(videoSections);
                CaCheVideoSections.AddRange(videoSectionsData);
            });
        }
    }

    /// <summary>
    /// 解析视频流
    /// </summary>
    /// <param name="videoInfoService"></param>
    private void ParseVideo(IInfoService videoInfoService, VideoPage videoPage)
    {
        videoInfoService.GetVideoStream(videoPage);
    }

    /// <summary>
    /// 添加到下载列表事件
    /// </summary>
    /// <param name="isAll">是否下载所有，包括未选中项</param>
    private async void AddToDownload(bool isAll)
    {
        AddToDownloadService addToDownloadService = null;
        // 视频
        if (ParseEntrance.IsAvUrl(input) || ParseEntrance.IsBvUrl(input))
        {
            addToDownloadService = new AddToDownloadService(PlayStreamType.VIDEO);
        }
        // 番剧（电影、电视剧）
        else if (ParseEntrance.IsBangumiSeasonUrl(input) || ParseEntrance.IsBangumiEpisodeUrl(input) ||
                 ParseEntrance.IsBangumiMediaUrl(input))
        {
            addToDownloadService = new AddToDownloadService(PlayStreamType.BANGUMI);
        }
        // 课程
        else if (ParseEntrance.IsCheeseSeasonUrl(input) || ParseEntrance.IsCheeseEpisodeUrl(input))
        {
            addToDownloadService = new AddToDownloadService(PlayStreamType.CHEESE);
        }
        else
        {
            return;
        }

        // 选择文件夹
        string directory = await addToDownloadService.SetDirectory(DialogService);

        // 视频计数
        int i = 0;
        await Task.Run(() =>
        {
            // 传递video对象
            addToDownloadService.GetVideo(VideoInfoView, VideoSections.ToList(),
                selectedVideoPages.Select(video => video.Order).ToList());
            // 下载
            i = addToDownloadService.AddToDownload(EventAggregator, directory, isAll);
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

    #endregion

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        ArrowBack.Fill = DictionaryResource.GetColor("ColorTextDark");

        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");
        // Parent参数为null时，表示是从下一个页面返回到本页面，不需要执行任务
        if (navigationContext.Parameters.GetValue<string>("Parent") != null)
        {
            string param = navigationContext.Parameters.GetValue<string>("Parameter");
            // 移除剪贴板id
            string input = param.Replace(AppConstant.ClipboardId, "");

            // 检测是否从剪贴板传入
            if (InputText == input && param.EndsWith(AppConstant.ClipboardId))
            {
                return;
            }

            // 正在执行任务时不开启新任务
            if (LoadingVisibility != true)
            {
                InputText = input;
                PropertyChangeAsync(ExecuteInputCommand);
            }
        }

        base.OnNavigatedTo(navigationContext);
    }
}