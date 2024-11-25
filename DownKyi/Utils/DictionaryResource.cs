using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;

namespace DownKyi.Utils;

public static class DictionaryResource
{
    /// <summary>
    /// 从资源获取颜色的16进制字符串
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns></returns>
    public static string GetColor(string resourceKey)
    {
        var obj = Dispatcher.UIThread.Invoke(() =>
        {
            object? obj = null;
            Application.Current?.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out obj);
            return obj;
        });
        return obj == null ? "#00000000" : ((Color)obj).ToString();
    }

    /// <summary>
    /// 从资源获取字符串
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns></returns>
    public static string GetString(string resourceKey)
    {
        var obj = Dispatcher.UIThread.Invoke(() =>
        {
            object? obj = null;
            Application.Current?.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out obj);
            return obj;
        });
        return obj == null ? "" : (string)obj;
    }

    public static T Get<T>(string resourceKey)
    {
        var obj = Dispatcher.UIThread.Invoke(() =>
        {
            object? obj = null;
            Application.Current?.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out obj);
            return obj;
        });
        return (T)obj;
    }
}