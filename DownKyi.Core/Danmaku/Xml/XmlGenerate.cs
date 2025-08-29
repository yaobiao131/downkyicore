using System.Globalization;
using System.Xml.Linq;
using DownKyi.Core.BiliApi.Danmaku;
using DownKyi.Core.BiliApi.Danmaku.Models;

namespace DownKyi.Core.Danmaku.Xml;

public class XmlGenerate
{
    public string GenerateFromDanmakuList(long avid, long cid, string savePath)
    {
        try
        {
            var biliDanmakus = DanmakuProtobuf
                .GetAllDanmakuProto(avid, cid);
            biliDanmakus.Sort((x, y) => x.Progress.CompareTo(y.Progress));
            var xmlDocument = CreateXmlDocument(avid, cid, biliDanmakus);
            SaveXmlFile(xmlDocument, savePath);
        }
        catch
        {
            /**/
        }
        return savePath;
    }
    
    private XDocument CreateXmlDocument(long avid, long cid, List<BiliDanmaku> danmakus)
    {
        return new XDocument(
            new XElement("i",
                new XElement("chatserver", "chat.bilibili.com"),
                new XElement("chatid", $"{avid}_{cid}"),
                new XElement("mission", "0"),
                new XElement("maxlimit", danmakus.Count.ToString()),
                new XElement("state", "0"),
                new XElement("real_name", "0"),
                new XElement("source", "k-v"),
                CreateDanmakuElements(danmakus)
            )
        );
    }


    private XElement[] CreateDanmakuElements(List<BiliDanmaku> danmakus)
    {
        return danmakus.Select(danmaku =>
        {
            float progressInSeconds = danmaku.Progress / 1000f;

            return new XElement("d",
                new XAttribute("p",
                    $"{progressInSeconds.ToString("F3", CultureInfo.InvariantCulture)}," + 
                    $"{danmaku.Mode}," +                 // 模式
                    $"{danmaku.Fontsize}," +             // 字体大小
                    $"{danmaku.Color}," +                // 颜色
                    $"{danmaku.Ctime}," +                // 发送时间
                    $"{danmaku.Weight}," +               // 权重
                    $"{danmaku.MidHash}," +              // 发送者UID的HASH
                    $"{danmaku.Pool}"                    // 弹幕池
                ),
                EscapeXmlContent(danmaku.Content)
            );
        }).ToArray();
    }
    
    private string EscapeXmlContent(string content)
    {
        return new XText(content).ToString();
    }


    private void SaveXmlFile(XDocument xmlDocument, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        xmlDocument.Save(filePath, SaveOptions.None);
    }
}

