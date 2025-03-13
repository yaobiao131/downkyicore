using Avalonia.Media.Imaging;
using DownKyi.Core.BiliApi.Login.Models;
using DownKyi.Core.Logging;
using DownKyi.Core.Utils;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.BiliApi.Login;

public static class LoginQr
{
    /// <summary>
    /// 申请二维码URL及扫码密钥（web端）
    /// </summary>
    /// <returns></returns>
    public static LoginUrlOrigin? GetLoginUrl()
    {
        const string getLoginUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";
        var response = WebClient.RequestWeb(getLoginUrl);
        try
        {
            return JsonConvert.DeserializeObject<LoginUrlOrigin>(response);
        }
        catch (Exception e)
        {
            Console.PrintLine("GetLoginUrl()发生异常: {0}", e);
            LogManager.Error("LoginQR", e);
            return null;
        }
    }

    /// <summary>
    /// 使用扫码登录（web端）
    /// </summary>
    /// <param name="qrcodeKey"></param>
    /// <returns></returns>
    public static LoginStatus? GetLoginStatus(string qrcodeKey)
    {
        var url = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}";

        var response = WebClient.RequestWeb(url);

        try
        {
            return JsonConvert.DeserializeObject<LoginStatus>(response);
        }
        catch (Exception e)
        {
            Console.PrintLine("GetLoginInfo()发生异常: {0}", e);
            LogManager.Error("LoginQR", e);
            return null;
        }
    }

    /// <summary>
    /// 获得登录二维码
    /// </summary>
    /// <returns></returns>
    public static Bitmap? GetLoginQrCode()
    {
        try
        {
            var loginUrl = GetLoginUrl()?.Data?.Url;
            return GetLoginQrCode(loginUrl);
        }
        catch (Exception e)
        {
            Console.PrintLine("GetLoginQrCode()发生异常: {0}", e);
            LogManager.Error("LoginQR", e);
            return null;
        }
    }

    /// <summary>
    /// 根据输入url生成二维码
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Bitmap? GetLoginQrCode(string? url)
    {
        if (url == null) return null;
        // 设置的参数影响app能否成功扫码
        var qrCode = QrCode.EncodeQrCode(url, 11, 10, null, 0, 0, false);

        return qrCode;
    }
}