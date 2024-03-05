using DownKyi.Core.BiliApi.Models;
using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Users.Models;

public class SpacePublicationList : BaseModel
{
    [JsonProperty("tlist")] public SpacePublicationListType Tlist { get; set; }
    [JsonProperty("vlist")] public List<SpacePublicationListVideo>? Vlist { get; set; }
}