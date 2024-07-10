namespace DownKyi.Core.Settings.Models;

/// <summary>
/// 基本
/// </summary>
public class BasicSettings
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Default;
    public AfterDownloadOperation AfterDownload { get; set; } = AfterDownloadOperation.NOT_SET;
    public AllowStatus IsListenClipboard { get; set; } = AllowStatus.NONE;
    public AllowStatus IsAutoParseVideo { get; set; } = AllowStatus.NONE;
    public ParseScope ParseScope { get; set; } = ParseScope.NOT_SET;
    public AllowStatus IsAutoDownloadAll { get; set; } = AllowStatus.NONE;
    public DownloadFinishedSort DownloadFinishedSort { get; set; } = DownloadFinishedSort.NotSet;
    public RepeatDownloadStrategy RepeatDownloadStrategy { get; set; } = RepeatDownloadStrategy.Ask;
    public bool RepeatFileAutoAddNumberSuffix { get; set; } = false;
}