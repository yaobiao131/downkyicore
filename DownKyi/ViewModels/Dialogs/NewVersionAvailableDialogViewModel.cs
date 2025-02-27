using System.Collections.ObjectModel;
using Avalonia.Controls.Documents;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace DownKyi.ViewModels.Dialogs
{
    public class NewVersionAvailableDialogViewModel :  BaseDialogViewModel
    {
        public const string Tag = "NewVersionAvailable";

        private DelegateCommand? _allowCommand;
        public DelegateCommand AllowCommand => _allowCommand ??= new DelegateCommand(ExecuteAllowCommand);

        /// <summary>
        /// 确认事件
        /// </summary>
        private void ExecuteAllowCommand()
        {
            const ButtonResult result = ButtonResult.OK;
            RaiseRequestClose(new DialogResult(result));
        }

        public ObservableCollection<Run> Messages { get; set; } = new();
      

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Messages.AddRange(MarkdownUtil.ConvertMarkdownToRuns(parameters.GetValue<string>("body")));
        }
    }
}
