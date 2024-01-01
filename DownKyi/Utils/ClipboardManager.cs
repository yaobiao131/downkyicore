using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace DownKyi.Utils;

public static class ClipboardManager
{
    private static readonly Window Window = App.Current.MainWindow;
    private static readonly IClipboard? Clipboard = Window.Clipboard;

    public static async Task SetText(string text)
    {
        if (Clipboard != null)
        {
            await Clipboard.SetTextAsync(text);
        }
    }
}