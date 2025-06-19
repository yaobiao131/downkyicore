using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DownKyi.Converter;

public class MarkdownToInlinesConverter : IValueConverter
{
    private static readonly Dictionary<int, (double FontSize, FontWeight Weight)> HeaderStyles = new()
    {
        [1] = (24, FontWeight.Bold),  // #
        [2] = (20, FontWeight.Bold),  // ##
        [3] = (16, FontWeight.Bold),  // ###
        [4] = (14, FontWeight.SemiBold) // ####
    };
    private bool TryParseHeader(string line, out int level, out string text)
    {
        level = 0;
        text = string.Empty;

        if (!line.StartsWith("#")) return false;
        
        level = line.TakeWhile(c => c == '#').Count();
        if (level == 0 || level > 6) return false;
        
        text = line[level..].TrimStart();
        return true;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string markdown) return null;

        var inlines = new List<Inline>();
        var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                inlines.Add(new LineBreak());
                continue;
            }
            
            if (line.TrimStart().StartsWith("<!--") && line.TrimEnd().EndsWith("-->")) continue;

            
            if (TryParseHeader(line, out var headerLevel, out var headerText))
            {
                var style = HeaderStyles[Math.Min(headerLevel, HeaderStyles.Count)];
                inlines.Add(new Run(headerText)
                {
                    FontSize = style.FontSize,
                    FontWeight = style.Weight
                });
            }
            else if (line.Contains("**"))
            {
                var parts = Regex.Split(line, @"\*\*(.*?)\*\*");
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i % 2 == 1)
                        inlines.Add(new Run(parts[i]) { FontWeight = FontWeight.Bold });
                    else
                        inlines.Add(new Run(parts[i]));
                }
            }
            else if (line.StartsWith("- "))
            {
                inlines.Add(new Run("• " + line.Substring(2)){FontSize = 14});
            }
            else
            {
                inlines.Add(new Run(line));
            }

            inlines.Add(new LineBreak());
        }

        var conn = new InlineCollection();
        foreach (var inline in inlines)
        {
            conn.Add(inline);
        }
        return conn;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}