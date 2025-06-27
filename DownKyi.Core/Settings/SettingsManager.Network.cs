// using DownKyi.Core.Aria2cNet.Server;

using System.Net;
using DownKyi.Core.Aria2cNet.Server;

namespace DownKyi.Core.Settings
{
    public partial class SettingsManager
    {
        // 是否开启解除地区限制
        private const AllowStatus IsLiftingOfRegion = AllowStatus.Yes;

        // 启用https
        private const AllowStatus UseSsl = AllowStatus.Yes;

        // UserAgent
        private const string UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        // 下载器
        private const Downloader Downloader = Settings.Downloader.Aria;

        private const NetworkProxy NetworkProxy = Settings.NetworkProxy.None;
        private readonly string _customNetworkProxy = string.Empty;

        // 最大同时下载数(任务数)
        private const int MaxCurrentDownloads = 3;

        // 单文件最大线程数
        private const int Split = 8;

        // HttpProxy代理
        private const AllowStatus IsHttpProxy = AllowStatus.No;
        private readonly string _httpProxy = string.Empty;
        private const int HttpProxyListenPort = 0;

        // Aria服务器token
        private const string AriaToken = "downkyi";

        // Aria服务器host
        private const string AriaHost = "http://localhost";

        // Aria服务器端口号
        private const int AriaListenPort = 6800;

        // Aria日志等级
        private const AriaConfigLogLevel AriaLogLevel = AriaConfigLogLevel.INFO;

        // Aria单文件最大线程数
        private const int AriaSplit = 5;

        // Aria下载速度限制
        private const int AriaMaxOverallDownloadLimit = 0;

        // Aria下载单文件速度限制
        private const int AriaMaxDownloadLimit = 0;

        // Aria文件预分配
        private const AriaConfigFileAllocation AriaFileAllocation = AriaConfigFileAllocation.NONE;

        // Aria HttpProxy代理
        private const AllowStatus IsAriaHttpProxy = AllowStatus.No;
        private readonly string _ariaHttpProxy = string.Empty;
        private const int AriaHttpProxyListenPort = 0;

        /// <summary>
        /// 获取是否解除地区限制
        /// </summary>
        /// <returns></returns>
        public AllowStatus GetIsLiftingOfRegion()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.IsLiftingOfRegion == AllowStatus.None)
            {
                // 第一次获取，先设置默认值
                SetIsLiftingOfRegion(IsLiftingOfRegion);
                return IsLiftingOfRegion;
            }

