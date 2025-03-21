using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace DownKyi.CustomAction;

public class IncrementalLoadingBehavior<T>: Behavior<ListBox>
{
    
    public static readonly StyledProperty<Func<int, Task<T[]>>> LoadPageFuncProperty =
        AvaloniaProperty.Register<IncrementalLoadingBehavior<T>, Func<int, Task<T[]>>>(nameof(LoadPageFunc));
    
    private int currentPage = 1;
    private bool isLoading = false;
    public Func<int, Task<T[]>> LoadPageFunc
    {
        get => GetValue(LoadPageFuncProperty);
        set => SetValue(LoadPageFuncProperty, value);
    }
    
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(ScrollViewer.ScrollChangedEvent, OnScrollChanged);
        LoadNextPageAsync();
    }
    
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.RemoveHandler(ScrollViewer.ScrollChangedEvent, OnScrollChanged);
    }
    
    private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var scrollViewer = e.Source as ScrollViewer;
        if (scrollViewer == null) return;
        
        if (scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height)
        {
            await LoadNextPageAsync();
        }
    }
    
    private async Task LoadNextPageAsync()
    {
        if (isLoading || LoadPageFunc == null ) return;

        isLoading = true;
        var items = await LoadPageFunc(currentPage);
        foreach (var item in items)
        {
            AssociatedObject.Items.Add(item);
        }
        currentPage++;
        isLoading = false;
    }
}