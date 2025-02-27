namespace DownKyi.Models;

public enum DownloadStatus
{
    NotStarted, // 未开始，从未开始下载
    WaitForDownload, // 等待下载，下载过，但是启动本次下载周期未开始，如重启程序后未开始
    PauseStarted, // 暂停启动下载
    Pause, // 暂停

    //PAUSE_TO_WAIT,     // 暂停后等待
    Downloading, // 下载中
    DownloadSucceed, // 下载成功
    DownloadFailed, // 下载失败
}