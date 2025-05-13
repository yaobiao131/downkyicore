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
    }
    
    protected internal virtual void ExecuteBackSpace()
    {
      
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
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