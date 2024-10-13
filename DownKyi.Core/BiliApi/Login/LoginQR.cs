using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.Login.Models;
using DownKyi.Core.Logging;
using DownKyi.Core.Utils;
using Newtonsoft.Json;

namespace DownKyi.Core.BiliApi.Login;

public static class LoginQR
{
    /// <summary>
    /// 申请二维码URL及扫码密钥（web端）
    /// </summary>
    /// <returns></returns>
    public static LoginUrlOrigin? GetLoginUrl()
    {
        string getLoginUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";
        string response = WebClient.RequestWeb(getLoginUrl);
        Console.Out.WriteLine(response);
        try
        {
            return JsonConvert.DeserializeObject<LoginUrlOrigin>(response);
        }
        catch (Exception e)
        {
            Utils.Debugging.Console.PrintLine("GetLoginUrl()发生异常: {0}", e);
            LogManager.Error("LoginQR", e);
            return null;
        }
    }

    /// <summary>
    /// 使用扫码登录（web端）
    /// </summary>
    /// <param name="qrcodeKey"></param>
    /// <param name="goUrl"></param>
    /// <returns></returns>
    public static LoginStatus? GetLoginStatus(string qrcodeKey, string goUrl = "https://www.bilibili.com")
    {
        string url = "https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key=" + qrcodeKey;

        string response = WebClient.RequestWeb(url);

        try
        {
            return JsonConvert.DeserializeObject<LoginStatus>(response);
        }
        catch (Exception e)
        {
            Utils.Debugging.Console.PrintLine("GetLoginInfo()发生异常: {0}", e);
            LogManager.Error("LoginQR", e);
            return null;
        }
    }

    /// <summary>
    /// 获得登录二维码
    /// </summary>
    /// <returns></returns>
    public static Bitmap GetLoginQRCode()
    {
        try
        {
            string loginUrl = GetLoginUrl().Data.Url;
            return GetLoginQRCode(loginUrl);
        }
        catch (Exception e)
        {
            Utils.Debugging.Console.PrintLine("GetLoginQRCode()发生异常: {0}", e);
            LogManager.Error("LoginQR", e);
            return null;
        }
    }

    /// <summary>
    /// 根据输入url生成二维码
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Bitmap GetLoginQRCode(string url)
    {
        // 设置的参数影响app能否成功扫码
        Bitmap qrCode = QRCode.EncodeQRCode(url, 11, 10, null, 0, 0, false);

        return qrCode;
    }
}