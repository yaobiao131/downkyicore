namespace DownKyi.Core.Settings.Models;

public class WindowSettings
{
    public double Width { get; set; } = 1100; // 默认宽度
    public double Height { get; set; } = 750; // 默认高度
    public double X { get; set; } = double.NaN; // 默认位置未设置
    public double Y { get; set; } = double.NaN; // 默认位置未设置
}