            return _appSettings.Network.IsLiftingOfRegion;
        }

        /// <summary>
        /// 设置是否解除地区限制
        /// </summary>
        /// <param name="isLiftingOfRegion"></param>
        /// <returns></returns>
        public bool SetIsLiftingOfRegion(AllowStatus isLiftingOfRegion)
        {
            return SetProperty(
                _appSettings.Network.IsLiftingOfRegion,
                isLiftingOfRegion,
                v => _appSettings.Network.IsLiftingOfRegion = v);
        }

        /// <summary>
        /// 获取是否启用https
        /// </summary>
        /// <returns></returns>
        public AllowStatus GetUseSsl()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.UseSsl == AllowStatus.None)
            {
                // 第一次获取，先设置默认值
                SetUseSsl(UseSsl);
                return UseSsl;
            }

            return _appSettings.Network.UseSsl;
        }

        /// <summary>
        /// 设置是否启用https
        /// </summary>
        /// <param name="useSsl"></param>
        /// <returns></returns>
        public bool SetUseSsl(AllowStatus useSsl)
        {
            return SetProperty(
                _appSettings.Network.UseSsl,
                useSsl,
                v => _appSettings.Network.UseSsl = v);
        }

        /// <summary>
        /// 获取UserAgent
        /// </summary>
        /// <returns></returns>
        public string GetUserAgent()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.UserAgent == string.Empty)
            {
                // 第一次获取，先设置默认值
                SetUserAgent(UserAgent);
                return UserAgent;
            }

            return _appSettings.Network.UserAgent;
        }

        /// <summary>
        /// 设置UserAgent
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public bool SetUserAgent(string userAgent)
        {
            return SetProperty(
                _appSettings.Network.UserAgent,
                userAgent,
                v => _appSettings.Network.UserAgent = v);
        }

        /// <summary>
        /// 获取下载器
        /// </summary>
        /// <returns></returns>
        public Downloader GetDownloader()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.Downloader != Downloader.NotSet) return _appSettings.Network.Downloader;
            // 第一次获取，先设置默认值
            SetDownloader(Downloader);
            return Downloader;
        }

        /// <summary>
        /// 设置下载器
        /// </summary>
        /// <param name="downloader"></param>
        /// <returns></returns>
        public bool SetDownloader(Downloader downloader)
        {
            return SetProperty(
                _appSettings.Network.Downloader,
                downloader,
                v => _appSettings.Network.Downloader = v);
        }

        /// <summary>
        /// 获取网络代理类型
        /// </summary>
        /// <returns></returns>
        public NetworkProxy GetNetworkProxy()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.NetworkProxy != NetworkProxy.None) return _appSettings.Network.NetworkProxy;
            SetNetworkProxy(NetworkProxy);
            return NetworkProxy;
        }

        /// <summary>
        /// 设置网络代理类型
        /// </summary>
        /// <param name="networkProxy"></param>
        /// <returns></returns>
        public bool SetNetworkProxy(NetworkProxy networkProxy)
        {
            return SetProperty(
                _appSettings.Network.NetworkProxy,
                networkProxy,
                v => _appSettings.Network.NetworkProxy = v);
        }

        public string GetCustomProxy()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.NetworkProxy == NetworkProxy.Custom && !string.IsNullOrEmpty(_appSettings.Network.CustomNetworkProxy))
                return _appSettings.Network.CustomNetworkProxy;
            // 第一次获取，先设置默认值
            SetCustomProxy(_customNetworkProxy);
            return _customNetworkProxy;
        }

        public bool SetCustomProxy(string proxyUrl)
        {
            try
            {
                _ = new WebProxy(proxyUrl);
                return SetProperty(
                    _appSettings.Network.CustomNetworkProxy,
                    proxyUrl,
                    v => _appSettings.Network.CustomNetworkProxy = v);
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取最大同时下载数(任务数)
        /// </summary>
        /// <returns></returns>
        public int GetMaxCurrentDownloads()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.MaxCurrentDownloads != -1) return _appSettings.Network.MaxCurrentDownloads;
            // 第一次获取，先设置默认值
            SetMaxCurrentDownloads(MaxCurrentDownloads);
            return MaxCurrentDownloads;
        }

        /// <summary>
        /// 设置最大同时下载数(任务数)
        /// </summary>
        /// <param name="maxCurrentDownloads"></param>
        /// <returns></returns>
        public bool SetMaxCurrentDownloads(int maxCurrentDownloads)
        {
            return SetProperty(
                _appSettings.Network.MaxCurrentDownloads,
                maxCurrentDownloads,
                v => _appSettings.Network.MaxCurrentDownloads = v);
        }

        /// <summary>
        /// 获取单文件最大线程数
        /// </summary>
        /// <returns></returns>
        public int GetSplit()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.Split != -1) return _appSettings.Network.Split;
            // 第一次获取，先设置默认值
            SetSplit(Split);
            return Split;
        }

        /// <summary>
        /// 设置单文件最大线程数
        /// </summary>
        /// <param name="split"></param>
        /// <returns></returns>
        public bool SetSplit(int split)
        {
            return SetProperty(
                _appSettings.Network.Split,
                split,
                v => _appSettings.Network.Split = v);
        }

        /// <summary>
        /// 获取是否开启Http代理
        /// </summary>
        /// <returns></returns>
        public AllowStatus GetIsHttpProxy()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.IsHttpProxy != AllowStatus.None) return _appSettings.Network.IsHttpProxy;
            // 第一次获取，先设置默认值
            SetIsHttpProxy(IsHttpProxy);
            return IsHttpProxy;
        }

        /// <summary>
        /// 设置是否开启Http代理
        /// </summary>
        /// <param name="isHttpProxy"></param>
        /// <returns></returns>
        public bool SetIsHttpProxy(AllowStatus isHttpProxy)
        {
            return SetProperty(
                _appSettings.Network.IsHttpProxy,
                isHttpProxy,
                v => _appSettings.Network.IsHttpProxy = v);
        }

        /// <summary>
        /// 获取Http代理的地址
        /// </summary>
        /// <returns></returns>
        public string GetHttpProxy()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.HttpProxy != null) return _appSettings.Network.HttpProxy;
            // 第一次获取，先设置默认值
            SetHttpProxy(_httpProxy);
            return _httpProxy;
        }

        /// <summary>
        /// 设置Aria的http代理的地址
        /// </summary>
        /// <param name="httpProxy"></param>
        /// <returns></returns>
        public bool SetHttpProxy(string httpProxy)
        {
            return SetProperty(
                _appSettings.Network.HttpProxy,
                httpProxy,
                v => _appSettings.Network.HttpProxy = v);
        }

        /// <summary>
        /// 获取Http代理的端口
        /// </summary>
        /// <returns></returns>
        public int GetHttpProxyListenPort()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.HttpProxyListenPort != -1) return _appSettings.Network.HttpProxyListenPort;
            // 第一次获取，先设置默认值
            SetHttpProxyListenPort(HttpProxyListenPort);
            return HttpProxyListenPort;
        }

        /// <summary>
        /// 设置Http代理的端口
        /// </summary>
        /// <param name="httpProxyListenPort"></param>
        /// <returns></returns>
        public bool SetHttpProxyListenPort(int httpProxyListenPort)
        {
            return SetProperty(
                _appSettings.Network.HttpProxyListenPort,
                httpProxyListenPort,
                v => _appSettings.Network.HttpProxyListenPort = v);
        }

        /// <summary>
        /// 获取Aria服务器的token
        /// </summary>
        /// <returns></returns>
        public string GetAriaToken()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaToken == null)
            {
                // 第一次获取，先设置默认值
                SetAriaToken(AriaToken);
                return AriaToken;
            }

            return _appSettings.Network.AriaToken;
        }

        /// <summary>
        /// 设置Aria服务器的token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool SetAriaToken(string token)
        {
            return SetProperty(
                _appSettings.Network.AriaToken,
                token,
                v => _appSettings.Network.AriaToken = v);
        }

        /// <summary>
        /// 获取Aria服务器的host
        /// </summary>
        /// <returns></returns>
        public string GetAriaHost()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaHost == null)
            {
                // 第一次获取，先设置默认值
                SetAriaHost(AriaHost);
                return AriaHost;
            }

            return _appSettings.Network.AriaHost;
        }

        /// <summary>
        /// 设置Aria服务器的host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool SetAriaHost(string host)
        {
            return SetProperty(
                _appSettings.Network.AriaHost,
                host,
                v => _appSettings.Network.AriaHost = v);
        }

        /// <summary>
        /// 获取Aria服务器的端口号
        /// </summary>
        /// <returns></returns>
        public int GetAriaListenPort()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaListenPort == -1)
            {
                // 第一次获取，先设置默认值
                SetAriaListenPort(AriaListenPort);
                return AriaListenPort;
            }

            return _appSettings.Network.AriaListenPort;
        }

        /// <summary>
        /// 设置Aria服务器的端口号
        /// </summary>
        /// <param name="ariaListenPort"></param>
        /// <returns></returns>
        public bool SetAriaListenPort(int ariaListenPort)
        {
            return SetProperty(
                _appSettings.Network.AriaListenPort,
                ariaListenPort,
                v => _appSettings.Network.AriaListenPort = v);
        }

        /// <summary>
        /// 获取Aria日志等级
        /// </summary>
        /// <returns></returns>
        public AriaConfigLogLevel GetAriaLogLevel()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaLogLevel == AriaConfigLogLevel.NOT_SET)
            {
                // 第一次获取，先设置默认值
                SetAriaLogLevel(AriaLogLevel);
                return AriaLogLevel;
            }

            return _appSettings.Network.AriaLogLevel;
        }

        /// <summary>
        /// 设置Aria日志等级
        /// </summary>
        /// <param name="ariaLogLevel"></param>
        /// <returns></returns>
        public bool SetAriaLogLevel(AriaConfigLogLevel ariaLogLevel)
        {
            return SetProperty(
                _appSettings.Network.AriaLogLevel,
                ariaLogLevel,
                v => _appSettings.Network.AriaLogLevel = v);
        }

        /// <summary>
        /// 获取Aria单文件最大线程数
        /// </summary>
        /// <returns></returns>
        public int GetAriaSplit()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaSplit == -1)
            {
                // 第一次获取，先设置默认值
                SetAriaSplit(AriaSplit);
                return AriaSplit;
            }

            return _appSettings.Network.AriaSplit;
        }

        /// <summary>
        /// 设置Aria单文件最大线程数
        /// </summary>
        /// <param name="ariaSplit"></param>
        /// <returns></returns>
        public bool SetAriaSplit(int ariaSplit)
        {
            return SetProperty(
                _appSettings.Network.AriaSplit,
                ariaSplit,
                v => _appSettings.Network.AriaSplit = v);
        }

        /// <summary>
        /// 获取Aria下载速度限制
        /// </summary>
        /// <returns></returns>
        public int GetAriaMaxOverallDownloadLimit()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaMaxOverallDownloadLimit == -1)
            {
                // 第一次获取，先设置默认值
                SetAriaMaxOverallDownloadLimit(AriaMaxOverallDownloadLimit);
                return AriaMaxOverallDownloadLimit;
            }

            return _appSettings.Network.AriaMaxOverallDownloadLimit;
        }

        /// <summary>
        /// 设置Aria下载速度限制
        /// </summary>
        /// <param name="ariaMaxOverallDownloadLimit"></param>
        /// <returns></returns>
        public bool SetAriaMaxOverallDownloadLimit(int ariaMaxOverallDownloadLimit)
        {
            return SetProperty(
                _appSettings.Network.AriaMaxOverallDownloadLimit,
                ariaMaxOverallDownloadLimit,
                v => _appSettings.Network.AriaMaxOverallDownloadLimit = v);
        }

        /// <summary>
        /// 获取Aria下载单文件速度限制
        /// </summary>
        /// <returns></returns>
        public int GetAriaMaxDownloadLimit()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaMaxDownloadLimit == -1)
            {
                // 第一次获取，先设置默认值
                SetAriaMaxDownloadLimit(AriaMaxDownloadLimit);
                return AriaMaxDownloadLimit;
            }

            return _appSettings.Network.AriaMaxDownloadLimit;
        }

        /// <summary>
        /// 设置Aria下载单文件速度限制
        /// </summary>
        /// <param name="ariaMaxDownloadLimit"></param>
        /// <returns></returns>
        public bool SetAriaMaxDownloadLimit(int ariaMaxDownloadLimit)
        {
            return SetProperty(
                _appSettings.Network.AriaMaxDownloadLimit,
                ariaMaxDownloadLimit,
                v => _appSettings.Network.AriaMaxDownloadLimit = v);
        }

        /// <summary>
        /// 获取Aria文件预分配
        /// </summary>
        /// <returns></returns>
        public AriaConfigFileAllocation GetAriaFileAllocation()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaFileAllocation == AriaConfigFileAllocation.NOT_SET)
            {
                // 第一次获取，先设置默认值
                SetAriaFileAllocation(AriaFileAllocation);
                return AriaFileAllocation;
            }

            return _appSettings.Network.AriaFileAllocation;
        }

        /// <summary>
        /// 设置Aria文件预分配
        /// </summary>
        /// <param name="ariaFileAllocation"></param>
        /// <returns></returns>
        public bool SetAriaFileAllocation(AriaConfigFileAllocation ariaFileAllocation)
        {
            return SetProperty(
                _appSettings.Network.AriaFileAllocation,
                ariaFileAllocation,
                v => _appSettings.Network.AriaFileAllocation = v);
        }

        /// <summary>
        /// 获取是否开启Aria http代理
        /// </summary>
        /// <returns></returns>
        public AllowStatus GetIsAriaHttpProxy()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.IsAriaHttpProxy == AllowStatus.None)
            {
                // 第一次获取，先设置默认值
                SetIsAriaHttpProxy(IsAriaHttpProxy);
                return IsAriaHttpProxy;
            }

            return _appSettings.Network.IsAriaHttpProxy;
        }

        /// <summary>
        /// 设置是否开启Aria http代理
        /// </summary>
        /// <param name="isAriaHttpProxy"></param>
        /// <returns></returns>
        public bool SetIsAriaHttpProxy(AllowStatus isAriaHttpProxy)
        {
            return SetProperty(
                _appSettings.Network.IsAriaHttpProxy,
                isAriaHttpProxy,
                v => _appSettings.Network.IsAriaHttpProxy = v);
        }

        /// <summary>
        /// 获取Aria的http代理的地址
        /// </summary>
        /// <returns></returns>
        public string GetAriaHttpProxy()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaHttpProxy == null)
            {
                // 第一次获取，先设置默认值
                SetAriaHttpProxy(_ariaHttpProxy);
                return _ariaHttpProxy;
            }

            return _appSettings.Network.AriaHttpProxy;
        }

        /// <summary>
        /// 设置Aria的http代理的地址
        /// </summary>
        /// <param name="ariaHttpProxy"></param>
        /// <returns></returns>
        public bool SetAriaHttpProxy(string ariaHttpProxy)
        {
            return SetProperty(
                _appSettings.Network.AriaHttpProxy,
                ariaHttpProxy,
                v => _appSettings.Network.AriaHttpProxy = v);
        }

        /// <summary>
        /// 获取Aria的http代理的端口
        /// </summary>
        /// <returns></returns>
        public int GetAriaHttpProxyListenPort()
        {
            _appSettings = GetSettings();
            if (_appSettings.Network.AriaHttpProxyListenPort == -1)
            {
                // 第一次获取，先设置默认值
                SetAriaHttpProxyListenPort(AriaHttpProxyListenPort);
                return AriaHttpProxyListenPort;
            }

            return _appSettings.Network.AriaHttpProxyListenPort;
        }

        /// <summary>
        /// 设置Aria的http代理的端口
        /// </summary>
        /// <param name="ariaHttpProxyListenPort"></param>
        /// <returns></returns>
        public bool SetAriaHttpProxyListenPort(int ariaHttpProxyListenPort)
        {
            return SetProperty(
                _appSettings.Network.AriaHttpProxyListenPort,
                ariaHttpProxyListenPort,
                v => _appSettings.Network.AriaHttpProxyListenPort = v);
        }
    }
}