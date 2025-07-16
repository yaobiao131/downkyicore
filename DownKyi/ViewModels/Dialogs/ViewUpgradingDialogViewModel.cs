using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Nrbf;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Threading;
using DownKyi.Core.BiliApi.BiliUtils;
using DownKyi.Core.BiliApi.Login;
using DownKyi.Core.Storage;
using DownKyi.Core.Storage.Database;
using DownKyi.Models;
using FreeSql;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace DownKyi.ViewModels.Dialogs;

public class ViewUpgradingDialogViewModel : BaseDialogViewModel
{
    public const string Tag = "DialogLoading";
    private readonly IBaseRepository<Downloaded> _downloadedRepository;

    #region 页面属性申明

    private double _percent;

    public double Percent
    {
        get => _percent;
        set => SetProperty(ref _percent, value);
    }

    private string? _message;

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _restartedVisible;

    public bool RestartVisible
    {
        get => _restartedVisible;
        set => SetProperty(ref _restartedVisible, value);
    }

    #endregion

    #region 命令申明

    private DelegateCommand? _restartCommand;

    public DelegateCommand RestartCommand => _restartCommand ??= new DelegateCommand(ExecuteRestart);

    private void ExecuteRestart()
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (executablePath != null)
        {
            Process.Start(executablePath);
            App.Current.AppLife?.Shutdown();
        }
    }

    #endregion

    public ViewUpgradingDialogViewModel(IBaseRepository<Downloaded> downloadedRepository)
    {
        _downloadedRepository = downloadedRepository;
        Message = "数据迁移中、请不要关闭软件";
    }

    private void SetMessage(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() => Message = message);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        Upgrade();
    }

    private void Upgrade()
    {
        Task.Run(() => { Upgrade1_0_20To1_0_21(); });
    }

