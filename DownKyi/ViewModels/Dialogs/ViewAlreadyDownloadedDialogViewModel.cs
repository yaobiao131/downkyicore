using DownKyi.Images;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace DownKyi.ViewModels.Dialogs;

public class ViewAlreadyDownloadedDialogViewModel : BaseDialogViewModel
{
    public const string Tag = "AlreadyDownloadedAlert";

    #region 属性声明

    private VectorImage? _image;

    public VectorImage? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    private string? _message;

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    #endregion

    public ViewAlreadyDownloadedDialogViewModel()
    {
        Title = "提示";
        Image = SystemIcon.Instance().Warning;
    }

    #region 命令声明

    private DelegateCommand? _yesCommand;

    public DelegateCommand YesCommand => _yesCommand ??= new DelegateCommand(ExecuteYesCommand);

    private void ExecuteYesCommand()
    {
        RaiseRequestClose(new DialogResult(ButtonResult.OK));
    }

    // 关闭窗口事件
    private DelegateCommand? _closeCommand;
    public new DelegateCommand CloseCommand => _closeCommand ??= new DelegateCommand(ExecuteCloseCommand);

    /// <summary>
    /// 关闭窗口事件
    /// </summary>
    private void ExecuteCloseCommand()
    {
        RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
    }

    #endregion

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        Message = parameters.GetValue<string>("message");
    }
}