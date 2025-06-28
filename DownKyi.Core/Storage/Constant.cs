namespace DownKyi.Core.Storage;

/// <summary>
/// 存储到本地时使用的一些常量
/// </summary>
internal static class Constant
{
    // 根目录
#if NET8_0_OR_GREATER //兼容8.0中断性变更https://learn.microsoft.com/zh-cn/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix
    private static string Root => OperatingSystem.IsWindows()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DownKyi");
#else
    private static string Root
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }

            if (OperatingSystem.IsMacOS())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                    "Application Support", "DownKyi");
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DownKyi");
        }
    }
#endif
    // private static string Root { get; } = AppDomain.CurrentDomain.BaseDirectory;

    // Aria
    public static string Aria { get; } = Path.Combine(Root, "Aria");

    // 日志
    public static string Logs { get; } = Path.Combine(Root, "Logs");

    // 数据库
    public static string Database { get; } = Path.Combine(Root, "Storage");

    // 历史(搜索、下载) (加密)
    public static string Download { get; } = Path.Combine(Database, "Download.db");

    public static string DbPath { get; } = Path.Combine(Database, "Data.db");

    // 配置
    public static string Config { get; } = Path.Combine(Root, "Config");

    // 设置
    public static string Settings { get; } = Path.Combine(Config, "Settings");

    // 登录cookies
    public static string Login { get; } = Path.Combine(Config, "Login");

    // Bilibili
    private static string Bilibili { get; } = Path.Combine(Root, "Bilibili");

    // 弹幕
    public static string Danmaku { get; } = Path.Combine(Bilibili, "Danmakus");

    // 下载
    public static string Media { get; } = Path.Combine(Root, "Media");

    // 缓存
    public static string Cache { get; } = Path.Combine(Root, "Cache");
}