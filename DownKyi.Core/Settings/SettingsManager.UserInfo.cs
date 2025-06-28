using DownKyi.Core.Settings.Models;

namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 登录用户的mid
    private readonly UserInfoSettings _userInfo = new()
    {
        Mid = -1,
        Name = "",
        IsLogin = false,
        IsVip = false
    };

    /// <summary>
    /// 获取登录用户信息
    /// </summary>
    /// <returns></returns>
    public UserInfoSettings GetUserInfo()
    {
        _appSettings = GetSettings();
        if (_appSettings.UserInfo == null)
        {
            // 第一次获取，先设置默认值
            SetUserInfo(_userInfo);
            return _userInfo;
        }

        return _appSettings.UserInfo;
    }

    /// <summary>
    /// 设置中保存登录用户的信息，在index刷新用户状态时使用
    /// </summary>
    /// <param name="userInfo"></param>
    /// <returns></returns>
    public bool SetUserInfo(UserInfoSettings userInfo)
    {
        return SetProperty(
            _appSettings.UserInfo,
            userInfo,
            v => _appSettings.UserInfo = v);
    }
}