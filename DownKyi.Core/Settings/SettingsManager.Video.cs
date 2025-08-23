using DownKyi.Core.FileName;
using DownKyi.Core.Settings.Models;
using DownKyi.Core.Storage;

namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 设置优先下载的视频编码
    private const int VideoCodecs = 7;

    // 设置优先下载画质
    private const int Quality = 120;

    // 设置优先下载音质
    private const int AudioQuality = 30280;

    // 设置首选视频解析方式
    private const int VideoParseType = 0;

    // 是否下载flv视频后转码为mp4
    private const AllowStatus IsTranscodingFlvToMp4 = AllowStatus.Yes;

    // 是否下载aac音频后转码为mp3
    private const AllowStatus IsTranscodingAacToMp3 = AllowStatus.Yes;

    // 默认下载目录
    private readonly string _saveVideoRootPath = StorageManager.GetMedia();

    // 历史下载目录
    private readonly List<string> _historyVideoRootPaths = new();

    // 是否使用默认下载目录，如果是，则每次点击下载选中项时不再询问下载目录
    private const AllowStatus IsUseSaveVideoRootPath = AllowStatus.No;

    // 下载内容
    private readonly VideoContentSettings _videoContent = new();

    // 文件命名格式
    private readonly List<FileNamePart> _fileNameParts = new()
    {
        FileNamePart.MainTitle,
        FileNamePart.Slash,
        FileNamePart.Section,
        FileNamePart.Slash,
        FileNamePart.Order,
        FileNamePart.Hyphen,
        FileNamePart.PageTitle,
        FileNamePart.Hyphen,
        FileNamePart.VideoQuality,
        FileNamePart.Hyphen,
        FileNamePart.VideoCodec,
    };

    // 文件命名中的时间格式
    private const string FileNamePartTimeFormat = "yyyy-MM-dd";

    // 文件命名中的序号格式
    private const OrderFormat OrderFormat = Settings.OrderFormat.Natural;

    /// <summary>
    /// 获取优先下载的视频编码
    /// </summary>
    /// <returns></returns>
    public int GetVideoCodecs()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.VideoCodecs == -1)
        {
            // 第一次获取，先设置默认值
            SetVideoCodecs(VideoCodecs);
            return VideoCodecs;
        }

        return _appSettings.Video.VideoCodecs;
    }

    /// <summary>
    /// 设置优先下载的视频编码
    /// </summary>
    /// <param name="videoCodecs"></param>
    /// <returns></returns>
    public bool SetVideoCodecs(int videoCodecs)
    {
        return SetProperty(
            _appSettings.Video.VideoCodecs,
            videoCodecs,
            v => _appSettings.Video.VideoCodecs = v);
    }

    /// <summary>
    /// 获取优先下载画质
    /// </summary>
    /// <returns></returns>
    public int GetQuality()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.Quality == -1)
        {
            // 第一次获取，先设置默认值
            SetQuality(Quality);
            return Quality;
        }

        return _appSettings.Video.Quality;
    }

    /// <summary>
    /// 设置优先下载画质
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    public bool SetQuality(int quality)
    {
        return SetProperty(
            _appSettings.Video.Quality,
            quality,
            v => _appSettings.Video.Quality = v);
    }

    /// <summary>
    /// 获取优先下载音质
    /// </summary>
    /// <returns></returns>
    public int GetAudioQuality()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.AudioQuality == -1)
        {
            // 第一次获取，先设置默认值
            SetAudioQuality(AudioQuality);
            return AudioQuality;
        }

        return _appSettings.Video.AudioQuality;
    }

    /// <summary>
    /// 设置优先下载音质
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    public bool SetAudioQuality(int quality)
    {
        return SetProperty(
            _appSettings.Video.AudioQuality,
            quality,
            v => _appSettings.Video.AudioQuality = v);
    }
    
    /// <summary>
    /// 获取首选视频解析方式
    /// </summary>
    /// <returns></returns>
    public int GetVideoParseType()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.VideoParseType == null)
        {
            // 第一次获取，先设置默认值
            SetVideoParseType(VideoParseType);
            return VideoParseType;
        }

        return _appSettings.Video.VideoParseType;
    }
    
    /// <summary>
    /// 设置首选视频解析方式
    /// </summary>
    /// <param name="videoParseType"></param>
    /// <returns></returns>
    public bool SetVideoParseType(int videoParseType)
    {
        return SetProperty(
            _appSettings.Video.VideoParseType,
            videoParseType,
            v => _appSettings.Video.VideoParseType = v);
    }

    /// <summary>
    /// 获取是否下载flv视频后转码为mp4
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsTranscodingFlvToMp4()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.IsTranscodingFlvToMp4 == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsTranscodingFlvToMp4(IsTranscodingFlvToMp4);
            return IsTranscodingFlvToMp4;
        }

        return _appSettings.Video.IsTranscodingFlvToMp4;
    }

    /// <summary>
    /// 设置是否下载flv视频后转码为mp4
    /// </summary>
    /// <param name="isTranscodingFlvToMp4"></param>
    /// <returns></returns>
    public bool SetIsTranscodingFlvToMp4(AllowStatus isTranscodingFlvToMp4)
    {
        return SetProperty(
            _appSettings.Video.IsTranscodingFlvToMp4,
            isTranscodingFlvToMp4,
            v => _appSettings.Video.IsTranscodingFlvToMp4 = v);
    }
    
    /// <summary>
    /// 获取是否下载aac音频后转码为mp3
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsTranscodingAacToMp3()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.IsTranscodingAacToMp3 == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsTranscodingAacToMp3(IsTranscodingAacToMp3);
            return IsTranscodingAacToMp3;
        }

        return _appSettings.Video.IsTranscodingAacToMp3;
    }
    
    /// <summary>
    /// 设置是否下载aac音频后转码为mp3
    /// </summary>
    /// <param name="isTranscodingAacToMp3"></param>
    /// <returns></returns>
    public bool SetIsTranscodingAacToMp3(AllowStatus isTranscodingAacToMp3)
    {
        return SetProperty(
            _appSettings.Video.IsTranscodingAacToMp3,
            isTranscodingAacToMp3,
            v => _appSettings.Video.IsTranscodingAacToMp3 = v);
    }

    /// <summary>
    /// 获取下载目录
    /// </summary>
    /// <returns></returns>
    public string GetSaveVideoRootPath()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.SaveVideoRootPath == null)
        {
            // 第一次获取，先设置默认值
            SetSaveVideoRootPath(_saveVideoRootPath);
            return _saveVideoRootPath;
        }

        return _appSettings.Video.SaveVideoRootPath;
    }

    /// <summary>
    /// 设置下载目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool SetSaveVideoRootPath(string path)
    {
        return SetProperty(
            _appSettings.Video.SaveVideoRootPath,
            path,
            v => _appSettings.Video.SaveVideoRootPath = v);
    }

    /// <summary>
    /// 获取历史下载目录
    /// </summary>
    /// <returns></returns>
    public List<string> GetHistoryVideoRootPaths()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.HistoryVideoRootPaths == null)
        {
            // 第一次获取，先设置默认值
            SetHistoryVideoRootPaths(_historyVideoRootPaths);
            return _historyVideoRootPaths;
        }

        return _appSettings.Video.HistoryVideoRootPaths;
    }

    /// <summary>
    /// 设置历史下载目录
    /// </summary>
    /// <param name="historyPaths"></param>
    /// <returns></returns>
    public bool SetHistoryVideoRootPaths(List<string> historyPaths)
    {
        return SetProperty(
            _appSettings.Video.HistoryVideoRootPaths,
            historyPaths,
            v => _appSettings.Video.HistoryVideoRootPaths = v);
    }

    /// <summary>
    /// 获取是否使用默认下载目录
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsUseSaveVideoRootPath()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.IsUseSaveVideoRootPath == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsUseSaveVideoRootPath(IsUseSaveVideoRootPath);
            return IsUseSaveVideoRootPath;
        }

        return _appSettings.Video.IsUseSaveVideoRootPath;
    }

    /// <summary>
    /// 设置是否使用默认下载目录
    /// </summary>
    /// <param name="isUseSaveVideoRootPath"></param>
    /// <returns></returns>
    public bool SetIsUseSaveVideoRootPath(AllowStatus isUseSaveVideoRootPath)
    {
        return SetProperty(
            _appSettings.Video.IsUseSaveVideoRootPath,
            isUseSaveVideoRootPath,
            v => _appSettings.Video.IsUseSaveVideoRootPath = v);
    }

    /// <summary>
    /// 获取下载内容
    /// </summary>
    /// <returns></returns>
    public VideoContentSettings GetVideoContent()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.VideoContent == null)
        {
            // 第一次获取，先设置默认值
            SetVideoContent(_videoContent);
            return _videoContent;
        }

        return _appSettings.Video.VideoContent;
    }

    /// <summary>
    /// 设置下载内容
    /// </summary>
    /// <param name="videoContent"></param>
    /// <returns></returns>
    public bool SetVideoContent(VideoContentSettings videoContent)
    {
        return SetProperty(
            _appSettings.Video.VideoContent,
            videoContent,
            v => _appSettings.Video.VideoContent = v);
    }

    /// <summary>
    /// 获取文件命名格式
    /// </summary>
    /// <returns></returns>
    public List<FileNamePart> GetFileNameParts()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.FileNameParts == null || _appSettings.Video.FileNameParts.Count == 0)
        {
            // 第一次获取，先设置默认值
            SetFileNameParts(_fileNameParts);
            return _fileNameParts;
        }

        return _appSettings.Video.FileNameParts;
    }

    /// <summary>
    /// 设置文件命名格式
    /// </summary>
    /// <param name="fileNameParts"></param>
    /// <returns></returns>
    public bool SetFileNameParts(List<FileNamePart> fileNameParts)
    {
        return SetProperty(
            _appSettings.Video.FileNameParts,
            fileNameParts,
            v => _appSettings.Video.FileNameParts = v);
    }

    /// <summary>
    /// 获取文件命名中的时间格式
    /// </summary>
    /// <returns></returns>
    public string GetFileNamePartTimeFormat()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.FileNamePartTimeFormat == null ||
            _appSettings.Video.FileNamePartTimeFormat == string.Empty)
        {
            // 第一次获取，先设置默认值
            SetFileNamePartTimeFormat(FileNamePartTimeFormat);
            return FileNamePartTimeFormat;
        }

        return _appSettings.Video.FileNamePartTimeFormat;
    }

    /// <summary>
    /// 设置文件命名中的时间格式
    /// </summary>
    /// <param name="fileNamePartTimeFormat"></param>
    /// <returns></returns>
    public bool SetFileNamePartTimeFormat(string fileNamePartTimeFormat)
    {
        return SetProperty(
            _appSettings.Video.FileNamePartTimeFormat,
            fileNamePartTimeFormat,
            v => _appSettings.Video.FileNamePartTimeFormat = v);
    }

    /// <summary>
    /// 获取文件命名中的序号格式
    /// </summary>
    /// <returns></returns>
    public OrderFormat GetOrderFormat()
    {
        _appSettings = GetSettings();
        if (_appSettings.Video.OrderFormat == OrderFormat.NotSet)
        {
            // 第一次获取，先设置默认值
            SetOrderFormat(OrderFormat);
            return OrderFormat;
        }

        return _appSettings.Video.OrderFormat;
    }

    /// <summary>
    /// 设置文件命名中的序号格式
    /// </summary>
    /// <param name="orderFormat"></param>
    /// <returns></returns>
    public bool SetOrderFormat(OrderFormat orderFormat)
    {
        return SetProperty(
            _appSettings.Video.OrderFormat,
            orderFormat,
            v => _appSettings.Video.OrderFormat = v);
    }
}