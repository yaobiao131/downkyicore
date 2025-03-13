namespace DownKyi.Core.Danmaku2Ass;

public class Producer
{
    public Dictionary<string, bool> Config;
    public Dictionary<string, Filter> Filters;
    public List<Danmaku> Danmakus;
    public List<Danmaku> KeepedDanmakus;
    public Dictionary<string, int> FilterDetail;

    public Producer(Dictionary<string, bool> config, List<Danmaku> danmakus)
    {
        Config = config;
        Danmakus = danmakus;
    }

    public void StartHandle()
    {
        LoadFilter();
        ApplyFilter();
    }

    public void LoadFilter()
    {
        Filters = new Dictionary<string, Filter>();
        if (Config["top_filter"])
        {
            Filters.Add("top_filter", new TopFilter());
        }

        if (Config["bottom_filter"])
        {
            Filters.Add("bottom_filter", new BottomFilter());
        }

        if (Config["scroll_filter"])
        {
            Filters.Add("scroll_filter", new ScrollFilter());
        }
        //if (Config["custom_filter"])
        //{
        //    Filters.Add("custom_filter", new CustomFilter());
        //}
    }

    public void ApplyFilter()
    {
        var filterDetail = new Dictionary<string, int>()
        {
            { "top_filter", 0 },
            { "bottom_filter", 0 },
            { "scroll_filter", 0 },
            //{ "custom_filter",0}
        };

        var danmakus = Danmakus;
        //string[] orders = { "top_filter", "bottom_filter", "scroll_filter", "custom_filter" };
        string[] orders = { "top_filter", "bottom_filter", "scroll_filter" };
        foreach (var name in orders)
        {
            Filter filter;
            try
            {
                filter = Filters[name];
            }
            catch (Exception e)
            {
                Console.WriteLine("ApplyFilter()发生异常: {0}", e);
                continue;
            }

            var count = danmakus.Count;
            danmakus = filter.DoFilter(danmakus);
            filterDetail[name] = count - danmakus.Count;
        }

        KeepedDanmakus = danmakus;
        FilterDetail = filterDetail;
    }

    public Dictionary<string, int> Report()
    {
        var blockedCount = FilterDetail.Values.Sum();

        var passedCount = KeepedDanmakus.Count;
        var totalCount = blockedCount + passedCount;

        var ret = new Dictionary<string, int>
        {
            { "blocked", blockedCount },
            { "passed", passedCount },
            { "total", totalCount }
        };

        return (Dictionary<string, int>)ret.Concat(FilterDetail);
    }
}