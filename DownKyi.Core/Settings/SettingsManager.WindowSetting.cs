using DownKyi.Core.Settings.Models;

namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    private readonly WindowSettings _windowSettings = new();

    public WindowSettings GetWindowSettings()
    {
        _appSettings = GetSettings();
        if (_appSettings.WindowSettings == null)
        {
            // 第一次获取，先设置默认值
            SettingWindowSettings(_windowSettings);
            return _windowSettings;
        }

        return _appSettings.WindowSettings;
    }

    public bool SettingWindowSettings(WindowSettings windowSettings)
    {
        _appSettings.WindowSettings = windowSettings;
        return SetSettings();
    }
}