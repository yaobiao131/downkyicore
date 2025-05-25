using System.Collections.Generic;
using System.Linq;
using DownKyi.Models;
using DownKyi.ViewModels.DownloadManager;
using SqlSugar;

namespace DownKyi.Services.Download;

public class DownloadStorageService
{
    private readonly ISqlSugarClient _sqlSugarClient = (ISqlSugarClient)App.Current.Container.Resolve(typeof(ISqlSugarClient));

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

        _sqlSugarClient.UpdateNav(downloading, new UpdateNavRootOptions { IsInsertRoot = true }).IncludesAllFirstLayer().ExecuteCommand();
    }

    /// <summary>
    /// 删除下载中数据
    /// </summary>
    /// <param name="downloadingItem"></param>
    public void RemoveDownloading(DownloadingItem? downloadingItem)
    {
        if (downloadingItem?.DownloadBase == null)
        {
            return;
        }

        _sqlSugarClient.Deleteable<Downloading>(it => it.Id == downloadingItem.Downloading.Id).ExecuteCommand();
    }

    /// <summary>
    /// 获取所有的下载中数据
    /// </summary>
    /// <returns></returns>
    public List<DownloadingItem> GetDownloading()
    {
        var downloadingList = _sqlSugarClient.Queryable<Downloading>().IncludesAllFirstLayer().ToList();

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

        _sqlSugarClient.UpdateNav(downloading).IncludesAllFirstLayer().ExecuteCommand();
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
        _sqlSugarClient.AsTenant().BeginTran();
        _sqlSugarClient.Storageable(downloaded).TranLock().ExecuteCommand();
        _sqlSugarClient.AsTenant().CommitTran();
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

        _sqlSugarClient.DeleteNav<Downloaded>(it => it.Id == downloadedItem.Downloaded.Id).Include(o1 => o1.DownloadBase).ExecuteCommand();
    }

    /// <summary>
    /// 获取所有的下载完成数据
    /// </summary>
    /// <returns></returns>
    public List<DownloadedItem> GetDownloaded()
    {
        var downloadedList = _sqlSugarClient.Queryable<Downloaded>().IncludesAllFirstLayer().ToList();

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
        _sqlSugarClient.UpdateNav(downloaded).IncludesAllFirstLayer().ExecuteCommand();
    }

    #endregion
}