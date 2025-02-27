using System;
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

public class ViewExtractMediaViewModel : ViewModelBase
{
    public const string Tag = "PageToolboxExtractMedia";

    // 是否正在执行任务
    private bool _isExtracting;

    #region 页面属性申明

    private string _videoPathsStr;

    public string VideoPathsStr
    {
        get => _videoPathsStr;
        set => SetProperty(ref _videoPathsStr, value);
    }

    private string[] _videoPaths;

    public string[] VideoPaths
    {
        get => _videoPaths;
        set
        {
            _videoPaths = value;
            VideoPathsStr = string.Join(Environment.NewLine, value);
        }
    }

    private string _status;

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    #endregion

    public ViewExtractMediaViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        VideoPaths = Array.Empty<string>();

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
        if (_isExtracting)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        VideoPaths = await DialogUtils.SelectMultiVideoFile();
    }

    // 提取音频事件
    private DelegateCommand? _extractAudioCommand;

    public DelegateCommand ExtractAudioCommand => _extractAudioCommand ??= new DelegateCommand(ExecuteExtractAudioCommand);

    /// <summary>
    /// 提取音频事件
    /// </summary>
    private async void ExecuteExtractAudioCommand()
    {
        if (_isExtracting)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        if (VideoPaths.Length <= 0)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipNoSelectedVideo"));
            return;
        }

        Status = string.Empty;

        await Task.Run(() =>
        {
            _isExtracting = true;
            foreach (var item in VideoPaths)
            {
                // 音频文件名
                var audioFileName = item.Remove(item.Length - 4, 4) + ".aac";
                // 执行提取音频程序
                FFMpeg.Instance.ExtractAudio(item, audioFileName, output => { Status += output + "\n"; });
            }

            _isExtracting = false;
        });
    }

    // 提取视频事件
    private DelegateCommand? _extractVideoCommand;

    public DelegateCommand ExtractVideoCommand => _extractVideoCommand ??= new DelegateCommand(ExecuteExtractVideoCommand);

    /// <summary>
    /// 提取视频事件
    /// </summary>
    private async void ExecuteExtractVideoCommand()
    {
        if (_isExtracting)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }

        if (VideoPaths.Length <= 0)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipNoSeletedVideo"));
            return;
        }

        Status = string.Empty;

        await Task.Run(() =>
        {
            _isExtracting = true;
            foreach (var item in VideoPaths)
            {
                // 视频文件名
                var videoFileName = item.Remove(item.Length - 4, 4) + "_onlyVideo.mp4";
                // 执行提取视频程序
                FFMpeg.Instance.ExtractVideo(item, videoFileName, new Action<string>((output) => { Status += output + "\n"; }));
            }

            _isExtracting = false;
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