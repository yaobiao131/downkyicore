using Avalonia.Controls.Documents;
using DownKyi.Models;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            ButtonResult result = ButtonResult.OK;
            RaiseRequestClose(new DialogResult(result));
        }

        public ObservableCollection<Run> Messages { get; set; } = new();
      

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Messages.AddRange(MarkdownUtil.ConvertMarkdownToRuns(parameters.GetValue<string>("body")));
        }
    }
}
