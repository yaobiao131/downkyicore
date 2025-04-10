using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace DownKyi.CustomAction;

public class NumericInputBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
            AssociatedObject.TextChanging += OnTextChanging;
        }
        base.OnAttached();
    }
    
    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.TextInputEvent, OnTextInput);
            AssociatedObject.TextChanging -= OnTextChanging;
        }
        base.OnDetaching();
    }
    
    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || e.Text.All(char.IsControl)) 
            return;

        if (!char.IsDigit(e.Text[0]))
            e.Handled = true; 
    }

    private void OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        if (e.Source is TextBox t && 
            !string.IsNullOrEmpty(t.Text) && !t.Text.All(char.IsDigit))
        {
            t.Text = new string(t.Text.Where(char.IsDigit).ToArray());
            t.CaretIndex = t.Text?.Length ?? 0;
        }
    }
}