using System.Collections.Generic;
using Prism.Ioc;

namespace DownKyi.Utils;

public static class NavigationRegistrationExtensions
{
    private static readonly Dictionary<string, string> TitleMap = new();
    
    public static void RegisterViewWithTitle<TView>(
        this IContainerRegistry registry, 
        string tag, 
        string title) where TView : class
    {
        registry.RegisterForNavigation<TView>(tag);
        TitleMap[tag] = title;
    }

    public static string GetNavigationTitle(string? tag)
    {
        return string.IsNullOrEmpty(tag) || !TitleMap.ContainsKey(tag) 
            ? "新标签" 
            : TitleMap.GetValueOrDefault(tag, "新标签");
    }
}