using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace DownKyi.CustomControl;

public class VideoFramePreview : Control
{
    public static readonly DirectProperty<VideoFramePreview, IImage?> SourceProperty =
        AvaloniaProperty.RegisterDirect<VideoFramePreview, IImage?>(
            nameof(Source),
            o => o.Source,
            (o, v) => o.Source = v);

    private IImage? _source;

    public IImage? Source
    {
        get => _source;
        set => SetAndRaise(SourceProperty, ref _source, value);
    }

    /// <summary>
    /// 当前进度属性（秒）
    /// </summary>
    public static readonly DirectProperty<VideoFramePreview, double> PositionProperty =
        AvaloniaProperty.RegisterDirect<VideoFramePreview, double>(
            nameof(Position),
            o => o.Position,
            (o, v) => o.Position = v);

    private double _position;

    public double Position
    {
        get => _position;
        set => SetAndRaise(PositionProperty, ref _position, value);
    }

    /// <summary>
    /// 水印框属性
    /// </summary>
    public static readonly DirectProperty<VideoFramePreview, Rect?> WatermarkRectProperty =
        AvaloniaProperty.RegisterDirect<VideoFramePreview, Rect?>(
            nameof(WatermarkRect),
            o => o.WatermarkRect,
            (o, v) => o.WatermarkRect = v);

    private Rect? _watermarkRect;

    public Rect? WatermarkRect
    {
        get => _watermarkRect;
        set => SetAndRaise(WatermarkRectProperty, ref _watermarkRect, value);
    }

    private double _edgeThreshold;

    /// <summary>
    /// 边界阈值
    /// </summary>
    public double EdgeThreshold
    {
        get => _edgeThreshold;
        set => SetAndRaise(EdgeThresholdProperty, ref _edgeThreshold, value);
    }
    
    public static readonly DirectProperty<VideoFramePreview, double> EdgeThresholdProperty =
        AvaloniaProperty.RegisterDirect<VideoFramePreview, double>(
            nameof(WatermarkRect),
            o => o.EdgeThreshold,
            (o, v) => o.EdgeThreshold = v);

    /// <summary>
    /// 水印框颜色
    /// </summary>
    public static readonly StyledProperty<IBrush> WatermarkBrushProperty =
        AvaloniaProperty.Register<VideoFramePreview, IBrush>(
            nameof(WatermarkBrush),
            Brushes.Red);

    public IBrush WatermarkBrush
    {
        get => GetValue(WatermarkBrushProperty);
        set => SetValue(WatermarkBrushProperty, value);
    }

    // 水印框线宽
    public static readonly StyledProperty<double> WatermarkThicknessProperty =
        AvaloniaProperty.Register<VideoFramePreview, double>(
            nameof(WatermarkThickness),
            2.0);

    public double WatermarkThickness
    {
        get => GetValue(WatermarkThicknessProperty);
        set => SetValue(WatermarkThicknessProperty, value);
    }

    // 是否允许交互调整水印框
    public static readonly StyledProperty<bool> IsWatermarkInteractiveProperty =
        AvaloniaProperty.Register<VideoFramePreview, bool>(
            nameof(IsWatermarkInteractive),
            true);

    public bool IsWatermarkInteractive
    {
        get => GetValue(IsWatermarkInteractiveProperty);
        set => SetValue(IsWatermarkInteractiveProperty, value);
    }

    // 内部状态
    private Rect? _transformedWatermarkRect; // 转换到控件坐标的水印框
    private Point? _dragStartPoint;
    private Rect? _dragStartRect;
    private WatermarkHandle _activeHandle = WatermarkHandle.None;
    private const double HandleSize = 8;

