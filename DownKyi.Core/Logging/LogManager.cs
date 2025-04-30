using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DownKyi.Core.Storage;
using static System.DateTime;
using static System.Threading.Thread;

namespace DownKyi.Core.Logging;

/// <summary>
/// 日志组件
/// </summary>
public class LogManager
{
    private static readonly ConcurrentQueue<Tuple<string, string>> LogQueue = new();

    /// <summary>
    /// 自定义事件
    /// </summary>
    public static event Action<LogInfo>? Event;

    static LogManager()
    {
        var writeTask = new Task(obj =>
        {
            while (true)
            {
                Pause.WaitOne(1000, true);
                var temp = new List<string[]>();
                foreach (var logItem in LogQueue)
                {
                    var logPath = logItem.Item1;
                    var logMergeContent = string.Concat(logItem.Item2, Environment.NewLine,
                        "----------------------------------------------------------------------------------------------------------------------",
                        Environment.NewLine);
                    var logArr = temp.FirstOrDefault(d => d[0].Equals(logPath));
                    if (logArr != null)
                    {
                        logArr[1] = string.Concat(logArr[1], logMergeContent);
                    }
                    else
                    {
                        logArr = new[]
                        {
                            logPath,
                            logMergeContent
                        };
                        temp.Add(logArr);
                    }

                    LogQueue.TryDequeue(out Tuple<string, string> _);
                }

                foreach (var item in temp)
                {
                    WriteText(item[0], item[1]);
                }
            }
        }, null, TaskCreationOptions.LongRunning);
        writeTask.Start();
    }

    private static AutoResetEvent Pause => new(false);

    /// <summary>
    /// 日志存放目录，windows默认日志放在当前应用程序运行目录下的Logs文件夹中,macOS、linux存放于applicationData目录下
    /// </summary>
    private static string LogDirectory => StorageManager.GetLogsDir();

