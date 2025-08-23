namespace DownKyi.Core.BiliApi.BiliUtils;

public class Quality
{
    public string Name { get; set; }
    public int Id { get; set; }


    public Quality Clone()
    {
        return new Quality
        {
            Name = Name,
            Id = Id
        };
    }
}