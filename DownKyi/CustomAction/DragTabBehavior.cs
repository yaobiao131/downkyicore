using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
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
    private bool _isDragging;
    private const double DragThreshold = 3;
    private Transitions? _originalTransitions;
    private ScrollViewer? _scrollViewer;
    private List<double>? _originalCenters;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject == null) return;

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject == null) return;

        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        ResetDragState();
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

        if (!_isDragging)
        {
            if (Math.Abs(position.X - _dragStartPoint.X) <= DragThreshold &&
                Math.Abs(position.Y - _dragStartPoint.Y) <= DragThreshold)
            {
                return;
            }

            _isDragging = true;
            BeginDrag();
        }

        e.Handled = true;

        // 让被拖拽标签严格跟随鼠标（仅水平方向）
        var offsetX = position.X - _dragStartPoint.X;
        _draggedItem.RenderTransform = new TranslateTransform(offsetX, 0);

        // 计算目标插入位置并实时让位
        var targetIndex = CalculateTargetIndex(position);
        if (targetIndex >= 0 && targetIndex != _draggedIndex)
        {
            ApplySlideTransforms(targetIndex);
        }
        else if (targetIndex == _draggedIndex)
        {
            ClearSlideTransforms();
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && _draggedItem != null && AssociatedObject != null)
        {
            var position = e.GetPosition(AssociatedObject);
            var targetIndex = CalculateTargetIndex(position);

            if (targetIndex >= 0 && targetIndex != _draggedIndex)
            {
                MoveTabCommand?.Execute(new MoveTabParam(_draggedIndex, targetIndex));
            }
        }

        ResetDragState();
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetDragState();
    }

    private void BeginDrag()
    {
        if (_draggedItem == null || AssociatedObject == null) return;

        // 保存并临时清除 transitions，让被拖拽项严格跟随鼠标（无动画延迟）
        _originalTransitions = _draggedItem.Transitions;
        _draggedItem.Transitions = null;

        _draggedItem.SetValue(Visual.ZIndexProperty, 1000);
        _draggedItem.Opacity = 0.92;

        // 临时禁用祖先 ScrollViewer 的裁剪，使标签能拖到容器边缘外也不消失
        _scrollViewer = FindAncestor<ScrollViewer>(_draggedItem);
        if (_scrollViewer != null)
        {
            _scrollViewer.ClipToBounds = false;
        }

        // 缓存所有标签的原始中心点 X 坐标，避免 RenderTransform 干扰后续位置计算
        _originalCenters = new List<double>();
        var containers = GetOrderedContainers();
        foreach (var container in containers)
        {
            var center = container.TranslatePoint(new Point(container.Bounds.Width / 2, 0), AssociatedObject);
            _originalCenters.Add(center?.X ?? 0);
        }
    }

    private void EndDrag()
    {
        if (_draggedItem == null) return;

        _draggedItem.RenderTransform = null;
        _draggedItem.Opacity = 1.0;
        _draggedItem.SetValue(Visual.ZIndexProperty, 0);
        _draggedItem.Transitions = _originalTransitions;
        _originalTransitions = null;

        if (_scrollViewer != null)
        {
            _scrollViewer.ClipToBounds = true;
            _scrollViewer = null;
        }

        _originalCenters = null;
    }

    private int CalculateTargetIndex(Point mousePosition)
    {
        if (_originalCenters == null || _originalCenters.Count == 0) return 0;

        int targetIndex = 0;
        for (int i = 0; i < _originalCenters.Count; i++)
        {
            if (mousePosition.X > _originalCenters[i])
            {
                targetIndex = i + 1;
            }
            else
            {
                break;
            }
        }

        return Math.Clamp(targetIndex, 0, _originalCenters.Count);
    }

    private void ApplySlideTransforms(int targetIndex)
    {
        if (AssociatedObject == null || _draggedItem == null || _originalCenters == null) return;

        var containers = GetOrderedContainers();
        if (containers.Count == 0) return;

        // 用相邻原始中心点间距作为滑动距离，更准确
        double draggedWidth = _draggedItem.Bounds.Width;
        if (_originalCenters.Count > 1 && _draggedIndex < _originalCenters.Count - 1)
        {
            draggedWidth = _originalCenters[_draggedIndex + 1] - _originalCenters[_draggedIndex];
        }
        else if (_originalCenters.Count > 1 && _draggedIndex > 0)
        {
            draggedWidth = _originalCenters[_draggedIndex] - _originalCenters[_draggedIndex - 1];
        }

        for (int i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
            if (container == _draggedItem) continue;

            double offsetX = 0;

            if (_draggedIndex < targetIndex)
            {
                if (i > _draggedIndex && i <= targetIndex)
                {
                    offsetX = -draggedWidth;
                }
            }
            else
            {
                if (i >= targetIndex && i < _draggedIndex)
                {
                    offsetX = draggedWidth;
                }
            }

            container.RenderTransform = offsetX != 0
                ? new TranslateTransform(offsetX, 0)
                : null;
        }
    }

    private void ClearSlideTransforms()
    {
        if (AssociatedObject == null) return;

        foreach (var container in GetOrderedContainers())
        {
            if (container != _draggedItem)
            {
                container.RenderTransform = null;
            }
        }
    }

    private List<ListBoxItem> GetOrderedContainers()
    {
        if (AssociatedObject == null) return new List<ListBoxItem>();

        var result = new List<ListBoxItem>();
        for (int i = 0; i < AssociatedObject.Items.Count; i++)
        {
            if (AssociatedObject.ContainerFromIndex(i) is ListBoxItem item)
            {
                result.Add(item);
            }
        }
        return result;
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
        if (_draggedItem != null)
        {
            EndDrag();
        }

        ClearSlideTransforms();

        _draggedItem = null;
        _draggedIndex = -1;
        _isDragging = false;
    }

    private static T? FindAncestor<T>(Visual? visual) where T : Visual
    {
        while (visual != null)
        {
            if (visual is T target) return target;
            visual = visual.GetVisualParent();
        }
        return null;
    }
}
