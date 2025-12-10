using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DownKyi.CustomAction;

public class ScrollIntoViewBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject.SelectedItem == null)
        {
            return;
        }

        // 等待UI更新完成
        await Task.Delay(100);

        // 使用UI线程异步执行滚动操作
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            // 直接使用DataGrid的ScrollIntoView方法滚动到选中项
            AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem, null);
        });
    }
}