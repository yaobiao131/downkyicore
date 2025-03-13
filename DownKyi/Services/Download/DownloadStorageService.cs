using DownKyi.Core.Storage.Database.Download;
using DownKyi.Models;
using DownKyi.ViewModels.DownloadManager;
using System.Collections.Generic;

namespace DownKyi.Services.Download;

public class DownloadStorageService
{
    ~DownloadStorageService()
    {
        var downloadingDb = new DownloadingDb();
        downloadingDb.Close();
        var downloadedDb = new DownloadedDb();
        downloadedDb.Close();
        var downloadBaseDb = new DownloadBaseDb();
        downloadBaseDb.Close();
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

        AddDownloadBase(downloadingItem.DownloadBase);

        var downloadingDb = new DownloadingDb();
        var obj = downloadingDb.QueryById(downloadingItem.DownloadBase.Uuid);
        if (obj == null)
        {
            downloadingDb.Insert(downloadingItem.DownloadBase.Uuid, downloadingItem.Downloading);
        }
        //downloadingDb.Close();
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

        RemoveDownloadBase(downloadingItem.DownloadBase.Uuid);

        var downloadingDb = new DownloadingDb();
        downloadingDb.Delete(downloadingItem.DownloadBase.Uuid);
        //downloadingDb.Close();
    }

    /// <summary>
    /// 获取所有的下载中数据
    /// </summary>
    /// <returns></returns>
    public List<DownloadingItem> GetDownloading()
    {
        // 从数据库获取数据
        var downloadingDb = new DownloadingDb();
        var dic = downloadingDb.QueryAll();
        //downloadingDb.Close();

        // 遍历
        var list = new List<DownloadingItem>();
        foreach (var item in dic)
        {
            if (item.Value is not Downloading downloading) continue;
            var downloadingItem = new DownloadingItem
            {
                DownloadBase = GetDownloadBase(item.Key),
                Downloading = downloading
            };

            if (downloadingItem.DownloadBase == null)
            {
                continue;
            }

            list.Add(downloadingItem);
        }

        return list;
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

        UpdateDownloadBase(downloadingItem.DownloadBase);

        var downloadingDb = new DownloadingDb();
        downloadingDb.Update(downloadingItem.DownloadBase.Uuid, downloadingItem.Downloading);
        //downloadingDb.Close();
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

        AddDownloadBase(downloadedItem.DownloadBase);

        var downloadedDb = new DownloadedDb();
        var obj = downloadedDb.QueryById(downloadedItem.DownloadBase.Uuid);
        if (obj == null)
        {
            downloadedDb.Insert(downloadedItem.DownloadBase.Uuid, downloadedItem.Downloaded);
        }
        //downloadedDb.Close();
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

        RemoveDownloadBase(downloadedItem.DownloadBase.Uuid);

        var downloadedDb = new DownloadedDb();
        downloadedDb.Delete(downloadedItem.DownloadBase.Uuid);
        //downloadedDb.Close();
    }

    /// <summary>
    /// 获取所有的下载完成数据
    /// </summary>
    /// <returns></returns>
    public List<DownloadedItem> GetDownloaded()
    {
        // 从数据库获取数据
        var downloadedDb = new DownloadedDb();
        var dic = downloadedDb.QueryAll();
        //downloadedDb.Close();

        // 遍历
        var list = new List<DownloadedItem>();
        foreach (var item in dic)
        {
            if (item.Value is Downloaded downloaded)
            {
                var downloadedItem = new DownloadedItem
                {
                    DownloadBase = GetDownloadBase(item.Key),
                    Downloaded = downloaded
                };

                if (downloadedItem.DownloadBase == null)
                {
                    continue;
                }

                list.Add(downloadedItem);
            }
        }

        return list;
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

        UpdateDownloadBase(downloadedItem.DownloadBase);

        var downloadedDb = new DownloadedDb();
        downloadedDb.Update(downloadedItem.DownloadBase.Uuid, downloadedItem.Downloaded);
        //downloadedDb.Close();
    }

    #endregion

    #region DownloadBase

    /// <summary>
    /// 向数据库添加DownloadBase
    /// </summary>
    /// <param name="downloadBase"></param>
    private void AddDownloadBase(DownloadBase? downloadBase)
    {
        if (downloadBase == null)
        {
            return;
        }

        var downloadBaseDb = new DownloadBaseDb();
        var obj = downloadBaseDb.QueryById(downloadBase.Uuid);
        if (obj == null)
        {
            downloadBaseDb.Insert(downloadBase.Uuid, downloadBase);
        }
        //downloadBaseDb.Close();
    }

    /// <summary>
    /// 从数据库删除DownloadBase
    /// </summary>
    /// <param name="uuid"></param>
    private void RemoveDownloadBase(string uuid)
    {
        var downloadBaseDb = new DownloadBaseDb();
        downloadBaseDb.Delete(uuid);
        //downloadBaseDb.Close();
    }

    /// <summary>
    /// 从数据库获取所有的DownloadBase
    /// </summary>
    /// <returns></returns>
    private DownloadBase GetDownloadBase(string uuid)
    {
        var downloadBaseDb = new DownloadBaseDb();
        var obj = downloadBaseDb.QueryById(uuid);
        //downloadBaseDb.Close();

        return obj is DownloadBase downloadBase ? downloadBase : null;
    }

    /// <summary>
    /// 从数据库修改DownloadBase
    /// </summary>
    /// <param name="downloadBase"></param>
    private void UpdateDownloadBase(DownloadBase? downloadBase)
    {
        if (downloadBase == null)
        {
            return;
        }

        var downloadBaseDb = new DownloadBaseDb();
        downloadBaseDb.Update(downloadBase.Uuid, downloadBase);
        //downloadBaseDb.Close();
    }

    #endregion
}