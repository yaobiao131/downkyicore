using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Services;
using DownKyi.Services.Download;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels.DownloadManager;

public class ViewDownloadFinishedViewModel : ViewModelBase
{
    public const string Tag = "PageDownloadManagerDownloadFinished";

    private DownloadStorageService _downloadStorageService;

    #region 页面属性申明

    private ObservableCollection<DownloadedItem> _downloadedList = new();

    public ObservableCollection<DownloadedItem> DownloadedList
    {
        get => _downloadedList;
        set => SetProperty(ref _downloadedList, value);
    }

    private int _finishedSortBy;

    public int FinishedSortBy
    {
        get => _finishedSortBy;
        set => SetProperty(ref _finishedSortBy, value);
    }

    #endregion

    public ViewDownloadFinishedViewModel(
        IEventAggregator eventAggregator,
        IDialogService dialogService,
        DownloadStorageService downloadStorageService
    ) : base(eventAggregator,
        dialogService)
    {
        // 初始化DownloadedList
        DownloadedList = App.DownloadedList;
        _downloadStorageService = downloadStorageService;

        var finishedSort = SettingsManager.GetInstance().GetDownloadFinishedSort();
        FinishedSortBy = finishedSort switch
        {
            DownloadFinishedSort.DownloadAsc => 0,
            DownloadFinishedSort.DownloadDesc => 1,
            DownloadFinishedSort.Number => 2,
            _ => 0
        };
        App.SortDownloadedList(finishedSort);
    }

    #region 命令申明

    // 下载完成列表排序事件
    private DelegateCommand<object>? _finishedSortCommand;
    public DelegateCommand<object> FinishedSortCommand => _finishedSortCommand ??= new DelegateCommand<object>(ExecuteFinishedSortCommand);

    /// <summary>
    /// 下载完成列表排序事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteFinishedSortCommand(object parameter)
    {
        if (parameter is not int index)
        {
            return;
        }

        switch (index)
        {
            case 0:
                App.SortDownloadedList(DownloadFinishedSort.DownloadAsc);
                // 更新设置
                SettingsManager.GetInstance().SetDownloadFinishedSort(DownloadFinishedSort.DownloadAsc);
                break;
            case 1:
                App.SortDownloadedList(DownloadFinishedSort.DownloadDesc);
                // 更新设置
                SettingsManager.GetInstance().SetDownloadFinishedSort(DownloadFinishedSort.DownloadDesc);
                break;
            case 2:
                App.SortDownloadedList(DownloadFinishedSort.Number);
                // 更新设置
                SettingsManager.GetInstance().SetDownloadFinishedSort(DownloadFinishedSort.Number);
                break;
            default:
                App.SortDownloadedList(DownloadFinishedSort.DownloadAsc);
                // 更新设置
                SettingsManager.GetInstance().SetDownloadFinishedSort(DownloadFinishedSort.DownloadAsc);
                break;
        }
    }

    // 清空下载完成列表事件
    private DelegateCommand? _clearAllDownloadedCommand;
    public DelegateCommand ClearAllDownloadedCommand => _clearAllDownloadedCommand ??= new DelegateCommand(ExecuteClearAllDownloadedCommand);

    /// <summary>
    /// 清空下载完成列表事件
    /// </summary>
    private async void ExecuteClearAllDownloadedCommand()
    {
        try
        {
            var alertService = new AlertService(DialogService);
            var result = await alertService.ShowWarning(DictionaryResource.GetString("ConfirmDelete"));
            if (result != ButtonResult.OK)
            {
                return;
            }


            // 使用Clear()不能触发NotifyCollectionChangedAction.Remove事件
            // 因此遍历删除
            // DownloadingList中元素被删除后不能继续遍历
            _downloadStorageService.ClearDownloaded();
            App.PropertyChangeAsync(() => { App.DownloadedList.Clear(); });
        }
        catch (Exception e)
        {
            var alertService = new AlertService(DialogService);
            await alertService.ShowError(e.Message);
        }
    }

    // 打开视频事件
    private DelegateCommand<DownloadedItem>? _openVideoCommand;
    public DelegateCommand<DownloadedItem> OpenVideoCommand => _openVideoCommand ??= new DelegateCommand<DownloadedItem>(ExecuteOpenVideoCommand);

    /// <summary>
    /// 打开视频事件
    /// </summary>
    private void ExecuteOpenVideoCommand(DownloadedItem downloadedItem)
    {
        if (downloadedItem.DownloadBase == null)
        {
            return;
        }

        var videoPath = $"{downloadedItem.DownloadBase.FilePath}.mp4";
        var fileInfo = new FileInfo(videoPath);
        if (File.Exists(fileInfo.FullName))
        {
            PlatformHelper.Open(fileInfo.FullName);
        }
        else
        {
            //eventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipAddDownloadingZero"));
            EventAggregator.GetEvent<MessageEvent>().Publish("没有找到视频文件，可能被删除或移动！");
        }
    }

    // 打开文件夹事件
    private DelegateCommand<DownloadedItem>? _openFolderCommand;

    public DelegateCommand<DownloadedItem> OpenFolderCommand => _openFolderCommand ??= new DelegateCommand<DownloadedItem>(ExecuteOpenFolderCommand);


    private static readonly IReadOnlyDictionary<string, string[]> FileSuffixMap = new Dictionary<string, string[]>
    {
        { "downloadVideo", new[] { ".mp4", ".flv" } },
        { "downloadAudio", new[] { ".aac", ".mp3" } },
        { "downloadCover", new[] { ".jpg" } },
        { "downloadDanmaku", new[] { ".ass" } },
        { "downloadSubtitle", new[] { ".srt" } }
    };

    /// <summary>
    /// 打开文件夹事件
    /// </summary>
    private void ExecuteOpenFolderCommand(DownloadedItem downloadedItem)
    {
        if (downloadedItem.DownloadBase == null)
        {
            return;
        }

        var downLoadContents = downloadedItem.DownloadBase.NeedDownloadContent.Where(e => e.Value == true).Select(e => e.Key);
        var fileSuffixes = downLoadContents
            .Where(content => FileSuffixMap.ContainsKey(content))
            .SelectMany(content => FileSuffixMap[content])
            .ToArray();
        if (fileSuffixes.Length <= 0) return;
        foreach (var suffix in fileSuffixes)
        {
            var videoPath = $"{downloadedItem.DownloadBase.FilePath}{suffix}";
            var fileInfo = new FileInfo(videoPath);
            if (!File.Exists(fileInfo.FullName) || fileInfo.DirectoryName == null) continue;
            PlatformHelper.OpenFolder(fileInfo.DirectoryName);
            return;
        }

        EventAggregator.GetEvent<MessageEvent>().Publish("没有找到视频文件，可能被删除或移动！");
    }

    // 删除事件
    private DelegateCommand<DownloadedItem>? _removeVideoCommand;

    public DelegateCommand<DownloadedItem> RemoveVideoCommand => _removeVideoCommand ??= new DelegateCommand<DownloadedItem>(ExecuteRemoveVideoCommand);

    /// <summary>
    /// 删除事件
    /// </summary>
    private async void ExecuteRemoveVideoCommand(DownloadedItem downloadedItem)
    {
        var alertService = new AlertService(DialogService);
        var result = await alertService.ShowWarning(DictionaryResource.GetString("ConfirmDelete"), 2);
        if (result != ButtonResult.OK)
        {
            return;
        }

        App.DownloadedList.Remove(downloadedItem);
        _downloadStorageService.RemoveDownloaded(downloadedItem);
    }

    #endregion
}