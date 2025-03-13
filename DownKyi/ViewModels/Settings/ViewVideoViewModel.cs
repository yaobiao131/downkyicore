using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.FileName;
using DownKyi.Core.Settings;
using DownKyi.Core.Settings.Models;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.Settings;

public class ViewVideoViewModel : ViewModelBase
{
    public const string Tag = "PageSettingsVideo";

    private bool _isOnNavigatedTo;

    #region 页面属性申明

    private List<Quality> _videoCodecs;

    public List<Quality> VideoCodecs
    {
        get => _videoCodecs;
        set => SetProperty(ref _videoCodecs, value);
    }

    private Quality _selectedVideoCodec;

    public Quality SelectedVideoCodec
    {
        get => _selectedVideoCodec;
        set => SetProperty(ref _selectedVideoCodec, value);
    }

    private List<Quality> _videoQualityList;

    public List<Quality> VideoQualityList
    {
        get => _videoQualityList;
        set => SetProperty(ref _videoQualityList, value);
    }

    private Quality _selectedVideoQuality;

    public Quality SelectedVideoQuality
    {
        get => _selectedVideoQuality;
        set => SetProperty(ref _selectedVideoQuality, value);
    }

    private List<Quality> _audioQualityList;

    public List<Quality> AudioQualityList
    {
        get => _audioQualityList;
        set => SetProperty(ref _audioQualityList, value);
    }

    private Quality _selectedAudioQuality;

    public Quality SelectedAudioQuality
    {
        get => _selectedAudioQuality;
        set => SetProperty(ref _selectedAudioQuality, value);
    }

    private List<VideoParseType> _videoParseTypeList;

    public List<VideoParseType> VideoParseTypeList
    {
        get => _videoParseTypeList;
        set => SetProperty(ref _videoParseTypeList, value);
    }

    private VideoParseType _selectedVideoParseType;

    public VideoParseType SelectedVideoParseType
    {
        get => _selectedVideoParseType;
        set => SetProperty(ref _selectedVideoParseType, value);
    }

    private bool _isTranscodingFlvToMp4;

    public bool IsTranscodingFlvToMp4
    {
        get => _isTranscodingFlvToMp4;
        set => SetProperty(ref _isTranscodingFlvToMp4, value);
    }

    private bool _isTranscodingAacToMp3;

    public bool IsTranscodingAacToMp3
    {
        get => _isTranscodingAacToMp3;
        set => SetProperty(ref _isTranscodingAacToMp3, value);
    }

    private bool _isUseDefaultDirectory;

    public bool IsUseDefaultDirectory
    {
        get => _isUseDefaultDirectory;
        set => SetProperty(ref _isUseDefaultDirectory, value);
    }

    private string _saveVideoDirectory;

    public string SaveVideoDirectory
    {
        get => _saveVideoDirectory;
        set => SetProperty(ref _saveVideoDirectory, value);
    }

    private bool _downloadAll;

    public bool DownloadAll
    {
        get => _downloadAll;
        set => SetProperty(ref _downloadAll, value);
    }

    private bool _downloadAudio;

    public bool DownloadAudio
    {
        get => _downloadAudio;
        set => SetProperty(ref _downloadAudio, value);
    }

    private bool _downloadVideo;

    public bool DownloadVideo
    {
        get => _downloadVideo;
        set => SetProperty(ref _downloadVideo, value);
    }

    private bool _downloadDanmaku;

    public bool DownloadDanmaku
    {
        get => _downloadDanmaku;
        set => SetProperty(ref _downloadDanmaku, value);
    }

    private bool _downloadSubtitle;

    public bool DownloadSubtitle
    {
        get => _downloadSubtitle;
        set => SetProperty(ref _downloadSubtitle, value);
    }

    private bool _downloadCover;

    public bool DownloadCover
    {
        get => _downloadCover;
        set => SetProperty(ref _downloadCover, value);
    }

    private ObservableCollection<DisplayFileNamePart> _selectedFileName;

