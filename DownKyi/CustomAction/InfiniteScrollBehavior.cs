using Avalonia.Controls;
using System.Windows.Input;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;


namespace DownKyi.CustomAction;
public class InfiniteScrollBehavior : Behavior<ListBox>
{
    private bool _isExecuting;

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<InfiniteScrollBehavior, ICommand?>(
            nameof(LoadMoreCommand));

    public ICommand? LoadMoreCommand
    {
        get => GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }
    
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(
            ScrollViewer.ScrollChangedEvent, 
            HandleScrollChanged);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.RemoveHandler(ScrollViewer.ScrollChangedEvent, HandleScrollChanged);
    }

    private void HandleScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isExecuting || LoadMoreCommand == null)
            return;
        
        var scrollViewer = e.Source as ScrollViewer;

        if (scrollViewer == null || 
            scrollViewer.Offset.Y + scrollViewer.Viewport.Height < scrollViewer.Extent.Height - 50)
            return;

        _isExecuting = true;
    
        try
        {
            if (LoadMoreCommand?.CanExecute(null) == true)
            {
                LoadMoreCommand.Execute(null);
            }
        }
        finally
        {
            _isExecuting = false;
        }
    }
    
}