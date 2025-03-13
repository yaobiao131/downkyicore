namespace DownKyi.Core.FileName;

/// <summary>
/// 文件名字段
/// </summary>
public static class HyphenSeparated
{
    // 文件名的分隔符
    public static readonly Dictionary<int, string> Hyphen = new()
    {
        { 100, "/" },
        { 101, "_" },
        { 102, "-" },
        { 103, "+" },
        { 104, "," },
        { 105, "." },
        { 106, "&" },
        { 107, "#" },
        { 108, "(" },
        { 109, ")" },
        { 110, "[" },
        { 111, "]" },
        { 112, "{" },
        { 113, "}" },
        { 114, " " },
    };
}