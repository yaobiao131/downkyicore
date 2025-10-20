using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Models.Json;

public class SubtitleJson : BaseModel
{
    [JsonProperty("font_size")] public float FontSize { get; set; }
    [JsonProperty("font_color")] public string FontColor { get; set; }
    [JsonProperty("background_alpha")] public float BackgroundAlpha { get; set; }
    [JsonProperty("background_color")] public string BackgroundColor { get; set; }
    [JsonProperty("Stroke")] public string Stroke { get; set; }
    [JsonProperty("body")] public List<Subtitle> Body { get; set; }

    /// <summary>
    /// srt格式字幕
    /// </summary>
    /// <returns></returns>
    public string ToSubRip()
    {
        string subRip = string.Empty;
        for (int i = 0; i < Body.Count; i++)
        {
            subRip += $"{i + 1}\n";
            subRip += $"{Second2hms(Body[i].From)} --> {Second2hms(Body[i].To)}\n";
            subRip += $"{Body[i].Content}\n";
            subRip += "\n";
        }

        return subRip;
    }

    /// <summary>
    /// 秒数转 时:分:秒 格式
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private static string Second2hms(float seconds)
    {
        if (seconds < 0) return "00:00:00,000";
    
        var span = TimeSpan.FromSeconds(seconds);
        return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2},{span.Milliseconds:D3}";
    }
}