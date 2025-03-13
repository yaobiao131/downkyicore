using System.Text;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings.Models;
using DownKyi.Core.Storage;
using Newtonsoft.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

#if DEBUG
#else
using DownKyi.Core.Utils.Encryptor;
#endif

namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    private static SettingsManager? _instance;

    // 内存中保存一份配置
    private AppSettings _appSettings;

#if DEBUG
    // 设置的配置文件
    private readonly string _settingsName = StorageManager.GetSettings() + "_debug.json";
#else
        // 设置的配置文件
        private readonly string _settingsName = Storage.StorageManager.GetSettings();

        // 密钥
        private readonly string password = "YO1J$4#p";
#endif

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
            //FileStream fileStream = new FileStream(settingsName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //StreamReader streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8);
            //string jsonWordTemplate = streamReader.ReadToEnd();
            //streamReader.Close();
            //fileStream.Close();
            var jsonWordTemplate = string.Empty;
            using (var stream = new FileStream(_settingsName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    jsonWordTemplate = reader.ReadToEnd();
                }
            }

#if DEBUG
#else
                // 解密字符串
                jsonWordTemplate = Encryptor.DecryptString(jsonWordTemplate, password);
#endif

            return JsonConvert.DeserializeObject<AppSettings>(jsonWordTemplate);
        }
        catch (Exception e)
        {
            Console.PrintLine("GetSettings()发生异常: {0}", e);
            LogManager.Error("SettingsManager", e);
            return new AppSettings();
        }
    }

    /// <summary>
    /// 设置AppSettingsModel
    /// </summary>
    /// <returns></returns>
    private bool SetSettings()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_appSettings);

#if DEBUG
#else
                // 加密字符串
                json = Encryptor.EncryptString(json, password);
#endif

            File.WriteAllText(_settingsName, json);
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