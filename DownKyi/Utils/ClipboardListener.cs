using System;
using System.Threading;
using Avalonia.Controls;

namespace DownKyi.Utils;

public sealed class ClipboardListener : IDisposable
{
    private readonly object _countLocker = new();
    private readonly object _disposeLock = new();
    private int _count;
    private bool _disposed;

    private Timer? _timer;
    private Action<string>? _action;
    private string? _lastClipboardContent;
    private readonly SemaphoreSlim _tickSemaphore = new(1, 1);
    private CancellationTokenSource? _cts;

    private readonly Window _mainWindow;

    public ClipboardListener(Window mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    private void RegisterSystemEvent(Action<string> action)
    {
        ThrowIfDisposed();

        _action = action;
        _timer = new Timer(InvokeTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void UnRegisterSystemEvent()
    {
        _timer?.Dispose();
        _timer = null;
        _action = null;

        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource was already disposed
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    private event Action<string>? ChangedImpl;

    public event Action<string>? Changed
    {
        add
        {
            ThrowIfDisposed();

            lock (_countLocker)
            {
                if (_count == 0)
                {
                    RegisterSystemEvent(NotifyAll);
                }

                _count++;
                ChangedImpl += value;
            }
        }
        remove
        {
            lock (_countLocker)
            {
                ChangedImpl -= value;
                _count--;
                if (_count == 0)
                {
                    UnRegisterSystemEvent();
                }
            }
        }
    }

    private void NotifyAll(string content) => ChangedImpl?.Invoke(content);

    private async void InvokeTick(object? _)
    {
        if (_disposed || !await _tickSemaphore.WaitAsync(0))
        {
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            _cts = cts;

            if (_disposed || _mainWindow.Clipboard == null)
            {
                return;
            }

            var currentContent = await _mainWindow.Clipboard.GetTextAsync();

            // 检查内容是否发生变化
            if (string.Equals(currentContent, _lastClipboardContent, StringComparison.Ordinal))
            {
                return;
            }

            // 只有在不是第一次获取内容时才触发事件
            if (_lastClipboardContent != null && !string.IsNullOrEmpty(currentContent))
            {
                _action?.Invoke(currentContent);
            }

            _lastClipboardContent = currentContent;
        }
        catch (OperationCanceledException)
        {
            // 正常的取消操作，忽略
        }
        catch (Exception)
        {
            // 记录异常但不抛出，避免定时器停止工作
        }
        finally
        {
            _tickSemaphore.Release();
            _cts = null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ClipboardListener));
        }
    }

    public void Dispose()
    {
        lock (_disposeLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        // 清理事件订阅
        ChangedImpl = null;

        // 停止系统事件监听
        UnRegisterSystemEvent();

        // 释放信号量
        _tickSemaphore?.Dispose();

        // 重置计数器
        lock (_countLocker)
        {
            _count = 0;
        }

        GC.SuppressFinalize(this);
    }
}