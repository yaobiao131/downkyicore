using System;
using System.Collections.Generic;
using Avalonia.Threading;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.Models;
using DownKyi.Core.BiliApi.Video;
using DownKyi.Core.BiliApi.Video.Models;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.BiliApi.Zone;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using DownKyi.ViewModels.PageViewModels;
using VideoPage = DownKyi.ViewModels.PageViewModels.VideoPage;

namespace DownKyi.Services;

public class VideoInfoService : IInfoService
{
    private readonly VideoView? _videoView;

    public VideoInfoService(string? input)
    {
        if (input == null)
        {
            return;
        }

        if (ParseEntrance.IsAvId(input) || ParseEntrance.IsAvUrl(input))
        {
            var avid = ParseEntrance.GetAvId(input);
            _videoView = VideoInfo.VideoViewInfo(null, avid);
        }

        if (ParseEntrance.IsBvId(input) || ParseEntrance.IsBvUrl(input))
        {
            var bvid = ParseEntrance.GetBvId(input);
            _videoView = VideoInfo.VideoViewInfo(bvid);
        }
    }

    /// <summary>
    /// 获取视频剧集
    /// </summary>
    /// <returns></returns>
    public List<VideoPage>? GetVideoPages()
    {
        if (_videoView?.Pages == null)
        {
            return null;
        }

        if (_videoView.Pages.Count == 0)
        {
            return null;
        }

        var videoPages = new List<VideoPage>();

        var order = 0;
        foreach (var page in _videoView.Pages)
        {
            order++;

            // 标题
            string name;
            if (_videoView.Pages.Count == 1)
            {
                name = _videoView.Title;
            }
            else
            {
                //name = page.part;
                if (page.Part == "")
                {
                    // 如果page.part为空字符串
                    name = $"{_videoView.Title}-P{order}";
                }
                else
                {
                    name = page.Part;
                }
            }

            var videoPage = new VideoPage
            {
                Avid = _videoView.Aid,
                Bvid = _videoView.Bvid,
                Cid = page.Cid,
                EpisodeId = -1,
                FirstFrame = page.FirstFrame,
                Order = order,
                Name = name,
                Duration = "N/A",
                Page = page.Page
            };

            // UP主信息
            videoPage.Owner = _videoView.Owner;
            if (videoPage.Owner == null)
            {
                videoPage.Owner = new VideoOwner
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
            var dateTime = startTime.AddSeconds(_videoView.Pubdate);
            videoPage.PublishTime = dateTime.ToString(timeFormat);

            videoPages.Add(videoPage);
        }

        return videoPages;
    }

    /// <summary>
    /// 获取视频章节与剧集
    /// </summary>
    /// <returns></returns>
    public List<VideoSection>? GetVideoSections(bool noUgc = false)
    {
        if (!(_videoView?.UgcSeason?.Sections?.Count > 0)) return null;
        var videoSections = new List<VideoSection>();

        // 不需要ugc内容
        if (noUgc)
        {
            videoSections.Add(CreateDefaultVideoSection());
            return videoSections;
        }

        var timeFormat = SettingsManager.GetInstance().GetFileNamePartTimeFormat();
        var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local);

        foreach (var section in _videoView.UgcSeason.Sections)
        {
            var pages = new List<VideoPage>();
            var order = 0;
            foreach (var episode in section.Episodes)
            {
                if (episode.Pages?.Count > 1)
                {
                    var videoSection = CreateVideoSectionFromEpisode(section, episode, startTime, timeFormat);
                    videoSections.Add(videoSection);
                }
                else
                {
                    pages.Add(GenerateVideoPage(episode, ++order, startTime, timeFormat));
                }
            }

            if (pages.Count <= 0) continue;
            {
                var videoSection = new VideoSection
                {
                    Id = section.Id,
                    Title = section.Title,
                    VideoPages = pages
                };
                videoSections.Add(videoSection);
            }
        }

        if (videoSections.Count > 0)
        {
            videoSections[0].IsSelected = true;
        }

        return videoSections;
    }

    private VideoSection CreateDefaultVideoSection()
    {
        return new VideoSection
        {
            Id = 0,
            Title = "default",
            IsSelected = true,
            VideoPages = GetVideoPages()
        };
    }

