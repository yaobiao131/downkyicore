using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.BiliApi.Zone;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.UserSpace;

/// <summary>
/// 投稿页面
/// </summary>
public class ViewArchiveViewModel : ViewModelBase
{
    public const string Tag = "PageUserSpaceArchive";

    private long _mid = -1;

    #region 页面属性申明

    private ObservableCollection<PublicationZone> _publicationZones;

    public ObservableCollection<PublicationZone> PublicationZones
    {
        get => _publicationZones;
        set => SetProperty(ref _publicationZones, value);
    }

    private int _selectedItem;

    public int SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    #endregion

    public ViewArchiveViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        PublicationZones = new ObservableCollection<PublicationZone>();

        #endregion
    }

    #region 命令申明

    // 视频选择事件
    private DelegateCommand<object>? _publicationZonesCommand;

    public DelegateCommand<object> PublicationZonesCommand => _publicationZonesCommand ??= new DelegateCommand<object>(ExecutePublicationZonesCommand);

    /// <summary>
    /// 视频选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecutePublicationZonesCommand(object parameter)
    {
        if (parameter is not PublicationZone zone)
        {
            return;
        }

        var data = new Dictionary<string, object>
        {
            { "mid", _mid },
            { "tid", zone.Tid },
            { "list", PublicationZones.ToList() }
        };

        // 进入视频页面
        NavigateToView.NavigationView(EventAggregator, ViewPublicationViewModel.Tag, ViewUserSpaceViewModel.Tag, data);

        SelectedItem = -1;
    }

    #endregion

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        base.OnNavigatedFrom(navigationContext);

        PublicationZones.Clear();
        SelectedItem = -1;
    }

    /// <summary>
    /// 接收mid参数
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        PublicationZones.Clear();
        SelectedItem = -1;

        // 根据传入参数不同执行不同任务
        var parameter = navigationContext.Parameters.GetValue<List<SpacePublicationListTypeVideoZone>>("object");
        if (parameter == null)
        {
            return;
        }

        // 传入mid
        _mid = navigationContext.Parameters.GetValue<long>("mid");

        var videoCount = 0;
        foreach (var zone in parameter)
        {
            videoCount += zone.Count;
            var iconKey = VideoZoneIcon.Instance().GetZoneImageKey(zone.Tid);

            _publicationZones.Add(new PublicationZone
            {
                Tid = zone.Tid,
                Icon = DictionaryResource.Get<DrawingImage>(iconKey),
                Name = zone.Name,
                Count = zone.Count
            });
        }

        // 全部
        _publicationZones.Insert(0, new PublicationZone
        {
            Tid = 0,
            Icon = DictionaryResource.Get<DrawingImage>("videoUpDrawingImage"),
            Name = DictionaryResource.GetString("AllPublicationZones"),
            Count = videoCount
        });
    }
}