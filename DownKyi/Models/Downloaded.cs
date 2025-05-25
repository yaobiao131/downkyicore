using System;
using SqlSugar;

namespace DownKyi.Models;

[Serializable]
[SugarTable(TableName = "downloaded")]
public class Downloaded
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
    public string Id { get; set; } = null!;
    //  下载速度
    [SugarColumn(ColumnName = "max_speed_display")]
    public string? MaxSpeedDisplay { get; set; }

    // 完成时间戳
    [SugarColumn(ColumnName = "finished_timestamp")]
    public long FinishedTimestamp { get; set; }

    public void SetFinishedTimestamp(long finishedTimestamp)
    {
        FinishedTimestamp = finishedTimestamp;

        var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
        var dateTime = startTime.AddSeconds(finishedTimestamp);
        FinishedTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // 完成时间
    [SugarColumn(ColumnName = "finished_time")]
    public string FinishedTime { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(Id))]
    [SugarColumn(IsIgnore = true)]
    public DownloadBase DownloadBase { get; set; }
}