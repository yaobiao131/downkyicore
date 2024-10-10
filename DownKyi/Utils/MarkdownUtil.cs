using Avalonia.Controls.Documents;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    runs.Add(new Run(line.Substring(2)) { FontSize = 18, FontWeight = FontWeight.Bold }); 
                }
                else if (line.StartsWith("## "))
                {
                    runs.Add(new Run(line.Substring(3)) { FontSize = 19, FontWeight = FontWeight.Bold }); // 二级标题
                }
                else if (line.StartsWith("### "))
                {
                    runs.Add(new Run(line.Substring(4)) { FontSize = 15, FontWeight = FontWeight.Bold }); // 三级标题
                }
                else
                {
                    var parts = line.Split(new[] { "**" }, StringSplitOptions.None);
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            runs.Add(new Run(parts[i]) { FontWeight = FontWeight.Bold });
                        }
                        else
                        {
                            runs.Add(new Run(parts[i]) { FontSize = 13});
                        }
                    }
                }
            }
            return runs;
        }
    }
}