    private enum WatermarkHandle
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top,
        Bottom,
        Left,
        Right,
        Center
    }

    static VideoFramePreview()
    {
        AffectsRender<VideoFramePreview>(
            SourceProperty,
            PositionProperty,
            WatermarkRectProperty,
            WatermarkBrushProperty,
            WatermarkThicknessProperty,
            IsWatermarkInteractiveProperty);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!IsWatermarkInteractive || !WatermarkRect.HasValue || _transformedWatermarkRect == null)
            return;

        var point = e.GetPosition(this);
        _activeHandle = GetHandleAtPoint(point);

        if (_activeHandle != WatermarkHandle.None)
        {
            _dragStartPoint = point;
            _dragStartRect = _transformedWatermarkRect;
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!IsWatermarkInteractive || !WatermarkRect.HasValue || _transformedWatermarkRect == null)
            return;

        var point = e.GetPosition(this);

        if (_dragStartPoint == null)
        {
            var handle = GetHandleAtPoint(point);
            Cursor = handle switch
            {
                WatermarkHandle.TopLeft => new Cursor(StandardCursorType.TopLeftCorner),
                WatermarkHandle.TopRight => new Cursor(StandardCursorType.TopRightCorner),
                WatermarkHandle.BottomLeft => new Cursor(StandardCursorType.BottomLeftCorner),
                WatermarkHandle.BottomRight => new Cursor(StandardCursorType.BottomRightCorner),
                WatermarkHandle.Top => new Cursor(StandardCursorType.TopSide),
                WatermarkHandle.Bottom => new Cursor(StandardCursorType.BottomSide),
                WatermarkHandle.Left => new Cursor(StandardCursorType.LeftSide),
                WatermarkHandle.Right => new Cursor(StandardCursorType.RightSide),
                WatermarkHandle.Center => new Cursor(StandardCursorType.SizeAll),
                _ => Cursor.Default
            };
        }
        // 处理拖动
        else if (_dragStartPoint.HasValue && _dragStartRect.HasValue)
        {
            var delta = point - _dragStartPoint.Value;
            var newRect = _dragStartRect.Value;

            var videoBounds = _imageDestRect;

            switch (_activeHandle)
            {
                case WatermarkHandle.TopLeft:
                    newRect = new Rect(
                        Math.Max(videoBounds.X + EdgeThreshold, _dragStartRect.Value.X + delta.X),
                        Math.Max(videoBounds.Y + EdgeThreshold, _dragStartRect.Value.Y + delta.Y),
                        Math.Min(_dragStartRect.Value.Width - delta.X, videoBounds.Right - newRect.X - EdgeThreshold),
                        Math.Min(_dragStartRect.Value.Height - delta.Y, videoBounds.Bottom - newRect.Y - EdgeThreshold));
                    break;
                case WatermarkHandle.TopRight:
                    newRect = new Rect(
                        _dragStartRect.Value.X,
                        Math.Max(videoBounds.Y, _dragStartRect.Value.Y + delta.Y),
                        Math.Min(_dragStartRect.Value.Width + delta.X, videoBounds.Right - newRect.X - EdgeThreshold),
                        Math.Min(_dragStartRect.Value.Height - delta.Y, videoBounds.Bottom - newRect.Y - EdgeThreshold));
                    break;
                case WatermarkHandle.BottomLeft:
                    newRect = new Rect(
                        Math.Max(videoBounds.X, _dragStartRect.Value.X + delta.X),
                        _dragStartRect.Value.Y,
                        Math.Min(_dragStartRect.Value.Width - delta.X, videoBounds.Right - newRect.X - EdgeThreshold),
                        Math.Min(_dragStartRect.Value.Height + delta.Y, videoBounds.Bottom - newRect.Y - EdgeThreshold));
                    break;
                case WatermarkHandle.BottomRight:
                    newRect = new Rect(
                        _dragStartRect.Value.X,
                        _dragStartRect.Value.Y,
                        Math.Min(_dragStartRect.Value.Width + delta.X, videoBounds.Right - newRect.X - EdgeThreshold),
                        Math.Min(_dragStartRect.Value.Height + delta.Y, videoBounds.Bottom - newRect.Y - EdgeThreshold));
                    break;
                case WatermarkHandle.Top:
                    newRect = new Rect(
                        _dragStartRect.Value.X,
                        Math.Max(videoBounds.Y, _dragStartRect.Value.Y + delta.Y),
                        _dragStartRect.Value.Width,
                        Math.Min(_dragStartRect.Value.Height - delta.Y, videoBounds.Bottom - newRect.Y - EdgeThreshold));
                    break;
                case WatermarkHandle.Bottom:
                    newRect = new Rect(
                        _dragStartRect.Value.X,
                        _dragStartRect.Value.Y,
                        _dragStartRect.Value.Width,
                        Math.Min(_dragStartRect.Value.Height + delta.Y, videoBounds.Bottom - newRect.Y - EdgeThreshold));
                    break;
                case WatermarkHandle.Left:
                    newRect = new Rect(
                        Math.Max(videoBounds.X, _dragStartRect.Value.X + delta.X),
                        _dragStartRect.Value.Y,
                        Math.Min(_dragStartRect.Value.Width - delta.X, videoBounds.Right - newRect.X - EdgeThreshold),
                        _dragStartRect.Value.Height);
                    break;
                case WatermarkHandle.Right:
                    newRect = new Rect(
                        _dragStartRect.Value.X,
                        _dragStartRect.Value.Y,
                        Math.Min(_dragStartRect.Value.Width + delta.X, videoBounds.Right - newRect.X - EdgeThreshold),
                        _dragStartRect.Value.Height);
                    break;
                case WatermarkHandle.Center:
                    var maxX = videoBounds.Right - newRect.Width - EdgeThreshold;
                    var maxY = videoBounds.Bottom - newRect.Height - EdgeThreshold;
                    newRect = new Rect(
                        Math.Max(videoBounds.X + EdgeThreshold, Math.Min(maxX, _dragStartRect.Value.X + delta.X)),
                        Math.Max(videoBounds.Y + EdgeThreshold, Math.Min(maxY, _dragStartRect.Value.Y + delta.Y)),
                        newRect.Width,
                        newRect.Height);
                    break;
            }

            if (newRect.Width < 10)
            {
                if (_activeHandle == WatermarkHandle.Left || _activeHandle == WatermarkHandle.TopLeft ||
                    _activeHandle == WatermarkHandle.BottomLeft)
                    newRect = new Rect(newRect.Right - 10, newRect.Y, 10, newRect.Height);
                else
                    newRect = new Rect(newRect.X, newRect.Y, 10, newRect.Height);
            }

            if (newRect.Height < 10)
            {
                if (_activeHandle == WatermarkHandle.Top || _activeHandle == WatermarkHandle.TopLeft ||
                    _activeHandle == WatermarkHandle.TopRight)
                    newRect = new Rect(newRect.X, newRect.Bottom - 10, newRect.Width, 10);
                else
                    newRect = new Rect(newRect.X, newRect.Y, newRect.Width, 10);
            }

            _transformedWatermarkRect = newRect;

            // 转换回视频坐标并更新WatermarkRect
            var scale = GetCurrentScale();
            if (scale > 0)
            {
                var videoRect = new Rect(
                    (_transformedWatermarkRect.Value.X - _imageDestRect.X) / scale,
                    (_transformedWatermarkRect.Value.Y - _imageDestRect.Y) / scale,
                    _transformedWatermarkRect.Value.Width / scale,
                    _transformedWatermarkRect.Value.Height / scale);

                WatermarkRect = videoRect;
            }

            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragStartPoint = null;
        _dragStartRect = null;
        _activeHandle = WatermarkHandle.None;
    }

    private Rect _imageDestRect;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        var tBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        // 绘制视频帧
        if (Source != null)
        {
            var sourceSize = new Size(Source.Size.Width, Source.Size.Height);
            var scale = CalculateScale(bounds.Size, sourceSize);
            var scaledSize = sourceSize * scale;
            _imageDestRect = new Rect(
                (bounds.Width - scaledSize.Width) / 2,
                (bounds.Height - scaledSize.Height) / 2,
                scaledSize.Width,
                scaledSize.Height);

            context.DrawImage(Source, new Rect(sourceSize), _imageDestRect);

            // 绘制水印框（如果有）
            if (WatermarkRect.HasValue)
            {
                // 将水印框从视频坐标空间转换到控件坐标空间
                var watermarkRect = WatermarkRect.Value;
                var scale2 = GetCurrentScale();
                _transformedWatermarkRect = new Rect(
                    _imageDestRect.X + watermarkRect.X * scale2,
                    _imageDestRect.Y + watermarkRect.Y * scale2,
                    watermarkRect.Width * scale2,
                    watermarkRect.Height * scale2);

                var pen = new Pen(WatermarkBrush, WatermarkThickness, dashStyle: DashStyle.Dash);
                context.DrawRectangle(null, pen, _transformedWatermarkRect.Value);

                // 绘制调整手柄（如果可交互）
                if (IsWatermarkInteractive)
                {
                    DrawResizeHandles(context, _transformedWatermarkRect.Value);
                }
            }
        }
    }

    private void DrawResizeHandles(DrawingContext context, Rect rect)
    {
        var handleBrush = WatermarkBrush;
        var handlePen = new Pen(Brushes.White, 1);

        // 四个角的手柄
        DrawHandle(context, new Rect(rect.X - HandleSize / 2, rect.Y - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
        DrawHandle(context, new Rect(rect.Right - HandleSize / 2, rect.Y - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
        DrawHandle(context, new Rect(rect.X - HandleSize / 2, rect.Bottom - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
        DrawHandle(context, new Rect(rect.Right - HandleSize / 2, rect.Bottom - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);

        // 四边中间的手柄
        DrawHandle(context,
            new Rect(rect.X + rect.Width / 2 - HandleSize / 2, rect.Y - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
        DrawHandle(context,
            new Rect(rect.X + rect.Width / 2 - HandleSize / 2, rect.Bottom - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
        DrawHandle(context,
            new Rect(rect.X - HandleSize / 2, rect.Y + rect.Height / 2 - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
        DrawHandle(context,
            new Rect(rect.Right - HandleSize / 2, rect.Y + rect.Height / 2 - HandleSize / 2, HandleSize, HandleSize),
            handleBrush, handlePen);
    }

    private void DrawHandle(DrawingContext context, Rect rect, IBrush brush, Pen pen)
    {
        var shrinkAmount = 2;
        var smallerRect = new Rect(
            rect.X + shrinkAmount, 
            rect.Y + shrinkAmount, 
            rect.Width - 2 * shrinkAmount, 
            rect.Height - 2 * shrinkAmount);
    
        context.DrawRectangle(brush, pen, smallerRect);
    }

    private WatermarkHandle GetHandleAtPoint(Point point)
    {
        if (_transformedWatermarkRect == null)
            return WatermarkHandle.None;

        var rect = _transformedWatermarkRect.Value;

        // 检查四个角
        if (new Rect(rect.X - HandleSize, rect.Y - HandleSize, HandleSize * 2, HandleSize * 2).Contains(point))
            return WatermarkHandle.TopLeft;
        if (new Rect(rect.Right - HandleSize, rect.Y - HandleSize, HandleSize * 2, HandleSize * 2).Contains(point))
            return WatermarkHandle.TopRight;
        if (new Rect(rect.X - HandleSize, rect.Bottom - HandleSize, HandleSize * 2, HandleSize * 2).Contains(point))
            return WatermarkHandle.BottomLeft;
        if (new Rect(rect.Right - HandleSize, rect.Bottom - HandleSize, HandleSize * 2, HandleSize * 2).Contains(point))
            return WatermarkHandle.BottomRight;

        // 检查四边中间
        if (new Rect(rect.X + rect.Width / 2 - HandleSize, rect.Y - HandleSize, HandleSize * 2, HandleSize * 2)
            .Contains(point))
            return WatermarkHandle.Top;
        if (new Rect(rect.X + rect.Width / 2 - HandleSize, rect.Bottom - HandleSize, HandleSize * 2, HandleSize * 2)
            .Contains(point))
            return WatermarkHandle.Bottom;
        if (new Rect(rect.X - HandleSize, rect.Y + rect.Height / 2 - HandleSize, HandleSize * 2, HandleSize * 2)
            .Contains(point))
            return WatermarkHandle.Left;
        if (new Rect(rect.Right - HandleSize, rect.Y + rect.Height / 2 - HandleSize, HandleSize * 2, HandleSize * 2)
            .Contains(point))
            return WatermarkHandle.Right;

        // 检查中心区域
        if (rect.Contains(point))
            return WatermarkHandle.Center;

        return WatermarkHandle.None;
    }

    private double GetCurrentScale()
    {
        if (Source == null || !WatermarkRect.HasValue)
            return 0;

        var sourceSize = new Size(Source.Size.Width, Source.Size.Height);
        var bounds = new Size(Bounds.Width, Bounds.Height);
        return CalculateScale(bounds, sourceSize);
    }

    private double CalculateScale(Size bounds, Size source)
    {
        if (source.Width <= 0 || source.Height <= 0)
            return 1;

        double scaleX = bounds.Width / source.Width;
        double scaleY = bounds.Height / source.Height;
        return Math.Min(scaleX, scaleY);
    }
}