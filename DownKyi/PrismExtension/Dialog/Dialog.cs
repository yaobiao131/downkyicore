using Avalonia;
using Avalonia.Styling;

namespace DownKyi.PrismExtension.Dialog;

public class Dialog : Prism.Services.Dialogs.Dialog
{
    public static readonly AvaloniaProperty ThemeProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, ControlTheme>("Theme", typeof(Dialog));

    /// <summary>
    /// Gets the value for the <see cref="ThemeProperty"/> attached property.
    /// </summary>
    /// <param name="obj">The target element.</param>
    /// <returns>The <see cref="ThemeProperty"/> attached to the <paramref name="obj"/> element.</returns>
    public static ControlTheme? GetTheme(AvaloniaObject obj)
    {
        return (ControlTheme?)obj.GetValue(ThemeProperty);
    }

    /// <summary>
    /// Sets the <see cref="ThemeProperty"/> attached property.
    /// </summary>
    /// <param name="obj">The target element.</param>
    /// <param name="value">The Style to attach.</param>
    public static void SetTheme(AvaloniaObject obj, ControlTheme value)
    {
        obj.SetValue(ThemeProperty, value);
    }
}