using System;
using System.Collections.Generic;
using System.ComponentModel;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Utils.DataAnnotations;
using FreeSql.DataAnnotations;

namespace DownKyi.Models;

[Table(Name = "download_base")]
[Description("下载项的基础信息")]
public class DownloadBase
{
    public DownloadBase()
    {
        // 唯一id
        Id = Guid.NewGuid().ToString("N");

        // 初始化需要下载的内容
        NeedDownloadContent = new Dictionary<string, bool>
        {
            { "downloadAudio", true },
            { "downloadVideo", true },
            { "downloadDanmaku", true },
            { "downloadSubtitle", true },
            { "downloadCover", true }
        };
    }

    // 此条下载项的id
    [Column(IsPrimary = true, Name = "id")]
    public string Id { get; set; }

    // 需要下载的内容
    [Column(Name = "need_download_content"), JsonMap]
    public Dictionary<string, bool> NeedDownloadContent { get; set; }

    // 视频的id
    [Column(Name = "bvid")] public string Bvid { get; set; }

    [Column(Name = "avid")] public long Avid { get; set; }

    [Column(Name = "cid")] public long Cid { get; set; }

    [Column(Name = "episode_id")] public long EpisodeId { get; set; }

    // 视频封面的url
    [Column(Name = "cover_url"), Description("视频封面的url")]
    public string CoverUrl { get; set; }

    // 视频page的封面的url
    [Column(Name = "page_cover_url"), Description("视频page的封面的url")]
    public string PageCoverUrl { get; set; }

    // 分区id
    [Column(Name = "zone_id"), Description("分区id")]
    public int ZoneId { get; set; }

    // 视频序号
    [Column(Name = "order"), Description("视频序号")]
    public int Order { get; set; }

    // 视频主标题
    [Column(Name = "main_title"), Description("视频主标题")]
    public string MainTitle { get; set; }

    // 视频标题
    [Column(Name = "name"), Description("视频标题")]
    public string Name { get; set; }

    // 时长
    [Column(Name = "duration"), Description("时长")]
    public string Duration { get; set; }

    // 视频编码名称，AVC、HEVC
    [Column(Name = "video_codec_name"), Description("视频编码名称，AVC、HEVC")]
    public string VideoCodecName { get; set; }

    // 视频画质
    [Column(Name = "resolution"), Description("视频画质"), JsonMap]
    public Quality Resolution { get; set; }

    // 音频编码
    [Column(Name = "audio_codec", IsNullable = true), Description("音频编码"), JsonMap]
    public Quality AudioCodec { get; set; }

    // 文件路径，不包含扩展名，所有内容均以此路径下载
    [Column(Name = "file_path"), Description("文件路径，不包含扩展名，所有内容均以此路径下载")]
    public string FilePath { get; set; }

    // 文件大小
    [Column(Name = "file_size", IsNullable = true), Description("文件大小")]
    public string? FileSize { get; set; }

    // 视频分p(默认为1)
    [Column(Name = "page"), Description("视频分p(默认为1)")]
    public int Page { get; set; } = 1;

    [Navigate(nameof(Id))] public Downloaded? Downloaded { get; set; }
    [Navigate(nameof(Id))] public Downloading? Downloading { get; set; }
}