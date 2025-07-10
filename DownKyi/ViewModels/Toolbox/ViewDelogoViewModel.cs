using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using DownKyi.Commands;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.FFMpeg;
using DownKyi.Core.Storage;
using DownKyi.Events;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Path = System.IO.Path;


namespace DownKyi.ViewModels.Toolbox;

public class ViewDelogoViewModel : ViewModelBase
{
    public const string Tag = "PageToolboxDelogo";

    // 是否正在执行去水印任务
    private bool _isDelogo = false;

    private IImage _source;


    public IImage Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

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
        set
        {
            WatermarkArea = new Rect(_watermarkArea.X, _watermarkArea.Y, value, _watermarkArea.Height );
            SetProperty(ref _logoWidth, value);
        }
    }

    private int _logoHeight;

    public int LogoHeight
    {
        get => _logoHeight;
        set
        {
            WatermarkArea = new Rect(_watermarkArea.X, _watermarkArea.Y, _watermarkArea.Width,value );
            SetProperty(ref _logoHeight, value);
        }
    }

    private int _logoX;

    public int LogoX
    {
        get => _logoX;
        set
        {
            WatermarkArea = new Rect(value, _watermarkArea.Y, _watermarkArea.Width,_watermarkArea.Height );
            SetProperty(ref _logoX, value);
        }
    }

    private int _logoY;

    public int LogoY
    {
        get => _logoY;
        set
        {
            WatermarkArea = new Rect( _watermarkArea.X, value, _watermarkArea.Width,_watermarkArea.Height );
            SetProperty(ref _logoY, value);
        }
    }

    private string _status;

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    private bool _updatingWatermarkArea;
    
    private Rect _watermarkArea;

    public Rect WatermarkArea
    {
        get => _watermarkArea;
        set
        {
            if (_updatingWatermarkArea) return;
            _updatingWatermarkArea = true;
            LogoHeight = (int)Math.Round(value.Height);
            LogoWidth = (int)Math.Round(value.Width);
            LogoX = (int)Math.Round(value.X);
            LogoY = (int)Math.Round(value.Y);
            SetProperty(ref _watermarkArea, value);
            _updatingWatermarkArea = false;
        }
    }


    public List<SolidColorBrush> AvailableColors { get; }
    
    
    private SolidColorBrush _selectedColor;

    public SolidColorBrush SelectedColor
    {
        get => _selectedColor;
        set => SetProperty(ref _selectedColor, value);
    }

    #endregion

    public ViewDelogoViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        VideoPath = string.Empty;
        
        
        AvailableColors =  new (){
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Blue),
            new SolidColorBrush(Colors.White),
            new SolidColorBrush(Colors.Black),
            new SolidColorBrush(Colors.Gray),
            new SolidColorBrush(Colors.Fuchsia),
        };
        SelectedColor = AvailableColors[0];
        WatermarkArea = new Rect(20,20,100,100);
        #endregion
    }

    #region 命令申明

    // 选择视频事件
    private AsyncDelegateCommand? _selectVideoCommand;

    public AsyncDelegateCommand SelectVideoCommand => _selectVideoCommand ??= new AsyncDelegateCommand(ExecuteSelectVideoCommand);

    /// <summary>
    /// 选择视频事件
    /// </summary>
    private async Task ExecuteSelectVideoCommand(object obj, CancellationToken token)
    {
        if (_isDelogo)
        {
            EventAggregator.GetEvent<MessageEvent>().Publish(DictionaryResource.GetString("TipWaitTaskFinished"));
            return;
        }
        VideoPath = await DialogUtils.SelectVideoFile();
        if (!string.IsNullOrEmpty(VideoPath))
        {
            try
            {
                Source = new Bitmap(await FFMpeg.Instance.ExtractVideoFrame(VideoPath, TimeSpan.FromSeconds(1)));
            }
            catch (Exception e)
            {
                /**/
            }
        }
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
            FFMpeg.Instance.Delogo
            (
                VideoPath, 
                newFileName,
                _logoX,
                _logoY, 
                _logoWidth,
                _logoHeight, 
                output => { Status += output + "\n"; });
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
        
        var scrollViewer = output.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();

        scrollViewer?.ScrollToEnd();
    }

    #endregion
}