    public ObservableCollection<DisplayFileNamePart> SelectedFileName
    {
        get => _selectedFileName;
        set => SetProperty(ref _selectedFileName, value);
    }

    private ObservableCollection<DisplayFileNamePart> _optionalFields;

    public ObservableCollection<DisplayFileNamePart> OptionalFields
    {
        get => _optionalFields;
        set => SetProperty(ref _optionalFields, value);
    }

    private int _selectedOptionalField;

    public int SelectedOptionalField
    {
        get => _selectedOptionalField;
        set => SetProperty(ref _selectedOptionalField, value);
    }

    private List<string> _fileNamePartTimeFormatList;

    public List<string> FileNamePartTimeFormatList
    {
        get => _fileNamePartTimeFormatList;
        set => SetProperty(ref _fileNamePartTimeFormatList, value);
    }

    private string _selectedFileNamePartTimeFormat;

    public string SelectedFileNamePartTimeFormat
    {
        get => _selectedFileNamePartTimeFormat;
        set => SetProperty(ref _selectedFileNamePartTimeFormat, value);
    }

    private List<OrderFormatDisplay> _orderFormatList;

    public List<OrderFormatDisplay> OrderFormatList
    {
        get => _orderFormatList;
        set => SetProperty(ref _orderFormatList, value);
    }

    private OrderFormatDisplay _orderFormatDisplay;

    public OrderFormatDisplay OrderFormatDisplay
    {
        get => _orderFormatDisplay;
        set => SetProperty(ref _orderFormatDisplay, value);
    }

    #endregion

    public ViewVideoViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        // 优先下载的视频编码
        VideoCodecs = Constant.GetCodecIds();
        //VideoCodecs = new List<string>
        //{
        //    "H.264/AVC",
        //    "H.265/HEVC",
        //};

        // 优先下载画质
        VideoQualityList = Constant.GetResolutions();

        // 优先下载音质
        AudioQualityList = Constant.GetAudioQualities();
        //AudioQualityList.RemoveAt(3);
        AudioQualityList[3].Id += 1000;
        AudioQualityList[4].Id += 1000;

        // 首选视频解析方式
        VideoParseTypeList = new List<VideoParseType>
        {
            new() { Name = "API(解析快、易风控)", Id = 0 },
            new() { Name = "WebPage(解析慢、不易风控)", Id = 1 },
        };

        // 文件命名格式
        SelectedFileName = new ObservableCollection<DisplayFileNamePart>();

        SelectedFileName.CollectionChanged += (sender, e) =>
        {
            // 当前显示的命名格式part
            var fileName = SelectedFileName.Select(item => item.Id).ToList();

            var isSucceed = SettingsManager.GetInstance().SetFileNameParts(fileName);
            PublishTip(isSucceed);
        };

        OptionalFields = new ObservableCollection<DisplayFileNamePart>();
        foreach (FileNamePart item in Enum.GetValues(typeof(FileNamePart)))
        {
            var display = DisplayFileNamePart(item);
            OptionalFields.Add(new DisplayFileNamePart { Id = item, Title = display });
        }

        SelectedOptionalField = -1;

        // 文件命名中的时间格式
        FileNamePartTimeFormatList = new List<string>
        {
            "yyyy-MM-dd",
            "yyyy.MM.dd",
        };

        // 文件命名中的序号格式
        OrderFormatList = new List<OrderFormatDisplay>
        {
            new() { Name = DictionaryResource.GetString("OrderFormatNatural"), OrderFormat = OrderFormat.Natural },
            new()
            {
                Name = DictionaryResource.GetString("OrderFormatLeadingZeros"), OrderFormat = OrderFormat.LeadingZeros
            },
        };

