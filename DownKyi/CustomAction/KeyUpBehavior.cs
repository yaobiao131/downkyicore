using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace DownKyi.CustomAction;

public class KeyUpBehavior : Trigger<Control>
{
    private Key _key = Key.None;

    public static readonly StyledProperty<Key> KeyProperty = AvaloniaProperty.Register<KeyUpBehavior, Key>(nameof(Key));

    public Key Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }
    
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is null) return;
        AssociatedObject.KeyUp += AssociatedObject_OnClick;
        AssociatedObject.AddHandler(InputElement.KeyDownEvent, Button_OnKeyDown, RoutingStrategies.Tunnel);
        // AssociatedObject.AddHandler(InputElement.KeyUpEvent, Button_OnKeyUp, RoutingStrategies.Tunnel);
    }
    
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is null) return;
        AssociatedObject.KeyUp -= AssociatedObject_OnClick;
        AssociatedObject.RemoveHandler(InputElement.KeyDownEvent, Button_OnKeyDown);
        // AssociatedObject.RemoveHandler(InputElement.KeyUpEvent, Button_OnKeyUp);
    }

    private void AssociatedObject_OnClick(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject is null || Key != _key) return;
        _key = Key.None;
        Interaction.ExecuteActions(AssociatedObject, Actions, e);
    }

    private void Button_OnKeyDown(object? sender, KeyEventArgs e)
    {
        _key = e.Key;
    }
}