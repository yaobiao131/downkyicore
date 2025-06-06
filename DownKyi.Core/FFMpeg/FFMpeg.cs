using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Helpers;

namespace DownKyi.Core.FFMpeg;

public class FFMpeg
{
    private const string Tag = "FFmpegHelper";
    private static readonly FFMpeg instance = new();

    static FFMpeg()
    {
    }

    private FFMpeg()
    {
        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg") });
        FFMpegHelper.VerifyFFMpegExists(GlobalFFOptions.Current);
    }

    public static FFMpeg Instance => instance;

    /// <summary>
    /// 合并音频和视频
    /// </summary>
    /// <param name="audio">音频</param>
    /// <param name="video">视频</param>
    /// <param name="destVideo"></param>
    public bool MergeVideo(string? audio, string? video, string destVideo)
    {
        if (!File.Exists(audio) && !File.Exists(video)) return false;

        var arguments = FFMpegArguments
            .FromFileInput(audio)
            .AddFileInput(video)
            .OutputToFile(destVideo, true, options => options
                .WithCustomArgument("-strict -2")
                .WithAudioCodec("copy")
                .WithVideoCodec("copy")
                .ForceFormat("mp4")
            );
        if (audio == null || !File.Exists(audio))
        {
            arguments = FFMpegArguments.FromFileInput(video).OutputToFile(
                destVideo,
                true,
                options => options.WithCustomArgument("-strict -2").WithVideoCodec("copy").WithAudioCodec("copy").ForceFormat("mp4")
            );
        }

        if (video == null || !File.Exists(video))
        {
            if (SettingsManager.GetInstance().GetIsTranscodingAacToMp3() == AllowStatus.Yes)
            {
                arguments = FFMpegArguments.FromFileInput(audio).OutputToFile(
                    destVideo,
                    true,
                    options => options.WithCustomArgument("-strict -2").DisableChannel(Channel.Video).ForceFormat("mp3")
                );
            }
            else
            {
                arguments = FFMpegArguments.FromFileInput(audio).OutputToFile(
                    destVideo,
                    true,
                    options => options.WithCustomArgument("-strict -2").DisableChannel(Channel.Video).WithAudioCodec("copy")
                );
            }
        }

        LogManager.Debug(Tag, arguments.Arguments);

        arguments
            .NotifyOnError(s => LogManager.Debug(Tag, s))
            .ProcessSynchronously(false);
        try
        {
            if (audio != null)
            {
                File.Delete(audio);
            }

            if (video != null)
            {
                File.Delete(video);
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("MergeVideo()发生IO异常: {0}", e);
            LogManager.Error(Tag, e);
        }

        return true;
    }

    /// <summary>
    /// 去水印，非常消耗cpu资源
    /// </summary>
    /// <param name="video"></param>
    /// <param name="destVideo"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="action"></param>
    public void Delogo(string video, string destVideo, int x, int y, int width, int height, Action<string> action)
    {
        FFMpegArguments
            .FromFileInput(video)
            .OutputToFile(
                destVideo,
                true,
                option => option
                    .WithCustomArgument($"-vf delogo=x={x}:y={y}:w={width}:h={height}:show=0 -hide_banner"))
            .NotifyOnOutput(action.Invoke)
            .NotifyOnError(action.Invoke)
            .ProcessSynchronously(false);
    }

    /// <summary>
    /// 从一个视频中仅提取音频
    /// </summary>
    /// <param name="video">源视频</param>
    /// <param name="audio">目标音频</param>
    /// <param name="action">输出信息</param>
    public void ExtractAudio(string video, string audio, Action<string> action)
    {
        FFMpegArguments
            .FromFileInput(video)
            .OutputToFile(audio,
                true,
                options => options
                    .WithCustomArgument("-hide_banner")
                    .WithAudioCodec("copy")
                    .DisableChannel(Channel.Video)
            )
            .NotifyOnOutput(action.Invoke)
            .NotifyOnError(action.Invoke)
            .ProcessSynchronously(false);
    }

    /// <summary>
    /// 从一个视频中仅提取视频
    /// </summary>
    /// <param name="video">源视频</param>
    /// <param name="destVideo">目标视频</param>
    /// <param name="action">输出信息</param>
    public void ExtractVideo(string video, string destVideo, Action<string> action)
    {
        FFMpegArguments.FromFileInput(video)
            .OutputToFile(
                destVideo,
                true,
                options => options
                    .WithCustomArgument("-hide_banner")
                    .WithVideoCodec("copy")
                    .DisableChannel(Channel.Audio))
            .NotifyOnOutput(action.Invoke)
            .NotifyOnError(action.Invoke)
            .ProcessSynchronously(false);
    }


    /// <summary>
    /// 合并多个FLV视频片段为一个完整视频
    /// </summary>
    /// <param name="inputFlvs">FLV片段路径列表(按顺序)</param>
    /// <param name="outputVideo">输出视频路径</param>
    /// <param name="action">进度回调</param>
    /// <returns>是否成功</returns>
    public bool ConcatVideos(List<string> inputFlvs, string outputVideo, Action<string> action)
    {
        try
        {
            if (inputFlvs == null || inputFlvs.Count == 0)
            {
                return false;
            }

            // 验证所有输入文件都存在
            foreach (var video in inputFlvs)
            {
                if (!File.Exists(video))
                {
                    action?.Invoke($"文件不存在: {video}");
                    return false;
                }
            }

            LogManager.Debug(Tag, $"开始合并 {inputFlvs.Count} 个视频到 {outputVideo}");


            var listFile = Path.Combine(Path.GetTempPath(), $"flvlist_{DateTime.Now:yyyyMMddHHmmss}.txt");
            File.WriteAllLines(listFile, inputFlvs.Select(f => $"file '{f.Replace("'", "'\\''")}'"));

            FFMpegArguments
             .FromFileInput(listFile, false, options => options
                 .WithCustomArgument("-f concat -safe 0"))
             .OutputToFile(outputVideo, true, options => options
                 .WithVideoCodec("libx264")  
                 .WithAudioCodec("aac")   
                 .WithCustomArgument("-movflags +faststart")
                 .WithCustomArgument("-avoid_negative_ts make_zero")
             )
             .NotifyOnOutput(action.Invoke)
             .NotifyOnError(action.Invoke)
             .ProcessSynchronously(false);

            try { File.Delete(listFile); } catch {  }
            LogManager.Debug(Tag, "视频合并完成");
            return true;
        }
        catch (Exception ex)
        {
            LogManager.Error(Tag, ex);
            return false;
        }
    }
}