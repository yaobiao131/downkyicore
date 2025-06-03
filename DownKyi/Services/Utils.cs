﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.VideoStream.Models;
using DownKyi.Core.Settings;
using DownKyi.Core.Settings.Models;
using DownKyi.Core.Utils;
using DownKyi.ViewModels.PageViewModels;

namespace DownKyi.Services;

internal static class Utils
{
    /// <summary>
    /// 从视频流更新VideoPage
    /// </summary>
    /// <param name="playUrl"></param>
    /// <param name="page"></param>
    internal static void VideoPageInfo(PlayUrl? playUrl, VideoPage page)
    {
        if (playUrl == null)
        {
            return;
        }

        // 视频流信息
        page.PlayUrl = playUrl;

        // 获取设置
        var userInfo = SettingsManager.GetInstance().GetUserInfo();
        var defaultQuality = SettingsManager.GetInstance().GetQuality();
        var videoCodecs = SettingsManager.GetInstance().GetVideoCodecs();
        var defaultAudioQuality = SettingsManager.GetInstance().GetAudioQuality();

        // 未登录时，最高仅720P
        if (userInfo.Mid == -1)
        {
            if (defaultQuality > 64)
            {
                defaultQuality = 64;
            }
        }

        // 非大会员账户登录时，如果设置的画质高于1080P，则这里限制为1080P
        if (!userInfo.IsVip)
        {
            if (defaultQuality > 80)
            {
                defaultQuality = 80;
            }
        }

        if (playUrl.Dash != null)
        {
            // 如果video列表或者audio列表没有内容，则返回false
            if (playUrl.Dash.Video == null)
            {
                return;
            }

            if (playUrl.Dash.Video.Count == 0)
            {
                return;
            }

            // 音质
            page.AudioQualityFormatList = GetAudioQualityFormatList(playUrl, defaultAudioQuality);
            if (page.AudioQualityFormatList.Count > 0)
            {
                page.AudioQualityFormat = page.AudioQualityFormatList[0];
            }

            // 画质 & 视频编码
            page.VideoQualityList = GetVideoQualityList(playUrl, userInfo, defaultQuality, videoCodecs);
            if (page.VideoQualityList.Count > 0)
            {
                page.VideoQuality = page.VideoQualityList[0];
            }

            // 时长
            page.Duration = Format.FormatDuration(playUrl.Dash.Duration);

            return;
        }


        if (playUrl.Durl?.Count > 0)
        {
            var codeIds = Constant.GetCodecIds();
            var qns = Constant.GetResolutions();
            var quality = new VideoQuality
            {
                Quality = playUrl.Quality,
                QualityFormat = qns.First(x => x.Id == playUrl.Quality).Name,
                VideoCodecList = new(codeIds.Where(x => x.Id == playUrl.VideoCodecid)
                .Select(x => x.Name).ToList()),
                SelectedVideoCodec = codeIds.First(x => x.Id == playUrl.VideoCodecid).Name
            };

            page.VideoQualityList = new List<VideoQuality> { quality };
            page.VideoQuality = page.VideoQualityList[0];
            page.Duration = Format.FormatDuration(playUrl.Durl.Select(x => x.Length).Sum() / 1000);
            return;
        }
    }

    /// <summary>
    /// 设置音质
    /// </summary>
    /// <param name="playUrl"></param>
    /// <param name="defaultAudioQuality"></param>
    /// <returns></returns>
    private static ObservableCollection<string> GetAudioQualityFormatList(PlayUrl playUrl, int defaultAudioQuality)
    {
        var audioQualityFormatList = new List<string>();
        var sortList = new List<string>();
        var audioQualities = Constant.GetAudioQualities();

        if (playUrl.Dash.Audio != null && playUrl.Dash.Audio.Count > 0)
        {
            foreach (var audio in playUrl.Dash.Audio)
            {
                // 音质id大于设置音质时，跳过
                if (audio.Id > defaultAudioQuality)
                {
                    continue;
                }

                var audioQuality = audioQualities.FirstOrDefault(t => { return t.Id == audio.Id; });
                if (audioQuality != null)
                {
                    ListHelper.AddUnique(audioQualityFormatList, audioQuality.Name);
                }
            }
        }

        if (audioQualities[3].Id <= defaultAudioQuality - 1000 && playUrl.Dash.Dolby != null)
        {
            if (playUrl.Dash.Dolby.Audio != null && playUrl.Dash.Dolby.Audio.Count > 0)
            {
                ListHelper.AddUnique(audioQualityFormatList, audioQualities[3].Name);
            }
        }

        if (audioQualities[4].Id <= defaultAudioQuality - 1000 && playUrl.Dash.Flac != null)
        {
            if (playUrl.Dash.Flac.Audio != null)
            {
                ListHelper.AddUnique(audioQualityFormatList, audioQualities[4].Name);
            }
        }

        //audioQualityFormatList.Sort(new StringLogicalComparer<string>());
        //audioQualityFormatList.Reverse();

        foreach (var item in audioQualities)
        {
            if (audioQualityFormatList.Contains(item.Name))
            {
                sortList.Add(item.Name);
            }
        }

        sortList.Reverse();

        return new ObservableCollection<string>(sortList);
    }

