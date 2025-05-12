using System.Collections.Generic;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.Utils;
using DownKyi.ViewModels.DownloadManager;
using DownKyi.ViewModels.PageViewModels;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class ViewDownloadManagerViewModel : ViewModelBase
{
    public const string Tag = "PageDownloadManager";

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

    public ViewDownloadManagerViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(
        eventAggregator)
    {
        _regionManager = regionManager;

        #region 属性初始化

        TabHeaders = new List<TabHeader>
        {
            new()
            {
                Id = 0, Image = NormalIcon.Instance().Downloading, Title = DictionaryResource.GetString("Downloading")
            },
            new()
            {
                Id = 1, Image = NormalIcon.Instance().DownloadFinished,
                Title = DictionaryResource.GetString("DownloadFinished")
            }
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
    protected internal override void ExecuteBackSpace()
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

        var param = new NavigationParameters();

        switch (tabHeader.Id)
        {
            case 0:
                _regionManager.RequestNavigate("DownloadManagerContentRegion", ViewDownloadingViewModel.Tag, param);
                break;
            case 1:
                _regionManager.RequestNavigate("DownloadManagerContentRegion", ViewDownloadFinishedViewModel.Tag, param);
                break;
            default:
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

        //// 进入设置页面时显示的设置项
        SelectTabId = 0;

        PropertyChangeAsync(() => { _regionManager.RequestNavigate("DownloadManagerContentRegion", ViewDownloadingViewModel.Tag, new NavigationParameters()); });
    }
}