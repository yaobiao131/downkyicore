namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 是否接收测试版更新
    private const AllowStatus IsReceiveBetaVersion = AllowStatus.No;

    // 是否在启动时自动检查更新
    private const AllowStatus AutoUpdateWhenLaunch = AllowStatus.Yes;

    /// <summary>
    /// 获取是否接收测试版更新
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetIsReceiveBetaVersion()
    {
        _appSettings = GetSettings();
        if (_appSettings.About.IsReceiveBetaVersion == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetIsReceiveBetaVersion(IsReceiveBetaVersion);
            return IsReceiveBetaVersion;
        }

        return _appSettings.About.IsReceiveBetaVersion;
    }

    /// <summary>
    /// 设置是否接收测试版更新
    /// </summary>
    /// <param name="isReceiveBetaVersion"></param>
    /// <returns></returns>
    public bool SetIsReceiveBetaVersion(AllowStatus isReceiveBetaVersion)
    {
        return SetProperty(
            _appSettings.About.IsReceiveBetaVersion,
            isReceiveBetaVersion,
            v => _appSettings.About.IsReceiveBetaVersion = v);
    }

    /// <summary>
    /// 获取是否允许启动时检查更新
    /// </summary>
    /// <returns></returns>
    public AllowStatus GetAutoUpdateWhenLaunch()
    {
        _appSettings = GetSettings();
        if (_appSettings.About.AutoUpdateWhenLaunch == AllowStatus.None)
        {
            // 第一次获取，先设置默认值
            SetAutoUpdateWhenLaunch(AutoUpdateWhenLaunch);
            return AutoUpdateWhenLaunch;
        }

        return _appSettings.About.AutoUpdateWhenLaunch;
    }

    /// <summary>
    /// 设置是否允许启动时检查更新
    /// </summary>
    /// <param name="autoUpdateWhenLaunch"></param>
    /// <returns></returns>
    public bool SetAutoUpdateWhenLaunch(AllowStatus autoUpdateWhenLaunch)
    {
        return SetProperty(
            _appSettings.About.AutoUpdateWhenLaunch,
            autoUpdateWhenLaunch,
            v => _appSettings.About.AutoUpdateWhenLaunch = v);
    }

    public bool SetSkipVersionOnLaunch(string skipVersionOnLaunch)
    {
        if (Version.TryParse(skipVersionOnLaunch,out var _))
        {
            return SetProperty(
                _appSettings.About.SkipVersionOnLaunch,
                skipVersionOnLaunch,
                v => _appSettings.About.SkipVersionOnLaunch = v);
        }

        return false;
    }

    public string GetSkipVersionOnLaunch()
    {
        if (Version.TryParse(_appSettings.About.SkipVersionOnLaunch,out var _))
        {
            return _appSettings.About.SkipVersionOnLaunch;
        }
        return string.Empty;
    }
    
}