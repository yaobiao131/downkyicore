﻿namespace DownKyi.Core.Settings.Models;

/// <summary>
/// 关于
/// </summary>
public class AboutSettings
{
    public AllowStatus IsReceiveBetaVersion { get; set; } = AllowStatus.None;
    public AllowStatus AutoUpdateWhenLaunch { get; set; } = AllowStatus.None;
    
    public string SkipVersionOnLaunch  { get; set; } = string.Empty;
}