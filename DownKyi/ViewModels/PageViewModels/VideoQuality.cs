using System.Collections.Generic;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class VideoQuality : BindableBase
{
    private int _quality;

    public int Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    private string _qualityFormat;

    public string QualityFormat
    {
        get => _qualityFormat;
        set => SetProperty(ref _qualityFormat, value);
    }

    private List<string> _videoCodecList;

    public List<string> VideoCodecList
    {
        get => _videoCodecList;
        set => SetProperty(ref _videoCodecList, value);
    }

    private string _selectedVideoCodec;

    public string SelectedVideoCodec
    {
        get => _selectedVideoCodec;
        set
        {
            if (value != null)
            {
                SetProperty(ref _selectedVideoCodec, value);
            }
        }
    }
}