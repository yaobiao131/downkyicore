using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Models;
using DownKyi.Services;
using DownKyi.Utils;
using DownKyi.ViewModels.Dialogs;
using DownKyi.CustomAction;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.ViewModels;

public class MainWindowViewModel : BindableBase
{
    public const string Tag = "MainWindow";

    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;
    private readonly IDialogService _dialogService;

    private const string ContentRegion = nameof(ContentRegion);

    private ClipboardListener? _clipboardListener;
    private bool _messageVisibility;
    private string? _oldMessage;

    private readonly Dictionary<string, object> _tabViewCache = new();

    public bool MessageVisibility
    {
        get => _messageVisibility;
        set => SetProperty(ref _messageVisibility, value);
    }

    private string? _message;

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
    
    private ObservableCollection<TabItemModel> _tabs = new();

    public ObservableCollection<TabItemModel> Tabs
    {
        get => _tabs;
        set => SetProperty(ref _tabs, value);
    }
    
    private TabItemModel? _selectedTab;

    public TabItemModel? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value) && value != null)
            {
                SwitchToTab(value);
            }
        }
    }
    
    
    public DelegateCommand? LoadedCommand { get; }

    private DelegateCommand? _closingCommand;

    public DelegateCommand ClosingCommand => _closingCommand ??= _closingCommand = new DelegateCommand(ExecuteClosingCommand);

    public DelegateCommand<PointerPressedEventArgs> PointerPressedCommand =>
        new(ExecutePointerPressed);

    private DelegateCommand<TabItemModel?>? _closeTabCommand;
    
    public DelegateCommand<TabItemModel?> CloseTabCommand =>
        _closeTabCommand ??= new DelegateCommand<TabItemModel?>(ExecuteCloseTab);

    private DelegateCommand<TabItemModel?>? _closeOtherTabsCommand;
    
    public DelegateCommand<TabItemModel?> CloseOtherTabsCommand =>
        _closeOtherTabsCommand ??= new DelegateCommand<TabItemModel?>(ExecuteCloseOtherTabs);

    private DelegateCommand<TabItemModel?>? _closeRightTabsCommand;
    
    public DelegateCommand<TabItemModel?> CloseRightTabsCommand =>
        _closeRightTabsCommand ??= new DelegateCommand<TabItemModel?>(ExecuteCloseRightTabs);

    private DelegateCommand<TabItemModel?>? _closeLeftTabsCommand;
    
    public DelegateCommand<TabItemModel?> CloseLeftTabsCommand =>
        _closeLeftTabsCommand ??= new DelegateCommand<TabItemModel?>(ExecuteCloseLeftTabs);

    private DelegateCommand? _closeAllTabsCommand;
    
    public DelegateCommand CloseAllTabsCommand =>
        _closeAllTabsCommand ??= new DelegateCommand(ExecuteCloseAllTabs);

    private DelegateCommand<TabItemModel?>? _duplicateTabCommand;
    
    public DelegateCommand<TabItemModel?> DuplicateTabCommand =>
        _duplicateTabCommand ??= new DelegateCommand<TabItemModel?>(ExecuteDuplicateTab);

    private DelegateCommand<TabItemModel?>? _refreshTabCommand;

 
    public DelegateCommand<TabItemModel?> RefreshTabCommand =>
        _refreshTabCommand ??= new DelegateCommand<TabItemModel?>(ExecuteRefreshTab);

    private DelegateCommand<MoveTabParam?>? _moveTabCommand;

    public DelegateCommand<MoveTabParam?> MoveTabCommand =>
        _moveTabCommand ??= new DelegateCommand<MoveTabParam?>(ExecuteMoveTab);

    public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IDialogService dialogService)
    {
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        _dialogService = dialogService;

        // 订阅导航事件
        _eventAggregator.GetEvent<NavigationEvent>().Subscribe(OnNavigationRequest);

        // 订阅标签页标题更新事件
        _eventAggregator.GetEvent<TabTitleUpdateEvent>().Subscribe(OnTabTitleUpdate);

        // 订阅消息发送事件
        _eventAggregator.GetEvent<MessageEvent>().Subscribe(message =>
        {
            MessageVisibility = true;
            _oldMessage = Message;
            Message = message;
            var sleep = 2000;
            if (_oldMessage == Message)
            {
                sleep = 1500;
            }
            Thread.Sleep(sleep);
            MessageVisibility = false;
        }, ThreadOption.BackgroundThread);

        LoadedCommand = new DelegateCommand(() =>
        {
            if (Design.IsDesignMode)
            {
                return;
            }
            Upgrade();
            CheckForUpdates();
            _clipboardListener = new ClipboardListener(App.Current.MainWindow);
            _clipboardListener.Changed += ClipboardListenerOnChanged;

            // 默认打开首页标签
            var param = new NavigationParameters
            {
                { "Parent", "" },
                { "Parameter", "start" }
            };
            OpenNewTab(ViewIndexViewModel.Tag, "首页", param, true);
        });
    }

    
    private int _tabIdCounter;
    
    private void OnTabTitleUpdate(TabTitleUpdateParam param)
    {
        if (string.IsNullOrEmpty(param.NavigationKey) || string.IsNullOrEmpty(param.Title))
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var tab = Tabs.FirstOrDefault(t => t.NavigationKey == param.NavigationKey);
            if (tab != null)
            {
                tab.Title = param.Title;
            }
        });
    }

    private void OnNavigationRequest(NavigationParam view)
    {
        var param = new NavigationParameters
        {
            { "Parent", view.ParentViewName },
            { "Parameter", view.Parameter }
        };

        if (!string.IsNullOrEmpty(view.NavigationKey))
        {
            param.Add("NavigationKey", view.NavigationKey);
        }

        var title = string.IsNullOrEmpty(view.Title) ? GetDefaultTitle(view.ViewName) : view.Title;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            // 如果要求关闭某个源Tab（如登录成功后关闭登录页），先处理关闭
            if (!string.IsNullOrEmpty(view.CloseTabNavigationKey))
            {
                var tabToClose = Tabs.FirstOrDefault(t => t.NavigationKey == view.CloseTabNavigationKey);
                if (tabToClose != null)
                {
                    RemoveTabFromRegion(tabToClose);
                    Tabs.Remove(tabToClose);
                }
            }

            if (view.IsBackNavigation)
            {
                if (!string.IsNullOrEmpty(view.NavigationKey))
                {
                    var existingTab = Tabs.FirstOrDefault(t => t.NavigationKey == view.NavigationKey);
                    if (existingTab != null)
                    {
                        SelectedTab = existingTab;
                        return;
                    }
                }

                var fallbackTab = Tabs.FirstOrDefault(t =>
                    t.ViewName == view.ViewName &&
                    Equals(t.Parameters.FirstOrDefault(kvp => kvp.Key == "Parameter").Value, view.Parameter));
                if (fallbackTab != null)
                {
                    SelectedTab = fallbackTab;
                    return;
                }

                // 返回导航找不到目标Tab时，不应创建新Tab
                return;
            }

            // 将当前选中Tab的NavigationKey作为ParentNavigationKey传给新Tab
            if (SelectedTab != null && !string.IsNullOrEmpty(SelectedTab.NavigationKey))
            {
                param.Add("ParentNavigationKey", SelectedTab.NavigationKey);
            }

            OpenNewTab(view.ViewName, title, param);
        });
    }

    
    private string GetDefaultTitle(string? viewName)
    {
        return viewName switch
        {
            "PageIndex" => "首页",
            "PageVideoDetail" => "视频详情",
            "PageUserSpace" => "用户空间",
            "PageMySpace" => "我的空间",
            "PageDownloadManager" => "下载管理",
            "PageSettings" => "设置",
            "PageToolbox" => "工具箱",
            "PageMyFavorites" => "我的收藏",
            "PageMyHistory" => "历史记录",
            "PageMyBangumiFollow" => "追番",
            "PageMyToView" => "稍后再看",
            "PageFriends" => "关注",
            "PagePublication" => "投稿",
            "PageSeasonsSeries" => "合集",
            "PagePublicFavorites" => "公开收藏",
            "PageLogin" => "登录",
            _ => viewName ?? "新标签页"
        };
    }

   
    private void OpenNewTab(string? viewName, string title, NavigationParameters parameters, bool isHome = false)
    {
        if (string.IsNullOrEmpty(viewName)) return;

        var tabId = $"tab_{Interlocked.Increment(ref _tabIdCounter)}";
        var navigationKey = $"{viewName}_{tabId}";
        parameters.Add("NavigationKey", navigationKey);

        // 浅拷贝参数，防止外部后续修改影响 Tab 状态
        var tabParameters = new NavigationParameters();
        foreach (var kvp in parameters)
        {
            tabParameters.Add(kvp.Key, kvp.Value);
        }

        var tab = new TabItemModel
        {
            Id = tabId,
            Title = title,
            ViewName = viewName,
            Parameters = tabParameters,
            CanClose = !isHome,
            IsHome = isHome,
            NavigationKey = navigationKey
        };

        Tabs.Add(tab);
        SelectedTab = tab;
    }

   
    private void SwitchToTab(TabItemModel tab, bool isRefresh = false)
    {
        try
        {
            var region = _regionManager.Regions[ContentRegion];

            if (!isRefresh
                && tab.ViewName != ViewIndexViewModel.Tag
                && !string.IsNullOrEmpty(tab.NavigationKey)
                && _tabViewCache.TryGetValue(tab.NavigationKey, out var cachedView)
                && region.Views.Contains(cachedView))
            {
                region.Activate(cachedView);
                return;
            }

            var navService = region.NavigationService;
            EventHandler<RegionNavigationEventArgs>? successHandler = null;
            EventHandler<RegionNavigationFailedEventArgs>? failedHandler = null;

            var sch = successHandler;
            var fdh = failedHandler;
            successHandler = (_, e) =>
            {
                navService.Navigated -= sch;
                navService.NavigationFailed -= fdh;
                if (e.Uri.OriginalString != tab.ViewName) return;
                var activeView = region.ActiveViews.FirstOrDefault();
                if (activeView != null && !string.IsNullOrEmpty(tab.NavigationKey))
                {
                    _tabViewCache[tab.NavigationKey] = activeView;
                }
            };

            failedHandler = (_, _) =>
            {
                navService.Navigated -= sch;
                navService.NavigationFailed -= fdh;
            };

            navService.Navigated += successHandler;
            navService.NavigationFailed += failedHandler;

            _regionManager.RequestNavigate(ContentRegion, tab.ViewName, tab.Parameters);
        }
        catch (Exception e)
        {
            LogManager.Error(Tag, e);
            Console.PrintLine("切换标签页发生异常: {0}", e);
        }
    }

  
    private void RemoveTabFromRegion(TabItemModel? tab)
    {
        if (tab == null || string.IsNullOrEmpty(tab.NavigationKey)) return;
        if (_tabViewCache.TryGetValue(tab.NavigationKey, out var cachedView))
        {
            try
            {
                if (cachedView is Avalonia.Visual visual && visual.DataContext is ViewModelBase vm)
                {
                    vm.OnTabClosed();
                }

                _regionManager.Regions[ContentRegion].Remove(cachedView);
            }
            catch (Exception e)
            {
                LogManager.Error(Tag, e);
            }
            _tabViewCache.Remove(tab.NavigationKey);
        }
    }

    private void ExecuteCloseTab(TabItemModel? tab)
    {
        if (tab == null || !tab.CanClose) return;

        var index = Tabs.IndexOf(tab);
        if (index < 0) return;

        if (SelectedTab == tab)
        {
            // 优先切换到左侧标签，如果没有则切换到右侧
            var newIndex = index > 0 ? index - 1 : (index < Tabs.Count - 1 ? index + 1 : -1);
            if (newIndex >= 0)
            {
                SelectedTab = Tabs[newIndex];
            }
            else
            {
                SelectedTab = null;
            }
        }

        RemoveTabFromRegion(tab);
        Tabs.RemoveAt(index);

        if (Tabs.Count == 0)
        {
            var param = new NavigationParameters
            {
                { "Parent", "" },
                { "Parameter", "start" }
            };
            OpenNewTab(ViewIndexViewModel.Tag, "首页", param, true);
        }
    }
    
    private void ExecuteCloseOtherTabs(TabItemModel? tab)
    {
        if (tab == null) return;

        var tabsToClose = Tabs.Where(t => t != tab && t.CanClose).ToList();
        foreach (var t in tabsToClose)
        {
            RemoveTabFromRegion(t);
            Tabs.Remove(t);
        }

        SelectedTab = tab;
    }
    
    private void ExecuteCloseRightTabs(TabItemModel? tab)
    {
        if (tab == null) return;

        var index = Tabs.IndexOf(tab);
        if (index < 0 || index >= Tabs.Count - 1) return;

        var tabsToClose = Tabs.Skip(index + 1).Where(t => t.CanClose).ToList();
        var selectedRemoved = tabsToClose.Contains(SelectedTab);
        foreach (var t in tabsToClose)
        {
            RemoveTabFromRegion(t);
            Tabs.Remove(t);
        }

        if (selectedRemoved)
        {
            SelectedTab = tab;
        }
    }
    
    private void ExecuteCloseLeftTabs(TabItemModel? tab)
    {
        if (tab == null) return;

        var index = Tabs.IndexOf(tab);
        if (index <= 0) return;

        var tabsToClose = Tabs.Take(index).Where(t => t.CanClose).ToList();
        foreach (var t in tabsToClose)
        {
            RemoveTabFromRegion(t);
            Tabs.Remove(t);
        }

        SelectedTab = tab;
    }

  
    private void ExecuteCloseAllTabs()
    {
        var tabsToClose = Tabs.Where(t => t.CanClose).ToList();
        foreach (var t in tabsToClose)
        {
            RemoveTabFromRegion(t);
            Tabs.Remove(t);
        }

        // 如果没有标签了，默认打开首页
        if (Tabs.Count == 0)
        {
            var param = new NavigationParameters
            {
                { "Parent", "" },
                { "Parameter", "start" }
            };
            OpenNewTab(ViewIndexViewModel.Tag, "首页", param, true);
        }
        else
        {
            SelectedTab = Tabs.First();
        }
    }

   
    private void ExecuteDuplicateTab(TabItemModel? tab)
    {
        if (tab == null) return;

        var newParams = new NavigationParameters();
        foreach (var kvp in tab.Parameters)
        {
            if (kvp.Key == "NavigationKey") continue;
            newParams.Add(kvp.Key, kvp.Value);
        }

        OpenNewTab(tab.ViewName, tab.Title, newParams);
    }

    
    private void ExecuteRefreshTab(TabItemModel? tab)
    {
        if (tab == null) return;

        if (SelectedTab != tab)
        {
            SelectedTab = tab;
        }

        // 刷新前先从 Region 和缓存中移除旧视图，防止重复实例和内存泄漏
        RemoveTabFromRegion(tab);

        // 使用完整参数重新导航，以实现真正的刷新
        SwitchToTab(tab, true);
    }

    private void ExecuteMoveTab(MoveTabParam? param)
    {
        if (param == null) return;

        var sourceIndex = param.SourceIndex;
        var targetIndex = param.TargetIndex;
        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex) return;
        if (sourceIndex >= Tabs.Count || targetIndex >= Tabs.Count) return;

        var item = Tabs[sourceIndex];
        var wasSelected = SelectedTab == item;

        if (wasSelected)
        {
            SelectedTab = null;
        }

        Tabs.RemoveAt(sourceIndex);
        Tabs.Insert(targetIndex, item);

        if (wasSelected)
        {
            SelectedTab = item;
        }
    }

    #region 剪贴板

    private int _times;

    private void ClipboardListenerOnChanged(string obj)
    {
        #region 执行第二遍时跳过

        _times += 1;
        var timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 300)
        };
        timer.Tick += (_, _) =>
        {
            timer.IsEnabled = false;
            _times = 0;
        };
        timer.IsEnabled = true;

        if (_times % 2 == 0)
        {
            timer.IsEnabled = false;
            _times = 0;
            return;
        }

        #endregion

        var isListenClipboard = SettingsManager.GetInstance().GetIsListenClipboard();
        if (isListenClipboard != AllowStatus.Yes)
        {
            return;
        }

        var searchService = new SearchService();
        Dispatcher.UIThread.InvokeAsync(() => { searchService.BiliInput(obj + AppConstant.ClipboardId, ViewIndexViewModel.Tag, _eventAggregator); });
    }

    #endregion

    private void ExecuteClosingCommand()
    {
        foreach (var tab in Tabs.ToList())
        {
            RemoveTabFromRegion(tab);
        }
        Tabs.Clear();

        _eventAggregator.GetEvent<NavigationEvent>().Unsubscribe(OnNavigationRequest);
        _eventAggregator.GetEvent<TabTitleUpdateEvent>().Unsubscribe(OnTabTitleUpdate);

        if (_clipboardListener == null) return;
        _clipboardListener.Changed -= ClipboardListenerOnChanged;
        _clipboardListener.Dispose();
    }

    private void ExecutePointerPressed(PointerPressedEventArgs e)
    {
    }

    private UserControl? GetCurrentUserControl() => _regionManager
        .Regions[ContentRegion].ActiveViews
        .FirstOrDefault() as UserControl;

    private void Upgrade()
    {
        _dialogService.ShowDialogAsync(ViewUpgradingDialogViewModel.Tag, new DialogParameters(), (result) => { });
    }

    private async void CheckForUpdates()
    {
        try
        {
            var isAutoUpdate = SettingsManager.GetInstance().GetAutoUpdateWhenLaunch() != AllowStatus.Yes;
            if (isAutoUpdate) return;
            var service = new VersionCheckerService(App.RepoOwner, App.RepoName,
                SettingsManager.GetInstance().GetIsReceiveBetaVersion() == AllowStatus.Yes);
            var release = await service.GetLatestReleaseAsync(SettingsManager.GetInstance().GetSkipVersionOnLaunch());
            if (release != null && service.IsNewVersionAvailable(release.TagName))
            {
                await _dialogService?.ShowDialogAsync(NewVersionAvailableDialogViewModel.Tag, new
                    DialogParameters { { "release", release }, { "enableSkipVersion", true } })!;
            }
        }
        catch (Exception ex)
        {
            /**/
        }
    }
}
