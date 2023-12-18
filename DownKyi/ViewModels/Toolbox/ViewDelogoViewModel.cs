using System.Threading.Tasks;
using Avalonia.Controls;
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
    private bool isDelogo = false;

    #region 页面属性申明

    private string videoPath;

    public string VideoPath
    {
        get { return videoPath; }
        set { SetProperty(ref videoPath, value); }
    }

    private int logoWidth;

    public int LogoWidth
    {
        get { return logoWidth; }
        set { SetProperty(ref logoWidth, value); }
    }

    private int logoHeight;

    public int LogoHeight
    {
        get { return logoHeight; }
        set { SetProperty(ref logoHeight, value); }
    }

    private int logoX;

    public int LogoX
    {
        get { return logoX; }
        set { SetProperty(ref logoX, value); }
    }

    private int logoY;

    public int LogoY
    {
        get { return logoY; }
        set { SetProperty(ref logoY, value); }
    }

    private string status;

    public string Status
    {
        get { return status; }
        set { SetProperty(ref status, value); }
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
    private DelegateCommand selectVideoCommand;

    public DelegateCommand SelectVideoCommand =>
        selectVideoCommand ?? (selectVideoCommand = new DelegateCommand(ExecuteSelectVideoCommand));

    /// <summary>
    /// 选择视频事件
    /// </summary>
    private async void ExecuteSelectVideoCommand()
    {
        if (isDelogo)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        VideoPath = await DialogUtils.SelectVideoFile();
    }

    // 去水印事件
    private DelegateCommand delogoCommand;

    public DelegateCommand DelogoCommand =>
        delogoCommand ?? (delogoCommand = new DelegateCommand(ExecuteDelogoCommand));

    /// <summary>
    /// 去水印事件
    /// </summary>
    private async void ExecuteDelogoCommand()
    {
        if (isDelogo)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        if (VideoPath == "")
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
        string newFileName = VideoPath.Insert(VideoPath.Length - 4, "_delogo");
        Status = string.Empty;

        await Task.Run(() =>
        {
            // 执行去水印程序
            isDelogo = true;
            FFMpeg.Instance.Delogo(VideoPath, newFileName, LogoX, LogoY, LogoWidth, LogoHeight, output =>
            {
                Status += output + "\n";
            });
            isDelogo = false;
        });
    }

    // Status改变事件
    private DelegateCommand<object> statusCommand;

    public DelegateCommand<object> StatusCommand =>
        statusCommand ?? (statusCommand = new DelegateCommand<object>(ExecuteStatusCommand));

    /// <summary>
    /// Status改变事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteStatusCommand(object parameter)
    {
        if (!(parameter is TextBox output))
        {
            return;
        }

        // TextBox滚动到底部
        // output.ScrollToEnd();
    }

    #endregion
}