using Avalonia.Styling;
using DownKyi.Core.Settings;

namespace DownKyi.Utils;

public static class ThemeHelper
{
    public static void SetTheme(ThemeMode themeMode)
    {
        var themeVariant = themeMode switch
        {
            ThemeMode.Default => ThemeVariant.Default,
            ThemeMode.Dark => ThemeVariant.Dark,
            ThemeMode.Light => ThemeVariant.Light,
            _ => ThemeVariant.Dark
        };
        App.Current.RequestedThemeVariant = themeVariant;
    }
}