using DownKyi.Core.Settings;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace DownKyi.ViewModels.Dialogs;

public class ViewParsingSelectorViewModel : BaseDialogViewModel
{
    public const string Tag = "DialogParsingSelector";

    #region 页面属性申明

    private bool _isParseDefault;

    public bool IsParseDefault
    {
        get => _isParseDefault;
        set => SetProperty(ref _isParseDefault, value);
    }

    #endregion

    public ViewParsingSelectorViewModel()
    {
        #region 属性初始化

        Title = DictionaryResource.GetString("ParsingSelector");

        // 解析范围
        var parseScope = SettingsManager.GetInstance().GetParseScope();
        IsParseDefault = parseScope != ParseScope.None;

        #endregion
    }

    #region 命令申明

    // 解析当前项事件
    private DelegateCommand? _parseSelectedItemCommand;

    public DelegateCommand ParseSelectedItemCommand => _parseSelectedItemCommand ??= new DelegateCommand(ExecuteParseSelectedItemCommand);

    /// <summary>
    /// 解析当前项事件
    /// </summary>
    private void ExecuteParseSelectedItemCommand()
    {
        SetParseScopeSetting(ParseScope.SelectedItem);

        IDialogParameters parameters = new DialogParameters
        {
            { "parseScope", ParseScope.SelectedItem }
        };

        RaiseRequestClose(new DialogResult(ButtonResult.OK, parameters));
    }

    // 解析当前页视频事件
    private DelegateCommand? _parseCurrentSectionCommand;

    public DelegateCommand ParseCurrentSectionCommand => _parseCurrentSectionCommand ??= new DelegateCommand(ExecuteParseCurrentSectionCommand);

    /// <summary>
    /// 解析当前页视频事件
    /// </summary>
    private void ExecuteParseCurrentSectionCommand()
    {
        SetParseScopeSetting(ParseScope.CurrentSection);

        IDialogParameters parameters = new DialogParameters
        {
            { "parseScope", ParseScope.CurrentSection }
        };

        RaiseRequestClose(new DialogResult(ButtonResult.OK, parameters));
    }

    // 解析所有视频事件
    private DelegateCommand? _parseAllCommand;

    public DelegateCommand ParseAllCommand => _parseAllCommand ??= new DelegateCommand(ExecuteParseAllCommand);

    /// <summary>
    /// 解析所有视频事件
    /// </summary>
    private void ExecuteParseAllCommand()
    {
        SetParseScopeSetting(ParseScope.All);

        IDialogParameters parameters = new DialogParameters
        {
            { "parseScope", ParseScope.All }
        };

        RaiseRequestClose(new DialogResult(ButtonResult.OK, parameters));
    }

    #endregion

    /// <summary>
    /// 写入设置
    /// </summary>
    /// <param name="parseScope"></param>
    private void SetParseScopeSetting(ParseScope parseScope)
    {
        SettingsManager.GetInstance().SetParseScope(IsParseDefault ? parseScope : ParseScope.None);
    }
}