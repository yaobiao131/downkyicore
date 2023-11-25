using DownKyi.Core.BiliApi.Models;
using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Cheese.Models;

public class CheeseBrief : BaseModel
{
    // content
    [JsonProperty("img")] public List<CheeseImg> Img { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("type")] public int Type { get; set; }
}