    private VideoSection CreateVideoSectionFromEpisode(UgcSection section, UgcEpisode episode, DateTime startTime, string timeFormat)
    {
        var videoSection = new VideoSection
        {
            Id = section.Id,
            Title = episode.Title,
            VideoPages = new List<VideoPage>()
        };
        var order = 1;
        foreach (var p in episode.Pages)
        {
            var dateTime = startTime.AddSeconds(episode.Arc.Ctime);
            videoSection.VideoPages.Add(new VideoPage
            {
                Avid = episode.Aid,
                Bvid = episode.Bvid,
                Cid = p.Cid,
                EpisodeId = -1,
                FirstFrame = episode.Arc.Pic,
                Order = order++,
                Name = p.Part,
                Duration = "N/A",
                Owner = _videoView.Owner,
                Page = p.Page,
                PublishTime = dateTime.ToString(timeFormat)
            });
        }

        return videoSection;
    }

    private VideoPage GenerateVideoPage(UgcEpisode episode, int order, DateTime startTime, string timeFormat)
    {
        var page = new VideoPage
        {
            Avid = episode.Aid,
            Bvid = episode.Bvid,
            Cid = episode.Cid,
            EpisodeId = -1,
            FirstFrame = episode.Arc.Pic,
            Order = order,
            Name = episode.Title,
            Duration = "N/A",
            Owner = _videoView?.Owner ?? new VideoOwner { Name = "", Face = "", Mid = -1 },
            Page = episode.Page.Page
        };
        var dateTime = startTime.AddSeconds(episode.Arc.Ctime);
        page.PublishTime = dateTime.ToString(timeFormat);
        return page;
    }

    /// <summary>
    /// 获取视频流的信息，从VideoPage返回
    /// </summary>
    /// <param name="page"></param>
    public void GetVideoStream(VideoPage page)
    {
        var playUrl = SettingsManager.GetInstance().GetVideoParseType() switch
        {
            0 => VideoStream.GetVideoPlayUrl(page.Avid, page.Bvid, page.Cid),
            1 => VideoStream.GetVideoPlayUrlWebPage(page.Avid, page.Bvid, page.Cid, page.Page),
            _ => null
        };

        Dispatcher.UIThread.Invoke(() => { Utils.VideoPageInfo(playUrl, page); });
    }

    /// <summary>
    /// 获取视频信息
    /// </summary>
    /// <returns></returns>
    public VideoInfoView? GetVideoView()
    {
        if (_videoView == null)
        {
            return null;
        }

        // 查询、保存封面
        var coverUrl = _videoView.Pic;

        // 分区
        var videoZone = string.Empty;
        var zoneList = VideoZone.Instance().GetZones();
        var zone = zoneList.Find(it => it.Id == _videoView.Tid);
        if (zone != null)
        {
            var zoneParent = zoneList.Find(it => it.Id == zone.ParentId);
            if (zoneParent != null)
            {
                videoZone = zoneParent.Name + ">" + zone.Name;
            }
            else
            {
                videoZone = zone.Name;
            }
        }
        else
        {
            videoZone = _videoView.Tname;
        }

        // 获取用户头像
        string upName;
        if (_videoView?.Owner != null)
        {
            upName = _videoView.Owner.Name;
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
            videoInfoView.Title = _videoView.Title;

            // 分区id
            videoInfoView.TypeId = _videoView.Tid;

            videoInfoView.VideoZone = videoZone;

            var startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
            var dateTime = startTime.AddSeconds(_videoView.Pubdate);
            videoInfoView.CreateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            videoInfoView.PlayNumber = Format.FormatNumber(_videoView.Stat.View);
            videoInfoView.DanmakuNumber = Format.FormatNumber(_videoView.Stat.Danmaku);
            videoInfoView.LikeNumber = Format.FormatNumber(_videoView.Stat.Like);
            videoInfoView.CoinNumber = Format.FormatNumber(_videoView.Stat.Coin);
            videoInfoView.FavoriteNumber = Format.FormatNumber(_videoView.Stat.Favorite);
            videoInfoView.ShareNumber = Format.FormatNumber(_videoView.Stat.Share);
            videoInfoView.ReplyNumber = Format.FormatNumber(_videoView.Stat.Reply);
            videoInfoView.Description = _videoView.Desc;
            videoInfoView.UpHeader = _videoView.Owner.Face ?? "";
            videoInfoView.UpperMid = _videoView.Owner.Mid;

            videoInfoView.UpName = upName;
        });

        return videoInfoView;
    }
}