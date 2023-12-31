using System;
using System.Threading;
using Avalonia.Controls;

namespace DownKyi.Utils;

public class ClipboardListener
{
    private readonly object _countLocker = new();
    private int _count;

    private Timer? _timer;
    private Action<string>? _action;
    private string? _meta;
    private readonly SemaphoreSlim _tickSemaphore = new(1, 1);
    private CancellationTokenSource? _cts;

    private Window _mainWindow;

    public ClipboardListener(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    private void RegisterSystemEvent(Action<string> action)
    {
        _action = action;
        _timer = new Timer(InvokeTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void UnRegisterSystemEvent(Action<string> action)
    {
        _timer?.Dispose();
        _timer = null;

        _action = null;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private event Action<string>? ChangedImpl;

    public event Action<string>? Changed
    {
        add
        {
            lock (_countLocker)
            {
                if (_count == 0)
                {
                    _count++;
                    RegisterSystemEvent(NotifyAll);
                }
            }

            ChangedImpl += value;
        }
        remove
        {
            ChangedImpl -= value;
            lock (_countLocker)
            {
                _count--;
                if (_count == 0)
                {
                    UnRegisterSystemEvent(NotifyAll);
                }
            }
        }
    }

    private Action<string> NotifyAll => meta => { ChangedImpl?.Invoke(meta); };

    private async void InvokeTick(object? _)
    {
        if (_tickSemaphore.Wait(0) is false)
        {
            return;
        }

        try
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(1000));

            var clipboard = _mainWindow.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            var meta = await clipboard.GetTextAsync();
            if (meta == _meta)
            {
                return;
            }

            if (_meta is not null)
            {
                _meta = meta;
                if (meta != null) _action?.Invoke(meta);
            }
            else
            {
                _meta = meta;
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _tickSemaphore.Release();
        }
    }

    ~ClipboardListener()
    {
        Dispose();
    }

    public void Dispose()
    {
        ChangedImpl = null;
        UnRegisterSystemEvent(NotifyAll);
        _count = 0;
    }
}