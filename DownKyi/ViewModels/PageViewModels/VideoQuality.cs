using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ImTools;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class VideoQuality : BindableBase, IEquatable<VideoQuality>
{
    private int quality;

    public int Quality
    {
        get => quality;
        set => SetProperty(ref quality, value);
    }

    private string qualityFormat;

    public string QualityFormat
    {
        get => qualityFormat;
        set => SetProperty(ref qualityFormat, value);
    }

    private List<string> videoCodecList;

    public List<string> VideoCodecList
    {
        get => videoCodecList;
        set => SetProperty(ref videoCodecList, value);
    }

    private string selectedVideoCodec;

    public string SelectedVideoCodec
    {
        get => selectedVideoCodec;
        set
        {
            if (value != null)
            {
                SetProperty(ref selectedVideoCodec, value);
            }
        }
    }

    public bool Equals(VideoQuality? other)
    {
        if (other is null) return false;
        return this.Quality == other.Quality;
    }

    public override bool Equals(object? obj) => Equals(obj as VideoQuality);

    public override int GetHashCode() => quality.GetHashCode();
}