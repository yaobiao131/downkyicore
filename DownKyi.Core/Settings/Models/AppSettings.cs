namespace DownKyi.Core.Settings.Models;

public class AppSettings
{
    public BasicSettings Basic { get; set; } = new();
    public NetworkSettings Network { get; set; } = new();
    public VideoSettings Video { get; set; } = new();
    public DanmakuSettings Danmaku { get; set; } = new();
    public AboutSettings About { get; set; } = new();
    public UserInfoSettings UserInfo { get; set; } = new();
    public WindowSettings WindowSettings { get; set; } = new();
}