using System;
using System.Threading.Tasks;
using Prism.Services.Dialogs;

namespace DownKyi.Services;

public interface IDialogService : Prism.Services.Dialogs.IDialogService
{
    public Task ShowDialogAsync(string name, IDialogParameters parameters, Action<IDialogResult>? callback = null,
        string? windowName = null);
}