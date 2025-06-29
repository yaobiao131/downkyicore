using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Settings.Models;
using DownKyi.Core.Storage;
using DownKyi.Core.Utils;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Login
{
    public static class LoginHelper
    {
        // 本地位置
        private static readonly string LocalLoginInfo = StorageManager.GetLogin();

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
            }

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            return isSucceed;
        }


        /// <summary>
        /// 获得登录的cookies
        /// </summary>
        /// <returns></returns>
        public static List<DownKyiCookie> GetLoginInfoCookies()
        {
            var tempFile = LocalLoginInfo + "-" + Guid.NewGuid().ToString("N");

            if (File.Exists(LocalLoginInfo))
            {
                try
                {
                    File.Copy(LocalLoginInfo, tempFile, true);
                }
                catch (Exception e)
                {
                    Console.PrintLine("GetLoginInfoCookies()发生异常: {0}", e);
                    LogManager.Error(e);
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }

                    return new List<DownKyiCookie>();
                }
            }
            else
            {
                return new List<DownKyiCookie>();
            }

            var cookies = ObjectHelper.ReadCookiesFromDisk(tempFile);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            return cookies ?? new List<DownKyiCookie>();
        }

        /// <summary>
        /// 返回登录信息的cookies的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetLoginInfoCookiesString()
        {
            var cookies = GetLoginInfoCookies();
            if (cookies.Count == 0)
            {
                return "";
            }

            var cookie = string.Join("; ", cookies.Select(item => $"{item.Name}={item.Value}"));

            return cookie;
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
}