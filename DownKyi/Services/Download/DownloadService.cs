using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.BiliApi.VideoStream.Models;
using DownKyi.Core.Danmaku2Ass;
using DownKyi.Core.FFMpeg;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using DownKyi.Images;
using DownKyi.Models;
using DownKyi.PrismExtension.Dialog;
using DownKyi.Utils;
using DownKyi.ViewModels.DownloadManager;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Services.Download;

public abstract class DownloadService
{
    protected string Tag = "DownloadService";

    // protected TaskbarIcon _notifyIcon;
    protected readonly IDialogService? DialogService;
    protected ObservableCollection<DownloadingItem> downloadingList;
    protected ObservableCollection<DownloadedItem> downloadedList;

    protected Task workTask;
    protected CancellationTokenSource tokenSource;
    protected CancellationToken cancellationToken;
    protected List<Task> downloadingTasks = new();

    protected readonly int retry = 5;
    protected readonly string nullMark = "<null>";

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="downloadingList"></param>
    /// <param name="downloadedList"></param>
    /// <param name="dialogService"></param>
    /// <returns></returns>
    public DownloadService(ObservableCollection<DownloadingItem> downloadingList, ObservableCollection<DownloadedItem> downloadedList, IDialogService? dialogService)
    {
        this.downloadingList = downloadingList;
        this.downloadedList = downloadedList;
        DialogService = dialogService;
    }

    protected PlayUrlDashVideo? BaseDownloadAudio(DownloadingItem downloading)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("WhileDownloading");
        downloading.DownloadContent = DictionaryResource.GetString("DownloadingAudio");
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        downloading.Progress = 0;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        // 如果没有Dash，返回null
        if (downloading.PlayUrl == null || downloading.PlayUrl.Dash == null)
        {
            return null;
        }

        // 如果audio列表没有内容，则返回null
        if (downloading.PlayUrl.Dash.Audio == null)
        {
            return null;
        }
        else if (downloading.PlayUrl.Dash.Audio.Count == 0)
        {
            return null;
        }

        // 根据音频id匹配
        PlayUrlDashVideo? downloadAudio = null;
        foreach (var audio in downloading.PlayUrl.Dash.Audio)
        {
            if (audio.Id == downloading.AudioCodec.Id)
            {
                downloadAudio = audio;
                break;
            }
        }

        // 避免Dolby==null及其它未知情况，直接使用异常捕获
        try
        {
            // Dolby Atmos
            if (downloading.AudioCodec.Id == 30250)
            {
                downloadAudio = downloading.PlayUrl.Dash.Dolby.Audio[0];
            }

            // Hi-Res无损
            if (downloading.AudioCodec.Id == 30251)
            {
                downloadAudio = downloading.PlayUrl.Dash.Flac.Audio;
            }
        }
        catch (Exception)
        {
        }

