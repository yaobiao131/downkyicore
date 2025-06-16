using Avalonia.Media;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.Zone;
using DownKyi.Models;
using DownKyi.Utils;
using Prism.Mvvm;

namespace DownKyi.ViewModels.DownloadManager
{
    public class DownloadBaseItem : BindableBase
    {
        // model数据
        private DownloadBase? _downloadBase;

        public DownloadBase? DownloadBase
        {
            get => _downloadBase;
            set
            {
                _downloadBase = value;

                if (value != null && DownloadBase?.ZoneId != null)
                {
                    ZoneImage = DictionaryResource.Get<DrawingImage>(VideoZoneIcon.Instance().GetZoneImageKey(DownloadBase.ZoneId));
                }
            }
        }

        // 视频分区image
        private DrawingImage? _zoneImage;

        public DrawingImage? ZoneImage
        {
            get => _zoneImage;
            set => SetProperty(ref _zoneImage, value);
        }

        // 视频序号
        public int Order
        {
            get => DownloadBase == null ? 0 : DownloadBase.Order;
            set
            {
                if (DownloadBase != null) DownloadBase.Order = value;
                RaisePropertyChanged();
            }
        }

        // 视频主标题
        public string MainTitle
        {
            get => DownloadBase == null ? "" : DownloadBase.MainTitle;
            set
            {
                if (DownloadBase != null) DownloadBase.MainTitle = value;
                RaisePropertyChanged();
            }
        }

        // 视频标题
        public string Name
        {
            get => DownloadBase == null ? "" : DownloadBase.Name;
            set
            {
                if (DownloadBase != null) DownloadBase.Name = value;
                RaisePropertyChanged();
            }
        }

        // 时长
        public string Duration
        {
            get => DownloadBase == null ? "" : DownloadBase.Duration;
            set
            {
                if (DownloadBase != null) DownloadBase.Duration = value;
                RaisePropertyChanged();
            }
        }

        // 视频编码名称，AVC、HEVC
        public string VideoCodecName
        {
            get => DownloadBase == null ? "" : DownloadBase.VideoCodecName;
            set
            {
                if (DownloadBase != null) DownloadBase.VideoCodecName = value;
                RaisePropertyChanged();
            }
        }

        // 视频画质
        public Quality Resolution
        {
            get => DownloadBase == null ? null : DownloadBase.Resolution;
            set
            {
                if (DownloadBase != null) DownloadBase.Resolution = value;
                RaisePropertyChanged();
            }
        }

        // 音频编码
        public Quality AudioCodec
        {
            get => DownloadBase == null ? null : DownloadBase.AudioCodec;
            set
            {
                if (DownloadBase != null) DownloadBase.AudioCodec = value;
                RaisePropertyChanged();
            }
        }

        // 文件大小
        public string? FileSize
        {
            get => DownloadBase == null ? "" : DownloadBase.FileSize;
            set
            {
                if (DownloadBase != null) DownloadBase.FileSize = value;
                RaisePropertyChanged();
            }
        }
    }
}