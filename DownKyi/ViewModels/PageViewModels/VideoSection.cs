using System.Collections.Generic;
using Prism.Mvvm;

namespace DownKyi.ViewModels.PageViewModels;

public class VideoSection : BindableBase
{
    public long Id { get; set; }

    private string _title;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private List<VideoPage> _videoPages;

    public List<VideoPage> VideoPages
    {
        get => _videoPages;
        set => SetProperty(ref _videoPages, value);
    }
}