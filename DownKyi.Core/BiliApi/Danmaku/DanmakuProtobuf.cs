using Bilibili.Community.Service.Dm.V1;
using DownKyi.Core.BiliApi.Danmaku.Models;
using DownKyi.Core.Storage;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Danmaku;

public static class DanmakuProtobuf
{
    /// <summary>
    /// 下载6分钟内的弹幕，返回弹幕列表
    /// </summary>
    /// <param name="avid">稿件avID</param>
    /// <param name="cid">视频CID</param>
    /// <param name="segmentIndex">分包，每6分钟一包</param>
    /// <returns></returns>
    private static List<BiliDanmaku>? GetDanmakuProto(long avid, long cid, int segmentIndex)
    {
        var url = $"https://api.bilibili.com/x/v2/dm/web/seg.so?type=1&oid={cid}&pid={avid}&segment_index={segmentIndex}";
        const string referer = "https://www.bilibili.com";
        
        var danmakuList = new List<BiliDanmaku>();
        try
        {
            using var input =  WebClient.RequestStream(url,referer);
            var danmakus = DmSegMobileReply.Parser.ParseFrom(input);
            if (danmakus?.Elems == null)
            {
                return danmakuList;
            }

            danmakuList.AddRange(danmakus.Elems.Select(dm => new BiliDanmaku
            {
                Id = dm.Id,
                Progress = dm.Progress,
                Mode = dm.Mode,
                Fontsize = dm.Fontsize,
                Color = dm.Color,
                MidHash = dm.MidHash,
                Content = dm.Content,
                Ctime = dm.Ctime,
                Weight = dm.Weight,
                //Action = dm.Action,
                Pool = dm.Pool
            }));
        }
        catch (Exception e)
        {
            Console.PrintLine("GetDanmakuProto()发生异常: {0}", e);
            //Logging.LogManager.Error(e);
            return null;
        }

        return danmakuList;
    }

    /// <summary>
    /// 下载所有弹幕，返回弹幕列表
    /// </summary>
    /// <param name="avid">稿件avID</param>
    /// <param name="cid">视频CID</param>
    /// <returns></returns>
    public static List<BiliDanmaku> GetAllDanmakuProto(long avid, long cid)
    {
        var danmakuList = new List<BiliDanmaku>();

        var segmentIndex = 0;
        while (true)
        {
            segmentIndex += 1;
            var danmakus = GetDanmakuProto(avid, cid, segmentIndex);
            if (danmakus == null)
            {
                break;
            }

            danmakuList.AddRange(danmakus);
        }

        return danmakuList;
    }
}