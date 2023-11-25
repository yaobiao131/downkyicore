using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Threading;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace DownKyi.ViewModels;

public class ViewModelBase : BindableBase, INavigationAware
{
    protected readonly IEventAggregator eventAggregator;
    protected IDialogService dialogService;
    protected string ParentView = string.Empty;
    public event PropertyChangedEventHandler PropertyChanged;

    public ViewModelBase(IEventAggregator eventAggregator)
    {
        this.eventAggregator = eventAggregator;
    }

    public ViewModelBase(IEventAggregator eventAggregator, IDialogService dialogService)
    {
        this.eventAggregator = eventAggregator;
        this.dialogService = dialogService;
    }

    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {
        string viewName = navigationContext.Parameters.GetValue<string>("Parent");
        if (viewName != null)
        {
            ParentView = viewName;
        }
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
}