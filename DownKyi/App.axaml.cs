using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using DownKyi.Models;
using DownKyi.PrismExtension.Dialog;
using DownKyi.Services.Download;
using DownKyi.Utils;
using DownKyi.ViewModels;
using DownKyi.ViewModels.Dialogs;
using DownKyi.ViewModels.DownloadManager;
using DownKyi.ViewModels.Friends;
using DownKyi.ViewModels.Settings;
using DownKyi.ViewModels.Toolbox;
using DownKyi.ViewModels.UserSpace;
using DownKyi.Views;
using DownKyi.Views.Dialogs;
using DownKyi.Views.DownloadManager;
using DownKyi.Views.Friends;
using DownKyi.Views.Settings;
using DownKyi.Views.Toolbox;
using DownKyi.Views.UserSpace;
using FreeSql;
using Prism.DryIoc;
using Prism.Ioc;
using ViewSeasonsSeries = DownKyi.Views.ViewSeasonsSeries;
using ViewSeasonsSeriesViewModel = DownKyi.ViewModels.ViewSeasonsSeriesViewModel;

namespace DownKyi;

public partial class App : PrismApplication
{
    public const string RepoOwner = "yaobiao131";
    public const string RepoName = "downkyicore";

    public static ObservableCollection<DownloadingItem> DownloadingList { get; set; } = new();
    public static ObservableCollection<DownloadedItem> DownloadedList { get; set; } = new();
    public new static App Current => (App)Application.Current!;
    public new MainWindow MainWindow => Container.Resolve<MainWindow>();
    public IClassicDesktopStyleApplicationLifetime? AppLife;

    private static Mutex _mutex;

    // 下载服务
    private IDownloadService? _downloadService;

    public override void Initialize()
    {
#if !DEBUG
              _mutex = new Mutex(true, "Global\\DownKyi", out var createdNew);
                if (!createdNew)
                {
                    Environment.Exit(0);
                }
#endif

        AvaloniaXamlLoader.Load(this);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += OnExit!;
            AppLife = desktop;
        }

        base.Initialize();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IFreeSql>(() =>
        {
            var freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, $"Data Source={StorageManager.GetDbPath()}")
                .UseAdoConnectionPool(true)
#if DEBUG
                .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))
