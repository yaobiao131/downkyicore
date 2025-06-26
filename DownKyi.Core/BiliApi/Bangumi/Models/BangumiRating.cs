using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Bangumi.Models;

public class BangumiRating
{
    [JsonProperty("count")]
    public int Count { get; set; }
    
    [JsonProperty("score")]
    public float Score { get; set; }
}