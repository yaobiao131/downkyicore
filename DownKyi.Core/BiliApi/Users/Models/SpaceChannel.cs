using DownKyi.Core.BiliApi.Models;
using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Users.Models;

// https://api.bilibili.com/x/space/channel/list?mid={mid}
public class SpaceChannelOrigin : BaseModel
{
    [JsonProperty("data")]
    public SpaceChannel Data { get; set; }
}

public class SpaceChannel : BaseModel
{
    [JsonProperty("count")]
    public int Count { get; set; }
    [JsonProperty("list")]
    public List<SpaceChannelList> List { get; set; }
}