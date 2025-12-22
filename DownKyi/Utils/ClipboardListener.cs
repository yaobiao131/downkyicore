using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace DownKyi.Utils;

public sealed class ClipboardListener : IDisposable
{
    private readonly object _countLocker = new();
    private int _count;
    private bool _disposed;

    private DispatcherTimer? _timer;
    private Action<string>? _action;
    private string? _lastClipboardContent;

    private readonly Window _mainWindow;
    
    private bool _isTicking;
    public ClipboardListener(Window mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    private void RegisterSystemEvent(Action<string> action)
    {
        ThrowIfDisposed();
        _action = action;
        _timer = new DispatcherTimer(
            TimeSpan.FromSeconds(1), 
            DispatcherPriority.Background, 
            (_, _) => { _ = TickHandler(); });
        _timer.Start();
    }

    private void UnRegisterSystemEvent()
    {
        _timer?.Stop();
        _timer = null;
        _action = null;
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
                _count = Math.Max(0, _count - 1); 
                if (_count == 0)
                {
                    UnRegisterSystemEvent();
                }
            }
        }
    }

    private void NotifyAll(string content) => ChangedImpl?.Invoke(content);

    private async Task TickHandler()
    {
        if (_disposed || _isTicking || _mainWindow.Clipboard == null) return;

        try
        {
            _isTicking = true;
            var currentContent = await _mainWindow.Clipboard.TryGetTextAsync();

            if (string.Equals(currentContent, _lastClipboardContent, StringComparison.Ordinal))
            {
                return;
            }

            if (_lastClipboardContent != null && !string.IsNullOrEmpty(currentContent))
            {
                _action?.Invoke(currentContent);
            }

            _lastClipboardContent = currentContent;
        }
        finally
        {
            _isTicking = false;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ClipboardListener));
    }

    public void Dispose()
    {
        lock (_countLocker) 
        {
            if (_disposed) return;
            _disposed = true;
            
            UnRegisterSystemEvent();
            ChangedImpl = null;
        }
        GC.SuppressFinalize(this);
    }
}