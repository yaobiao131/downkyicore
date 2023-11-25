using DownKyi.Core.BiliApi.Models;
using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.VideoStream.Models;

public class SubtitleInfo : BaseModel
{
    [JsonProperty("allow_submit")] public bool AllowSubmit { get; set; }
    [JsonProperty("lan")] public string Lan { get; set; }
    [JsonProperty("lan_doc")] public string LanDoc { get; set; }
    [JsonProperty("subtitles")] public List<Subtitle> Subtitles { get; set; }
}