    /// <summary>
    /// 设置画质 & 视频编码
    /// </summary>
    /// <param name="playUrl"></param>
    /// <param name="defaultQuality"></param>
    /// <param name="userInfo"></param>
    /// <param name="videoCodecs"></param>
    /// <returns></returns>
    private static List<VideoQuality> GetVideoQualityList(PlayUrl playUrl, UserInfoSettings userInfo, int defaultQuality, int videoCodecs)
    {
        var videoQualityList = new List<VideoQuality>();
        var codeIds = Constant.GetCodecIds();

        if (playUrl.Dash.Video == null)
        {
            return videoQualityList;
        }

        foreach (var video in playUrl.Dash.Video)
        {
            // 画质id大于设置画质时，跳过
            if (video.Id > defaultQuality)
            {
                continue;
            }

            // 非大会员账户登录时
            if (!userInfo.IsVip)
            {
                // 如果画质为720P60，跳过
                if (video.Id == 74)
                {
                    continue;
                }
            }

            var qualityFormat = string.Empty;
            var selectedQuality = playUrl.SupportFormats.FirstOrDefault(t => t.Quality == video.Id);
            if (selectedQuality != null)
            {
                qualityFormat = selectedQuality.NewDescription;
            }

            // 寻找是否已存在这个画质
            // 不存在则添加，存在则修改
            //string codecName = GetVideoCodecName(video.Codecs);
            var codecName = codeIds.FirstOrDefault(t => t.Id == video.CodecId).Name;
            var videoQualityExist = videoQualityList.FirstOrDefault(t => t.Quality == video.Id);
            if (videoQualityExist == null)
            {
                var videoCodecList = new List<string>();
                if (codecName != string.Empty)
                {
                    ListHelper.AddUnique(videoCodecList, codecName);
                }

                var videoQuality = new VideoQuality
                {
                    Quality = video.Id,
                    QualityFormat = qualityFormat,
                    VideoCodecList = videoCodecList
                };
                videoQualityList.Add(videoQuality);
            }
            else
            {
                if (!videoQualityList[videoQualityList.IndexOf(videoQualityExist)].VideoCodecList
                        .Exists(t => t.Equals(codecName)))
                {
                    if (codecName != string.Empty)
                    {
                        videoQualityList[videoQualityList.IndexOf(videoQualityExist)].VideoCodecList.Add(codecName);
                    }
                }
            }

            // 设置选中的视频编码
            var selectedVideoQuality = videoQualityList.FirstOrDefault(t => t.Quality == video.Id);
            if (selectedVideoQuality == null)
            {
                continue;
            }

            // 设置选中的视频编码
            var videoCodecsName = codeIds.FirstOrDefault(t => t.Id == videoCodecs).Name;
            if (videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].VideoCodecList.Contains(videoCodecsName))
            {
                videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].SelectedVideoCodec = videoCodecsName;
            }
            else
            {
                // 当获取的视频没有设置的视频编码时
                foreach (var codec in codeIds)
                {
                    if (videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].VideoCodecList
                        .Contains(codec.Name))
                    {
                        videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].SelectedVideoCodec =
                            codec.Name;
                    }

                    if (codec.Id == videoCodecs)
                    {
                        break;
                    }
                }

                // 若默认编码为AVC，但画质为杜比视界时，
                // 上面的foreach不会选中HEVC编码，
                // 而杜比视界只有HEVC编码，
                // 因此这里再判断并设置一次
                if (videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].SelectedVideoCodec == null &&
                    videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].VideoCodecList.Count() > 0)
                {
                    videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].SelectedVideoCodec =
                        videoQualityList[videoQualityList.IndexOf(selectedVideoQuality)].VideoCodecList[0];
                }
            }
        }

        return videoQualityList;
    }
}