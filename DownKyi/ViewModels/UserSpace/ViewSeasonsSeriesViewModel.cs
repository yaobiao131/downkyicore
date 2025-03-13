using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.Users.Models;
using DownKyi.Core.Storage;
using DownKyi.Events;
using DownKyi.Images;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.UserSpace;

/// <summary>
/// 合集和列表
/// </summary>
public class ViewSeasonsSeriesViewModel : ViewModelBase
{
    public const string Tag = "PageUserSpaceSeasonsSeries";

    private long mid = -1;

    #region 页面属性申明

    private ObservableCollection<SeasonsSeries> _seasonsSeries;

    public ObservableCollection<SeasonsSeries> SeasonsSeries
    {
        get => _seasonsSeries;
        set => SetProperty(ref _seasonsSeries, value);
    }

    private int _selectedItem;

    public int SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    #endregion

    public ViewSeasonsSeriesViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        SeasonsSeries = new ObservableCollection<SeasonsSeries>();

        #endregion
    }

    #region 命令申明

    // 视频选择事件
    private DelegateCommand<object> _seasonsSeriesCommand;

    public DelegateCommand<object> SeasonsSeriesCommand => _seasonsSeriesCommand ??= new DelegateCommand<object>(ExecuteSeasonsSeriesCommand);

    /// <summary>
    /// 视频选择事件
    /// </summary>
    /// <param name="parameter"></param>
    private void ExecuteSeasonsSeriesCommand(object parameter)
    {
        if (parameter is not SeasonsSeries seasonsSeries)
        {
            return;
        }

        // 应该用枚举的，偷懒直接用数字
        var type = 0;
        if (seasonsSeries.TypeImage == NormalIcon.Instance().SeasonsSeries)
        {
            type = 1;
        }
        else if (seasonsSeries.TypeImage == NormalIcon.Instance().Channel1)
        {
            type = 2;
        }

        var data = new Dictionary<string, object>
        {
            { "mid", mid },
            { "id", seasonsSeries.Id },
            { "name", seasonsSeries.Name },
            { "count", seasonsSeries.Count },
            { "type", type }
        };

        // 进入视频页面
        var param = new NavigationParam
        {
            ViewName = ViewModels.ViewSeasonsSeriesViewModel.Tag,
            ParentViewName = ViewUserSpaceViewModel.Tag,
            Parameter = data
        };
        EventAggregator.GetEvent<NavigationEvent>().Publish(param);

        SelectedItem = -1;
    }

    #endregion

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        base.OnNavigatedFrom(navigationContext);

        SeasonsSeries.Clear();
        SelectedItem = -1;
    }

    /// <summary>
    /// 接收mid参数
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        SeasonsSeries.Clear();
        SelectedItem = -1;

        // 根据传入参数不同执行不同任务
        var parameter = navigationContext.Parameters.GetValue<SpaceSeasonsSeries>("object");
        if (parameter == null)
        {
            return;
        }

        // 传入mid
        mid = navigationContext.Parameters.GetValue<long>("mid");

        foreach (var item in parameter.SeasonsList)
        {
            if (item.Meta.Total <= 0)
            {
                continue;
            }

            string? image;
            if (item.Meta.Cover == null || item.Meta.Cover == "")
            {
                image = "avares://DownKyi/Resources/video-placeholder.png";
            }
            else
            {
                image = item.Meta.Cover;
            }

            // 当地时区
            var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            var dateCTime = startTime.AddSeconds(item.Meta.Ptime);
            var mtime = dateCTime.ToString("yyyy-MM-dd");

            SeasonsSeries.Add(new SeasonsSeries
            {
                Id = item.Meta.SeasonId,
                Cover = image,
                TypeImage = NormalIcon.Instance().SeasonsSeries,
                Name = item.Meta.Name,
                Count = item.Meta.Total,
                Ctime = mtime
            });
        }

        foreach (var item in parameter.SeriesList)
        {
            if (item.Meta.Total <= 0)
            {
                continue;
            }

            string? image;
            if (item.Meta.Cover == null || item.Meta.Cover == "")
            {
                image = "avares://DownKyi/Resources/video-placeholder.png";
            }
            else
            {
                image = item.Meta.Cover;
            }

            // 当地时区
            var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local);;
            var dateCTime = startTime.AddSeconds(item.Meta.Mtime);
            var mtime = dateCTime.ToString("yyyy-MM-dd");

            SeasonsSeries.Add(new SeasonsSeries
            {
                Id = item.Meta.SeriesId,
                Cover = image,
                TypeImage = NormalIcon.Instance().Channel1,
                Name = item.Meta.Name,
                Count = item.Meta.Total,
                Ctime = mtime
            });
        }
    }
}