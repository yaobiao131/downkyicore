using System.Net;
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

        // 16位密码，ps:密码位数没有限制，可任意设置
        private static readonly string SecretKey = "EsOat*^y1QR!&0J6";

        /// <summary>
        /// 保存登录的cookies到文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool SaveLoginInfoCookies(string url)
        {
            var tempFile = LocalLoginInfo + "-" + Guid.NewGuid().ToString("N");
            var cookieContainer = ObjectHelper.ParseCookie(url);

            var isSucceed = ObjectHelper.WriteCookiesToDisk(tempFile, cookieContainer);
            if (isSucceed)
            {
                // 加密密钥，增加机器码
                var password = SecretKey;

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
        public static CookieContainer? GetLoginInfoCookies()
        {
            var tempFile = LocalLoginInfo + "-" + Guid.NewGuid().ToString("N");

            if (File.Exists(LocalLoginInfo))
            {
                try
                {
                    File.Copy(LocalLoginInfo, tempFile, true);
                    // 加密密钥，增加机器码
                    var password = SecretKey;
                    // Encryptor.DecryptFile(LOCAL_LOGIN_INFO, tempFile, password);
                }
                catch (Exception e)
                {
                    Console.PrintLine("GetLoginInfoCookies()发生异常: {0}", e);
                    LogManager.Error(e);
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }

                    return null;
                }
            }
            else
            {
                return null;
            }

            var cookies = ObjectHelper.ReadCookiesFromDisk(tempFile);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            return cookies;
        }

        /// <summary>
        /// 返回登录信息的cookies的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetLoginInfoCookiesString()
        {
            var cookieContainer = GetLoginInfoCookies();
            if (cookieContainer == null)
            {
                return "";
            }

            var cookies = ObjectHelper.GetAllCookies(cookieContainer);

            var cookie = cookies.Aggregate(string.Empty, (current, item) => current + (item + ";"));

            return cookie.TrimEnd(';');
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