using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Media.Imaging;
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
        if (_videoView == null)
        {
            return null;
        }

        if (_videoView.Pages == null)
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
            string timeFormat = SettingsManager.GetInstance().GetFileNamePartTimeFormat();
            // 视频发布时间
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            DateTime dateTime = startTime.AddSeconds(_videoView.Pubdate);
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
        if (_videoView == null)
        {
            return null;
        }

        var videoSections = new List<VideoSection>();

        // 不需要ugc内容
        if (noUgc)
        {
            videoSections.Add(new VideoSection
            {
                Id = 0,
                Title = "default",
                IsSelected = true,
                VideoPages = GetVideoPages()
            });

            return videoSections;
        }

        if (_videoView.UgcSeason == null)
        {
            return null;
        }

        if (_videoView.UgcSeason.Sections == null)
        {
            return null;
        }

        if (_videoView.UgcSeason.Sections.Count == 0)
        {
            return null;
        }

        foreach (var section in _videoView.UgcSeason.Sections)
        {
            var pages = new List<VideoPage>();
            var order = 0;
            foreach (var episode in section.Episodes)
            {
                order++;
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
                    // UP主信息
                    Owner = _videoView.Owner,
                    Page = episode.Page.Page
                };

                if (page.Owner == null)
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
                var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
                var dateTime = startTime.AddSeconds(episode.Arc.Ctime);
                page.PublishTime = dateTime.ToString(timeFormat);
                // 这里的发布时间有问题，
                // 如果是合集，也会执行这里，
                // 但是发布时间是入口视频的，不是所有视频的
                // TODO 修复

                pages.Add(page);
            }

            var videoSection = new VideoSection
            {
                Id = section.Id,
                Title = section.Title,
                VideoPages = pages
            };
            videoSections.Add(videoSection);
        }

        videoSections[0].IsSelected = true;

        return videoSections;
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
        var storageCover = new StorageCover();
        var coverUrl = _videoView.Pic;
        var cover = storageCover.GetCover(_videoView.Aid, _videoView.Bvid, _videoView.Cid, coverUrl);

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
        string header;
        if (_videoView.Owner != null)
        {
            upName = _videoView.Owner.Name;
            var storageHeader = new StorageHeader();
            header = storageHeader.GetHeader(_videoView.Owner.Mid, _videoView.Owner.Name, _videoView.Owner.Face);
        }
        else
        {
            upName = "";
            header = null;
        }

        // 为videoInfoView赋值
        var videoInfoView = new VideoInfoView();
        App.PropertyChangeAsync(() =>
        {
            videoInfoView.CoverUrl = coverUrl;

            videoInfoView.Cover = cover == null ? null : new Bitmap(cover);
            videoInfoView.Title = _videoView.Title;

            // 分区id
            videoInfoView.TypeId = _videoView.Tid;

            videoInfoView.VideoZone = videoZone;

            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
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

            videoInfoView.UpName = upName;
            if (header != null)
            {
                var storageHeader = new StorageHeader();
                videoInfoView.UpHeader = storageHeader.GetHeaderThumbnail(header, 48, 48);

                videoInfoView.UpperMid = _videoView.Owner.Mid;
            }
            else
            {
                videoInfoView.UpHeader = null;
            }
        });

        return videoInfoView;
    }
}