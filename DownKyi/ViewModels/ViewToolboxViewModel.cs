using System.Collections.Generic;
using DownKyi.Events;
using DownKyi.Utils;
using DownKyi.ViewModels.PageViewModels;
using DownKyi.ViewModels.Toolbox;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels
{
    public class ViewToolboxViewModel : ViewModelBase
    {
        public const string Tag = "PageToolbox";

        private readonly IRegionManager _regionManager;

        #region 页面属性申明

        private List<TabHeader> _tabHeaders;

        public List<TabHeader> TabHeaders
        {
            get => _tabHeaders;
            set => SetProperty(ref _tabHeaders, value);
        }

        private int _selectTabId;

        public int SelectTabId
        {
            get => _selectTabId;
            set => SetProperty(ref _selectTabId, value);
        }

        #endregion

        public ViewToolboxViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(
            eventAggregator)
        {
            _regionManager = regionManager;

            #region 属性初始化

            TabHeaders = new List<TabHeader>
            {
                new() { Id = 0, Title = DictionaryResource.GetString("BiliHelper") },
                new() { Id = 1, Title = DictionaryResource.GetString("Delogo") },
                new() { Id = 2, Title = DictionaryResource.GetString("ExtractMedia") }
            };

            #endregion
        }

        #region 命令申明

        // 返回事件
        private DelegateCommand? _backSpaceCommand;

        public DelegateCommand BackSpaceCommand => _backSpaceCommand ??= new DelegateCommand(ExecuteBackSpace);

        /// <summary>
        /// 返回事件
        /// </summary>
        private void ExecuteBackSpace()
        {
            var parameter = new NavigationParam
            {
                ViewName = ParentView,
                ParentViewName = null,
                Parameter = null
            };
            EventAggregator.GetEvent<NavigationEvent>().Publish(parameter);
        }

        // 左侧tab点击事件
        private DelegateCommand<object>? _leftTabHeadersCommand;

        public DelegateCommand<object> LeftTabHeadersCommand => _leftTabHeadersCommand ??= new DelegateCommand<object>(ExecuteLeftTabHeadersCommand);

        /// <summary>
        /// 左侧tab点击事件
        /// </summary>
        /// <param name="parameter"></param>
        private void ExecuteLeftTabHeadersCommand(object parameter)
        {
            if (parameter is not TabHeader tabHeader)
            {
                return;
            }

            switch (tabHeader.Id)
            {
                case 0:
                    _regionManager.RequestNavigate("ToolboxContentRegion", ViewBiliHelperViewModel.Tag);
                    break;
                case 1:
                    _regionManager.RequestNavigate("ToolboxContentRegion", ViewDelogoViewModel.Tag);
                    break;
                case 2:
                    _regionManager.RequestNavigate("ToolboxContentRegion", ViewExtractMediaViewModel.Tag);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 导航到页面时执行
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            // 进入设置页面时显示的设置项
            SelectTabId = 0;
            PropertyChangeAsync(() => { _regionManager.RequestNavigate("ToolboxContentRegion", ViewBiliHelperViewModel.Tag); });
        }
    }
}