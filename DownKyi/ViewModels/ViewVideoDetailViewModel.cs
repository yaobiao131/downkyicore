using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.CustomAction;
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
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels;

public class ViewVideoDetailViewModel : ViewModelBase
{
    public const string Tag = "PageVideoDetail";

    // 保存输入字符串，避免被用户修改
    private string _input;

    private IInfoService? _infoService;

    #region 页面属性申明

    private string? _inputText;

    public string? InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    private string _inputSearchText;

    public string InputSearchText
    {
        get => _inputSearchText;
        set => SetProperty(ref _inputSearchText, value);
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

    private VectorImage _downloadManage;

    public VectorImage DownloadManage
    {
        get => _downloadManage;
        set => SetProperty(ref _downloadManage, value);
    }

    private VideoInfoView? _videoInfoView;

    public VideoInfoView? VideoInfoView
    {
        get => _videoInfoView;
        set => SetProperty(ref _videoInfoView, value);
    }

    private ObservableCollection<VideoSection> _videoSections;

    public ObservableCollection<VideoSection> VideoSections
    {
        get => _videoSections;
        set => SetProperty(ref _videoSections, value);
    }

    public ObservableCollection<VideoSection> CaCheVideoSections { get; set; }

    private bool _isSelectAll;

    public bool IsSelectAll
    {
        get => _isSelectAll;
        set => SetProperty(ref _isSelectAll, value);
    }

    private bool _contentVisibility;

    public bool ContentVisibility
    {
        get => _contentVisibility;
        set => SetProperty(ref _contentVisibility, value);
    }

    private bool _noDataVisibility;

    public bool NoDataVisibility
    {
        get => _noDataVisibility;
        set => SetProperty(ref _noDataVisibility, value);
    }


    public ResetGridSplitterBehavior ResetGridBehavior { get; set; } = new();

    #endregion

    public ViewVideoDetailViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator, dialogService)
    {
        // 初始化loading
        Loading = true;
        LoadingVisibility = false;

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
    private DelegateCommand? _backSpaceCommand;

    public DelegateCommand BackSpaceCommand => _backSpaceCommand ??= new DelegateCommand(ExecuteBackSpace);

    /// <summary>
    /// 返回
    /// </summary>
    protected internal override void ExecuteBackSpace()
    {
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

    // 输入确认事件
    private DelegateCommand? _inputCommand;

    public DelegateCommand InputCommand => _inputCommand ??= new DelegateCommand(ExecuteInputCommand, CanExecuteInputCommand);


    private DelegateCommand? _inputSearchCommand;


    public DelegateCommand InputSearchCommand => _inputSearchCommand ??= new DelegateCommand(ExecuteInputSearchCommand);

    /// <summary>
    /// 搜索视频输入事件
    /// </summary>
    private async void ExecuteInputSearchCommand()
    {
        await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(InputSearchText))
            {
                foreach (var section in VideoSections)
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
                foreach (var section in VideoSections)
                {
                    var cache = CaCheVideoSections.FirstOrDefault(e => e.Id == section.Id);

                    if (cache == null) continue;

                    var pages = cache.VideoPages.Where(e => e.Name.Contains(InputSearchText)).ToList();
                    section.VideoPages = pages;
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
                if (string.IsNullOrEmpty(InputText))
                {
                    return;
                }

                LogManager.Debug(Tag, $"InputText: {InputText}");
                InputText = Regex.Replace(InputText, @"[【]*[^【]*[^】]*[】 ]", "");
                _input = InputText;

                // 更新页面
                UnityUpdateView(UpdateView, _input, null, true);

                // 是否自动解析视频
                if (SettingsManager.GetInstance().GetIsAutoParseVideo() == AllowStatus.Yes)
                {
                    PropertyChangeAsync(ExecuteParseAllVideoCommand);
                }
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("InputCommand()发生异常: {0}", e);
            LogManager.Error(Tag, e);
            EventAggregator.GetEvent<MessageEvent>().Publish(e.Message);

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
    private DelegateCommand? _copyCoverCommand;

    public DelegateCommand CopyCoverCommand => _copyCoverCommand ??= new DelegateCommand(ExecuteCopyCoverCommand);

    /// <summary>
    /// 复制封面事件
    /// </summary>
    private async void ExecuteCopyCoverCommand()
    {
        // 复制封面图片到剪贴板
        // Clipboard.SetImage(VideoInfoView.Cover);
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
        if (_videoInfoView?.CoverUrl == null) return;
        // 复制封面url到剪贴板
        await ClipboardManager.SetText(_videoInfoView.CoverUrl);
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
        NavigateToView.NavigateToViewUserSpace(EventAggregator, Tag, VideoInfoView.UpperMid);
    }

    // 视频章节选择事件
    private DelegateCommand<object>? _videoSectionsCommand;

    public DelegateCommand<object> VideoSectionsCommand => _videoSectionsCommand ??= new DelegateCommand<object>(ExecuteVideoSectionsCommand);

    /// <summary>
    /// 视频章节选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoSectionsCommand(object parameter)
    {
        if (parameter is not DataGrid grid)
        {
            return;
        }

        var selectedSection = VideoSections.FirstOrDefault(x => x.IsSelected);
        if (selectedSection?.VideoPages == null)
        {
            IsSelectAll = false;
            return;
        }

        var selectedPages = selectedSection.VideoPages
            .Where(x => x.IsSelected).ToList();
        foreach (var page in selectedPages)
        {
            grid.SelectedItems.Add(page);
        }

        IsSelectAll = selectedSection.VideoPages.Count > 0 &&
                      selectedPages.Count == selectedSection.VideoPages.Count;
    }

    // 视频page选择事件
    private DelegateCommand<IList>? _videoPagesCommand;

    public DelegateCommand<IList> VideoPagesCommand => _videoPagesCommand ??= new DelegateCommand<IList>(ExecuteVideoPagesCommand);

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

        var section = VideoSections.FirstOrDefault(item => item.IsSelected);

        if (section == null)
        {
            return;
        }

        var avids = new HashSet<long>(parameter.Cast<VideoPage>().Select(x => x.Cid));
        section.VideoPages.ToList().ForEach(videoPage =>
            videoPage.IsSelected = avids.Contains(videoPage.Cid)
        );
        IsSelectAll = section.VideoPages.Count == videoPages.Count && section.VideoPages.Count != 0;
    }

    // 全选事件
    private DelegateCommand<object>? _selectAllCommand;
    public DelegateCommand<object> SelectAllCommand => _selectAllCommand ??= new DelegateCommand<object>(ExecuteSelectAllCommand);

    /// <summary>
    /// 全选事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteSelectAllCommand(object parameter)
    {
        if (parameter is not DataGrid dataGrid)
        {
            return;
        }

        if (IsSelectAll)
        {
            dataGrid.SelectAll();
        }
        else
        {
            dataGrid.SelectedIndex = -1;
        }
    }


    // 解析视频流事件
    private DelegateCommand<object>? _parseCommand;

    public DelegateCommand<object> ParseCommand => _parseCommand ??= new DelegateCommand<object>(ExecuteParseCommand, CanExecuteParseCommand);

    /// <summary>
    /// 解析视频流事件
    /// </summary>
    /// <param name="parameter"></param>
    private async void ExecuteParseCommand(object parameter)
    {
        if (parameter is not VideoPage videoPage)
        {
            return;
        }

        LoadingVisibility = true;

        try
        {
            await Task.Run(() =>
            {
                LogManager.Debug(Tag, $"Video Page: {videoPage.Cid}");

                UnityUpdateView(ParseVideo, _input, videoPage, true);
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("ParseCommand()发生异常: {0}", e);
            LogManager.Error(Tag, e);
            EventAggregator.GetEvent<MessageEvent>().Publish(e.Message);

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
    private DelegateCommand? _parseAllVideoCommand;

    public DelegateCommand ParseAllVideoCommand => _parseAllVideoCommand ??= new DelegateCommand(ExecuteParseAllVideoCommand, CanExecuteParseAllVideoCommand);

    /// <summary>
    /// 解析所有视频流事件
    /// </summary>
    private async void ExecuteParseAllVideoCommand()
    {
        // 解析范围
        var parseScope = SettingsManager.GetInstance().GetParseScope();

        // 是否选择了解析范围
        if (parseScope == ParseScope.None)
        {
            //打开解析选择器
            await DialogService?.ShowDialogAsync(ViewParsingSelectorViewModel.Tag, null, async result =>
            {
                if (result.Result != ButtonResult.OK) return;
                // 选择的解析范围
                parseScope = result.Parameters.GetValue<ParseScope>("parseScope");
                await ExecuteParse(parseScope);
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
                    case ParseScope.None:
                        break;
                    case ParseScope.SelectedItem:
                        foreach (var section in VideoSections)
                        {
                            foreach (var page in section.VideoPages)
                            {
                                if (page.IsSelected)
                                {
                                    // 执行解析任务
                                    UnityUpdateView(ParseVideo, _input, page);
                                }
                            }
                        }

                        break;
                    case ParseScope.CurrentSection:
                        foreach (var section in VideoSections)
                        {
                            if (section.IsSelected)
                            {
                                foreach (var page in section.VideoPages)
                                {
                                    // 执行解析任务
                                    UnityUpdateView(ParseVideo, _input, page);
                                }
                            }
                        }

                        break;
                    case ParseScope.All:
                        foreach (var section in VideoSections)
                        {
                            foreach (var page in section.VideoPages)
                            {
                                // 执行解析任务
                                UnityUpdateView(ParseVideo, _input, page);
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
            EventAggregator.GetEvent<MessageEvent>().Publish(e.Message);

            LoadingVisibility = false;
        }

        LoadingVisibility = false;

        // 解析后是否自动下载解析视频
        var isAutoDownloadAll = SettingsManager.GetInstance().GetIsAutoDownloadAll();
        if (parseScope != ParseScope.None && isAutoDownloadAll == AllowStatus.Yes)
        {
            AddToDownload(true);
        }

        LogManager.Debug(Tag, $"ParseScope: {parseScope:G}");
    }

    // 添加到下载列表事件
    private DelegateCommand? _addToDownloadCommand;

    public DelegateCommand AddToDownloadCommand => _addToDownloadCommand ??= new DelegateCommand(ExecuteAddToDownloadCommand, CanExecuteAddToDownloadCommand);

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
        ResetGridBehavior.ResetGrid();
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
    private void UnityUpdateView(Action<IInfoService, VideoPage> action, string input, VideoPage page, bool refresh = false)
    {
        if (_infoService == null || refresh)
        {
            // 视频
            if (ParseEntrance.IsAvUrl(input) || ParseEntrance.IsBvUrl(input)
                                             || ParseEntrance.IsAvId(input) || ParseEntrance.IsBvId(input))
            {
                _infoService = new VideoInfoService(input);
            }

            // 番剧（电影、电视剧）
            if (ParseEntrance.IsBangumiSeasonUrl(input) || ParseEntrance.IsBangumiEpisodeUrl(input) ||
                ParseEntrance.IsBangumiMediaUrl(input))
            {
                _infoService = new BangumiInfoService(input);
            }

            // 课程
            if (ParseEntrance.IsCheeseSeasonUrl(input) || ParseEntrance.IsCheeseEpisodeUrl(input))
            {
                _infoService = new CheeseInfoService(input);
            }
        }

        if (_infoService == null)
        {
            return;
        }

        action(_infoService, page);
    }

    /// <summary>
    /// 更新页面
    /// </summary>
    /// <param name="videoInfoService"></param>
    /// <param name="param"></param>
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
        var videoSections = videoInfoService.GetVideoSections(false);

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

            var pages = videoInfoService.GetVideoPages();

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
    /// <param name="videoPage"></param>
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
        AddToDownloadService? addToDownloadService;
        // 视频
        if (ParseEntrance.IsAvUrl(_input) || ParseEntrance.IsBvUrl(_input))
        {
            addToDownloadService = new AddToDownloadService(PlayStreamType.Video);
        }
        // 番剧（电影、电视剧）
        else if (ParseEntrance.IsBangumiSeasonUrl(_input) || ParseEntrance.IsBangumiEpisodeUrl(_input) ||
                 ParseEntrance.IsBangumiMediaUrl(_input))
        {
            addToDownloadService = new AddToDownloadService(PlayStreamType.Bangumi);
        }
        // 课程
        else if (ParseEntrance.IsCheeseSeasonUrl(_input) || ParseEntrance.IsCheeseEpisodeUrl(_input))
        {
            addToDownloadService = new AddToDownloadService(PlayStreamType.Cheese);
        }
        else
        {
            return;
        }

        // 选择文件夹
        var directory = await addToDownloadService.SetDirectory(DialogService);

        // 视频计数
        var i = 0;
        await Task.Run(async () =>
        {
            // 传递video对象
            addToDownloadService.GetVideo(VideoInfoView, VideoSections.ToList());
            // 下载
            i = await addToDownloadService.AddToDownload(EventAggregator, DialogService, directory, isAll);
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
        DownloadManage = ButtonIcon.Instance().DownloadManage;
        DownloadManage.Height = 24;
        DownloadManage.Width = 24;
        DownloadManage.Fill = DictionaryResource.GetColor("ColorPrimary");
        // Parent参数为null时，表示是从下一个页面返回到本页面，不需要执行任务
        if (navigationContext.Parameters.GetValue<string>("Parent") != null)
        {
            var param = navigationContext.Parameters.GetValue<string>("Parameter");
            // 移除剪贴板id
            var input = param.Replace(AppConstant.ClipboardId, "");

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