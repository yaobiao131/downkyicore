using Avalonia;
using Avalonia.Media.Imaging;
using DownKyi.Core.Logging;
using DownKyi.Core.Storage.Database;
using DownKyi.Core.Utils.Encryptor;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Storage;

public class StorageCover
{
    // 先判断本地有没有
    // 如果有
    // 则返回本地的图片路径
    // 如果没有
    // 则下载图片并返回本地的图片路径

    /// <summary>
    /// 获取封面缩略图
    /// </summary>
    /// <param name="avid"></param>
    /// <param name="bvid"></param>
    /// <param name="cid"></param>
    /// <param name="url"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public Bitmap GetCoverThumbnail(long avid, string bvid, long cid, string url, int width, int height)
    {
        var header = GetCover(avid, bvid, cid, url);

        return GetCoverThumbnail(header, width, height);
    }

    /// <summary>
    /// 获取封面缩略图
    /// </summary>
    /// <param name="cover"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public Bitmap GetCoverThumbnail(string cover, int width, int height)
    {
        if (cover == null)
        {
            return null;
        }

        try
        {
            var bitmap = new Bitmap(cover);
            return bitmap.CreateScaledBitmap(new PixelSize(width, height), BitmapInterpolationMode.Unspecified);

            // return StorageUtils.BitmapToBitmapImage(new Bitmap(thumbnail));
        }
        catch (ArgumentException)
        {
            try
            {
                // SimpleDecoder simpleDecoder = new SimpleDecoder(cover);
                // Bitmap bitmap = simpleDecoder.WebPtoBitmap();
                // Image thumbnail = bitmap.GetThumbnailImage(width, height, null, IntPtr.Zero);
                //
                // return StorageUtils.BitmapToBitmapImage(new Bitmap(thumbnail));
                return null;
            }
            catch (Exception ex)
            {
                Console.PrintLine("GetCoverThumbnail()发生异常: {0}", ex);
                LogManager.Error("StorageCover.GetCoverThumbnail()", ex);

                return null;
            }
        }
        catch (Exception e)
        {
            Console.PrintLine("GetCoverThumbnail()发生异常: {0}", e);
            LogManager.Error("StorageCover.GetCoverThumbnail()", e);

            return null;
        }
    }

    /// <summary>
    /// 获取封面
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetCover(string url)
    {
        return GetCover(0, "", 0, url);
    }

    /// <summary>
    /// 获取封面
    /// </summary>
    /// <param name="avid"></param>
    /// <param name="bvid"></param>
    /// <param name="cid"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetCover(long avid, string bvid, long cid, string url)
    {
        var coverDb = new CoverDb();
        var cover = coverDb.QueryByUrl(url);

        // 如果存在，直接返回
        // 如果不存在，则先下载
        if (cover != null)
        {
            var coverPath = $"{StorageManager.GetCover()}/{cover.Md5}";
            if (File.Exists(coverPath))
            {
                var newCover = new Cover
                {
                    Avid = avid,
                    Bvid = bvid,
                    Cid = cid,
                    Url = url,
                    Md5 = cover.Md5
                };
                coverDb.Update(newCover);

                //coverDb.Close();
                return $"{StorageManager.GetCover()}/{cover.Md5}";
            }
            else
            {
                var md5 = DownloadImage(url);
                if (md5 != null)
                {
                    Cover newCover = new Cover
                    {
                        Avid = avid,
                        Bvid = bvid,
                        Cid = cid,
                        Url = url,
                        Md5 = md5
                    };
                    coverDb.Update(newCover);

                    //coverDb.Close();
                    return $"{StorageManager.GetCover()}/{md5}";
                }
                else
                {
                    //coverDb.Close();
                    return null;
                }
            }
        }
        else
        {
            var md5 = DownloadImage(url);
            if (md5 != null)
            {
                Cover newCover = new Cover
                {
                    Avid = avid,
                    Bvid = bvid,
                    Cid = cid,
                    Url = url,
                    Md5 = md5
                };
                coverDb.Insert(newCover);

                //coverDb.Close();
                return $"{StorageManager.GetCover()}/{md5}";
            }
            else
            {
                //coverDb.Close();
                return null;
            }
        }
    }

    /// <summary>
    /// 下载图片
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private string DownloadImage(string url)
    {
        var localFile = Path.GetTempPath() + Guid.NewGuid().ToString("N");

        // 下载
        var isSuccessed = StorageUtils.DownloadImage(url, localFile);
        if (isSuccessed)
        {
            try
            {
                string md5 = Hash.GetMD5HashFromFile(localFile);

                if (File.Exists(localFile))
                {
                    string destFile = $"{StorageManager.GetCover()}/{md5}";

                    try
                    {
                        File.Delete(destFile);
                    }
                    catch
                    {
                    }

                    // 移动到指定位置
                    File.Move(localFile, destFile);

                    return md5;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.PrintLine("DownloadImage()发生异常: {0}", e);
                LogManager.Error("StorageCover", e);
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// 本地是否存在
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public bool IsLocal(CoverDb coverDb, string url)
    {
        var cover = coverDb.QueryByUrl(url);
        return cover != null;
    }

    /// <summary>
    /// 返回图片md5值
    /// </summary>
    /// <param name="coverDb"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public string LocalCover(CoverDb coverDb, string url)
    {
        var cover = coverDb.QueryByUrl(url);
        return cover.Md5;
    }
}