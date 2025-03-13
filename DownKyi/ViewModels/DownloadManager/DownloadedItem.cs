using System.Collections.Generic;
using System.IO;
using System.Linq;
using DownKyi.Images;
using DownKyi.Models;
using DownKyi.Services;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels.DownloadManager
{
    public class DownloadedItem : DownloadBaseItem
    {
        public DownloadedItem() : this(null)
        {
        }

        public DownloadedItem(IDialogService? dialogService) : base(dialogService)
        {
            // 打开文件夹按钮
            OpenFolder = ButtonIcon.Instance().Folder;
            OpenFolder.Fill = DictionaryResource.GetColor("ColorPrimary");

            // 打开视频按钮
            OpenVideo = ButtonIcon.Instance().Start;
            OpenVideo.Fill = DictionaryResource.GetColor("ColorPrimary");

            // 删除按钮
            RemoveVideo = ButtonIcon.Instance().Trash;
            RemoveVideo.Fill = DictionaryResource.GetColor("ColorWarning");
        }

        // model数据
        public Downloaded Downloaded { get; set; }

        //  下载速度
        public string? MaxSpeedDisplay
        {
            get => Downloaded.MaxSpeedDisplay;
            set
            {
                Downloaded.MaxSpeedDisplay = value;
                RaisePropertyChanged();
            }
        }

        // 完成时间
        public string FinishedTime
        {
            get => Downloaded.FinishedTime;
            set
            {
                Downloaded.FinishedTime = value;
                RaisePropertyChanged("FinishedTime");
            }
        }

        #region 控制按钮

        private VectorImage _openFolder;

        public VectorImage OpenFolder
        {
            get => _openFolder;
            set => SetProperty(ref _openFolder, value);
        }

        private VectorImage _openVideo;

        public VectorImage OpenVideo
        {
            get => _openVideo;
            set => SetProperty(ref _openVideo, value);
        }

        private VectorImage _removeVideo;

        public VectorImage RemoveVideo
        {
            get => _removeVideo;
            set => SetProperty(ref _removeVideo, value);
        }

        #endregion

        #region 命令申明

        // 打开文件夹事件
        private DelegateCommand? _openFolderCommand;

        public DelegateCommand OpenFolderCommand => _openFolderCommand ??= new DelegateCommand(ExecuteOpenFolderCommand);


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
        private void ExecuteOpenFolderCommand()
        {
            if (DownloadBase == null)
            {
                return;
            }

            var downLoadContents = DownloadBase.NeedDownloadContent.Where(e => e.Value == true).Select(e => e.Key);
            var fileSuffixes = downLoadContents
                .Where(content => FileSuffixMap.ContainsKey(content))
                .SelectMany(content => FileSuffixMap[content])
                .ToArray();
            if (fileSuffixes.Length <= 0) return;
            foreach (var suffix in fileSuffixes)
            {
                var videoPath = $"{DownloadBase.FilePath}{suffix}";
                var fileInfo = new FileInfo(videoPath);
                if (File.Exists(fileInfo.FullName) && fileInfo.DirectoryName != null)
                {
                    PlatformHelper.OpenFolder(fileInfo.DirectoryName);
                    return;
                }
            }
            // eventAggregator.GetEvent<MessageEvent>().Publish("没有找到视频文件，可能被删除或移动！");
        }

        // 打开视频事件
        private DelegateCommand? _openVideoCommand;
        public DelegateCommand OpenVideoCommand => _openVideoCommand ??= new DelegateCommand(ExecuteOpenVideoCommand);

        /// <summary>
        /// 打开视频事件
        /// </summary>
        private void ExecuteOpenVideoCommand()
        {
            if (DownloadBase == null)
            {
                return;
            }

            var videoPath = $"{DownloadBase.FilePath}.mp4";
            var fileInfo = new FileInfo(videoPath);
            if (File.Exists(fileInfo.FullName))
            {
                PlatformHelper.Open(fileInfo.FullName);
            }
            else
            {
                //eventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipAddDownloadingZero"));
                //eventAggregator.GetEvent<MessageEvent>().Publish("没有找到视频文件，可能被删除或移动！");
            }
        }

        // 删除事件
        private DelegateCommand? _removeVideoCommand;

        public DelegateCommand RemoveVideoCommand => _removeVideoCommand ??= new DelegateCommand(ExecuteRemoveVideoCommand);

        /// <summary>
        /// 删除事件
        /// </summary>
        private async void ExecuteRemoveVideoCommand()
        {
            var alertService = new AlertService(DialogService);
            var result = await alertService.ShowWarning(DictionaryResource.GetString("ConfirmDelete"), 2);
            if (result != ButtonResult.OK)
            {
                return;
            }

            App.DownloadedList?.Remove(this);
        }

        #endregion
    }
}