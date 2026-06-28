using System;
using Avalonia.Threading;
using DownKyi.PrismExtension.Dialog;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace DownKyi.ViewModels;

public class ViewModelBase : BindableBase, INavigationAware
{
    protected readonly IEventAggregator EventAggregator;
    protected IDialogService? DialogService;
    protected IRegionNavigationJournal? Journal;
    protected string ParentView = string.Empty;
    protected string ParentNavigationKey = string.Empty;
    protected string NavigationKey = string.Empty;

    public IRegionManager? ScopedRegionManager { get; private set; }

    public void SetScopedRegionManager(IRegionManager regionManager)
    {
        ScopedRegionManager = regionManager;
    }

    public ViewModelBase(IEventAggregator eventAggregator)
    {
        EventAggregator = eventAggregator;
    }

    public ViewModelBase(IEventAggregator eventAggregator, IDialogService dialogService)
    {
        EventAggregator = eventAggregator;
        DialogService = dialogService;
    }
    

    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {
        Journal = navigationContext.NavigationService.Journal;
        var viewName = navigationContext.Parameters.GetValue<string>("Parent");
        if (viewName != null)
        {
            ParentView = viewName;
        }

        var parentKey = navigationContext.Parameters.GetValue<string>("ParentNavigationKey");
        if (parentKey != null)
        {
            ParentNavigationKey = parentKey;
        }

        var navKey = navigationContext.Parameters.GetValue<string>("NavigationKey");
        if (navKey != null)
        {
            NavigationKey = navKey;
        }
    }
    
    protected internal virtual void ExecuteBackSpace()
    {
        var parameter = new Events.NavigationParam
        {
            ViewName = ParentView,
            ParentViewName = null,
            Parameter = null,
            IsBackNavigation = true,
            NavigationKey = ParentNavigationKey
        };
        EventAggregator.GetEvent<Events.NavigationEvent>().Publish(parameter);
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        var requestedKey = navigationContext.Parameters.GetValue<string>("NavigationKey");
        return string.IsNullOrEmpty(NavigationKey) || NavigationKey == requestedKey;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public virtual void OnTabClosed()
    {
    }

    /// <summary>
    /// 异步修改绑定到UI的属性
    /// </summary>
    /// <param name="callback"></param>
    protected void PropertyChangeAsync(Action callback)
    {
        Dispatcher.UIThread.InvokeAsync(callback);
    }

    /// <summary>
    /// 同步修改绑定到UI的属性
    /// </summary>
    /// <param name="callback"></param>
    protected void PropertyChange(Action callback)
    {
        Dispatcher.UIThread.Invoke(callback);
    }
    
}