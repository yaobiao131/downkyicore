using System.Collections.ObjectModel;
using Avalonia.Controls.Documents;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading.Tasks;
using DownKyi.Commands;
using DownKyi.Core.Settings;
using DownKyi.Models;

namespace DownKyi.ViewModels.Dialogs
{
    public class NewVersionAvailableDialogViewModel : BaseDialogViewModel
    {
        public const string Tag = "NewVersionAvailable";

        private AsyncDelegateCommand? _allowCommand;

        private DelegateCommand? _skipCurrentVersionCommand;

        public DelegateCommand SkipCurrentVersionCommand => _skipCurrentVersionCommand ??= new DelegateCommand(ExecuteSkipCurrentVersionCommand);
        public AsyncDelegateCommand AllowCommand => _allowCommand ??= new AsyncDelegateCommand(ExecuteAllowCommand);

        private async Task ExecuteAllowCommand()
        {
            const ButtonResult result = ButtonResult.OK;
            await PlatformHelper.OpenUrl($"https://github.com/{App.RepoOwner}/{App.RepoName}/releases/tag/{TagName}");
            RaiseRequestClose(new DialogResult(result));
        }

        private void ExecuteSkipCurrentVersionCommand()
        {
            SettingsManager.GetInstance().SetSkipVersionOnLaunch(NewVersion);
            RaiseRequestClose(new DialogResult());
        }

        private string _tagName;

        public string TagName
        {
            get => _tagName;
            set => SetProperty(ref _tagName, value);
        }

        private string _markdownText;

        public string MarkdownText
        {
            get => _markdownText;
            set => SetProperty(ref _markdownText, value);
        }

        private bool _enableSkipVersionOnLaunch = false;


        private string _newVersion;

        private string NewVersion
        {
            get => _newVersion;
            set => SetProperty(ref _newVersion, value);
        }

        public bool EnableSkipVersionOnLaunch
        {
            get => _enableSkipVersionOnLaunch;
            set => SetProperty(ref _enableSkipVersionOnLaunch, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            var release = parameters.GetValue<GitHubRelease>("release");
            EnableSkipVersionOnLaunch = parameters.GetValue<bool>("enableSkipVersion");
            MarkdownText = release.Body;
            TagName = release.TagName;
            NewVersion = release.TagName.TrimStart('v');
        }
    }
}