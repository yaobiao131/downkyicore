using System;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace DownKyi.CustomControl;

public class Loading : TemplatedControl
{
    private const string LargeState = ":large";
    private const string SmallState = ":small";

    private const string InactiveState = ":inactive";
    private const string ActiveState = ":active";

    private double _maxSideLength = 10;
    private double _ellipseDiameter = 10;
    private Thickness _ellipseOffset = new(2);

    static Loading()
    {
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing),
        //    new FrameworkPropertyMetadata(typeof(ProgressRing)));
    }

    public Loading()
    {
    }

    #region IsActive

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }


    public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<Loading, bool>(nameof(IsActive), defaultValue: true);

    private static void OnIsActiveChanged(AvaloniaObject obj, bool arg2)
    {
        ((Loading)obj).UpdateVisualStates();
    }

    public static readonly DirectProperty<Loading, double> MaxSideLengthProperty = AvaloniaProperty.RegisterDirect<Loading, double>(nameof(MaxSideLength), o => o.MaxSideLength);

    public double MaxSideLength
    {
        get => _maxSideLength;
        private set => SetAndRaise(MaxSideLengthProperty, ref _maxSideLength, value);
    }

    public static readonly DirectProperty<Loading, double> EllipseDiameterProperty =
        AvaloniaProperty.RegisterDirect<Loading, double>(nameof(EllipseDiameter), o => o.EllipseDiameter);

    public double EllipseDiameter
    {
        get => _ellipseDiameter;
        private set => SetAndRaise(EllipseDiameterProperty, ref _ellipseDiameter, value);
    }

    public static readonly DirectProperty<Loading, Thickness> EllipseOffsetProperty =
        AvaloniaProperty.RegisterDirect<Loading, Thickness>(nameof(EllipseOffset), o => o.EllipseOffset);

    public Thickness EllipseOffset
    {
        get => _ellipseOffset;
        private set => SetAndRaise(EllipseOffsetProperty, ref _ellipseOffset, value);
    }

    #endregion


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var maxSideLength = Math.Min(Width, Height);
        var ellipseDiameter = 0.1 * maxSideLength;
        if (maxSideLength <= 40)
        {
            ellipseDiameter += 1;
        }

        EllipseDiameter = ellipseDiameter;
        MaxSideLength = maxSideLength;
        EllipseOffset = new Thickness(0, maxSideLength / 2 - ellipseDiameter, 0, 0);
        UpdateVisualStates();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdateVisualStates();
        }
    }

    private void UpdateVisualStates()
    {
        PseudoClasses.Remove(ActiveState);
        PseudoClasses.Remove(InactiveState);
        PseudoClasses.Remove(SmallState);
        PseudoClasses.Remove(LargeState);
        PseudoClasses.Add(IsActive ? ActiveState : InactiveState);
        PseudoClasses.Add(_maxSideLength < 60 ? SmallState : LargeState);
    }
}