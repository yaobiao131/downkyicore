namespace DownKyi.Core.Settings.Models;

/// <summary>
/// 基本
/// </summary>
public class BasicSettings
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Default;
    public AfterDownloadOperation AfterDownload { get; set; } = AfterDownloadOperation.NotSet;
    public AllowStatus IsListenClipboard { get; set; } = AllowStatus.None;
    public AllowStatus IsAutoParseVideo { get; set; } = AllowStatus.None;
    public ParseScope ParseScope { get; set; } = ParseScope.NotSet;
    public AllowStatus IsAutoDownloadAll { get; set; } = AllowStatus.None;
    public DownloadFinishedSort DownloadFinishedSort { get; set; } = DownloadFinishedSort.NotSet;
    public RepeatDownloadStrategy RepeatDownloadStrategy { get; set; } = RepeatDownloadStrategy.Ask;
    public bool RepeatFileAutoAddNumberSuffix { get; set; }
}