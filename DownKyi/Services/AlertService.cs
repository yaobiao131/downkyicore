using System.Threading.Tasks;
using DownKyi.Images;
using DownKyi.Utils;
using DownKyi.ViewModels.Dialogs;
using Prism.Services.Dialogs;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.Services;

public class AlertService
{
    private readonly IDialogService? _dialogService;

    public AlertService(IDialogService? dialogService)
    {
        _dialogService = dialogService;
    }

    /// <summary>
    /// 显示一个信息弹窗
    /// </summary>
    /// <param name="message"></param>
    /// <param name="buttonNumber"></param>
    /// <returns></returns>
    public Task<ButtonResult> ShowInfo(string message, int buttonNumber = 2)
    {
        var image = SystemIcon.Instance().Info;
        var title = DictionaryResource.GetString("Info");
        return ShowMessage(image, title, message, buttonNumber);
    }

    /// <summary>
    /// 显示一个警告弹窗
    /// </summary>
    /// <param name="message"></param>
    /// <param name="buttonNumber"></param>
    /// <returns></returns>
    public Task<ButtonResult> ShowWarning(string message, int buttonNumber = 1)
    {
        var image = SystemIcon.Instance().Warning;
        var title = DictionaryResource.GetString("Warning");
        return ShowMessage(image, title, message, buttonNumber);
    }

    /// <summary>
    /// 显示一个错误弹窗
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<ButtonResult> ShowError(string message)
    {
        var image = SystemIcon.Instance().Error;
        var title = DictionaryResource.GetString("Error");
        return ShowMessage(image, title, message, 1);
    }

    public async Task<ButtonResult> ShowMessage(VectorImage image, string title, string message, int buttonNumber)
    {
        var result = ButtonResult.None;
        if (_dialogService == null)
        {
            return result;
        }

        var param = new DialogParameters
        {
            { "image", image },
            { "title", title },
            { "message", message },
            { "button_number", buttonNumber }
        };

        await _dialogService.ShowDialogAsync(ViewAlertDialogViewModel.Tag, param, buttonResult => { result = buttonResult.Result; });
        return result;
    }
}