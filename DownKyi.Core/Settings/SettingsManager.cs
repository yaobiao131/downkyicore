using System.Text;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings.Models;
using DownKyi.Core.Utils.Encryptor;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    private bool SetProperty<T>(T currentValue, T newValue, Action<T> setter)
    {
        if (!EqualityComparer<T>.Default.Equals(currentValue, newValue))
        {
            setter(newValue);
            return SetSettings();
        }
        return true;
    }
    
    private static SettingsManager? _instance;

    private static readonly object _settingsLock = new object();
    // 内存中保存一份配置
    private AppSettings _appSettings;
    
    
    
    // 设置的配置文件
    private readonly string _settingsName = Storage.StorageManager.GetSettings();
    
    // 密钥
    private readonly string password = "YO1J$4#p";


    /// <summary>
    /// 获取SettingsManager实例
    /// </summary>
    /// <returns></returns>
    public static SettingsManager GetInstance()
    {
        return _instance ??= new SettingsManager();
    }

    /// <summary>
    /// 隐藏Settings()方法，必须使用单例模式
    /// </summary>
    private SettingsManager()
    {
        _appSettings = GetSettings();
    }

    /// <summary>
    /// 获取AppSettingsModel
    /// </summary>
    /// <returns></returns>
    private AppSettings GetSettings()
    {
        if (_appSettings != null)
        {
            return _appSettings;
        }

        try
        {
            var jsonWordTemplate = File.ReadAllText(_settingsName, Encoding.UTF8);
            try
            {
                return JsonConvert.DeserializeObject<AppSettings>(jsonWordTemplate);
            }
            catch (Exception e)
            {
                try
                {
                    string decryptedJson = Encryptor.DecryptString(jsonWordTemplate, password);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(decryptedJson);
                    if (settings != null)
                    {
                        _appSettings = settings;
                        SetSettings();
                        return settings;
                    }
                }
                catch (Exception decryptEx)
                {
                    Console.PrintLine("配置文件解密失败: {0}", decryptEx.Message);
                    LogManager.Error("SettingsManager", decryptEx);
                }
            }
        }
        catch (Exception e)
        {
            Console.PrintLine("GetSettings()发生异常: {0}", e);
            LogManager.Error("SettingsManager", e);
        }
        return new AppSettings();
    }

    /// <summary>
    /// 设置AppSettingsModel
    /// </summary>
    /// <returns></returns>
    private bool SetSettings()
    {
        lock (_settingsLock)
        {
            try
            {
                var json = JsonConvert.SerializeObject(_appSettings);
                File.WriteAllText(_settingsName, json, Encoding.UTF8);
                return true;
            }
            catch (Exception e)
            {
                Console.PrintLine("SetSettings()发生异常: {0}", e);
                LogManager.Error("SettingsManager", e);
                return false;
            }
        }
    }
}