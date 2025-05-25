using System;
using System.Collections.Generic;
using DownKyi.Core.BiliApi.BiliUtils;

namespace DownKyi.Models;

[Serializable]
[SqlSugar.SugarTable("download_base", TableDescription = "下载项的基础信息")]
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
    [SqlSugar.SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
    public string Id { get; set; }

    // 需要下载的内容
    [SqlSugar.SugarColumn(IsJson = true, ColumnName = "need_download_content")]
    public Dictionary<string, bool> NeedDownloadContent { get; set; }

    // 视频的id
    [SqlSugar.SugarColumn(ColumnName = "bvid")]
    public string Bvid { get; set; }

    [SqlSugar.SugarColumn(ColumnName = "avid")]
    public long Avid { get; set; }

    [SqlSugar.SugarColumn(ColumnName = "cid")]
    public long Cid { get; set; }

    [SqlSugar.SugarColumn(ColumnName = "episode_id")]
    public long EpisodeId { get; set; }

    // 视频封面的url
    [SqlSugar.SugarColumn(ColumnName = "cover_url", ColumnDescription = "视频封面的url")]
    public string CoverUrl { get; set; }

    // 视频page的封面的url
    [SqlSugar.SugarColumn(ColumnName = "page_cover_url", ColumnDescription = "视频page的封面的url")]
    public string PageCoverUrl { get; set; }

    // 分区id
    [SqlSugar.SugarColumn(ColumnName = "zone_id", ColumnDescription = "分区id")]
    public int ZoneId { get; set; }

    // 视频序号
    [SqlSugar.SugarColumn(ColumnName = "order", ColumnDescription = "视频序号")]
    public int Order { get; set; }

    // 视频主标题
    [SqlSugar.SugarColumn(ColumnName = "main_title", ColumnDescription = "视频主标题")]
    public string MainTitle { get; set; }

    // 视频标题
    [SqlSugar.SugarColumn(ColumnName = "name", ColumnDescription = "视频标题")]
    public string Name { get; set; }

    // 时长
    [SqlSugar.SugarColumn(ColumnName = "duration", ColumnDescription = "时长")]
    public string Duration { get; set; }

    // 视频编码名称，AVC、HEVC
    [SqlSugar.SugarColumn(ColumnName = "video_codec_name", ColumnDescription = "视频编码名称，AVC、HEVC")]
    public string VideoCodecName { get; set; }

    // 视频画质
    [SqlSugar.SugarColumn(ColumnName = "resolution", ColumnDescription = "视频画质", IsJson = true)]
    public Quality Resolution { get; set; }

    // 音频编码
    [SqlSugar.SugarColumn(ColumnName = "audio_codec", ColumnDescription = "音频编码", IsJson = true)]
    public Quality AudioCodec { get; set; }

    // 文件路径，不包含扩展名，所有内容均以此路径下载
    [SqlSugar.SugarColumn(ColumnName = "file_path", ColumnDescription = "文件路径，不包含扩展名，所有内容均以此路径下载")]
    public string FilePath { get; set; }

    // 文件大小
    [SqlSugar.SugarColumn(ColumnName = "file_size", ColumnDescription = "文件大小", IsNullable = true)]
    public string? FileSize { get; set; }

    // 视频分p(默认为1)
    [SqlSugar.SugarColumn(ColumnName = "page", ColumnDescription = "视频分p(默认为1)")]
    public int Page { get; set; } = 1;
}