using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using DownKyi.Core.BiliApi.Bangumi;
using DownKyi.Core.BiliApi.Bangumi.Models;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.Models;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using DownKyi.Utils;
using DownKyi.ViewModels.PageViewModels;

namespace DownKyi.Services;

public class BangumiInfoService : IInfoService
{
    private readonly BangumiSeason? _bangumiSeason;

    public BangumiInfoService(string? input)
    {
        if (input == null)
        {
            return;
        }

        if (ParseEntrance.IsBangumiSeasonId(input) || ParseEntrance.IsBangumiSeasonUrl(input))
        {
            var seasonId = ParseEntrance.GetBangumiSeasonId(input);
            _bangumiSeason = BangumiInfo.BangumiSeasonInfo(seasonId);
        }

        if (ParseEntrance.IsBangumiEpisodeId(input) || ParseEntrance.IsBangumiEpisodeUrl(input))
        {
            var episodeId = ParseEntrance.GetBangumiEpisodeId(input);
            _bangumiSeason = BangumiInfo.BangumiSeasonInfo(-1, episodeId);
        }

        if (ParseEntrance.IsBangumiMediaId(input) || ParseEntrance.IsBangumiMediaUrl(input))
        {
            var mediaId = ParseEntrance.GetBangumiMediaId(input);
            var bangumiMedia = BangumiInfo.BangumiMediaInfo(mediaId);
            _bangumiSeason = BangumiInfo.BangumiSeasonInfo(bangumiMedia.SeasonId);
        }
    }

    /// <summary>
    /// 获取视频剧集
    /// </summary>
    /// <returns></returns>
    public List<VideoPage> GetVideoPages()
    {
        var pages = new List<VideoPage>();
        if (_bangumiSeason == null)
        {
            return pages;
        }

        if (_bangumiSeason.Episodes == null)
        {
            return pages;
        }

        if (_bangumiSeason.Episodes.Count == 0)
        {
            return pages;
        }

        var order = 0;
        foreach (var episode in _bangumiSeason.Episodes)
        {
            order++;

            // 标题
            string name;

            // 判断title是否为数字，如果是，则将share_copy作为name，否则将title作为name
            //if (int.TryParse(episode.Title, out int result))
            //{
            //    name = Regex.Replace(episode.ShareCopy, @"《.*?》", "");
            //    //name = episode.ShareCopy;
            //}
            //else
            //{
            //    if (episode.LongTitle != null && episode.LongTitle != "")
            //    {
            //        name = $"{episode.Title} {episode.LongTitle}";
            //    }
            //    else
            //    {
            //        name = episode.Title;
            //    }
            //}

            // 将share_copy作为name，删除《》中的标题
            name = Regex.Replace(episode.ShareCopy, @"^《.*?》", "");

            // 删除前后空白符
            name = name.Trim();

            var page = new VideoPage
            {
                Avid = episode.Aid,
                Bvid = episode.Bvid,
                Cid = episode.Cid,
                EpisodeId = -1,
                FirstFrame = episode.Cover,
                Order = order,
                Name = name,
                Duration = "N/A"
            };

            // UP主信息
            if (_bangumiSeason.UpInfo != null)
            {
                page.Owner = new VideoOwner
                {
                    Name = _bangumiSeason.UpInfo.Name,
                    Face = _bangumiSeason.UpInfo.Avatar,
                    Mid = _bangumiSeason.UpInfo.Mid,
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
            var dateTime = startTime.AddSeconds(episode.PubTime);
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
        if (_bangumiSeason == null)
        {
            return null;
        }

        var videoSections = new List<VideoSection>
        {
            new()
            {
                Id = _bangumiSeason.Positive.Id,
                Title = _bangumiSeason.Positive.Title,
                IsSelected = true,
                VideoPages = GetVideoPages()
            }
        };

        // 不需要其他季或花絮内容
        if (noUgc)
        {
            return videoSections;
        }

        if (_bangumiSeason.Section == null)
        {
            return null;
        }

        if (_bangumiSeason.Section.Count == 0)
        {
            return null;
        }

        foreach (var section in _bangumiSeason.Section)
        {
            var pages = new List<VideoPage>();
            var order = 0;
            foreach (var episode in section.Episodes)
            {
                order++;

                // 标题
                var name = episode.LongTitle != null && episode.LongTitle != "" ? $"{episode.Title} {episode.LongTitle}" : episode.Title;
                var page = new VideoPage
                {
                    Avid = episode.Aid,
                    Bvid = episode.Bvid,
                    Cid = episode.Cid,
                    EpisodeId = -1,
                    FirstFrame = episode.Cover,
                    Order = order,
                    Name = name,
                    Duration = "N/A"
                };

                // UP主信息
                if (_bangumiSeason.UpInfo != null)
                {
                    page.Owner = new VideoOwner
                    {
                        Name = _bangumiSeason.UpInfo.Name,
                        Face = _bangumiSeason.UpInfo.Avatar,
                        Mid = _bangumiSeason.UpInfo.Mid,
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
                var dateTime = startTime.AddSeconds(episode.PubTime);
                page.PublishTime = dateTime.ToString(timeFormat);

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

        return videoSections;
    }

    /// <summary>
    /// 获取视频流的信息，从VideoPage返回
    /// </summary>
    /// <param name="page"></param>
    public void GetVideoStream(VideoPage page)
    {
        var playUrl = VideoStream.GetBangumiPlayUrl(page.Avid, page.Bvid, page.Cid);
        Dispatcher.UIThread.Invoke(() => Utils.VideoPageInfo(playUrl, page));
    }

    /// <summary>
    /// 获取视频信息
    /// </summary>
    /// <returns></returns>
    public VideoInfoView? GetVideoView()
    {
        if (_bangumiSeason == null)
        {
            return null;
        }

        // 查询、保存封面
        // 将SeasonId保存到avid字段中
        // 每集封面的cid保存到cid字段，EpisodeId保存到bvid字段中
        var coverUrl = _bangumiSeason.Cover;

        // 获取用户头像
        string upName;
        if (_bangumiSeason.UpInfo != null)
        {
            upName = _bangumiSeason.UpInfo.Name;
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

            videoInfoView.Title = _bangumiSeason.Title;

            // 分区id
            videoInfoView.TypeId = BangumiType.TypeId[_bangumiSeason.Type];

            videoInfoView.VideoZone = DictionaryResource.GetString(BangumiType.Type[_bangumiSeason.Type]);

            videoInfoView.PlayNumber = Format.FormatNumber(_bangumiSeason.Stat.Views);
            videoInfoView.DanmakuNumber = Format.FormatNumber(_bangumiSeason.Stat.Danmakus);
            videoInfoView.LikeNumber = Format.FormatNumber(_bangumiSeason.Stat.Likes);
            videoInfoView.CoinNumber = Format.FormatNumber(_bangumiSeason.Stat.Coins);
            videoInfoView.FavoriteNumber = Format.FormatNumber(_bangumiSeason.Stat.Favorites);
            videoInfoView.ShareNumber = Format.FormatNumber(_bangumiSeason.Stat.Share);
            videoInfoView.ReplyNumber = Format.FormatNumber(_bangumiSeason.Stat.Reply);
            videoInfoView.Description = _bangumiSeason.Evaluate;

            videoInfoView.UpName = upName;
            videoInfoView.UpHeader = _bangumiSeason.UpInfo?.Avatar;
            videoInfoView.UpperMid = _bangumiSeason.UpInfo?.Mid ?? -1;
        });

        return videoInfoView;
    }
}