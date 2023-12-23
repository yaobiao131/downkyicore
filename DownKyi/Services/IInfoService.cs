using System.Collections.Generic;
using DownKyi.ViewModels.PageViewModels;

namespace DownKyi.Services;

public interface IInfoService
{
    VideoInfoView? GetVideoView();

    List<VideoSection>? GetVideoSections(bool noUgc);

    List<VideoPage>? GetVideoPages();

    void GetVideoStream(VideoPage page);
}