using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DownKyi.PrismExtension.Common;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace DownKyi.PrismExtension.Dialog;

public class DialogService : Prism.Services.Dialogs.DialogService, IDialogService
{
    private readonly IContainerExtension _containerExtension;

    /// <summary>Initializes a new instance of the <see cref="DialogService"/> class.</summary>
    /// <param name="containerExtension">The <see cref="IContainerExtension" /></param>
    public DialogService(IContainerExtension containerExtension) : base(containerExtension)
    {
        _containerExtension = containerExtension;
    }

    public Task ShowDialogAsync(string name, IDialogParameters parameters, Action<IDialogResult>? callback = null,
        string? windowName = null)
    {
        return ShowDialogInternal(name, parameters, callback, true, windowName);
    }

    private Task ShowDialogInternal(string name, IDialogParameters parameters, Action<IDialogResult>? callback,
        bool isModal, string? windowName = null, Window? parentWindow = null)
    {
        if (parameters == null)
            parameters = new DialogParameters();

        IDialogWindow dialogWindow = CreateDialogWindow(windowName);
        ConfigureDialogWindowEvents(dialogWindow, callback);
        ConfigureDialogWindowContent(name, dialogWindow, parameters);

        return ShowDialogWindow(dialogWindow, isModal, parentWindow);
    }

    protected virtual IDialogWindow CreateDialogWindow(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return _containerExtension.Resolve<IDialogWindow>();
        else
            return _containerExtension.Resolve<IDialogWindow>(name);
    }

    protected new virtual Task ShowDialogWindow(IDialogWindow dialogWindow, bool isModal, Window? owner = null)
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

    /// <summary>
    /// Configure <see cref="IDialogWindow"/> content.
    /// </summary>
    /// <param name="dialogName">The name of the dialog to show.</param>
    /// <param name="window">The hosting window.</param>
    /// <param name="parameters">The parameters to pass to the dialog.</param>
    protected virtual void ConfigureDialogWindowContent(string dialogName, IDialogWindow window,
        IDialogParameters parameters)
    {
        var content = _containerExtension.Resolve<object>(dialogName);
        if (!(content is Control dialogContent))
            throw new NullReferenceException("A dialog's content must be a FrameworkElement");

        MvvmHelpers.AutowireViewModel(dialogContent);

        if (!(dialogContent.DataContext is IDialogAware viewModel))
            throw new NullReferenceException("A dialog's ViewModel must implement the IDialogAware interface");

        ConfigureDialogWindowProperties(window, dialogContent, viewModel);

        MvvmHelpers.ViewAndViewModelAction<IDialogAware>(viewModel, d => d.OnDialogOpened(parameters));
    }

    protected void ConfigureDialogWindowProperties(IDialogWindow window, Control dialogContent,
        IDialogAware viewModel)
    {
        // Avalonia returns 'null' for Dialog.GetWindowStyle(dialogContent);
        // WPF: Window > ContentControl > FrameworkElement
        // Ava: Window > WindowBase > TopLevel > Control > InputElement > Interactive > Layoutable > Visual > StyledElement.Styles (collection)

        var windowTheme = Dialog.GetTheme(dialogContent);
        if (windowTheme != null)
        {
            window.Theme = windowTheme;
        }
        
        window.Content = dialogContent;
        window.DataContext = viewModel;


        // WPF
        //// var windowStyle = Dialog.GetWindowStyle(dialogContent);
        //// if (windowStyle != null)
        ////     window.Style = windowStyle;
        ////
        //// window.Content = dialogContent;
        //// window.DataContext = viewModel; //we want the host window and the dialog to share the same data context
        ////
        //// if (window.Owner == null)
        //     window.Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
    }
}