namespace DownKyi.Core.Utils;

public static class Format
{
    /// <summary>
    /// 格式化Duration时间
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static string FormatDuration(long duration)
    {
        string formatDuration;
        if (duration / 60 > 0)
        {
            var dur = duration / 60;
            formatDuration = dur / 60 > 0 ? $"{dur / 60}h{dur % 60}m{duration % 60}s" : $"{duration / 60}m{duration % 60}s";
        }
        else
        {
            formatDuration = $"{duration}s";
        }

        return formatDuration;
    }

    /// <summary>
    /// 格式化Duration时间，格式为00:00:00
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static string FormatDuration2(long duration)
    {
        string formatDuration;
        if (duration / 60 > 0)
        {
            var dur = duration / 60;
            formatDuration = dur / 60 > 0 ? $"{dur / 60:D2}:{dur % 60:D2}:{duration % 60:D2}" : $"00:{duration / 60:D2}:{duration % 60:D2}";
        }
        else
        {
            formatDuration = $"00:00:{duration:D2}";
        }

        return formatDuration;
    }

    /// <summary>
    /// 格式化Duration时间，格式为00:00
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static string FormatDuration3(long duration)
    {
        string formatDuration;
        if (duration / 60 > 0)
        {
            var dur = duration / 60;
            formatDuration = dur / 60 > 0 ? $"{dur / 60:D2}:{dur % 60:D2}:{duration % 60:D2}" : $"{duration / 60:D2}:{duration % 60:D2}";
        }
        else
        {
            formatDuration = $"00:{duration:D2}";
        }

        return formatDuration;
    }

    /// <summary>
    /// 格式化数字，超过10000的数字将单位改为万，超过100000000的数字将单位改为亿，并保留1位小数
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string FormatNumber(long number)
    {
        return number switch
        {
            > 99999999 => (number / 100000000.0f).ToString("F1") + "亿",
            > 9999 => (number / 10000.0f).ToString("F1") + "万",
            _ => number.ToString()
        };
    }

    /// <summary>
    /// 格式化网速
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    public static string FormatSpeed(float speed)
    {
        return speed switch
        {
            <= 0 => "0B/s",
            < 1024 => $"{speed:F2}B/s",
            < 1024 * 1024 => $"{speed / 1024:F2}KB/s",
            _ => $"{speed / 1024 / 1024:F2}MB/s"
        };
    }

    /// <summary>
    /// 格式化字节大小，可用于文件大小的显示
    /// </summary>
    /// <param name="fileSize"></param>
    /// <returns></returns>
    public static string FormatFileSize(long fileSize)
    {
        return fileSize switch
        {
            <= 0 => "0B",
            < 1024 => fileSize + "B",
            < 1024 * 1024 => (fileSize / 1024.0).ToString("#.##") + "KB",
            < 1024 * 1024 * 1024 => (fileSize / 1024.0 / 1024.0).ToString("#.##") + "MB",
            _ => (fileSize / 1024.0 / 1024.0 / 1024.0).ToString("#.##") + "GB"
        };
    }

    /// <summary>
    /// 去除非法字符
    /// </summary>
    /// <param name="originName"></param>
    /// <returns></returns>
    public static string FormatFileName(string originName)
    {
        var destName = originName;
        destName = Path.GetInvalidFileNameChars().Aggregate(destName, (current, c) => current.Replace(c.ToString(), string.Empty));

        var cleanedName = destName
             .SkipWhile(c => c is ' ' or '.')
             .Reverse()
             .SkipWhile(c => c is ' ' or '.')
             .Reverse()
             .ToArray();

       return new string(cleanedName);
    }

    /// <summary>
    /// 清理可能导致 Avalonia UI 崩溃的特殊 Unicode 字符
    /// 包括表情符号、零宽度字符、数学符号等
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <returns>清理后的文本</returns>
    public static string SanitizeForAvalonia(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            // 跳过零宽度字符和某些控制字符
            if (c is '\u200B' or '\u200C' or '\u200D' or '\uFEFF' or '\u2060' or '\u180E')
            {
                continue;
            }

            // 跳过某些可能导致问题的 Unicode 区域
            // 0xE000-0xF8FF: 私人使用区
            // 0xF0000-0xFFFFD: 补充私人使用区-A
            // 0x100000-0x10FFFD: 补充私人使用区-B
            if ((c >= 0xE000 && c <= 0xF8FF) ||
                (c >= 0xF0000 && c <= 0xFFFFD) ||
                (c >= 0x100000 && c <= 0x10FFFD))
            {
                continue;
            }

            // 跳过表情符号 (Emoji)
            // 0x1F600-0x1F64F: 表情符号
            // 0x1F300-0x1F5FF: 杂项符号和象形文字
            // 0x1F680-0x1F6FF: 交通和地图符号
            // 0x1F700-0x1F77F: 炼金术符号
            // 0x1F780-0x1F7FF: 几何图形扩展
            // 0x1F800-0x1F8FF: 补充箭头
            // 0x1F900-0x1F9FF: 补充符号和象形文字
            // 0x1FA00-0x1FA6F: 象棋符号、符号扩展
            // 0x1FA70-0x1FAFF: 符号和象形文字扩展-A
            // 0x2600-0x26FF: 杂项符号
            // 0x2700-0x27BF: 装饰符号
            if ((c >= 0x1F600 && c <= 0x1F64F) ||
                (c >= 0x1F300 && c <= 0x1F5FF) ||
                (c >= 0x1F680 && c <= 0x1F6FF) ||
                (c >= 0x1F700 && c <= 0x1F77F) ||
                (c >= 0x1F780 && c <= 0x1F7FF) ||
                (c >= 0x1F800 && c <= 0x1F8FF) ||
                (c >= 0x1F900 && c <= 0x1F9FF) ||
                (c >= 0x1FA00 && c <= 0x1FA6F) ||
                (c >= 0x1FA70 && c <= 0x1FAFF) ||
                (c >= 0x2600 && c <= 0x26FF) ||
                (c >= 0x2700 && c <= 0x27BF))
            {
                continue;
            }

            // 跳过数学运算符和特殊符号
            // 0x2200-0x22FF: 数学运算符
            // 0x2300-0x23FF: 杂项技术符号
            // 0x2B00-0x2BFF: 杂项符号和箭头
            // 0x3000-0x303F: CJK 符号和标点
            if ((c >= 0x2200 && c <= 0x22FF) ||
                (c >= 0x2300 && c <= 0x23FF) ||
                (c >= 0x2B00 && c <= 0x2BFF))
            {
                continue;
            }

            // 跳过特殊标点符号
            // 0x30FB: 中点・ (KATAKANA MIDDLE DOT)
            if (c == 0x30FB)
            {
                continue;
            }

            result.Append(c);
        }

        return result.ToString();
    }
}
