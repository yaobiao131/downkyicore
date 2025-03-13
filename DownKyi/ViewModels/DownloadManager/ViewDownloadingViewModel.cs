using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DownKyi.Core.Logging;
using DownKyi.Images;
using DownKyi.Models;
using DownKyi.Services;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Console = DownKyi.Core.Utils.Debugging.Console;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.ViewModels.DownloadManager
{
    public class ViewDownloadingViewModel : ViewModelBase
    {
        public const string Tag = "PageDownloadManagerDownloading";

        #region 页面属性申明

        private ObservableCollection<DownloadingItem> _downloadingList;

        public ObservableCollection<DownloadingItem> DownloadingList
        {
            get => _downloadingList;
            set => SetProperty(ref _downloadingList, value);
        }

        #endregion

        public ViewDownloadingViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(
            eventAggregator, dialogService)
        {
            // 初始化DownloadingList
            DownloadingList = App.DownloadingList;
            DownloadingList.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    SetDialogService();
                }
            };
            SetDialogService();
        }

        #region 命令申明

        // 暂停所有下载事件
        private DelegateCommand? _pauseAllDownloadingCommand;

        public DelegateCommand PauseAllDownloadingCommand => _pauseAllDownloadingCommand ??= new DelegateCommand(ExecutePauseAllDownloadingCommand);

        /// <summary>
        /// 暂停所有下载事件
        /// </summary>
        private void ExecutePauseAllDownloadingCommand()
        {
            foreach (var downloading in _downloadingList)
            {
                switch (downloading.Downloading.DownloadStatus)
                {
                    case DownloadStatus.NotStarted:
                    case DownloadStatus.WaitForDownload:
                        downloading.Downloading.DownloadStatus = DownloadStatus.Pause;
                        downloading.DownloadStatusTitle = DictionaryResource.GetString("Pausing");
                        downloading.StartOrPause = ButtonIcon.Instance().Start;
                        downloading.StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                        break;
                    case DownloadStatus.PauseStarted:
                        break;
                    case DownloadStatus.Pause:
                        break;
                    //case DownloadStatus.PAUSE_TO_WAIT:
                    case DownloadStatus.Downloading:
                        downloading.Downloading.DownloadStatus = DownloadStatus.Pause;
                        downloading.DownloadStatusTitle = DictionaryResource.GetString("Pausing");
                        downloading.StartOrPause = ButtonIcon.Instance().Start;
                        downloading.StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
                        break;
                    case DownloadStatus.DownloadSucceed:
                        // 下载成功后会从下载列表中删除
                        // 不会出现此分支
                        break;
                    case DownloadStatus.DownloadFailed:
                        break;
                }
            }
        }

        // 继续所有下载事件
        private DelegateCommand? _continueAllDownloadingCommand;

        public DelegateCommand ContinueAllDownloadingCommand => _continueAllDownloadingCommand ??= new DelegateCommand(ExecuteContinueAllDownloadingCommand);

        /// <summary>
        /// 继续所有下载事件
        /// </summary>
        private void ExecuteContinueAllDownloadingCommand()
        {
            foreach (var downloading in _downloadingList)
            {
                switch (downloading.Downloading.DownloadStatus)
                {
                    case DownloadStatus.NotStarted:
                    case DownloadStatus.WaitForDownload:
                        downloading.Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                        downloading.DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                        break;
                    case DownloadStatus.PauseStarted:
                        downloading.Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                        downloading.DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                        break;
                    case DownloadStatus.Pause:
                        downloading.Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                        downloading.DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                        break;
                    //case DownloadStatus.PAUSE_TO_WAIT:
                    //    break;
                    case DownloadStatus.Downloading:
                        break;
                    case DownloadStatus.DownloadSucceed:
                        // 下载成功后会从下载列表中删除
                        // 不会出现此分支
                        break;
                    case DownloadStatus.DownloadFailed:
                        downloading.Downloading.DownloadStatus = DownloadStatus.WaitForDownload;
                        downloading.DownloadStatusTitle = DictionaryResource.GetString("Waiting");
                        break;
                }

                downloading.StartOrPause = ButtonIcon.Instance().Pause;
                downloading.StartOrPause.Fill = DictionaryResource.GetColor("ColorPrimary");
            }
        }

        // 删除所有下载事件
        private DelegateCommand? _deleteAllDownloadingCommand;

        public DelegateCommand DeleteAllDownloadingCommand => _deleteAllDownloadingCommand ??= new DelegateCommand(ExecuteDeleteAllDownloadingCommand);

        /// <summary>
        /// 删除所有下载事件
        /// </summary>
        private async void ExecuteDeleteAllDownloadingCommand()
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
            await Task.Run(() =>
            {
                var list = DownloadingList.ToList();
                foreach (var item in list)
                {
                    App.PropertyChangeAsync(() => { App.DownloadingList?.Remove(item); });
                }
            });
        }

        #endregion

        private async void SetDialogService()
        {
            try
            {
                await Task.Run(() =>
                {
                    var list = DownloadingList.ToList();
                    foreach (var item in list)
                    {
                        if (item != null && item.DialogService == null)
                        {
                            item.DialogService = DialogService;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Console.PrintLine("SetDialogService()发生异常: {0}", e);
                LogManager.Error($"{Tag}.SetDialogService()", e);
            }
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            SetDialogService();
        }
    }
}