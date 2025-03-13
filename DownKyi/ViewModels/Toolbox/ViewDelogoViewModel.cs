using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DownKyi.Core.FFMpeg;
using DownKyi.Events;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;

namespace DownKyi.ViewModels.Toolbox;

public class ViewDelogoViewModel : ViewModelBase
{
    public const string Tag = "PageToolboxDelogo";

    // 是否正在执行去水印任务
    private bool _isDelogo = false;

    #region 页面属性申明

    private string? _videoPath;

    public string? VideoPath
    {
        get => _videoPath;
        set => SetProperty(ref _videoPath, value);
    }

    private int _logoWidth;

    public int LogoWidth
    {
        get => _logoWidth;
        set => SetProperty(ref _logoWidth, value);
    }

    private int _logoHeight;

    public int LogoHeight
    {
        get => _logoHeight;
        set => SetProperty(ref _logoHeight, value);
    }

    private int _logoX;

    public int LogoX
    {
        get => _logoX;
        set => SetProperty(ref _logoX, value);
    }

    private int _logoY;

    public int LogoY
    {
        get => _logoY;
        set => SetProperty(ref _logoY, value);
    }

    private string _status;

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    #endregion

    public ViewDelogoViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        VideoPath = string.Empty;

        LogoWidth = 0;
        LogoHeight = 0;
        LogoX = 0;
        LogoY = 0;

        #endregion
    }

    #region 命令申明

    // 选择视频事件
    private DelegateCommand? _selectVideoCommand;

    public DelegateCommand SelectVideoCommand => _selectVideoCommand ??= new DelegateCommand(ExecuteSelectVideoCommand);

    /// <summary>
    /// 选择视频事件
    /// </summary>
    private async void ExecuteSelectVideoCommand()
    {
        if (_isDelogo)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        VideoPath = await DialogUtils.SelectVideoFile();
    }

    // 去水印事件
    private DelegateCommand? _delogoCommand;

    public DelegateCommand DelogoCommand => _delogoCommand ??= new DelegateCommand(ExecuteDelogoCommand);

    /// <summary>
    /// 去水印事件
    /// </summary>
    private async void ExecuteDelogoCommand()
    {
        if (_isDelogo)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        if (VideoPath is null or "")
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipNoSeletedVideo"));
            return;
        }

        if (LogoWidth == -1)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipInputRightLogoWidth"));
            return;
        }

        if (LogoHeight == -1)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipInputRightLogoHeight"));
            return;
        }

        if (LogoX == -1)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipInputRightLogoX"));
            return;
        }

        if (LogoY == -1)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipInputRightLogoY"));
            return;
        }

        // 新文件名
        var newFileName = VideoPath.Insert(VideoPath.Length - 4, "_delogo");
        Status = string.Empty;

        await Task.Run(() =>
        {
            // 执行去水印程序
            _isDelogo = true;
            FFMpeg.Instance.Delogo(VideoPath, newFileName, LogoX, LogoY, LogoWidth, LogoHeight, output => { Status += output + "\n"; });
            _isDelogo = false;
        });
    }

    // Status改变事件
    private DelegateCommand<object>? _statusCommand;

    public DelegateCommand<object> StatusCommand => _statusCommand ??= new DelegateCommand<object>(ExecuteStatusCommand);

    /// <summary>
    /// Status改变事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteStatusCommand(object parameter)
    {
        if (parameter is not TextBox output)
        {
            return;
        }

        // TextBox滚动到底部
        output.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault()?.ScrollToEnd();
    }

    #endregion
}