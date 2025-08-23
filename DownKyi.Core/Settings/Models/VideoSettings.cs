using DownKyi.Core.FileName;

namespace DownKyi.Core.Settings.Models;

/// <summary>
/// 视频
/// </summary>
public class VideoSettings
{
    public int VideoCodecs { get; set; } = -1; // AVC or HEVC
    public int Quality { get; set; } = -1; // 画质
    public int AudioQuality { get; set; } = -1; // 音质
    public int VideoParseType { get; set; } // 视频解析类型
    public AllowStatus IsTranscodingFlvToMp4 { get; set; } = AllowStatus.None; // 是否将flv转为mp4
    public AllowStatus IsTranscodingAacToMp3 { get; set; } = AllowStatus.None; // 是否将aac转为mp3
    public string? SaveVideoRootPath { get; set; } // 视频保存路径
    public List<string>? HistoryVideoRootPaths { get; set; } // 历史视频保存路径
    public AllowStatus IsUseSaveVideoRootPath { get; set; } = AllowStatus.None; // 是否使用默认视频保存路径
    public VideoContentSettings? VideoContent { get; set; } // 下载内容
    public List<FileNamePart>? FileNameParts { get; set; } // 文件命名格式
    public string? FileNamePartTimeFormat { get; set; } // 文件命名中的时间格式
    public OrderFormat OrderFormat { get; set; } = OrderFormat.NotSet; // 文件命名中的序号格式
}