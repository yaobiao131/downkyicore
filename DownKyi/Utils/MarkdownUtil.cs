using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace DownKyi.Utils
{
    public class MarkdownUtil
    {
        public static List<Run> ConvertMarkdownToRuns(string markdownText)
        {
            var runs = new List<Run>();
            var lines = markdownText.Split('\n');
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("<!--") && line.Trim().EndsWith("-->"))
                {
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    runs.Add(new Run(line[2..]) { FontSize = 18, FontWeight = FontWeight.Bold });
                }
                else if (line.StartsWith("## "))
                {
                    runs.Add(new Run(line[3..]) { FontSize = 19, FontWeight = FontWeight.Bold }); // 二级标题
                }
                else if (line.StartsWith("### "))
                {
                    runs.Add(new Run(line[4..]) { FontSize = 15, FontWeight = FontWeight.Bold }); // 三级标题
                }
                else
                {
                    var parts = line.Split(new[] { "**" }, StringSplitOptions.None);
                    runs.AddRange(parts.Select((t, i) => i % 2 == 1 ? new Run(t) { FontWeight = FontWeight.Bold } : new Run(t) { FontSize = 13 }));
                }
            }

            return runs;
        }
    }
}