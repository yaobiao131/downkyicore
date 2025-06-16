using System;
using FreeSql.DataAnnotations;

namespace DownKyi.Models;

[Table(Name = "downloaded")]
public class Downloaded
{
    [Column(IsPrimary = true, Name = "id")]
    public string Id { get; set; } = null!;

    //  下载速度
    [Column(Name = "max_speed_display")] public string? MaxSpeedDisplay { get; set; }

    // 完成时间戳
    [Column(Name = "finished_timestamp")] public long FinishedTimestamp { get; set; }

    public void SetFinishedTimestamp(long finishedTimestamp)
    {
        FinishedTimestamp = finishedTimestamp;

        var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
        var dateTime = startTime.AddSeconds(finishedTimestamp);
        FinishedTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // 完成时间
    [Column(Name = "finished_time")] public string FinishedTime { get; set; }

    [Navigate(nameof(Id))] public DownloadBase? DownloadBase { get; set; }
}