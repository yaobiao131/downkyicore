using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;

namespace DownKyi.CustomAction;

public class GridSplitterExtensions
{
    public static readonly AttachedProperty<ResetGridSplitterBehavior> ResetGridBehaviorProperty =
        AvaloniaProperty.RegisterAttached<GridSplitterExtensions, GridSplitter, ResetGridSplitterBehavior>(
            "ResetGridBehavior", coerce: OnResetGridBehaviorChanged);

    public static ResetGridSplitterBehavior GetResetGridBehavior(GridSplitter obj)
    {
        return obj.GetValue(ResetGridBehaviorProperty);
    }

    public static void SetResetGridBehavior(GridSplitter obj, ResetGridSplitterBehavior value)
    {
        obj.SetValue(ResetGridBehaviorProperty, value);
    }

    private static ResetGridSplitterBehavior OnResetGridBehaviorChanged(AvaloniaObject obj, ResetGridSplitterBehavior behavior)
    {
        if (obj is GridSplitter gridSplitter)
        {
            var oldBehavior = GetResetGridBehavior(gridSplitter);
            
            if (oldBehavior != null)
            {
                var behaviors = Interaction.GetBehaviors(gridSplitter);
                behaviors.Remove(oldBehavior);
            }
            
            if (behavior != null)
            {
                var behaviors = Interaction.GetBehaviors(gridSplitter);
                behaviors.Add(behavior);
            }
        }

        return behavior;
    }
}