using System.Collections.Generic;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Utils.DataAnnotations;
using FreeSql.DataAnnotations;

namespace DownKyi.Models;

[Table(Name = "downloading")]
public class Downloading
{
    [Column(IsPrimary = true, Name = "id")]
    public string Id { get; set; } = null!;

    // Aria相关
    [Column(Name = "gid", IsNullable = true)]
    public string? Gid { get; set; }

    // 下载的文件
    [Column(Name = "download_files"), JsonMap]
    public Dictionary<string, string> DownloadFiles { get; set; } = new();

    // 已下载的文件
    [Column(Name = "downloaded_files"), JsonMap]
    public List<string> DownloadedFiles { get; set; } = new();

    // 视频类别
    [Column(Name = "play_stream_type")] public PlayStreamType PlayStreamType { get; set; }

    // 下载状态
    [Column(Name = "download_status")] public DownloadStatus DownloadStatus { get; set; }

    // 正在下载内容（音频、视频、弹幕、字幕、封面）
    [Column(Name = "download_content", IsNullable = true)]
    public string? DownloadContent { get; set; }

    // 下载状态显示
    [Column(Name = "download_status_title", IsNullable = true)]
    public string? DownloadStatusTitle { get; set; }

    // 下载进度
    [Column(Name = "progress")] public float Progress { get; set; }

    //  已下载大小/文件大小
    [Column(Name = "downloading_file_size", IsNullable = true)]
    public string? DownloadingFileSize { get; set; }

    // 下载的最高速度
    [Column(Name = "max_speed")] public long MaxSpeed { get; set; }

    //  下载速度
    [Column(Name = "speed_display", IsNullable = true)]
    public string? SpeedDisplay { get; set; }

    [Navigate(nameof(Id))] public DownloadBase? DownloadBase { get; set; }
}