#endif
                .Build();
            freeSql.UseJsonMap();
            return freeSql;
        });
        containerRegistry.RegisterSingleton<DownloadStorageService>();
        containerRegistry.RegisterScoped<IBaseRepository<Downloading>>(cp =>
        {
            var freeSql = (IFreeSql)cp.Resolve(typeof(IFreeSql));
            var downloadingRepository = freeSql.GetRepository<Downloading>();
            downloadingRepository.DbContextOptions.EnableCascadeSave = true;
            return downloadingRepository;
        });
        containerRegistry.RegisterScoped<IBaseRepository<Downloaded>>(cp =>
        {
            var freeSql = (IFreeSql)cp.Resolve(typeof(IFreeSql));
            var downloadRepository = freeSql.GetRepository<Downloaded>();
            downloadRepository.DbContextOptions.EnableCascadeSave = true;
            return downloadRepository;
        });

        containerRegistry.RegisterSingleton<MainWindow>();
        containerRegistry.RegisterSingleton<IDialogService, DialogService>();
        containerRegistry.Register<IDialogWindow, DialogWindow>();
        // pages
        containerRegistry.RegisterForNavigation<ViewIndex>(ViewIndexViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewLogin>(ViewLoginViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewVideoDetail>(ViewVideoDetailViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewSettings>(ViewSettingsViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewToolbox>(ViewToolboxViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewDownloadManager>(ViewDownloadManagerViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewPublicFavorites>(ViewPublicFavoritesViewModel.Tag);

        containerRegistry.RegisterForNavigation<ViewUserSpace>(ViewUserSpaceViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewPublication>(ViewPublicationViewModel.Tag);
        // containerRegistry.RegisterForNavigation<Views.ViewChannel>(ViewModels.ViewChannelViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewSeasonsSeries>(ViewSeasonsSeriesViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewFriends>(ViewFriendsViewModel.Tag);

        containerRegistry.RegisterForNavigation<ViewMySpace>(ViewMySpaceViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewMyFavorites>(ViewMyFavoritesViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewMyBangumiFollow>(ViewMyBangumiFollowViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewMyToViewVideo>(ViewMyToViewVideoViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewMyHistory>(ViewMyHistoryViewModel.Tag);

        // downloadManager pages
        containerRegistry.RegisterForNavigation<ViewDownloading>(ViewDownloadingViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewDownloadFinished>(ViewDownloadFinishedViewModel.Tag);

        // Friend
        containerRegistry.RegisterForNavigation<ViewFollowing>(ViewFollowingViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewFollower>(ViewFollowerViewModel.Tag);

        // settings pages
        containerRegistry.RegisterForNavigation<ViewBasic>(ViewBasicViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewNetwork>(ViewNetworkViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewVideo>(ViewVideoViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewDanmaku>(ViewDanmakuViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewAbout>(ViewAboutViewModel.Tag);

        // tools pages
        containerRegistry.RegisterForNavigation<ViewBiliHelper>(ViewBiliHelperViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewDelogo>(ViewDelogoViewModel.Tag);
        containerRegistry.RegisterForNavigation<ViewExtractMedia>(ViewExtractMediaViewModel.Tag);

        // UserSpace
        containerRegistry.RegisterForNavigation<ViewArchive>(ViewArchiveViewModel.Tag);
        // containerRegistry.RegisterForNavigation<Views.UserSpace.ViewChannel>(ViewModels.UserSpace.ViewChannelViewModel.Tag);
        containerRegistry.RegisterForNavigation<Views.UserSpace.ViewSeasonsSeries>(ViewModels.UserSpace.ViewSeasonsSeriesViewModel.Tag);

        // dialogs
        containerRegistry.RegisterDialog<ViewAlertDialog>(ViewAlertDialogViewModel.Tag);
        containerRegistry.RegisterDialog<ViewDownloadSetter>(ViewDownloadSetterViewModel.Tag);
        containerRegistry.RegisterDialog<ViewParsingSelector>(ViewParsingSelectorViewModel.Tag);
        containerRegistry.RegisterDialog<ViewAlreadyDownloadedDialog>(ViewAlreadyDownloadedDialogViewModel.Tag);
        containerRegistry.RegisterDialog<NewVersionAvailableDialog>(NewVersionAvailableDialogViewModel.Tag);
        containerRegistry.RegisterDialog<ViewUpgradingDialog>(ViewUpgradingDialogViewModel.Tag);
    }


    protected override AvaloniaObject CreateShell()
    {
        if (Design.IsDesignMode)
        {
            return Container.Resolve<MainWindow>();
        }

        Container.Resolve<IFreeSql>().CodeFirst.SyncStructure(typeof(DownloadBase), typeof(Downloaded), typeof(Downloading));
        // 下载数据存储服务
        var downloadStorageService = Container.Resolve<DownloadStorageService>();

        // 从数据库读取
        var downloadingItems = downloadStorageService.GetDownloading();
        var downloadedItems = downloadStorageService.GetDownloaded();
        DownloadingList.AddRange(downloadingItems);
        DownloadedList.AddRange(downloadedItems);

        // 启动下载服务
        var download = SettingsManager.GetInstance().GetDownloader();
        switch (download)
        {
            case Core.Settings.Downloader.NotSet:
                break;
            case Core.Settings.Downloader.BuiltIn:
                _downloadService = new BuiltinDownloadService(DownloadingList, DownloadedList, (IDialogService?)Container.GetContainer().GetService(typeof(IDialogService)));
                break;
            case Core.Settings.Downloader.Aria:
                _downloadService = new AriaDownloadService(DownloadingList, DownloadedList, (IDialogService?)Container.GetContainer().GetService(typeof(IDialogService)));
                break;
            case Core.Settings.Downloader.CustomAria:
                _downloadService = new CustomAriaDownloadService(DownloadingList, DownloadedList, (IDialogService?)Container.GetContainer().GetService(typeof(IDialogService)));
                break;
        }

        _downloadService?.Start();
        return Container.Resolve<MainWindow>();
    }

    protected override void OnInitialized()
    {
        ThemeHelper.SetTheme(SettingsManager.GetInstance().GetThemeMode());
        // var regionManager = Container.Resolve<IRegionManager>();
        // regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewIndex));
        // regionManager.RegisterViewWithRegion("DownloadManagerContentRegion", typeof(ViewDownloading));
        // regionManager.RegisterViewWithRegion("SettingsContentRegion", typeof(ViewBasic));
    }

    public static void PropertyChangeAsync(Action callback)
    {
        Dispatcher.UIThread.Invoke(callback);
    }

    /// <summary>
    /// 下载完成列表排序
    /// </summary>
    /// <param name="finishedSort"></param>
    public static void SortDownloadedList(DownloadFinishedSort finishedSort)
    {
        var list = DownloadedList.ToList();
        switch (finishedSort)
        {
            case DownloadFinishedSort.DownloadAsc:
                // 按下载先后排序
                list.Sort((x, y) => x.Downloaded.FinishedTimestamp.CompareTo(y.Downloaded.FinishedTimestamp));
                break;
            case DownloadFinishedSort.DownloadDesc:
                // 按下载先后排序
                list.Sort((x, y) => y.Downloaded.FinishedTimestamp.CompareTo(x.Downloaded.FinishedTimestamp));
                break;
            case DownloadFinishedSort.Number:
                // 按序号排序
                list.Sort((x, y) =>
                {
                    var compare = string.Compare(x.MainTitle, y.MainTitle, StringComparison.Ordinal);
                    return compare == 0 ? x.Order.CompareTo(y.Order) : compare;
                });
                break;
            case DownloadFinishedSort.NotSet:
            default:
                break;
        }

        // 更新下载完成列表
        // 如果有更好的方法再重写
        DownloadedList.Clear();
        list.ForEach(item => DownloadedList.Add(item));
    }

    public void RefreshDownloadedList()
    {
        // 重新获取下载完成列表
        var downloadStorageService = Container.Resolve<DownloadStorageService>();
        var downloadedItems = downloadStorageService.GetDownloaded();
        DownloadedList.Clear();
        DownloadedList.AddRange(downloadedItems);
    }

    private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        // 关闭下载服务
        _downloadService?.End();
    }

    private void NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        AppLife?.Shutdown();
    }
}