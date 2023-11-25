using Avalonia.Media.Imaging;
using QRCoder;

namespace DownKyi.Core.Utils;

public static class QRCode
{
    /// <summary>
    /// 生成二维码
    /// </summary>
    /// <param name="msg">信息</param>
    /// <param name="version">版本 1 ~ 40</param>
    /// <param name="pixel">像素点大小</param>
    /// <param name="icon_path">图标路径</param>
    /// <param name="icon_size">图标尺寸</param>
    /// <param name="icon_border">图标边框厚度</param>
    /// <param name="white_edge">二维码白边</param>
    /// <returns>位图</returns>
    public static Bitmap EncodeQRCode(string msg, int version, int pixel, string icon_path, int icon_size,
        int icon_border, bool white_edge)
    {
        QRCodeGenerator code_generator = new QRCodeGenerator();

        QRCodeData code_data = code_generator.CreateQrCode(msg, QRCodeGenerator.ECCLevel.H /* 这里设置容错率的一个级别 */, true,
            false, QRCodeGenerator.EciMode.Utf8, version);

        BitmapByteQRCode qrCode = new BitmapByteQRCode(code_data);
        byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);

        Bitmap icon;
        if (icon_path == null || icon_path == "")
        {
            icon = null;
        }
        else
        {
            icon = new Bitmap(icon_path);
        }

        Bitmap bmp;
        using (var ms = new MemoryStream(qrCodeAsBitmapByteArr))
        {
            bmp = new Bitmap(ms);
        }

        // Bitmap bmp = qrCode.GetGraphic(pixel, Color.FromRgb(0,0,0), Color.FromRgb(255,255,255), icon, icon_size, icon_border, white_edge);

        return bmp;
    }
}