        #endregion
    }

    /// <summary>
    /// 导航到页面时执行
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        _isOnNavigatedTo = true;

        // 优先下载的视频编码
        var videoCodecs = SettingsManager.GetInstance().GetVideoCodecs();
        //SelectedVideoCodec = GetVideoCodecsString(videoCodecs);
        SelectedVideoCodec = VideoCodecs.FirstOrDefault(t => { return t.Id == videoCodecs; });

        // 优先下载画质
        var quality = SettingsManager.GetInstance().GetQuality();
        SelectedVideoQuality = VideoQualityList.FirstOrDefault(t => t.Id == quality);

        // 优先下载音质
        var audioQuality = SettingsManager.GetInstance().GetAudioQuality();
        SelectedAudioQuality = AudioQualityList.FirstOrDefault(t => t.Id == audioQuality);

        // 首选视频解析方式
        var videoParseType = SettingsManager.GetInstance().GetVideoParseType();
        SelectedVideoParseType = VideoParseTypeList.FirstOrDefault(t => t.Id == videoParseType);

        // 是否下载flv视频后转码为mp4
        var isTranscodingFlvToMp4 = SettingsManager.GetInstance().GetIsTranscodingFlvToMp4();
        IsTranscodingFlvToMp4 = isTranscodingFlvToMp4 == AllowStatus.Yes;

        // 是否下载aac音频后转码为mp3
        var isTranscodingAacToMp3 = SettingsManager.GetInstance().GetIsTranscodingAacToMp3();
        IsTranscodingAacToMp3 = isTranscodingAacToMp3 == AllowStatus.Yes;

        // 是否使用默认下载目录
        var isUseSaveVideoRootPath = SettingsManager.GetInstance().GetIsUseSaveVideoRootPath();
        IsUseDefaultDirectory = isUseSaveVideoRootPath == AllowStatus.Yes;

        // 默认下载目录
        SaveVideoDirectory = SettingsManager.GetInstance().GetSaveVideoRootPath();

        // 下载内容
        var videoContent = SettingsManager.GetInstance().GetVideoContent();

        DownloadAudio = videoContent.DownloadAudio;
        DownloadVideo = videoContent.DownloadVideo;
        DownloadDanmaku = videoContent.DownloadDanmaku;
        DownloadSubtitle = videoContent.DownloadSubtitle;
        DownloadCover = videoContent.DownloadCover;

        if (DownloadAudio && DownloadVideo && DownloadDanmaku && DownloadSubtitle && DownloadCover)
        {
            DownloadAll = true;
        }
        else
        {
            DownloadAll = false;
        }

        // 文件命名格式
        var fileNameParts = SettingsManager.GetInstance().GetFileNameParts();
        SelectedFileName.Clear();
        foreach (var item in fileNameParts)
        {
            var display = DisplayFileNamePart(item);
            SelectedFileName.Add(new DisplayFileNamePart { Id = item, Title = display });
        }

        // 文件命名中的时间格式
        SelectedFileNamePartTimeFormat = SettingsManager.GetInstance().GetFileNamePartTimeFormat();

        // 文件命名中的序号格式
        var orderFormat = SettingsManager.GetInstance().GetOrderFormat();
        OrderFormatDisplay = OrderFormatList.FirstOrDefault(t => { return t.OrderFormat == orderFormat; });

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 优先下载的视频编码事件
    private DelegateCommand<object>? _videoCodecsCommand;

    public DelegateCommand<object> VideoCodecsCommand => _videoCodecsCommand ??= new DelegateCommand<object>(ExecuteVideoCodecsCommand);

    /// <summary>
    /// 优先下载的视频编码事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoCodecsCommand(object parameter)
    {
        //VideoCodecs videoCodecs = GetVideoCodecs(parameter);

        if (parameter is not Quality videoCodecs)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetVideoCodecs(videoCodecs.Id);
        PublishTip(isSucceed);
    }

    // 优先下载画质事件
    private DelegateCommand<object>? _videoQualityCommand;

    public DelegateCommand<object> VideoQualityCommand => _videoQualityCommand ??= new DelegateCommand<object>(ExecuteVideoQualityCommand);

    /// <summary>
    /// 优先下载画质事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoQualityCommand(object parameter)
    {
        if (parameter is not Quality resolution)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetQuality(resolution.Id);
        PublishTip(isSucceed);
    }

    // 优先下载音质事件
    private DelegateCommand<object>? _audioQualityCommand;

    public DelegateCommand<object> AudioQualityCommand => _audioQualityCommand ??= new DelegateCommand<object>(ExecuteAudioQualityCommand);

    /// <summary>
    /// 优先下载音质事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteAudioQualityCommand(object parameter)
    {
        if (parameter is not Quality quality)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetAudioQuality(quality.Id);
        PublishTip(isSucceed);
    }


    // 首选视频解析线路事件
    private DelegateCommand<object>? _videoParseTypeCommand;

    public DelegateCommand<object> VideoParseTypeCommand => _videoParseTypeCommand ??= new DelegateCommand<object>(ExecuteVideoParseTypeCommand);

    /// <summary>
    /// 首选视频解析线路事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteVideoParseTypeCommand(object parameter)
    {
        if (parameter is not VideoParseType type)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetVideoParseType(type.Id ?? 1);
        PublishTip(isSucceed);
    }

    // 是否下载flv视频后转码为mp4事件
    private DelegateCommand? _isTranscodingFlvToMp4Command;

    public DelegateCommand IsTranscodingFlvToMp4Command => _isTranscodingFlvToMp4Command ??= new DelegateCommand(ExecuteIsTranscodingFlvToMp4Command);

    /// <summary>
    /// 是否下载flv视频后转码为mp4事件
    /// </summary>
    private void ExecuteIsTranscodingFlvToMp4Command()
    {
        var isTranscodingFlvToMp4 = IsTranscodingFlvToMp4 ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetIsTranscodingFlvToMp4(isTranscodingFlvToMp4);
        PublishTip(isSucceed);
    }

    // 是否下载aac音频后转码为mp3事件
    private DelegateCommand? _isTranscodingAacToMp3Command;

    public DelegateCommand IsTranscodingAacToMp3Command => _isTranscodingAacToMp3Command ??= new DelegateCommand(ExecuteIsTranscodingAacToMp3Command);

    /// <summary>
    /// 是否下载aac音频后转码为mp3事件
    /// </summary>
    private void ExecuteIsTranscodingAacToMp3Command()
    {
        var isTranscodingAacToMp3 = IsTranscodingAacToMp3 ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetIsTranscodingAacToMp3(isTranscodingAacToMp3);
        PublishTip(isSucceed);
    }

    // 是否使用默认下载目录事件
    private DelegateCommand? _isUseDefaultDirectoryCommand;

    public DelegateCommand IsUseDefaultDirectoryCommand => _isUseDefaultDirectoryCommand ??= new DelegateCommand(ExecuteIsUseDefaultDirectoryCommand);

    /// <summary>
    /// 是否使用默认下载目录事件
    /// </summary>
    private void ExecuteIsUseDefaultDirectoryCommand()
    {
        var isUseDefaultDirectory = IsUseDefaultDirectory ? AllowStatus.Yes : AllowStatus.No;

        var isSucceed = SettingsManager.GetInstance().SetIsUseSaveVideoRootPath(isUseDefaultDirectory);
        PublishTip(isSucceed);
    }

    // 修改默认下载目录事件
    private DelegateCommand? _changeSaveVideoDirectoryCommand;

    public DelegateCommand ChangeSaveVideoDirectoryCommand => _changeSaveVideoDirectoryCommand ??= new DelegateCommand(ExecuteChangeSaveVideoDirectoryCommand);

    /// <summary>
    /// 修改默认下载目录事件
    /// </summary>
    private async void ExecuteChangeSaveVideoDirectoryCommand()
    {
        var directory = await DialogUtils.SetDownloadDirectory();
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetSaveVideoRootPath(directory);
        PublishTip(isSucceed);

        if (isSucceed)
        {
            SaveVideoDirectory = directory;
        }
    }

    // 所有内容选择事件
    private DelegateCommand? _downloadAllCommand;

    public DelegateCommand DownloadAllCommand => _downloadAllCommand ??= new DelegateCommand(ExecuteDownloadAllCommand);

    /// <summary>
    /// 所有内容选择事件
    /// </summary>
    private void ExecuteDownloadAllCommand()
    {
        if (DownloadAll)
        {
            DownloadAudio = true;
            DownloadVideo = true;
            DownloadDanmaku = true;
            DownloadSubtitle = true;
            DownloadCover = true;
        }
        else
        {
            DownloadAudio = false;
            DownloadVideo = false;
            DownloadDanmaku = false;
            DownloadSubtitle = false;
            DownloadCover = false;
        }

        SetVideoContent();
    }

    // 音频选择事件
    private DelegateCommand? _downloadAudioCommand;

    public DelegateCommand DownloadAudioCommand => _downloadAudioCommand ??= new DelegateCommand(ExecuteDownloadAudioCommand);

    /// <summary>
    /// 音频选择事件
    /// </summary>
    private void ExecuteDownloadAudioCommand()
    {
        if (!DownloadAudio)
        {
            DownloadAll = false;
        }

        if (DownloadAudio && DownloadVideo && DownloadDanmaku && DownloadSubtitle && DownloadCover)
        {
            DownloadAll = true;
        }

        SetVideoContent();
    }

    // 视频选择事件
    private DelegateCommand? _downloadVideoCommand;

    public DelegateCommand DownloadVideoCommand => _downloadVideoCommand ??= new DelegateCommand(ExecuteDownloadVideoCommand);

    /// <summary>
    /// 视频选择事件
    /// </summary>
    private void ExecuteDownloadVideoCommand()
    {
        if (!DownloadVideo)
        {
            DownloadAll = false;
        }

        if (DownloadAudio && DownloadVideo && DownloadDanmaku && DownloadSubtitle && DownloadCover)
        {
            DownloadAll = true;
        }

        SetVideoContent();
    }

    // 弹幕选择事件
    private DelegateCommand? _downloadDanmakuCommand;

    public DelegateCommand DownloadDanmakuCommand => _downloadDanmakuCommand ??= new DelegateCommand(ExecuteDownloadDanmakuCommand);

    /// <summary>
    /// 弹幕选择事件
    /// </summary>
    private void ExecuteDownloadDanmakuCommand()
    {
        if (!DownloadDanmaku)
        {
            DownloadAll = false;
        }

        if (DownloadAudio && DownloadVideo && DownloadDanmaku && DownloadSubtitle && DownloadCover)
        {
            DownloadAll = true;
        }

        SetVideoContent();
    }

    // 字幕选择事件
    private DelegateCommand? _downloadSubtitleCommand;

    public DelegateCommand DownloadSubtitleCommand => _downloadSubtitleCommand ??= new DelegateCommand(ExecuteDownloadSubtitleCommand);

    /// <summary>
    /// 字幕选择事件
    /// </summary>
    private void ExecuteDownloadSubtitleCommand()
    {
        if (!DownloadSubtitle)
        {
            DownloadAll = false;
        }

        if (DownloadAudio && DownloadVideo && DownloadDanmaku && DownloadSubtitle && DownloadCover)
        {
            DownloadAll = true;
        }

        SetVideoContent();
    }

    // 封面选择事件
    private DelegateCommand? _downloadCoverCommand;

    public DelegateCommand DownloadCoverCommand => _downloadCoverCommand ??= new DelegateCommand(ExecuteDownloadCoverCommand);

    /// <summary>
    /// 封面选择事件
    /// </summary>
    private void ExecuteDownloadCoverCommand()
    {
        if (!DownloadCover)
        {
            DownloadAll = false;
        }

        if (DownloadAudio && DownloadVideo && DownloadDanmaku && DownloadSubtitle && DownloadCover)
        {
            DownloadAll = true;
        }

        SetVideoContent();
    }

    // 选中文件名字段右键点击事件
    private DelegateCommand<object>? _selectedFileNameRightCommand;

    public DelegateCommand<object> SelectedFileNameRightCommand => _selectedFileNameRightCommand ??= new DelegateCommand<object>(ExecuteSelectedFileNameRightCommand);

    /// <summary>
    /// 选中文件名字段右键点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteSelectedFileNameRightCommand(object parameter)
    {
        if (parameter == null)
        {
            return;
        }

        var isSucceed = SelectedFileName.Remove((DisplayFileNamePart)parameter);
        if (!isSucceed)
        {
            PublishTip(isSucceed);
            return;
        }

        //List<FileNamePart> fileName = new List<FileNamePart>();
        //foreach (DisplayFileNamePart item in SelectedFileName)
        //{
        //    fileName.Add(item.Id);
        //}

        //isSucceed = SettingsManager.GetInstance().SetFileNameParts(fileName);
        //PublishTip(isSucceed);

        SelectedOptionalField = -1;
    }

    // 可选文件名字段点击事件
    private DelegateCommand<object>? _optionalFieldsCommand;

    public DelegateCommand<object> OptionalFieldsCommand => _optionalFieldsCommand ??= new DelegateCommand<object>(ExecuteOptionalFieldsCommand);

    /// <summary>
    /// 可选文件名字段点击事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteOptionalFieldsCommand(object parameter)
    {
        if (SelectedOptionalField == -1)
        {
            return;
        }

        SelectedFileName.Add((DisplayFileNamePart)parameter);

        var fileName = SelectedFileName.Select(item => item.Id).ToList();

        var isSucceed = SettingsManager.GetInstance().SetFileNameParts(fileName);
        PublishTip(isSucceed);

        SelectedOptionalField = -1;
    }

    // 重置选中文件名字段
    private DelegateCommand? _resetCommand;
    public DelegateCommand ResetCommand => _resetCommand ??= new DelegateCommand(ExecuteResetCommand);

    /// <summary>
    /// 重置选中文件名字段
    /// </summary>
    private void ExecuteResetCommand()
    {
        var isSucceed = SettingsManager.GetInstance().SetFileNameParts(null);
        PublishTip(isSucceed);

        var fileNameParts = SettingsManager.GetInstance().GetFileNameParts();
        SelectedFileName.Clear();
        foreach (var item in fileNameParts)
        {
            var display = DisplayFileNamePart(item);
            SelectedFileName.Add(new DisplayFileNamePart { Id = item, Title = display });
        }

        SelectedOptionalField = -1;
    }

    // 文件命名中的时间格式事件
    private DelegateCommand<object>? _fileNamePartTimeFormatCommand;

    public DelegateCommand<object> FileNamePartTimeFormatCommand => _fileNamePartTimeFormatCommand ??= new DelegateCommand<object>(ExecuteFileNamePartTimeFormatCommand);

    /// <summary>
    /// 文件命名中的时间格式事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteFileNamePartTimeFormatCommand(object parameter)
    {
        if (parameter is not string timeFormat)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetFileNamePartTimeFormat(timeFormat);
        PublishTip(isSucceed);
    }

    // 文件命名中的序号格式事件
    private DelegateCommand<object>? _orderFormatCommand;

    public DelegateCommand<object> OrderFormatCommand => _orderFormatCommand ??= new DelegateCommand<object>(ExecuteOrderFormatCommandCommand);

    /// <summary>
    /// 文件命名中的序号格式事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteOrderFormatCommandCommand(object parameter)
    {
        if (parameter is not OrderFormatDisplay orderFormatDisplay)
        {
            return;
        }

        var isSucceed = SettingsManager.GetInstance().SetOrderFormat(orderFormatDisplay.OrderFormat);
        PublishTip(isSucceed);
    }

    #endregion

    /// <summary>
    /// 返回VideoCodecs的字符串
    /// </summary>
    /// <param name="videoCodecs"></param>
    /// <returns></returns>
    //private string GetVideoCodecsString(VideoCodecs videoCodecs)
    //{
    //    string codec;
    //    switch (videoCodecs)
    //    {
    //        case Core.Settings.VideoCodecs.NONE:
    //            codec = "";
    //            break;
    //        case Core.Settings.VideoCodecs.AVC:
    //            codec = "H.264/AVC";
    //            break;
    //        case Core.Settings.VideoCodecs.HEVC:
    //            codec = "H.265/HEVC";
    //            break;
    //        default:
    //            codec = "";
    //            break;
    //    }
    //    return codec;
    //}

    /// <summary>
    /// 返回VideoCodecs
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    //private VideoCodecs GetVideoCodecs(string str)
    //{
    //    VideoCodecs videoCodecs;
    //    switch (str)
    //    {
    //        case "H.264/AVC":
    //            videoCodecs = Core.Settings.VideoCodecs.AVC;
    //            break;
    //        case "H.265/HEVC":
    //            videoCodecs = Core.Settings.VideoCodecs.HEVC;
    //            break;
    //        default:
    //            videoCodecs = Core.Settings.VideoCodecs.NONE;
    //            break;
    //    }
    //    return videoCodecs;
    //}

    /// <summary>
    /// 保存下载视频内容到设置
    /// </summary>
    private void SetVideoContent()
    {
        var videoContent = new VideoContentSettings
        {
            DownloadAudio = DownloadAudio,
            DownloadVideo = DownloadVideo,
            DownloadDanmaku = DownloadDanmaku,
            DownloadSubtitle = DownloadSubtitle,
            DownloadCover = DownloadCover
        };

        var isSucceed = SettingsManager.GetInstance().SetVideoContent(videoContent);
        PublishTip(isSucceed);
    }

    /// <summary>
    /// 文件名字段显示
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private string DisplayFileNamePart(FileNamePart item)
    {
        var display = item switch
        {
            FileNamePart.Order => DictionaryResource.GetString("DisplayOrder"),
            FileNamePart.Section => DictionaryResource.GetString("DisplaySection"),
            FileNamePart.MainTitle => DictionaryResource.GetString("DisplayMainTitle"),
            FileNamePart.PageTitle => DictionaryResource.GetString("DisplayPageTitle"),
            FileNamePart.VideoZone => DictionaryResource.GetString("DisplayVideoZone"),
            FileNamePart.AudioQuality => DictionaryResource.GetString("DisplayAudioQuality"),
            FileNamePart.VideoQuality => DictionaryResource.GetString("DisplayVideoQuality"),
            FileNamePart.VideoCodec => DictionaryResource.GetString("DisplayVideoCodec"),
            FileNamePart.VideoPublishTime => DictionaryResource.GetString("DisplayVideoPublishTime"),
            FileNamePart.Avid => "avid",
            FileNamePart.Bvid => "bvid",
            FileNamePart.Cid => "cid",
            FileNamePart.UpMid => DictionaryResource.GetString("DisplayUpMid"),
            FileNamePart.UpName => DictionaryResource.GetString("DisplayUpName"),
            _ => string.Empty
        };

        if ((int)item >= 100)
        {
            display = HyphenSeparated.Hyphen[(int)item];
        }

        if (display == " ")
        {
            display = DictionaryResource.GetString("DisplaySpace");
        }

        return display;
    }

    /// <summary>
    /// 发送需要显示的tip
    /// </summary>
    /// <param name="isSucceed"></param>
    private void PublishTip(bool isSucceed)
    {
        if (_isOnNavigatedTo)
        {
            return;
        }

        EventAggregator.GetEvent<MessageEvent>().Publish(isSucceed ? DictionaryResource.GetString("TipSettingUpdated") : DictionaryResource.GetString("TipSettingFailed"));
    }
}