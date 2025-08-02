using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.VideoStream.Models;

public class PlayViewOrigin
{
    [JsonProperty("data")] public PlayView? Data { get; set; }
}

public class PlayView
{
    [JsonProperty("video_info")] public PlayUrl VideoInfo { get; set; } = new();
    [JsonProperty("plugins")] public List<PlayViewPlugin> Plugins { get; set; } = new();
}

public class PlayViewPlugin
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}