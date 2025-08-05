using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.BiliApi.Zone;
using DownKyi.Core.FileName;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Utils;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.Utils;
using DownKyi.ViewModels.Dialogs;
using DownKyi.ViewModels.DownloadManager;
using DownKyi.ViewModels.PageViewModels;
using Prism.Events;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.Services.Download;

/// <summary>
/// 添加到下载列表服务
/// </summary>
public class AddToDownloadService
{
    private readonly string Tag = "AddToDownloadService";
    private IInfoService _videoInfoService;
    private VideoInfoView? _videoInfoView;
    private List<VideoSection>? _videoSections;
    private DownloadStorageService _downloadStorageService = (DownloadStorageService)App.Current.Container.Resolve(typeof(DownloadStorageService));

    // 下载内容
    private bool _downloadAudio = true;
    private bool _downloadVideo = true;
    private bool _downloadDanmaku = true;
    private bool _downloadSubtitle = true;
    private bool _downloadCover = true;

    /// <summary>
    /// 添加下载
    /// </summary>
    /// <param name="streamType"></param>
    public AddToDownloadService(PlayStreamType streamType)
    {
        switch (streamType)
        {
            case PlayStreamType.Video:
                _videoInfoService = new VideoInfoService(null);
                break;
            case PlayStreamType.Bangumi:
                _videoInfoService = new BangumiInfoService(null);
                break;
            case PlayStreamType.Cheese:
                _videoInfoService = new CheeseInfoService(null);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 添加下载
    /// </summary>
    /// <param name="id"></param>
    /// <param name="streamType"></param>
    public AddToDownloadService(string id, PlayStreamType streamType)
    {
        switch (streamType)
        {
            case PlayStreamType.Video:
                _videoInfoService = new VideoInfoService(id);
                break;
            case PlayStreamType.Bangumi:
                _videoInfoService = new BangumiInfoService(id);
                break;
            case PlayStreamType.Cheese:
                _videoInfoService = new CheeseInfoService(id);
                break;
            default:
                break;
        }
    }

    public void SetVideoInfoService(IInfoService videoInfoService)
    {
        _videoInfoService = videoInfoService;
    }

    public void GetVideo(VideoInfoView videoInfoView, List<VideoSection> videoSections)
    {
        _videoInfoView = videoInfoView;
        _videoSections = videoSections;
    }

    public void GetVideo()
    {
        _videoInfoView = _videoInfoService.GetVideoView();
        if (_videoInfoView == null)
        {
            LogManager.Debug(Tag, "VideoInfoView is null.");
            return;
        }

        _videoSections = _videoInfoService.GetVideoSections(true);
        if (_videoSections == null)
        {
            LogManager.Debug(Tag, "videoSections is not exist.");

            _videoSections = new List<VideoSection>
            {
                new()
                {
                    Id = 0,
                    Title = "default",
                    IsSelected = true,
                    VideoPages = _videoInfoService.GetVideoPages()
                }
            };
        }

        // 将所有视频设置为选中
        foreach (var section in _videoSections)
        {
            foreach (var item in section.VideoPages)
            {
                item.IsSelected = true;
            }
        }
    }

    /// <summary>
    /// 解析视频流
    /// </summary>
    /// <param name="videoInfoService"></param>
    public void ParseVideo(IInfoService videoInfoService)
    {
        if (_videoSections == null)
        {
            return;
        }

        foreach (var section in _videoSections)
        {
            foreach (var page in section.VideoPages)
            {
                // 执行解析任务
                videoInfoService.GetVideoStream(page);
            }
        }
    }

    /// <summary>
    /// 选择文件夹和下载项
    /// </summary>
    /// <param name="dialogService"></param>
    public async Task<string?> SetDirectory(IDialogService? dialogService)
    {
        if (dialogService == null) return null;
        // 选择的下载文件夹
        var directory = string.Empty;

        // 是否使用默认下载目录
        if (SettingsManager.GetInstance().GetIsUseSaveVideoRootPath() == AllowStatus.Yes)
        {
            // 下载内容
            var videoContent = SettingsManager.GetInstance().GetVideoContent();
            _downloadAudio = videoContent.DownloadAudio;
            _downloadVideo = videoContent.DownloadVideo;
            _downloadDanmaku = videoContent.DownloadDanmaku;
            _downloadSubtitle = videoContent.DownloadSubtitle;
            _downloadCover = videoContent.DownloadCover;

            directory = SettingsManager.GetInstance().GetSaveVideoRootPath();
        }
        else
        {
            // 打开文件夹选择器
            await dialogService.ShowDialogAsync(ViewDownloadSetterViewModel.Tag, null, result =>
            {
                if (result.Result != ButtonResult.OK) return;
                // 选择的下载文件夹
                directory = result.Parameters.GetValue<string>("directory");

                // 下载内容
                _downloadAudio = result.Parameters.GetValue<bool>("downloadAudio");
                _downloadVideo = result.Parameters.GetValue<bool>("downloadVideo");
                _downloadDanmaku = result.Parameters.GetValue<bool>("downloadDanmaku");
                _downloadSubtitle = result.Parameters.GetValue<bool>("downloadSubtitle");
                _downloadCover = result.Parameters.GetValue<bool>("downloadCover");
            });
        }

        if (directory == string.Empty)
        {
            return null;
        }


        if (!Directory.Exists(Directory.GetDirectoryRoot(directory)))
        {
            var alert = new AlertService(dialogService);
            await alert.ShowError(DictionaryResource.GetString("DriveNotFound"));

            directory = string.Empty;
        }

        // 下载设置dialog中如果点击取消或者关闭窗口，
        // 会返回空字符串，
        // 这时直接退出
        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        // 文件夹不存在则创建
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    /// <summary>
    /// 添加到下载列表
    /// </summary>
    /// <param name="eventAggregator">传递事件的对象</param>
    /// <param name="dialogService">dialog</param>
    /// <param name="directory">下载路径</param>
    /// <param name="isAll">是否下载所有，包括未选中项</param>
    /// <returns>添加的数量</returns>
    public async Task<int> AddToDownload(IEventAggregator eventAggregator, IDialogService? dialogService, string? directory, bool isAll = false)
    {
        if (string.IsNullOrEmpty(directory))
        {
            return -1;
        }

        if (_videoSections == null)
        {
            return -1;
        }

        // 视频计数
        var i = 0;
        // 添加到下载
        foreach (var section in _videoSections)
        {
            if (section.VideoPages == null)
            {
                continue;
            }

            foreach (var page in section.VideoPages)
            {
                // 只下载选中项，跳过未选中项
                if (!isAll && !page.IsSelected)
                {
                    continue;
                }

                // 没有解析的也跳过
                if (page.PlayUrl == null)
                {
                    continue;
                }

                // 判断VideoQuality
                var retry = 0;
                while (page.VideoQuality == null && retry < 5)
                {
                    // 执行解析任务
                    _videoInfoService.GetVideoStream(page);
                    retry++;
                }

                if (page.VideoQuality == null)
                {
                    continue;
                }

                // 判断是否同一个视频，需要cid、画质、音质、视频编码都相同

                // 如果存在正在下载列表，则跳过，并提示
                var isDownloading = false;


                foreach (var item in App.DownloadingList)
                {
                    if (item.DownloadBase == null)
                    {
                        continue;
                    }

                    bool f = item.DownloadBase.Cid == page.Cid &&
                             item.Resolution.Id == page.VideoQuality.Quality &&
                             item.VideoCodecName == page.VideoQuality.SelectedVideoCodec &&
                             (
                                 (page.PlayUrl.Dash != null && item.AudioCodec.Name == page.AudioQualityFormat) ||
                                 (page.PlayUrl.Dash == null && page.PlayUrl.Durl != null)
                             );

                    if (f)
                    {
                        eventAggregator.GetEvent<MessageEvent>()
                            .Publish($"{page.Name}{DictionaryResource.GetString("TipAlreadyToAddDownloading")}");
                        isDownloading = true;
                        break;
                    }
                }

                if (isDownloading)
                {
                    continue;
                }

                // TODO 如果存在下载完成列表，弹出选择框是否再次下载
                var isDownloaded = false;
                foreach (var item in App.DownloadedList)
                {
                    if (item.DownloadBase == null)
                    {
                        continue;
                    }

                    bool f = item.DownloadBase.Cid == page.Cid &&
                             item.Resolution.Id == page.VideoQuality.Quality &&
                             item.VideoCodecName == page.VideoQuality.SelectedVideoCodec &&
                             (
                                 (page.PlayUrl.Dash != null && item.AudioCodec.Name == page.AudioQualityFormat) ||
                                 (page.PlayUrl.Dash == null && page.PlayUrl.Durl != null)
                             );

                    if (f)
                    {
                        // eventAggregator.GetEvent<MessageEvent>().Publish($"{page.Name}{DictionaryResource.GetString("TipAlreadyToAddDownloaded")}");
                        // isDownloaded = true;
                        var repeatDownloadStrategy = SettingsManager.GetInstance().GetRepeatDownloadStrategy();
                        switch (repeatDownloadStrategy)
                        {
                            case RepeatDownloadStrategy.Ask:
                            {
                                var result = ButtonResult.Cancel;
                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    var param = new DialogParameters
                                    {
                                        { "message", $"{item.Name}已下载，是否重新下载" },
                                    };

                                    await dialogService.ShowDialogAsync(ViewAlreadyDownloadedDialogViewModel.Tag, param, buttonResult => { result = buttonResult.Result; });
                                });

                                if (result == ButtonResult.OK)
                                {
                                    App.PropertyChangeAsync(() =>
                                    {
                                        App.DownloadedList.Remove(item);
                                        _downloadStorageService.RemoveDownloaded(item);
                                    });
                                    isDownloaded = false;
                                }
                                else
                                {
                                    isDownloaded = true;
                                }

                                break;
                            }
                            case RepeatDownloadStrategy.ReDownload:
                                isDownloaded = false;
                                break;
                            case RepeatDownloadStrategy.JumpOver:
                                isDownloaded = true;
                                break;
                            default:
                                isDownloaded = true;
                                break;
                        }

                        break;
                    }
                }

                if (isDownloaded)
                {
                    continue;
                }

                // 视频分区
                var zoneId = -1;
                var zoneList = VideoZone.Instance().GetZones();
                var zone = zoneList.Find(it => it.Id == _videoInfoView?.TypeId);
                if (zone != null)
                {
                    if (zone.ParentId == 0)
                    {
                        zoneId = zone.Id;
                    }
                    else
                    {
                        var zoneParent = zoneList.Find(it => it.Id == zone.ParentId);
                        if (zoneParent != null)
                        {
                            zoneId = zoneParent.Id;
                        }
                    }
                }

                // 如果只有一个视频章节，则不在命名中出现
                var sectionName = string.Empty;
                if (_videoSections.Count > 1)
                {
                    sectionName = section.Title;
                }

                // 文件路径
                var fileNameParts = SettingsManager.GetInstance().GetFileNameParts();
                var fileName = FileName.Builder(fileNameParts)
                    .SetSection(Format.FormatFileName(sectionName))
                    .SetMainTitle(Format.FormatFileName(_videoInfoView.Title))
                    .SetPageTitle(Format.FormatFileName(page.Name))
                    .SetVideoZone(_videoInfoView.VideoZone.Split('>')[0])
                    .SetAudioQuality(page.AudioQualityFormat)
                    .SetVideoQuality(page.VideoQuality == null ? "" : page.VideoQuality.QualityFormat)
                    .SetVideoCodec(page.VideoQuality == null ? "" :
                        page.VideoQuality.SelectedVideoCodec.Contains("AVC") ? "AVC" :
                        page.VideoQuality.SelectedVideoCodec.Contains("HEVC") ? "HEVC" :
                        page.VideoQuality.SelectedVideoCodec.Contains("Dolby") ? "Dolby Vision" :
                        page.VideoQuality.SelectedVideoCodec.Contains("AV1") ? "AV1" : "")
                    .SetVideoPublishTime(page.PublishTime)
                    .SetAvid(page.Avid)
                    .SetBvid(page.Bvid)
                    .SetCid(page.Cid)
                    .SetUpMid(page.Owner.Mid)
                    .SetUpName(Format.FormatFileName(page.Owner.Name));

                // 序号设置
                var orderFormat = SettingsManager.GetInstance().GetOrderFormat();
                switch (orderFormat)
                {
                    case OrderFormat.Natural:
                        fileName.SetOrder(page.Order);
                        break;
                    case OrderFormat.LeadingZeros:
                        fileName.SetOrder(page.Order, section.VideoPages.Count);
                        break;
                }

                // 合成绝对路径
                var filePath = Path.Combine(directory, fileName.RelativePath());

                if (SettingsManager.GetInstance().IsRepeatFileAutoAddNumberSuffix())
                {
                    // 如果存在同名文件，自动重命名
                    // todo 如果重新下载呢。还没想好
                    var directoryName = Path.GetDirectoryName(filePath);
                    if (Directory.Exists(directoryName))
                    {
                        var files = Directory.GetFiles(directoryName).Select(Path.GetFileNameWithoutExtension).Distinct().ToList();

                        if (files.Contains(Path.GetFileNameWithoutExtension(filePath)))
                        {
                            var count = 1;
                            var newFilePath = filePath;
                            while (files.Contains(Path.GetFileNameWithoutExtension(newFilePath)))
                            {
                                newFilePath = Path.Combine(directory, $"{fileName.RelativePath()}({count})");
                                count++;
                            }

                            filePath = newFilePath;
                        }
                    }
                }

                // 视频类别
                PlayStreamType playStreamType;
                switch (_videoInfoView.TypeId)
                {
                    case -10:
                        playStreamType = PlayStreamType.Cheese;
                        break;
                    case 13:
                    case 23:
                    case 177:
                    case 167:
                    case 11:
                        playStreamType = PlayStreamType.Bangumi;
                        break;
                    case 1:
                    case 3:
                    case 129:
                    case 4:
                    case 36:
                    case 188:
                    case 234:
                    case 223:
                    case 160:
                    case 211:
                    case 217:
                    case 119:
                    case 155:
                    case 202:
                    case 5:
                    case 181:
                    default:
                        playStreamType = PlayStreamType.Video;
                        break;
                }

                // 添加到下载列表
                App.PropertyChangeAsync(() =>
                {
                    // 如果不存在，直接添加到下载列表
                    var downloadBase = new DownloadBase
                    {
                        Bvid = page.Bvid,
                        Avid = page.Avid,
                        Cid = page.Cid,
                        EpisodeId = page.EpisodeId,
                        CoverUrl = _videoInfoView.CoverUrl,
                        PageCoverUrl = page.FirstFrame,
                        ZoneId = zoneId,
                        FilePath = filePath,
                        Order = page.Order,
                        MainTitle = _videoInfoView.Title,
                        Name = page.Name,
                        Duration = page.Duration,
                        VideoCodecName = page.VideoQuality.SelectedVideoCodec,
                        Resolution = new Quality { Name = page.VideoQuality.QualityFormat, Id = page.VideoQuality.Quality },
                        AudioCodec = Constant.GetAudioQualities().FirstOrDefault(t => { return t.Name == page.AudioQualityFormat; }),
                        Page = page.Page
                    };
                    var downloading = new Downloading
                    {
                        PlayStreamType = playStreamType,
                        DownloadStatus = DownloadStatus.NotStarted,
                    };

                    // 需要下载的内容
                    downloadBase.NeedDownloadContent["downloadAudio"] = _downloadAudio;
                    downloadBase.NeedDownloadContent["downloadVideo"] = _downloadVideo;
                    downloadBase.NeedDownloadContent["downloadDanmaku"] = _downloadDanmaku;
                    downloadBase.NeedDownloadContent["downloadSubtitle"] = _downloadSubtitle;
                    downloadBase.NeedDownloadContent["downloadCover"] = _downloadCover;

                    var downloadingItem = new DownloadingItem
                    {
                        DownloadBase = downloadBase,
                        Downloading = downloading,
                        PlayUrl = page.PlayUrl,
                    };

                    if (SettingsManager.GetInstance().GetVideoContent()
                            .GenerateMovieMetadata && _downloadVideo)
                    {
                        downloadingItem.Metadata = BuildMovieMetadata(page);
                    }

                    _downloadStorageService.AddDownloading(downloadingItem);
                    App.DownloadingList.Add(downloadingItem);
                    Thread.Sleep(10);
                });
                i++;
            }
        }

        return i;
    }

    private MovieMetadata BuildMovieMetadata(VideoPage page)
    {
        var metadata = new MovieMetadata
        {
            Title = page.Name,
            Plot = _videoInfoView.Description,
            Year = page.OriginalPublishTime.Year.ToString(),
            Premiered = page.OriginalPublishTime.ToString("yyyy-MM-dd"),
            BilibiliId = new UniqueId("bilibili", page.Bvid),
            Actors = new List<Actor> { new(page.Owner.Name, page.Owner.Mid.ToString()) },
            Genres = _videoInfoView.VideoZone?.Split(">")?.ToList() ?? new List<string>(),
            Tags = page.LazyTags?.Value ?? new List<string>(),
            Ratings = _videoInfoView.Score != null
                ? new List<Rating>
                {
                    new()
                    {
                        IsDefault = true,
                        Max = 10,
                        Name = "bilibili",
                        Value = _videoInfoView.Score.Value
                    }
                }
                : new List<Rating>()
        };
        return metadata;
    }
}