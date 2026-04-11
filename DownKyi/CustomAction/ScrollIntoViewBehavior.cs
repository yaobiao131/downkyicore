using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace DownKyi.CustomAction;

public class ScrollIntoViewBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }
        base.OnDetaching();
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var grid = AssociatedObject;
        if (grid == null || grid.SelectedItem == null)
        {
            return;
        }

        try
        {
            // 等待UI更新完成
            await Task.Delay(100);

            if (AssociatedObject == null || AssociatedObject.SelectedItem == null)
            {
                return;
            }

            // 使用UI线程异步执行滚动操作
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // 直接使用DataGrid的ScrollIntoView方法滚动到选中项
                AssociatedObject?.ScrollIntoView(AssociatedObject.SelectedItem, null);
            }, DispatcherPriority.Background);
        }
        catch (Exception)
        {
            /**/
        }
    }
}