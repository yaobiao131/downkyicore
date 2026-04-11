using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace DownKyi.CustomAction;

public record MoveTabParam(int SourceIndex, int TargetIndex);


public class DragTabBehavior : Behavior<ListBox>
{
    public static readonly StyledProperty<ICommand?> MoveTabCommandProperty =
        AvaloniaProperty.Register<DragTabBehavior, ICommand?>(nameof(MoveTabCommand));

    public ICommand? MoveTabCommand
    {
        get => GetValue(MoveTabCommandProperty);
        set => SetValue(MoveTabCommandProperty, value);
    }

    private ListBoxItem? _draggedItem;
    private Point _dragStartPoint;
    private int _draggedIndex = -1;
    private const double DragThreshold = 3;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject == null) return;

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AssociatedObject.AddHandler(DragDrop.DropEvent, OnDrop);
        DragDrop.SetAllowDrop(AssociatedObject, true);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject == null) return;

        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        AssociatedObject.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
        AssociatedObject.RemoveHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(AssociatedObject);
        if (!point.Properties.IsLeftButtonPressed) return;

        var item = GetItemAtPoint(e.GetPosition(AssociatedObject));
        if (item == null) return;

        _draggedItem = item;
        _dragStartPoint = e.GetPosition(AssociatedObject);
        _draggedIndex = GetItemIndex(item);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedItem == null || _draggedIndex < 0) return;
        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            ResetDragState();
            return;
        }

        var position = e.GetPosition(AssociatedObject);
        if (Math.Abs(position.X - _dragStartPoint.X) <= DragThreshold &&
            Math.Abs(position.Y - _dragStartPoint.Y) <= DragThreshold)
        {
            return;
        }

        e.Handled = true;

        var data = new DataObject();
        data.Set("TabIndex", _draggedIndex);

        DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
        ResetDragState();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ResetDragState();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains("TabIndex") ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;
        if (!e.Data.Contains("TabIndex")) return;

        var sourceIndex = e.Data.Get("TabIndex") is int idx ? idx : -1;
        if (sourceIndex < 0 || AssociatedObject == null) return;

        var targetItem = GetItemAtPoint(e.GetPosition(AssociatedObject));
        var targetIndex = targetItem != null ? GetItemIndex(targetItem) : -1;
        if (targetIndex < 0 || targetIndex == sourceIndex) return;

        MoveTabCommand?.Execute(new MoveTabParam(sourceIndex, targetIndex));
    }

    private ListBoxItem? GetItemAtPoint(Point point)
    {
        if (AssociatedObject == null) return null;
        var hitTest = AssociatedObject.InputHitTest(point);
        if (hitTest == null) return null;

        var current = hitTest as Control;
        while (current != null)
        {
            if (current is Button) return null;
            if (current is ListBoxItem item)
                return item;
            current = current.Parent as Control;
        }
        return null;
    }

    private int GetItemIndex(ListBoxItem item)
    {
        if (AssociatedObject == null || item.DataContext == null) return -1;
        return AssociatedObject.Items.IndexOf(item.DataContext);
    }

    private void ResetDragState()
    {
        _draggedItem = null;
        _draggedIndex = -1;
    }
}
