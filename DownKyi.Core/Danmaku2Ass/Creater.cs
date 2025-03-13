namespace DownKyi.Core.Danmaku2Ass;

/// <summary>
/// 创建器
/// </summary>
public class Creater
{
    public Config Config;
    public List<Danmaku> Danmakus;
    public List<Subtitle> Subtitles;
    public string Text;

    public Creater(Config config, List<Danmaku> danmakus)
    {
        Config = config;
        Danmakus = danmakus;
        Subtitles = SetSubtitles();
        Text = SetText();
    }

    protected List<Subtitle> SetSubtitles()
    {
        var scroll = new Collision(Config.LineCount);
        var stayed = new Collision(Config.LineCount);
        var collisions = new Dictionary<string, Collision>
        {
            { "scroll", scroll },
            { "top", stayed },
            { "bottom", stayed }
        };

        var subtitles = new List<Subtitle>();
        foreach (var danmaku in Danmakus)
        {
            // 丢弃不支持的
            if (danmaku.Style == "none")
            {
                continue;
            }

            // 创建显示方式对象
            var display = Display.Factory(Config, danmaku);
            var collision = collisions[danmaku.Style];
            var (lineIndex, waitingOffset) = collision.Detect(display);

            // 超过容忍的偏移量，丢弃掉此条弹幕
            if (waitingOffset > Config.DropOffset)
            {
                continue;
            }

            // 接受偏移，更新碰撞信息
            display.Relayout(lineIndex);
            collision.Update(display.Leave, lineIndex, waitingOffset);

            // 再加上自定义偏移
            var offset = waitingOffset + Config.CustomOffset;
            var subtitle = new Subtitle(danmaku, display, offset);

            subtitles.Add(subtitle);
        }

        return subtitles;
    }

    protected string SetText()
    {
        var header = Config.HeaderTemplate
            .Replace("{title}", Config.Title)
            .Replace("{width}", Config.ScreenWidth.ToString())
            .Replace("{height}", Config.ScreenHeight.ToString())
            .Replace("{fontname}", Config.FontName)
            .Replace("{fontsize}", Config.BaseFontSize.ToString());

        var events = Subtitles.Aggregate(string.Empty, (current, subtitle) => current + "\n" + subtitle.Text);

        return header + events;
    }
}