using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Models.Json;

public class BiliTagInfo
{
    [JsonProperty("tag_id")]
    public long TagId { get; set; }
    
    [JsonProperty("tag_name")]
    public string TagName { get; set; }
}

public class TagResult
{
    [JsonProperty("data")]
    public List<BiliTagInfo> Data { get; set; }
}