        return downloadAudio;
    }

    protected PlayUrlDashVideo? BaseDownloadVideo(DownloadingItem downloading)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("WhileDownloading");
        downloading.DownloadContent = DictionaryResource.GetString("DownloadingVideo");
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        downloading.Progress = 0;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        // 如果没有Dash，返回null
        if (downloading.PlayUrl == null || downloading.PlayUrl.Dash == null)
        {
            return null;
        }

        // 如果Video列表没有内容，则返回null
        if (downloading.PlayUrl.Dash.Video == null)
        {
            return null;
        }
        else if (downloading.PlayUrl.Dash.Video.Count == 0)
        {
            return null;
        }

        // 根据视频编码匹配
        PlayUrlDashVideo? downloadVideo = null;
        foreach (var video in downloading.PlayUrl.Dash.Video)
        {
            var codecs = Constant.GetCodecIds().FirstOrDefault(t => t.Id == video.CodecId);
            if (video.Id == downloading.Resolution.Id && codecs?.Name == downloading.VideoCodecName)
            {
                downloadVideo = video;
                break;
            }
        }

        return downloadVideo;
    }

    protected string BaseDownloadCover(DownloadingItem downloading, string coverUrl, string fileName)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("WhileDownloading");
        downloading.DownloadContent = DictionaryResource.GetString("DownloadingCover");
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        // 复制图片到指定位置
        try
        {
            if (coverUrl == null) return null;
            StorageUtils.DownloadImage(coverUrl, fileName);

            // 记录本次下载的文件
            downloading.Downloading.DownloadFiles.TryAdd(coverUrl, fileName);
            return fileName;
        }
        catch (Exception e)
        {
            Console.PrintLine($"{Tag}.DownloadCover()发生异常: {0}", e);
            LogManager.Error($"{Tag}.DownloadCover()", e);
        }

        return null;
    }

    protected string BaseDownloadDanmaku(DownloadingItem downloading)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("WhileDownloading");
        downloading.DownloadContent = DictionaryResource.GetString("DownloadingDanmaku");
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        var title = $"{downloading.Name}";
        var assFile = $"{downloading.DownloadBase.FilePath}.ass";

        // 记录本次下载的文件
        if (!downloading.Downloading.DownloadFiles.ContainsKey("danmaku"))
        {
            downloading.Downloading.DownloadFiles.Add("danmaku", assFile);
        }

        var screenWidth = SettingsManager.GetInstance().GetDanmakuScreenWidth();
        var screenHeight = SettingsManager.GetInstance().GetDanmakuScreenHeight();
        //if (SettingsManager.GetInstance().IsCustomDanmakuResolution() != AllowStatus.YES)
        //{
        //    if (downloadingEntity.Width > 0 && downloadingEntity.Height > 0)
        //    {
        //        screenWidth = downloadingEntity.Width;
        //        screenHeight = downloadingEntity.Height;
        //    }
        //}

        // 字幕配置
        var subtitleConfig = new Config
        {
            Title = title,
            ScreenWidth = screenWidth,
            ScreenHeight = screenHeight,
            FontName = SettingsManager.GetInstance().GetDanmakuFontName(),
            BaseFontSize = SettingsManager.GetInstance().GetDanmakuFontSize(),
            LineCount = SettingsManager.GetInstance().GetDanmakuLineCount(),
            LayoutAlgorithm =
                SettingsManager.GetInstance().GetDanmakuLayoutAlgorithm().ToString("G").ToLower(), // async/sync
            TuneDuration = 0,
            DropOffset = 0,
            BottomMargin = 0,
            CustomOffset = 0
        };

        Core.Danmaku2Ass.Bilibili.GetInstance()
            .SetTopFilter(SettingsManager.GetInstance().GetDanmakuTopFilter() == AllowStatus.Yes)
            .SetBottomFilter(SettingsManager.GetInstance().GetDanmakuBottomFilter() == AllowStatus.Yes)
            .SetScrollFilter(SettingsManager.GetInstance().GetDanmakuScrollFilter() == AllowStatus.Yes)
            .Create(downloading.DownloadBase.Avid, downloading.DownloadBase.Cid, subtitleConfig, assFile);

        return assFile;
    }

    protected List<string> BaseDownloadSubtitle(DownloadingItem downloading)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("WhileDownloading");
        downloading.DownloadContent = DictionaryResource.GetString("DownloadingSubtitle");
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        var srtFiles = new List<string>();

        var subRipTexts = VideoStream.GetSubtitle(downloading.DownloadBase.Avid, downloading.DownloadBase.Bvid, downloading.DownloadBase.Cid);
        if (subRipTexts == null)
        {
            return null;
        }

        foreach (var subRip in subRipTexts)
        {
            var srtFile = $"{downloading.DownloadBase.FilePath}_{subRip.LanDoc}.srt";
            try
            {
                File.WriteAllText(srtFile, subRip.SrtString);

                // 记录本次下载的文件
                downloading.Downloading.DownloadFiles.TryAdd("subtitle", srtFile);

                srtFiles.Add(srtFile);
            }
            catch (Exception e)
            {
                Console.PrintLine($"{Tag}.DownloadSubtitle()发生异常: {0}", e);
                LogManager.Error($"{Tag}.DownloadSubtitle()", e);
            }
        }

        // subRipTexts中第一个复制为不带后缀的字幕,保证能自动匹配到字幕
        if (srtFiles.Count > 0)
        {
            var srtFile = $"{downloading.DownloadBase.FilePath}.srt";
            File.Copy(srtFiles[0], srtFile,true);
            srtFiles.Add(srtFile);
        }

        return srtFiles;
    }

    protected string BaseMixedFlow(DownloadingItem downloading, string? audioUid, string? videoUid)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("MixedFlow");
        downloading.DownloadContent = DictionaryResource.GetString("DownloadingVideo");
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        //if (videoUid == nullMark)
        //{
        //    return null;
        //}

        var finalFile = $"{downloading.DownloadBase.FilePath}.mp4";
        if (videoUid == null)
        {
            finalFile = SettingsManager.GetInstance().GetIsTranscodingAacToMp3() == AllowStatus.Yes
                ? $"{downloading.DownloadBase.FilePath}.mp3"
                : downloading.AudioCodec.Id == 30251
                    ? $"{downloading.DownloadBase.FilePath}.flac"
                    : $"{downloading.DownloadBase.FilePath}.aac";
        }

        // 合并音视频
        FFMpeg.Instance.MergeVideo(audioUid, videoUid, finalFile);

        // 获取文件大小
        if (File.Exists(finalFile))
        {
            var info = new FileInfo(finalFile);
            downloading.FileSize = Format.FormatFileSize(info.Length);
        }
        else
        {
            downloading.FileSize = Format.FormatFileSize(0);
        }

        return finalFile;
    }

    protected void BaseParse(DownloadingItem downloading)
    {
        // 更新状态显示
        downloading.DownloadStatusTitle = DictionaryResource.GetString("Parsing");
        downloading.DownloadContent = string.Empty;
        // 下载大小
        downloading.DownloadingFileSize = string.Empty;
        downloading.Progress = 0;
        // 下载速度
        downloading.SpeedDisplay = string.Empty;

        if (downloading.PlayUrl != null && downloading.Downloading.DownloadStatus == DownloadStatus.NotStarted)
        {
            // 设置下载状态
            downloading.Downloading.DownloadStatus = DownloadStatus.Downloading;

            return;
        }

        // 设置下载状态
        downloading.Downloading.DownloadStatus = DownloadStatus.Downloading;

        // 解析
        switch (downloading.Downloading.PlayStreamType)
        {
            case PlayStreamType.Video:
                downloading.PlayUrl ??= SettingsManager.GetInstance().GetVideoParseType() switch
                {
                    0 => VideoStream.GetVideoPlayUrl(downloading.DownloadBase.Avid, downloading.DownloadBase.Bvid, downloading.DownloadBase.Cid),
                    1 => VideoStream.GetVideoPlayUrlWebPage(downloading.DownloadBase.Avid, downloading.DownloadBase.Bvid, downloading.DownloadBase.Cid,
                        downloading.DownloadBase.Page),
                    _ => throw new ArgumentException("Invalid video parse type. Valid values are: 0 (WebAPI) or 1 (WebPage).")
                };
                break;
            case PlayStreamType.Bangumi:
                downloading.PlayUrl ??= VideoStream.GetBangumiPlayUrl(downloading.DownloadBase.Avid,
                    downloading.DownloadBase.Bvid, downloading.DownloadBase.Cid);
                break;
            case PlayStreamType.Cheese:
                downloading.PlayUrl ??= VideoStream.GetCheesePlayUrl(downloading.DownloadBase.Avid,
                    downloading.DownloadBase.Bvid, downloading.DownloadBase.Cid,
                    downloading.DownloadBase.EpisodeId);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    protected async Task DoWork()
    {
        // 上次循环时正在下载的数量
        var lastDownloadingCount = 0;

        while (true)
        {
            var maxDownloading = SettingsManager.GetInstance().GetMaxCurrentDownloads();
            var downloadingCount = 0;

            try
            {
                downloadingTasks.RemoveAll((m) => m.IsCompleted);
                foreach (var downloading in downloadingList)
                {
                    if (downloading.Downloading.DownloadStatus == DownloadStatus.Downloading)
                    {
                        downloadingCount++;
                    }
                }

                foreach (var downloading in downloadingList)
                {
                    if (downloadingCount >= maxDownloading)
                    {
                        break;
                    }

                    // 开始下载
                    if (downloading.Downloading.DownloadStatus is not (DownloadStatus.NotStarted or DownloadStatus.WaitForDownload)) continue;
                    //这里需要立刻设置状态，否则如果SingleDownload没有及时执行，会重复创建任务
                    downloading.Downloading.DownloadStatus = DownloadStatus.Downloading;
                    downloadingTasks.Add(SingleDownload(downloading));
                    downloadingCount++;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.PrintLine($"{Tag}.DoWork()发生InvalidOperationException异常: {0}", e);
                LogManager.Error($"{Tag}.DoWork() InvalidOperationException", e);
            }
            catch (Exception e)
            {
                Console.PrintLine($"{Tag}.DoWork()发生异常: {0}", e);
                LogManager.Error($"{Tag}.DoWork()", e);
            }

            // 判断是否该结束线程，若为true，跳出while循环
            if (cancellationToken.IsCancellationRequested)
            {
                Console.PrintLine($"{Tag}.DoWork() 下载服务结束，跳出while循环");
                LogManager.Debug($"{Tag}.DoWork()", "下载服务结束");
                break;
            }

            // 判断下载列表中的视频是否全部下载完成
            if (lastDownloadingCount > 0 && downloadingList.Count == 0 && downloadedList.Count > 0)
            {
                AfterDownload();
            }

            lastDownloadingCount = downloadingList.Count;

            // 降低CPU占用
            await Task.Delay(500);
        }

        await Task.WhenAny(Task.WhenAll(downloadingTasks), Task.Delay(30000));
        foreach (var tsk in downloadingTasks.FindAll((m) => !m.IsCompleted))
        {
            Console.PrintLine($"{Tag}.DoWork() 任务结束超时");
            LogManager.Debug($"{Tag}.DoWork()", "任务结束超时");
        }
    }

    /// <summary>
    /// 下载一个视频
    /// </summary>
    /// <param name="downloading"></param>
    /// <returns></returns>
    private async Task SingleDownload(DownloadingItem downloading)
    {
        // 路径
        downloading.DownloadBase.FilePath = downloading.DownloadBase.FilePath.Replace("\\", "/");
        var temp = downloading.DownloadBase.FilePath.Split('/');
        //string path = downloading.DownloadBase.FilePath.Replace(temp[temp.Length - 1], "");
        var path = downloading.DownloadBase.FilePath.TrimEnd(temp[temp.Length - 1].ToCharArray());

        // 路径不存在则创建
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.PrintLine(Tag, e.ToString());
                LogManager.Debug(Tag, e.Message);

                var alertService = new AlertService(DialogService);
                var result = await alertService.ShowError($"{path}{DictionaryResource.GetString("DirectoryError")}");

                return;
            }
        }

        try
        {
            await Task.Run(() =>
            {
                // 初始化
                downloading.DownloadStatusTitle = string.Empty;
                downloading.DownloadContent = string.Empty;
                //downloading.Downloading.DownloadFiles.Clear();

                // 解析并依次下载音频、视频、弹幕、字幕、封面等内容
                Parse(downloading);

                // 暂停
                Pause(downloading);

                string? audioUid = null;
                // 如果需要下载音频
                if (downloading.DownloadBase.NeedDownloadContent["downloadAudio"])
                {
                    //audioUid = DownloadAudio(downloading);
                    for (var i = 0; i < retry; i++)
                    {
                        audioUid = DownloadAudio(downloading);
                        if (audioUid != null && audioUid != nullMark)
                        {
                            break;
                        }
                    }
                }

                if (audioUid == nullMark)
                {
                    DownloadFailed(downloading);
                    return;
                }

                // 暂停
                Pause(downloading);

                string? videoUid = null;
                // 如果需要下载视频
                if (downloading.DownloadBase.NeedDownloadContent["downloadVideo"])
                {
                    //videoUid = DownloadVideo(downloading);
                    for (var i = 0; i < retry; i++)
                    {
                        videoUid = DownloadVideo(downloading);
                        if (videoUid != null && videoUid != nullMark)
                        {
                            break;
                        }
                    }
                }

                if (videoUid == nullMark)
                {
                    DownloadFailed(downloading);
                    return;
                }

                // 暂停
                Pause(downloading);

                string? outputDanmaku = null;
                // 如果需要下载弹幕
                if (downloading.DownloadBase.NeedDownloadContent["downloadDanmaku"])
                {
                    outputDanmaku = DownloadDanmaku(downloading);
                }

                // 暂停
                Pause(downloading);

                List<string>? outputSubtitles = null;
                // 如果需要下载字幕
                if (downloading.DownloadBase.NeedDownloadContent["downloadSubtitle"])
                {
                    outputSubtitles = DownloadSubtitle(downloading);
                }

                // 暂停
                Pause(downloading);

                string? outputCover = null;
                string? outputPageCover = null;
                // 如果需要下载封面
                if (downloading.DownloadBase.NeedDownloadContent["downloadCover"])
                {
                    // page的封面
                    var pageCoverFileName = $"{downloading.DownloadBase.FilePath}.{GetImageExtension(downloading.DownloadBase.PageCoverUrl)}";
                    outputPageCover = DownloadCover(downloading, downloading.DownloadBase.PageCoverUrl, pageCoverFileName);


                    var coverFileName = $"{downloading.DownloadBase.FilePath}.Cover.{GetImageExtension(downloading.DownloadBase.CoverUrl)}";
                    // 封面
                    //outputCover = DownloadCover(downloading, downloading.DownloadBase.CoverUrl, $"{path}/Cover.{GetImageExtension(downloading.DownloadBase.CoverUrl)}");
                    outputCover = DownloadCover(downloading, downloading.DownloadBase.CoverUrl, coverFileName);
                }

                // 暂停
                Pause(downloading);

                // 混流
                var outputMedia = string.Empty;
                if (downloading.DownloadBase.NeedDownloadContent["downloadAudio"] || downloading.DownloadBase.NeedDownloadContent["downloadVideo"])
                {
                    outputMedia = MixedFlow(downloading, audioUid, videoUid);
                }

                // 这里本来只有IsExist，没有pause，不知道怎么处理
                // 是否存在
                //isExist = IsExist(downloading);
                //if (!isExist.Result)
                //{
                //    return;
                //}

                // 检测音频、视频是否下载成功
                var isMediaSuccess = true;
                if (downloading.DownloadBase.NeedDownloadContent["downloadAudio"] || downloading.DownloadBase.NeedDownloadContent["downloadVideo"])
                {
                    // 只有下载音频不下载视频时才输出aac
                    // 只要下载视频就输出mp4
                    // 成功
                    isMediaSuccess = File.Exists(outputMedia);
                }

                // 检测弹幕是否下载成功
                var isDanmakuSuccess = true;
                if (downloading.DownloadBase.NeedDownloadContent["downloadDanmaku"])
                {
                    // 成功
                    isDanmakuSuccess = File.Exists(outputDanmaku);
                }

                // 检测字幕是否下载成功
                var isSubtitleSuccess = true;
                if (downloading.DownloadBase.NeedDownloadContent["downloadSubtitle"])
                {
                    if (outputSubtitles == null)
                    {
                        // 为null时表示不存在字幕
                    }
                    else
                    {
                        foreach (var subtitle in outputSubtitles)
                        {
                            if (!File.Exists(subtitle))
                            {
                                // 如果有一个不存在则失败
                                isSubtitleSuccess = false;
                            }
                        }
                    }
                }

                // 检测封面是否下载成功
                var isCover = true;
                if (downloading.DownloadBase.NeedDownloadContent["downloadCover"])
                {
                    if (File.Exists(outputCover) || File.Exists(outputPageCover))
                    {
                        // 成功
                        isCover = true;
                    }
                    else
                    {
                        isCover = false;
                    }
                }

                if (!isMediaSuccess || !isDanmakuSuccess || !isSubtitleSuccess || !isCover)
                {
                    DownloadFailed(downloading);
                    return;
                }

                // 下载完成后处理
                var downloaded = new Downloaded
                {
                    MaxSpeedDisplay = Format.FormatSpeed(downloading.Downloading.MaxSpeed),
                };
                // 设置完成时间
                downloaded.SetFinishedTimestamp(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());

                var downloadedItem = new DownloadedItem
                {
                    DownloadBase = downloading.DownloadBase,
                    Downloaded = downloaded
                };

                App.PropertyChangeAsync(() =>
                {
                    // 加入到下载完成list中，并从下载中list去除
                    downloadedList.Add(downloadedItem);
                    downloadingList.Remove(downloading);

                    // 下载完成列表排序
                    var finishedSort = SettingsManager.GetInstance().GetDownloadFinishedSort();
                    App.SortDownloadedList(finishedSort);
                });
                // _notifyIcon.ShowBalloonTip(DictionaryResource.GetString("DownloadSuccess"), $"{downloadedItem.DownloadBase.Name}", BalloonIcon.Info);
            });
        }
        catch (OperationCanceledException e)
        {
            Console.PrintLine(Tag, e.ToString());
            LogManager.Debug(Tag, e.Message);
        }
    }

    /// <summary>
    /// 下载失败后的处理
    /// </summary>
    /// <param name="downloading"></param>
    protected void DownloadFailed(DownloadingItem downloading)
    {
        downloading.DownloadStatusTitle = DictionaryResource.GetString("DownloadFailed");
        downloading.DownloadContent = string.Empty;
        downloading.DownloadingFileSize = string.Empty;
        downloading.SpeedDisplay = string.Empty;
        downloading.Progress = 0;

        downloading.Downloading.DownloadStatus = DownloadStatus.DownloadFailed;
        downloading.StartOrPause = ButtonIcon.Instance().Retry;
        downloading.StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
    }

    /// <summary>
    /// 获取图片的扩展名
    /// </summary>
    /// <param name="coverUrl"></param>
    /// <returns></returns>
    protected string GetImageExtension(string? coverUrl)
    {
        if (coverUrl == null)
        {
            return string.Empty;
        }

        // 图片的扩展名
        var temp = coverUrl.Split('.');
        var fileExtension = temp[^1];
        return fileExtension;
    }

    /// <summary>
    /// 下载完成后的操作
    /// </summary>
    protected void AfterDownload()
    {
        var operation = SettingsManager.GetInstance().GetAfterDownloadOperation();
        switch (operation)
        {
            case AfterDownloadOperation.None:
                // 没有操作
                break;
            case AfterDownloadOperation.OpenFolder:
                // 打开文件夹
                break;
            case AfterDownloadOperation.CloseApp:
                // 关闭程序
                App.PropertyChangeAsync(() =>
                {
                    // System.Windows.Application.Current.Shutdown();
                });
                break;
            case AfterDownloadOperation.CloseSystem:
                // 关机
                // Process.Start("shutdown.exe", "-s");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 停止基本下载服务(转换await和Task.Wait两种调用形式)
    /// </summary>
    protected async Task BaseEndTask()
    {
        // 结束任务
        tokenSource.Cancel();

        await workTask;

        //先简单等待一下

        // 下载数据存储服务
        var downloadStorageService = new DownloadStorageService();
        // 保存数据
        foreach (var item in downloadingList)
        {
            switch (item.Downloading.DownloadStatus)
            {
                case DownloadStatus.NotStarted:
                    break;
                case DownloadStatus.WaitForDownload:
                    break;
                case DownloadStatus.PauseStarted:
                    break;
                case DownloadStatus.Pause:
                    break;
                case DownloadStatus.Downloading:
                    // TODO 添加设置让用户选择重启后是否自动开始下载
                    item.Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                    //item.Downloading.DownloadStatus = DownloadStatus.PAUSE;
                    break;
                case DownloadStatus.DownloadSucceed:
                case DownloadStatus.DownloadFailed:
                    break;
                default:
                    break;
            }

            item.Progress = 0;

            downloadStorageService.UpdateDownloading(item);
        }

        foreach (var item in downloadedList)
        {
            downloadStorageService.UpdateDownloaded(item);
        }
    }

    /// <summary>
    /// 启动基本下载服务
    /// </summary>
    protected void BaseStart()
    {
        tokenSource = new CancellationTokenSource();
        cancellationToken = tokenSource.Token;
        // _notifyIcon = new TaskbarIcon();
        // _notifyIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/favicon.ico"));

        workTask = Task.Run(DoWork);
    }

    #region 抽象接口函数

    public abstract void Parse(DownloadingItem downloading);
    public abstract string DownloadAudio(DownloadingItem downloading);
    public abstract string DownloadVideo(DownloadingItem downloading);
    public abstract string DownloadDanmaku(DownloadingItem downloading);
    public abstract List<string> DownloadSubtitle(DownloadingItem downloading);
    public abstract string DownloadCover(DownloadingItem downloading, string coverUrl, string fileName);
    public abstract string MixedFlow(DownloadingItem downloading, string? audioUid, string? videoUid);

    protected abstract void Pause(DownloadingItem downloading);

    #endregion
}