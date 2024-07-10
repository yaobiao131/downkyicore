using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
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
    public class ViewDownloadFinishedViewModel : ViewModelBase
    {
        public const string Tag = "PageDownloadManagerDownloadFinished";

        #region 页面属性申明

        private ObservableCollection<DownloadedItem> _downloadedList;
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

        public ViewDownloadFinishedViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            // 初始化DownloadedList
            DownloadedList = App.DownloadedList;
            DownloadedList.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    SetDialogService();
                }
            };
            SetDialogService();

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
            if (parameter is not int index) { return; }

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
                var list = DownloadedList.ToList();
                foreach (var item in list)
                {
                    App.PropertyChangeAsync(() =>
                    {
                        App.DownloadedList.Remove(item);
                    });
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
                    var list = DownloadedList.ToList();
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
