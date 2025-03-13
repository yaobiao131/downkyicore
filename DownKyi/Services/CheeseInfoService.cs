using System;
using System.Collections.Generic;
using Avalonia.Threading;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.Cheese;
using DownKyi.Core.BiliApi.Cheese.Models;
using DownKyi.Core.BiliApi.Models;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using DownKyi.Utils;
using DownKyi.ViewModels.PageViewModels;

namespace DownKyi.Services;

public class CheeseInfoService : IInfoService
{
    private readonly CheeseView? _cheeseView;

    public CheeseInfoService(string? input)
    {
        if (input == null)
        {
            return;
        }

        if (ParseEntrance.IsCheeseSeasonUrl(input))
        {
            var seasonId = ParseEntrance.GetCheeseSeasonId(input);
            _cheeseView = CheeseInfo.CheeseViewInfo(seasonId);
        }

        if (ParseEntrance.IsCheeseEpisodeUrl(input))
        {
            var episodeId = ParseEntrance.GetCheeseEpisodeId(input);
            _cheeseView = CheeseInfo.CheeseViewInfo(-1, episodeId);
        }
    }

    /// <summary>
    /// 获取视频剧集
    /// </summary>
    /// <returns></returns>
    public List<VideoPage> GetVideoPages()
    {
        var pages = new List<VideoPage>();
        if (_cheeseView == null)
        {
            return pages;
        }

        if (_cheeseView.Episodes == null)
        {
            return pages;
        }

        if (_cheeseView.Episodes.Count == 0)
        {
            return pages;
        }

        var order = 0;
        foreach (var episode in _cheeseView.Episodes)
        {
            order++;
            var name = episode.Title;

            var duration = Format.FormatDuration(episode.Duration - 1);

            var page = new VideoPage
            {
                Avid = episode.Aid,
                Bvid = null,
                Cid = episode.Cid,
                EpisodeId = episode.Id,
                FirstFrame = episode.Cover,
                Order = order,
                Name = name,
                Duration = "N/A"
            };

            // UP主信息
            if (_cheeseView.UpInfo != null)
            {
                page.Owner = new VideoOwner
                {
                    Name = _cheeseView.UpInfo.Name,
                    Face = _cheeseView.UpInfo.Avatar,
                    Mid = _cheeseView.UpInfo.Mid,
                };
            }
            else
            {
                page.Owner = new VideoOwner
                {
                    Name = "",
                    Face = "",
                    Mid = -1,
                };
            }

            // 文件命名中的时间格式
            var timeFormat = SettingsManager.GetInstance().GetFileNamePartTimeFormat();
            // 视频发布时间
            var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
            var dateTime = startTime.AddSeconds(episode.ReleaseDate);
            page.PublishTime = dateTime.ToString(timeFormat);

            pages.Add(page);
        }

        return pages;
    }

    /// <summary>
    /// 获取视频章节与剧集
    /// </summary>
    /// <returns></returns>
    public List<VideoSection>? GetVideoSections(bool noUgc = false)
    {
        return null;
    }

    /// <summary>
    /// 获取视频流的信息，从VideoPage返回
    /// </summary>
    /// <param name="page"></param>
    public void GetVideoStream(VideoPage page)
    {
        var playUrl = VideoStream.GetCheesePlayUrl(page.Avid, page.Bvid, page.Cid, page.EpisodeId);
        Dispatcher.UIThread.Invoke(() => { Utils.VideoPageInfo(playUrl, page); });
    }

    /// <summary>
    /// 获取视频信息
    /// </summary>
    /// <returns></returns>
    public VideoInfoView? GetVideoView()
    {
        if (_cheeseView == null)
        {
            return null;
        }

        // 查询、保存封面
        // 将SeasonId保存到avid字段中
        // 每集封面的cid保存到cid字段，EpisodeId保存到bvid字段中
        var coverUrl = _cheeseView.Cover;

        // 获取用户头像
        string upName;
        if (_cheeseView.UpInfo != null)
        {
            upName = _cheeseView.UpInfo.Name;
        }
        else
        {
            upName = "";
        }

        // 为videoInfoView赋值
        var videoInfoView = new VideoInfoView();
        App.PropertyChangeAsync(() =>
        {
            videoInfoView.CoverUrl = coverUrl;

            videoInfoView.Title = _cheeseView.Title;

            // 分区id
            // 课堂的type id B站没有定义，这里自定义为-10
            videoInfoView.TypeId = -10;

            videoInfoView.VideoZone = DictionaryResource.GetString("Cheese");
            videoInfoView.CreateTime = "";

            videoInfoView.PlayNumber = Format.FormatNumber(_cheeseView.Stat.Play);
            videoInfoView.DanmakuNumber = Format.FormatNumber(0);
            videoInfoView.LikeNumber = Format.FormatNumber(0);
            videoInfoView.CoinNumber = Format.FormatNumber(0);
            videoInfoView.FavoriteNumber = Format.FormatNumber(0);
            videoInfoView.ShareNumber = Format.FormatNumber(0);
            videoInfoView.ReplyNumber = Format.FormatNumber(0);
            videoInfoView.Description = _cheeseView.Subtitle;

            videoInfoView.UpName = upName;
            videoInfoView.UpHeader = _cheeseView.UpInfo.Avatar;
            videoInfoView.UpperMid = _cheeseView.UpInfo.Mid;
        });

        return videoInfoView;
    }
}