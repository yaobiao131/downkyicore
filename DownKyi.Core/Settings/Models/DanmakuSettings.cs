namespace DownKyi.Core.Settings.Models;

/// <summary>
/// 弹幕
/// </summary>
public class DanmakuSettings
{
    public AllowStatus DanmakuTopFilter { get; set; } = AllowStatus.None;
    public AllowStatus DanmakuBottomFilter { get; set; } = AllowStatus.None;
    public AllowStatus DanmakuScrollFilter { get; set; } = AllowStatus.None;
    public AllowStatus IsCustomDanmakuResolution { get; set; } = AllowStatus.None;
    public int DanmakuScreenWidth { get; set; } = -1;
    public int DanmakuScreenHeight { get; set; } = -1;
    public string? DanmakuFontName { get; set; }
    public int DanmakuFontSize { get; set; } = -1;
    public int DanmakuLineCount { get; set; } = -1;
    public DanmakuLayoutAlgorithm DanmakuLayoutAlgorithm { get; set; } = DanmakuLayoutAlgorithm.None;
}