using System.Text.RegularExpressions;

namespace DownKyi.Core.FileName;

public class FileName
{
    private readonly List<FileNamePart> _nameParts;
    private string _order = "ORDER";
    private string _section = "SECTION";
    private string _mainTitle = "MAIN_TITLE";
    private string _pageTitle = "PAGE_TITLE";
    private string _videoZone = "VIDEO_ZONE";
    private string _audioQuality = "AUDIO_QUALITY";
    private string _videoQuality = "VIDEO_QUALITY";
    private string _videoCodec = "VIDEO_CODEC";

    private string _videoPublishTime = "VIDEO_PUBLISH_TIME";

    private long _avid = -1;
    private string _bvid = "BVID";
    private long _cid = -1;

    private long _upMid = -1;
    private string _upName = "UP_NAME";

    private FileName(List<FileNamePart> nameParts)
    {
        this._nameParts = nameParts;
    }

    public static FileName Builder(List<FileNamePart> nameParts)
    {
        return new FileName(nameParts);
    }

    public FileName SetOrder(int order)
    {
        _order = order.ToString();
        return this;
    }

    public FileName SetOrder(int order, int count)
    {
        var length = Math.Abs(count).ToString().Length;
        _order = order.ToString("D" + length);

        return this;
    }

    public FileName SetSection(string section)
    {
        _section = section;
        return this;
    }

    public FileName SetMainTitle(string mainTitle)
    {
        _mainTitle = mainTitle;
        return this;
    }

    public FileName SetPageTitle(string pageTitle)
    {
        _pageTitle = pageTitle;
        return this;
    }

    public FileName SetVideoZone(string videoZone)
    {
        _videoZone = videoZone;
        return this;
    }

    public FileName SetAudioQuality(string audioQuality)
    {
        _audioQuality = audioQuality;
        return this;
    }

    public FileName SetVideoQuality(string videoQuality)
    {
        _videoQuality = videoQuality;
        return this;
    }

    public FileName SetVideoCodec(string videoCodec)
    {
        _videoCodec = videoCodec;
        return this;
    }

    public FileName SetVideoPublishTime(string videoPublishTime)
    {
        _videoPublishTime = videoPublishTime;
        return this;
    }

    public FileName SetAvid(long avid)
    {
        _avid = avid;
        return this;
    }

    public FileName SetBvid(string bvid)
    {
        _bvid = bvid;
        return this;
    }

    public FileName SetCid(long cid)
    {
        _cid = cid;
        return this;
    }

    public FileName SetUpMid(long upMid)
    {
        _upMid = upMid;
        return this;
    }

    public FileName SetUpName(string upName)
    {
        _upName = upName;
        return this;
    }

    public string RelativePath()
    {
        var path = string.Empty;

        foreach (var part in _nameParts)
        {
            switch (part)
            {
                case FileNamePart.Order:
                    path += _order;
                    break;
                case FileNamePart.Section:
                    path += _section;
                    break;
                case FileNamePart.MainTitle:
                    path += _mainTitle;
                    break;
                case FileNamePart.PageTitle:
                    path += _pageTitle;
                    break;
                case FileNamePart.VideoZone:
                    path += _videoZone;
                    break;
                case FileNamePart.AudioQuality:
                    path += _audioQuality;
                    break;
                case FileNamePart.VideoQuality:
                    path += _videoQuality;
                    break;
                case FileNamePart.VideoCodec:
                    path += _videoCodec;
                    break;
                case FileNamePart.VideoPublishTime:
                    path += _videoPublishTime;
                    break;
                case FileNamePart.Avid:
                    path += $"av{_avid}";
                    break;
                case FileNamePart.Bvid:
                    path += _bvid;
                    break;
                case FileNamePart.Cid:
                    path += _cid;
                    break;
                case FileNamePart.UpMid:
                    path += _upMid;
                    break;
                case FileNamePart.UpName:
                    path += _upName;
                    break;
            }

            if ((int)part >= 100)
            {
                path += HyphenSeparated.Hyphen[(int)part];
            }
        }

        // 避免连续多个斜杠
        path = Regex.Replace(path, @"//+", "/");
        // 避免以斜杠开头和结尾的情况
        return path.TrimEnd('/').TrimStart('/');
    }
}