    /// <summary>
    /// 写入Info级别的日志
    /// </summary>
    /// <param name="info"></param>
    public static void Info(string info)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(info).ToUpper()}  {info}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Info,
            Message = info,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入Info级别的日志
    /// </summary>
    /// <param name="source"></param>
    /// <param name="info"></param>
    public static void Info(string source, string info)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(info).ToUpper()}   {source}  {info}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Info,
            Message = info,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入Info级别的日志
    /// </summary>
    /// <param name="source"></param>
    /// <param name="info"></param>
    public static void Info(Type source, string info)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(info).ToUpper()}   {source.FullName}  {info}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Info,
            Message = info,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source.FullName
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入debug级别日志
    /// </summary>
    /// <param name="debug">异常对象</param>
    public static void Debug(string debug)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(debug).ToUpper()}   {debug}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Debug,
            Message = debug,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入debug级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="debug">异常对象</param>
    public static void Debug(string source, string debug)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(debug).ToUpper()}   {source}  {debug}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Debug,
            Message = debug,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入debug级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="debug">异常对象</param>
    public static void Debug(Type source, string debug)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(debug).ToUpper()}   {source.FullName}  {debug}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Debug,
            Message = debug,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source.FullName
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入error级别日志
    /// </summary>
    /// <param name="error">异常对象</param>
    public static void Error(Exception error)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(error).ToUpper()}   {error.Source}  {error.Message}{Environment.NewLine}{error.StackTrace}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Error,
            Message = error.Message,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = error.Source,
            Exception = error,
            ExceptionType = error.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入error级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="error">异常对象</param>
    public static void Error(Type source, Exception error)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(error).ToUpper()}   {source.FullName}  {error.Message}{Environment.NewLine}{error.StackTrace}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Error,
            Message = error.Message,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source.FullName,
            Exception = error,
            ExceptionType = error.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入error级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="error">异常信息</param>
    public static void Error(Type source, string error)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(error).ToUpper()}   {source.FullName}  {error}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Error,
            Message = error,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source.FullName,
            //Exception = error,
            ExceptionType = error.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入error级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="error">异常对象</param>
    public static void Error(string source, Exception error)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(error).ToUpper()}   {source}  {error.Message}{Environment.NewLine}{error.StackTrace}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Error,
            Message = error.Message,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source,
            Exception = error,
            ExceptionType = error.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入error级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="error">异常信息</param>
    public static void Error(string source, string error)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(error).ToUpper()}   {source}  {error}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Error,
            Message = error,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source,
            //Exception = error,
            ExceptionType = error.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入fatal级别日志
    /// </summary>
    /// <param name="fatal">异常对象</param>
    public static void Fatal(Exception fatal)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(fatal).ToUpper()}   {fatal.Source}  {fatal.Message}{Environment.NewLine}{fatal.StackTrace}"));
        LogInfo log = new LogInfo
        {
            LogLevel = LogLevel.Fatal,
            Message = fatal.Message,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = fatal.Source,
            Exception = fatal,
            ExceptionType = fatal.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入fatal级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="fatal">异常对象</param>
    public static void Fatal(Type source, Exception fatal)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(fatal).ToUpper()}   {source.FullName}  {fatal.Message}{Environment.NewLine}{fatal.StackTrace}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Fatal,
            Message = fatal.Message,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source.FullName,
            Exception = fatal,
            ExceptionType = fatal.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入fatal级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="fatal">异常对象</param>
    public static void Fatal(Type source, string fatal)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(fatal).ToUpper()}   {source.FullName}  {fatal}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Fatal,
            Message = fatal,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source.FullName,
            //Exception = fatal,
            ExceptionType = fatal.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入fatal级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="fatal">异常对象</param>
    public static void Fatal(string source, Exception fatal)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(),
            $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(fatal).ToUpper()}   {source}  {fatal.Message}{Environment.NewLine}{fatal.StackTrace}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Fatal,
            Message = fatal.Message,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source,
            Exception = fatal,
            ExceptionType = fatal.GetType().Name
        };
        Event?.Invoke(log);
    }

    /// <summary>
    /// 写入fatal级别日志
    /// </summary>
    /// <param name="source">异常源的类型</param>
    /// <param name="fatal">异常对象</param>
    public static void Fatal(string source, string fatal)
    {
        LogQueue.Enqueue(new Tuple<string, string>(GetLogPath(), $"{Now}   [{Environment.CurrentManagedThreadId}]   {nameof(fatal).ToUpper()}   {source}  {fatal}"));
        var log = new LogInfo
        {
            LogLevel = LogLevel.Fatal,
            Message = fatal,
            Time = Now,
            ThreadId = Environment.CurrentManagedThreadId,
            Source = source,
            ExceptionType = fatal.GetType().Name
        };
        Event?.Invoke(log);
    }

    private static string GetLogPath()
    {
        string newFilePath;
        var logDir = string.IsNullOrEmpty(LogDirectory) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs") : LogDirectory;
        Directory.CreateDirectory(logDir);
        const string extension = ".log";
        var fileNameNotExt = Now.ToString("yyyyMMdd");
        var fileNamePattern = string.Concat(fileNameNotExt, "(*)", extension);
        var filePaths = Directory.GetFiles(logDir, fileNamePattern, SearchOption.TopDirectoryOnly).ToList();

        if (filePaths.Count > 0)
        {
            var fileMaxLen = filePaths.Max(d => d.Length);
            var lastFilePath = filePaths.Where(d => d.Length == fileMaxLen).OrderByDescending(d => d).FirstOrDefault();
            if (lastFilePath != null && new FileInfo(lastFilePath).Length > 1 * 1024 * 1024)
            {
                var no = new Regex(@"(?is)(?<=\()(.*)(?=\))").Match(Path.GetFileName(lastFilePath)).Value;
                var parse = int.TryParse(no, out int tempno);
                var formatno = $"({(parse ? (tempno + 1) : tempno)})";
                var newFileName = String.Concat(fileNameNotExt, formatno, extension);
                newFilePath = Path.Combine(logDir, newFileName);
            }
            else
            {
                newFilePath = lastFilePath;
            }
        }
        else
        {
            var newFileName = string.Concat(fileNameNotExt, $"({0})", extension);
            newFilePath = Path.Combine(logDir, newFileName);
        }

        return newFilePath;
    }

    private static void WriteText(string logPath, string logContent)
    {
        try
        {
            if (!File.Exists(logPath))
            {
                File.CreateText(logPath).Close();
            }

            using var sw = File.AppendText(logPath);
            sw.Write(logContent);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}