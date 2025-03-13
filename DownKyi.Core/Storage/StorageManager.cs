namespace DownKyi.Core.Storage;

public static class StorageManager
{
    /// <summary>
    /// 获取Aria的文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetAriaDir()
    {
        CreateDirectory(Constant.Aria);
        return Constant.Aria;
    }

    /// <summary>
    /// 获取日志的文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetLogsDir()
    {
        CreateDirectory(Constant.Logs);
        return Constant.Logs;
    }

    /// <summary>
    /// 获取历史记录的文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetDownload()
    {
        CreateDirectory(Constant.Database);
        return Constant.Download;
    }

    /// <summary>
    /// 获取设置的文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetSettings()
    {
        CreateDirectory(Constant.Config);
        return Constant.Settings;
    }

    /// <summary>
    /// 获取登录cookies的文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetLogin()
    {
        CreateDirectory(Constant.Config);
        return Constant.Login;
    }

    /// <summary>
    /// 获取弹幕的文件夹路径
    /// </summary>
    /// <returns></returns>
    public static string GetDanmaku()
    {
        return CreateDirectory(Constant.Danmaku);
    }

    public static string GetMedia()
    {
        return CreateDirectory(Constant.Media);
    }

    public static string GetCache()
    {
        return CreateDirectory(Constant.Cache);
    }

    /// <summary>
    /// 若文件夹不存在，则创建文件夹
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    private static string CreateDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }
}