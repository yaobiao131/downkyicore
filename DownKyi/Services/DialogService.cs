using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace DownKyi.Services;

public class DialogService : Prism.Services.Dialogs.DialogService, IDialogService
{
    public DialogService(IContainerExtension containerExtension) : base(containerExtension)
    {
    }

    public Task ShowDialogAsync(string name, IDialogParameters parameters, Action<IDialogResult> callback = null,
        string windowName = null)
    {
        return ShowDialogInternal(name, parameters, callback, true, windowName);
    }

    private Task ShowDialogInternal(string name, IDialogParameters parameters, Action<IDialogResult>? callback,
        bool isModal, string windowName = null, Window parentWindow = null)
    {
        if (parameters == null)
            parameters = new DialogParameters();

        IDialogWindow dialogWindow = CreateDialogWindow(windowName);
        ConfigureDialogWindowEvents(dialogWindow, callback);
        ConfigureDialogWindowContent(name, dialogWindow, parameters);

        return ShowDialogWindow(dialogWindow, isModal, parentWindow);
    }

    protected virtual Task ShowDialogWindow(IDialogWindow dialogWindow, bool isModal, Window owner = null)
    {
        if (isModal &&
            Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime deskLifetime)
        {
            // Ref:
            //  - https://docs.avaloniaui.net/docs/controls/window#show-a-window-as-a-dialog
            //  - https://github.com/AvaloniaUI/Avalonia/discussions/7924
            // (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)

            if (owner != null)
                return dialogWindow.ShowDialog(owner);
            else
                return dialogWindow.ShowDialog(deskLifetime.MainWindow);
        }
        else
        {
            dialogWindow.Show();
        }

        return Task.CompletedTask;
    }
}