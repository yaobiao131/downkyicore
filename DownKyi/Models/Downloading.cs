using System;
using System.Collections.Generic;
using DownKyi.Core.BiliApi.VideoStream;
using Downloader;
using SqlSugar;

namespace DownKyi.Models;

[Serializable]
public class Downloading
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
    public string Id { get; set; } = null!;

    // Aria相关
    [SugarColumn(ColumnName = "gid", IsNullable = true)]
    public string? Gid { get; set; }

    // 下载的文件
    [SugarColumn(ColumnName = "download_files", IsJson = true)]
    public Dictionary<string, string> DownloadFiles { get; set; } = new();

    // 已下载的文件
    [SugarColumn(ColumnName = "downloaded_files", IsJson = true)]
    public List<string> DownloadedFiles { get; set; } = new();

    // 视频类别
    [SugarColumn(ColumnName = "play_stream_type")]
    public PlayStreamType PlayStreamType { get; set; }

    // 下载状态
    [SugarColumn(ColumnName = "download_status")]
    public DownloadStatus DownloadStatus { get; set; }

    // 正在下载内容（音频、视频、弹幕、字幕、封面）
    [SugarColumn(ColumnName = "download_content", IsNullable = true)]
    public string? DownloadContent { get; set; }

    // 下载状态显示
    [SugarColumn(ColumnName = "download_status_title", IsNullable = true)]
    public string? DownloadStatusTitle { get; set; }

    // 下载进度
    [SugarColumn(ColumnName = "progress")] public float Progress { get; set; }

    //  已下载大小/文件大小
    [SugarColumn(ColumnName = "downloading_file_size", IsNullable = true)]
    public string? DownloadingFileSize { get; set; }

    // 下载的最高速度
    [SugarColumn(ColumnName = "max_speed")]
    public long MaxSpeed { get; set; }

    //  下载速度
    [SugarColumn(ColumnName = "speed_display", IsNullable = true)]
    public string? SpeedDisplay { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(Id))]
    [SugarColumn(IsIgnore = true)]
    public DownloadBase DownloadBase { get; set; }
}