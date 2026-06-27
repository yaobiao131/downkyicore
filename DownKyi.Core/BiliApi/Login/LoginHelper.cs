using System.Web;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Settings.Models;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Login;

public static class LoginHelper
{
    // 本地位置
    private static readonly string LocalLoginInfo = StorageManager.GetLogin();

    // 内存缓存：读多写少，使用 ReaderWriterLockSlim 保证线程安全
    private static readonly ReaderWriterLockSlim CacheLock = new();
    private static List<DownKyiCookie>? _cachedCookies;
    private static string? _cachedCookieString;

    /// <summary>
    /// 使缓存失效，在写操作完成后调用
    /// </summary>
    private static void InvalidateCache()
    {
        CacheLock.EnterWriteLock();
        try
        {
            _cachedCookies = null;
            _cachedCookieString = null;
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 创建cookies的独立副本，避免调用方修改缓存内容
    /// </summary>
    /// <param name="cookies"></param>
    /// <returns></returns>
    private static List<DownKyiCookie> CloneCookies(IEnumerable<DownKyiCookie> cookies)
    {
        return cookies.Select(cookie => new DownKyiCookie(cookie.Name, cookie.Value, cookie.Domain)).ToList();
    }

    /// <summary>
    /// 保存登录的cookies到文件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool SaveLoginInfoCookies(string url)
    {
        var cookies = ObjectHelper.ParseCookie(url);

        return SaveLoginInfoCookies(cookies);
    }

    /// <summary>
    /// 保存登录的cookies到文件
    /// </summary>
    /// <param name="cookies"></param>
    /// <returns></returns>
    public static bool SaveLoginInfoCookies(List<DownKyiCookie> cookies)
    {
        var tempFile = LocalLoginInfo + "-" + Guid.NewGuid().ToString("N");

        var isSucceed = ObjectHelper.WriteCookiesToDisk(tempFile, cookies);
        if (isSucceed)
        {
            try
            {
                File.Copy(tempFile, LocalLoginInfo, true);
                // Encryptor.EncryptFile(tempFile, LOCAL_LOGIN_INFO, password);
            }
            catch (Exception e)
            {
                Console.PrintLine("SaveLoginInfoCookies()发生异常: {0}", e);
                LogManager.Error(e);
                return false;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

            // 写入成功后使缓存立即失效
            InvalidateCache();
        }
        else
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        return isSucceed;
    }

    /// <summary>
    /// 获得登录的cookies，缓存内容会以独立快照返回，直到下次写操作使缓存失效
    /// </summary>
    /// <returns></returns>
    public static List<DownKyiCookie> GetLoginInfoCookies()
    {
        // 先尝试从缓存读取
        CacheLock.EnterReadLock();
        try
        {
            if (_cachedCookies != null)
            {
                return CloneCookies(_cachedCookies);
            }
        }
        finally
        {
            CacheLock.ExitReadLock();
        }

        // 缓存未命中，从磁盘加载
        CacheLock.EnterWriteLock();
        try
        {
            // 双重检查：可能其他线程已完成加载
            if (_cachedCookies != null)
            {
                return CloneCookies(_cachedCookies);
            }

            if (!File.Exists(LocalLoginInfo))
            {
                return new List<DownKyiCookie>();
            }

            List<DownKyiCookie>? cookies;
            try
            {
                // 直接读取文件，用 FileShare.Read 避免独占锁，无需临时文件
                using var stream = new FileStream(LocalLoginInfo, FileMode.Open, FileAccess.Read, FileShare.Read);
                cookies = ObjectHelper.ReadCookiesFromStream(stream)?.Select(cookie =>
                {
                    cookie.Value = HttpUtility.UrlEncode(cookie.Value);
                    return cookie;
                }).ToList();
            }
            catch (Exception e)
            {
                Console.PrintLine("GetLoginInfoCookies()发生异常: {0}", e);
                LogManager.Error(e);
                return new List<DownKyiCookie>();
            }

            _cachedCookies = cookies ?? new List<DownKyiCookie>();
            // 同步更新字符串缓存
            _cachedCookieString = _cachedCookies.Count > 0
                ? string.Join("; ", _cachedCookies.Select(item => $"{item.Name}={item.Value}"))
                : "";

            return CloneCookies(_cachedCookies);
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 返回登录信息的cookies的字符串，结果会被缓存到内存中，直到下次写操作使缓存失效
    /// </summary>
    /// <returns></returns>
    public static string GetLoginInfoCookiesString()
    {
        // 先尝试从字符串缓存读取
        CacheLock.EnterReadLock();
        try
        {
            if (_cachedCookieString != null)
            {
                return _cachedCookieString;
            }
        }
        finally
        {
            CacheLock.ExitReadLock();
        }

        // 字符串缓存未命中时，触发完整加载（GetLoginInfoCookies 内部会同步填充字符串缓存）
        GetLoginInfoCookies();

        CacheLock.EnterReadLock();
        try
        {
            return _cachedCookieString ?? "";
        }
        finally
        {
            CacheLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 注销登录
    /// </summary>
    /// <returns></returns>
    public static bool Logout()
    {
        if (!File.Exists(LocalLoginInfo)) return false;
        try
        {
            File.Delete(LocalLoginInfo);

            // 注销后使缓存立即失效
            InvalidateCache();

            SettingsManager.GetInstance().SetUserInfo(new UserInfoSettings
            {
                Mid = -1,
                Name = "",
                IsLogin = false,
                IsVip = false
            });
            return true;
        }
        catch (IOException e)
        {
            Console.PrintLine("Logout()发生异常: {0}", e);
            LogManager.Error(e);
            return false;
        }
    }
}
