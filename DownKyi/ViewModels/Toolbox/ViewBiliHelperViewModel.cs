using System;
using System.Threading.Tasks;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.Logging;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.ViewModels.Toolbox;

public class ViewBiliHelperViewModel : ViewModelBase
{
    public const string Tag = "PageToolboxBiliHelper";

    #region 页面属性申明

    private string _avid;

    public string Avid
    {
        get => _avid;
        set => SetProperty(ref _avid, value);
    }

    private string _bvid;

    public string Bvid
    {
        get => _bvid;
        set => SetProperty(ref _bvid, value);
    }

    private string _danmakuUserId;

    public string DanmakuUserId
    {
        get => _danmakuUserId;
        set => SetProperty(ref _danmakuUserId, value);
    }

    private string? _userMid;

    public string? UserMid
    {
        get => _userMid;
        set => SetProperty(ref _userMid, value);
    }

    #endregion

    public ViewBiliHelperViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        #endregion
    }

    #region 命令申明

    // 输入avid事件
    private DelegateCommand<string>? _avidCommand;

    public DelegateCommand<string> AvidCommand => _avidCommand ??= new DelegateCommand<string>(ExecuteAvidCommand);

    /// <summary>
    /// 输入avid事件
    /// </summary>
    private async void ExecuteAvidCommand(string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
        {
            return;
        }

        if (!ParseEntrance.IsAvId(parameter))
        {
            return;
        }

        var avid = ParseEntrance.GetAvId(parameter);
        if (avid == -1)
        {
            return;
        }

        await Task.Run(() => { Bvid = BvId.Av2Bv(avid); });
    }

    // 输入bvid事件
    private DelegateCommand<string>? _bvidCommand;

    public DelegateCommand<string> BvidCommand => _bvidCommand ??= new DelegateCommand<string>(ExecuteBvidCommand);

    /// <summary>
    /// 输入bvid事件
    /// </summary>
    /// <param name="parameter"></param>
    private async void ExecuteBvidCommand(string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
        {
            return;
        }

        if (!ParseEntrance.IsBvId(parameter))
        {
            return;
        }

        await Task.Run(() =>
        {
            var avid = BvId.Bv2Av(parameter);
            Avid = $"av{avid}";
        });
    }

    // 访问网页事件
    private DelegateCommand? _gotoWebCommand;

    public DelegateCommand GotoWebCommand => _gotoWebCommand ??= new DelegateCommand(ExecuteGotoWebCommand);

    /// <summary>
    /// 访问网页事件
    /// </summary>
    private void ExecuteGotoWebCommand()
    {
        var url = $"https://www.bilibili.com/video/{Bvid}";
        PlatformHelper.Open(url, EventAggregator);
    }

    // 查询弹幕发送者事件
    private DelegateCommand? _findDanmakuSenderCommand;

    public DelegateCommand FindDanmakuSenderCommand => _findDanmakuSenderCommand ??= new DelegateCommand(ExecuteFindDanmakuSenderCommand);

    /// <summary>
    /// 查询弹幕发送者事件
    /// </summary>
    private async void ExecuteFindDanmakuSenderCommand()
    {
        await Task.Run(() =>
        {
            try
            {
                UserMid = DanmakuSender.FindDanmakuSender(DanmakuUserId);
            }
            catch (Exception e)
            {
                UserMid = null;

                Console.PrintLine("FindDanmakuSenderCommand()发生异常: {0}", e);
                LogManager.Error(Tag, e);
            }
        });
    }

    // 访问用户空间事件
    private DelegateCommand? _visitUserSpaceCommand;

    public DelegateCommand VisitUserSpaceCommand => _visitUserSpaceCommand ??= new DelegateCommand(ExecuteVisitUserSpaceCommand);

    /// <summary>
    /// 访问用户空间事件
    /// </summary>
    private void ExecuteVisitUserSpaceCommand()
    {
        if (UserMid == null)
        {
            return;
        }

        var userSpace = $"https://space.bilibili.com/{UserMid}";
        PlatformHelper.Open(userSpace, EventAggregator);
    }

    #endregion
}