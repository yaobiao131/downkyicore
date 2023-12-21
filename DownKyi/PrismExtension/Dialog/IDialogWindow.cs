using Avalonia.Styling;

namespace DownKyi.PrismExtension.Dialog;

public interface IDialogWindow: Prism.Services.Dialogs.IDialogWindow
{
    ControlTheme? Theme { get; set; }
}