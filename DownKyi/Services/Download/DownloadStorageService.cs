using System.Collections.Generic;
using System.Linq;
using DownKyi.Models;
using DownKyi.ViewModels.DownloadManager;
using FreeSql;

namespace DownKyi.Services.Download;

public class DownloadStorageService
{
    private readonly IBaseRepository<Downloading> _downloadingRepository;
    private readonly IBaseRepository<Downloaded> _downloadedRepository;


    public DownloadStorageService(IBaseRepository<Downloading> downloadingRepository, IBaseRepository<Downloaded> downloadedRepository)
    {
        _downloadingRepository = downloadingRepository;
        _downloadedRepository = downloadedRepository;
    }

    #region 下载中数据

    /// <summary>
    /// 添加下载中数据
    /// </summary>
    /// <param name="downloadingItem"></param>
    public void AddDownloading(DownloadingItem? downloadingItem)
    {
        if (downloadingItem?.DownloadBase == null)
        {
            return;
        }

        var downloading = downloadingItem.Downloading;
        downloading.Id = downloadingItem.DownloadBase.Id;
        downloading.DownloadBase = downloadingItem.DownloadBase;
        var exists = _downloadingRepository.Select.Any(download => download.Id == downloading.Id);
        if (!exists)
        {
            _downloadingRepository.Insert(downloading);
        }
    }

    /// <summary>
    /// 删除下载中数据
    /// </summary>
    /// <param name="downloadingItem"></param>
    /// <param name="cascadeRemove">级联删除、手动删除下载中项时需要</param>
    public void RemoveDownloading(DownloadingItem? downloadingItem, bool cascadeRemove = false)
    {
        if (downloadingItem?.DownloadBase == null)
        {
            return;
        }

        if (cascadeRemove)
        {
            _downloadingRepository.DeleteCascadeByDatabase(it => it.Id == downloadingItem.Downloading.Id);
        }
        else
        {
            _downloadingRepository.Delete(it => it.Id == downloadingItem.DownloadBase.Id);
        }
    }

    /// <summary>
    /// 获取所有的下载中数据
    /// </summary>
    /// <returns></returns>
    public List<DownloadingItem> GetDownloading()
    {
        var downloadingList = _downloadingRepository.Select.LeftJoin(downloading => downloading.DownloadBase.Id == downloading.Id).ToList();

        return downloadingList.Select(downloading => new DownloadingItem { Downloading = downloading, DownloadBase = downloading.DownloadBase }).ToList();
    }

    /// <summary>
    /// 修改下载中数据
    /// </summary>
    /// <param name="downloadingItem"></param>
    public void UpdateDownloading(DownloadingItem? downloadingItem)
    {
        if (downloadingItem?.DownloadBase == null)
        {
            return;
        }

        var downloading = downloadingItem.Downloading;
        downloading.DownloadBase = downloadingItem.DownloadBase;

        _downloadingRepository.Update(downloading);
    }

    #endregion

    #region 下载完成数据

    /// <summary>
    /// 添加下载完成数据
    /// </summary>
    /// <param name="downloadedItem"></param>
    public void AddDownloaded(DownloadedItem? downloadedItem)
    {
        if (downloadedItem?.DownloadBase == null)
        {
            return;
        }

        var downloaded = downloadedItem.Downloaded;
        downloaded.Id = downloadedItem.DownloadBase.Id;
        var exists = _downloadedRepository.Select.Any(download => download.Id == downloaded.Id);
        if (!exists)
        {
            _downloadedRepository.Insert(downloaded);
        }
    }

    /// <summary>
    /// 删除下载完成数据
    /// </summary>
    /// <param name="downloadedItem"></param>
    public void RemoveDownloaded(DownloadedItem? downloadedItem)
    {
        if (downloadedItem?.DownloadBase == null)
        {
            return;
        }

        _downloadedRepository.DeleteCascadeByDatabase(it => it.Id == downloadedItem.Downloaded.Id);
    }

    /// <summary>
    /// 获取所有的下载完成数据
    /// </summary>
    /// <returns></returns>
    public List<DownloadedItem> GetDownloaded()
    {
        var downloadedList = _downloadedRepository.Select.LeftJoin(downloaded => downloaded.DownloadBase.Id == downloaded.Id).ToList();

        return downloadedList.Select(downloaded => new DownloadedItem { Downloaded = downloaded, DownloadBase = downloaded.DownloadBase }).ToList();
    }

    /// <summary>
    /// 修改下载完成数据
    /// </summary>
    /// <param name="downloadedItem"></param>
    public void UpdateDownloaded(DownloadedItem? downloadedItem)
    {
        if (downloadedItem?.DownloadBase == null)
        {
            return;
        }

        var downloaded = downloadedItem.Downloaded;
        downloaded.DownloadBase = downloadedItem.DownloadBase;
        _downloadedRepository.Update(downloaded);
    }

    public void ClearDownloaded()
    {
        _downloadedRepository.DeleteCascadeByDatabase(item => true);
    }

    #endregion
}