#pragma warning disable SYSLIB5005
    private void Upgrade1_0_20To1_0_21()
    {
        var noMigrate = false;
        var loginInfoPath = StorageManager.GetLogin();
        if (File.Exists(loginInfoPath))
        {
            using Stream stream = File.Open(loginInfoPath, FileMode.Open);
            if (NrbfDecoder.StartsWithPayloadHeader(stream))
            {
                SetMessage("正在迁移登录信息");
                var cookies = new List<DownKyiCookie>();
                var cookieRecord = NrbfDecoder.DecodeClassRecord(stream);
                if (cookieRecord.TypeNameMatches(typeof(CookieContainer)))
                {
                    var domainTable = cookieRecord.GetClassRecord("m_domainTable");
                    var values = domainTable?.GetArrayRecord("Values");
                    var valuesArray = values?.GetArray(typeof(object[])).Cast<ClassRecord>();

                    foreach (var value in valuesArray ?? Array.Empty<ClassRecord>())
                    {
                        var valueObjects = value.GetClassRecord("m_list")?
                            .GetClassRecord("_list")?
                            .GetArrayRecord("values")?
                            .GetArray(typeof(object[]));
                        foreach (var valueObject in valueObjects?.Cast<ClassRecord>() ?? Array.Empty<ClassRecord>())
                        {
                            if (valueObject == null) continue;
                            foreach (var c in valueObject.GetClassRecord("m_list")?.GetArrayRecord("_items")
                                         ?.GetArray(typeof(object[])).Cast<ClassRecord>() ?? Array.Empty<ClassRecord>())
                            {
                                if (c == null) continue;
                                cookies.Add(new DownKyiCookie(c.GetString("m_name") ?? "", c.GetString("m_value") ?? "", c.GetString("m_domain")));
                            }
                        }
                    }
                }

                LoginHelper.SaveLoginInfoCookies(cookies);
                SetMessage("登录信息迁移完成");
            }
        }
        else
        {
            noMigrate = true;
        }

#if DEBUG
        var oldDbPath = StorageManager.GetDownload().Replace(".db", "_debug.db");
#else
        var oldDbPath = StorageManager.GetDownload();
#endif
        if (File.Exists(oldDbPath))
        {
            SetMessage("正在迁移下载信息");
#if DEBUG
            var dbHelper = new SqliteDatabase(oldDbPath);
#else
            var dbHelper = new SqliteDatabase(oldDbPath, "bdb8eb69-3698-4af9-b722-9312d0fba623");
#endif
            var validRecords = new Dictionary<string, DownloadedWithData>();
            var totalCount = 0;
            dbHelper.ExecuteQuery(@"SELECT d.id, d.data as downloaded_data, db.data as download_base_data
                                  FROM downloaded d
                                  JOIN download_base db ON d.id = db.id", reader =>
            {
                while (reader.Read())
                {
                    try
                    {
                        // 读取字节数组
                        var array = (byte[])reader["downloaded_data"];
                        // 定义一个流
                        using var stream = new MemoryStream(array);
                        // 反序列化
                        var record = NrbfDecoder.DecodeClassRecord(stream);
                        if (!record.TypeNameMatches(typeof(Downloaded))) continue;

                        var downloadedObj = new Downloaded
                        {
                            Id = reader["id"].ToString() ?? Guid.NewGuid().ToString("N"),
                            MaxSpeedDisplay = record.GetString($"<{nameof(Downloaded.MaxSpeedDisplay)}>k__BackingField"),
                            FinishedTime = record.GetString($"<{nameof(Downloaded.FinishedTime)}>k__BackingField") ?? "",
                            FinishedTimestamp = record.GetInt64($"<{nameof(Downloaded.FinishedTimestamp)}>k__BackingField")
                        };


                        validRecords.Add(
                            (string)reader["id"],
                            new DownloadedWithData
                            {
                                Downloaded = downloadedObj,
                                DownloadBaseData = (byte[])reader["download_base_data"]
                            });

                        totalCount++;
                    }
                    catch (Exception e)
                    {
                        SetMessage(e.Message);
                    }
                }
            });

            var readNeedDownloadContent = new Func<ClassRecord, Dictionary<string, bool>>(record =>
            {
                var keyArrayRecord = record.GetArrayRecord("KeyValuePairs");
                var keys = keyArrayRecord?.GetArray(typeof(KeyValuePair<string, bool>[]));
                var needDownloadContent = new Dictionary<string, bool>();
                foreach (var keyValuePairRecord in keys?.Cast<ClassRecord>() ?? Array.Empty<ClassRecord>())
                {
                    var key = keyValuePairRecord.GetString("key") ?? "";
                    var value = keyValuePairRecord.GetBoolean("value");
                    needDownloadContent.Add(key, value);
                }

                return needDownloadContent;
            });
            var readQuality = new Func<ClassRecord?, Quality>(record =>
            {
                if (record == null) return new Quality();
                var quality = new Quality
                {
                    Id = record.GetInt32($"<{nameof(Quality.Id)}>k__BackingField"),
                    Name = record.GetString($"<{nameof(Quality.Name)}>k__BackingField") ?? ""
                };

                return quality;
            });

            try
            {
                const int batchSize = 200;
                var downloadedList = new List<Downloaded>();
                var processedCount = 0;

                foreach (var item in validRecords)
                {
                    try
                    {
                        using var stream = new MemoryStream(item.Value.DownloadBaseData);
                        var record = NrbfDecoder.DecodeClassRecord(stream);
                        if (record.TypeNameMatches(typeof(DownloadBase)))
                        {
                            var needDownloadContentRecord = record.GetClassRecord($"<{nameof(DownloadBase.NeedDownloadContent)}>k__BackingField");
                            var needDownloadContent = needDownloadContentRecord != null ? readNeedDownloadContent(needDownloadContentRecord) : new Dictionary<string, bool>();

                            var download = new Downloaded
                            {
                                Id = item.Value.Downloaded.Id,
                                MaxSpeedDisplay = item.Value.Downloaded.MaxSpeedDisplay,
                                FinishedTime = item.Value.Downloaded.FinishedTime,
                                FinishedTimestamp = item.Value.Downloaded.FinishedTimestamp,

                                DownloadBase = new DownloadBase
                                {
                                    NeedDownloadContent = needDownloadContent,
                                    Bvid = record.GetString($"<{nameof(DownloadBase.Bvid)}>k__BackingField") ?? "",
                                    Avid = record.GetInt64($"<{nameof(DownloadBase.Avid)}>k__BackingField"),
                                    Cid = record.GetInt64($"<{nameof(DownloadBase.Cid)}>k__BackingField"),
                                    EpisodeId = record.GetInt64($"<{nameof(DownloadBase.EpisodeId)}>k__BackingField"),
                                    CoverUrl = record.GetString($"<{nameof(DownloadBase.CoverUrl)}>k__BackingField") ?? "",
                                    PageCoverUrl = record.GetString($"<{nameof(DownloadBase.PageCoverUrl)}>k__BackingField") ?? "",
                                    ZoneId = record.GetInt32($"<{nameof(DownloadBase.ZoneId)}>k__BackingField"),
                                    Order = record.GetInt32($"<{nameof(DownloadBase.Order)}>k__BackingField"),
                                    MainTitle = record.GetString($"<{nameof(DownloadBase.MainTitle)}>k__BackingField") ?? "",
                                    Name = record.GetString($"<{nameof(DownloadBase.Name)}>k__BackingField") ?? "",
                                    Duration = record.GetString($"<{nameof(DownloadBase.Duration)}>k__BackingField") ?? "",
                                    VideoCodecName = record.GetString($"<{nameof(DownloadBase.VideoCodecName)}>k__BackingField") ?? "",
                                    Resolution = readQuality(record.GetClassRecord($"<{nameof(DownloadBase.Resolution)}>k__BackingField")),
                                    AudioCodec = readQuality(record.GetClassRecord($"<{nameof(DownloadBase.AudioCodec)}>k__BackingField")),
                                    FilePath = record.GetString($"<{nameof(DownloadBase.FilePath)}>k__BackingField") ?? "",
                                    FileSize = record.GetString($"<{nameof(DownloadBase.FileSize)}>k__BackingField") ?? "",
                                    Page = record.GetInt32($"<{nameof(DownloadBase.Page)}>k__BackingField")
                                }
                            };
                            downloadedList.Add(download);
                            processedCount++;
                            if (processedCount % batchSize != 0 && processedCount != totalCount) continue;
                            _downloadedRepository.Insert(downloadedList);
                            downloadedList.Clear();

                            // 更新进度
                            var percent = processedCount / (double)totalCount * 100;
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                Percent = percent;
                                SetMessage($"正在迁移下载信息({processedCount}/{totalCount})");
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                dbHelper.Dispose();
                File.Delete(oldDbPath);

                Dispatcher.UIThread.Invoke(() =>
                {
                    RestartVisible = true;
                    SetMessage("下载信息迁移完成");
                    App.Current.RefreshDownloadedList();
                });
            }
            catch (Exception e)
            {
                SetMessage($"迁移失败: {e.Message}");
            }
        }
        else
        {
            noMigrate = true;
        }

        if (noMigrate)
        {
            Dispatcher.UIThread.Invoke(() => RaiseRequestClose(new DialogResult()));
        }
    }
#pragma warning restore SYSLIB5005

    private class DownloadedWithData
    {
        public Downloaded Downloaded { get; set; } = new();
        public byte[] DownloadBaseData { get; set; } = Array.Empty<byte>();
    }
}