using DownKyi.Core.BiliApi.VideoStream.Models;
using DownKyi.Images;
using DownKyi.Models;
using DownKyi.Utils;
using Downloader;
using Prism.Commands;
using DownloadStatus = DownKyi.Models.DownloadStatus;

namespace DownKyi.ViewModels.DownloadManager
{
    public class DownloadingItem : DownloadBaseItem
    {

        public DownloadingItem()
        {
            // 暂停继续按钮
            StartOrPause = ButtonIcon.Instance().Pause;
            StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");

            // 删除按钮
            Delete = ButtonIcon.Instance().Delete;
            Delete.Fill = DictionaryResource.GetColor("ColorPrimary");
        }
        
        public DownloadService? DownloadService;

        // model数据
        private Downloading _downloading;

        public Downloading Downloading
        {
            get => _downloading;
            set
            {
                _downloading = value;

                switch (value.DownloadStatus)
                {
                    case DownloadStatus.NotStarted:
                    case DownloadStatus.WaitForDownload:
                        StartOrPause = ButtonIcon.Instance().Pause;
                        break;
                    case DownloadStatus.PauseStarted:
                        StartOrPause = ButtonIcon.Instance().Start;
                        break;
                    case DownloadStatus.Pause:
                        StartOrPause = ButtonIcon.Instance().Start;
                        break;
                    case DownloadStatus.Downloading:
                        StartOrPause = ButtonIcon.Instance().Pause;
                        break;
                    case DownloadStatus.DownloadSucceed:
                        // 下载成功后会从下载列表中删除
                        // 不会出现此分支
                        break;
                    case DownloadStatus.DownloadFailed:
                        StartOrPause = ButtonIcon.Instance().Retry;
                        break;
                    default:
                        break;
                }

                StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
            }
        }

        // 视频流链接
        public PlayUrl PlayUrl { get; set; }

        // 正在下载内容（音频、视频、弹幕、字幕、封面）
        public string? DownloadContent
        {
            get => Downloading.DownloadContent;
            set
            {
                Downloading.DownloadContent = value;
                RaisePropertyChanged();
            }
        }

        // 下载状态显示
        public string? DownloadStatusTitle
        {
            get => Downloading.DownloadStatusTitle;
            set
            {
                Downloading.DownloadStatusTitle = value;
                RaisePropertyChanged();
            }
        }

        // 下载进度
        public float Progress
        {
            get => Downloading.Progress;
            set
            {
                Downloading.Progress = value;
                RaisePropertyChanged();
            }
        }

        //  已下载大小/文件大小
        public string? DownloadingFileSize
        {
            get => Downloading.DownloadingFileSize;
            set
            {
                Downloading.DownloadingFileSize = value;
                RaisePropertyChanged();
            }
        }

        //  下载速度
        public string? SpeedDisplay
        {
            get => Downloading.SpeedDisplay;
            set
            {
                Downloading.SpeedDisplay = value;
                RaisePropertyChanged();
            }
        }

        // 操作提示
        private string _operationTip;

        public string OperationTip
        {
            get => _operationTip;
            set => SetProperty(ref _operationTip, value);
        }

        #region 控制按钮

        private VectorImage _startOrPause;

        public VectorImage StartOrPause
        {
            get => _startOrPause;
            set
            {
                SetProperty(ref _startOrPause, value);

                OperationTip = value.Equals(ButtonIcon.Instance().Start) ? DictionaryResource.GetString("StartDownload")
                    : value.Equals(ButtonIcon.Instance().Pause) ? DictionaryResource.GetString("PauseDownload")
                    : value.Equals(ButtonIcon.Instance().Retry) ? DictionaryResource.GetString("RetryDownload") : null;
            }
        }

        private VectorImage _delete;

        public VectorImage Delete
        {
            get => _delete;
            set => SetProperty(ref _delete, value);
        }

        #endregion

        #region 命令申明

        // 下载列表暂停继续事件
        private DelegateCommand? _startOrPauseCommand;
        public DelegateCommand StartOrPauseCommand => _startOrPauseCommand ??= new DelegateCommand(ExecuteStartOrPauseCommand);

        /// <summary>
        /// 下载列表暂停继续事件
        /// </summary>
        private void ExecuteStartOrPauseCommand()
        {
            switch (Downloading.DownloadStatus)
            {
                case DownloadStatus.NotStarted:
                case DownloadStatus.WaitForDownload:
                    Downloading.DownloadStatus = DownloadStatus.PauseStarted;
                    DownloadStatusTitle = DictionaryResource.GetString("Pausing");
                    StartOrPause = ButtonIcon.Instance().Start;
                    StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                    break;
                case DownloadStatus.PauseStarted:
                    Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                    DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                    StartOrPause = ButtonIcon.Instance().Pause;
                    StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                    break;
                case DownloadStatus.Pause:
                    Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                    DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                    StartOrPause = ButtonIcon.Instance().Pause;
                    StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                    break;
                //case DownloadStatus.PAUSE_TO_WAIT:
                case DownloadStatus.Downloading:
                    Downloading.DownloadStatus = DownloadStatus.Pause;
                    DownloadStatusTitle = DictionaryResource.GetString("Pausing");
                    StartOrPause = ButtonIcon.Instance().Start;
                    StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                    break;
                case DownloadStatus.DownloadSucceed:
                    // 下载成功后会从下载列表中删除
                    // 不会出现此分支
                    break;
                case DownloadStatus.DownloadFailed:
                    Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                    DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                    StartOrPause = ButtonIcon.Instance().Pause;
                    StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}