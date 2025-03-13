namespace DownKyi.Core.Danmaku2Ass;

/// <summary>
/// 碰撞处理
/// </summary>
public class Collision
{
    private readonly int lineCount;
    private readonly List<int> leaves;

    public Collision(int lineCount)
    {
        this.lineCount = lineCount;
        leaves = Leaves();
    }

    private List<int> Leaves()
    {
        var ret = new List<int>(lineCount);
        for (var i = 0; i < lineCount; i++) ret.Add(0);
        return ret;
    }

    /// <summary>
    /// 碰撞检测
    /// 返回行号和时间偏移
    /// </summary>
    /// <param name="display"></param>
    /// <returns></returns>
    public Tuple<int, float> Detect(Display display)
    {
        var beyonds = new List<float>();
        for (var i = 0; i < leaves.Count; i++)
        {
            var beyond = display.Danmaku.Start - leaves[i];
            // 某一行有足够空间，直接返回行号和 0 偏移
            if (beyond >= 0)
            {
                return Tuple.Create(i, 0f);
            }

            beyonds.Add(beyond);
        }

        // 所有行都没有空间了，那么找出哪一行能在最短时间内让出空间
        var soon = beyonds.Max();
        var lineIndex = beyonds.IndexOf(soon);
        var offset = -soon;
        return Tuple.Create(lineIndex, offset);
    }

    public void Update(float leave, int lineIndex, float offset)
    {
        leaves[lineIndex] = Utils.IntCeiling(leave + offset);
    }
}