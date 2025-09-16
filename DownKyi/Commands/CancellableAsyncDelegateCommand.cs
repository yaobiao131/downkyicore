using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DownKyi.Commands;

public class CancellableAsyncDelegateCommand<T> : ICommand
{
    private readonly Func<T, CancellationToken, Task> _execute;
    private readonly Func<T, bool>? _canExecute;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;

    public CancellableAsyncDelegateCommand(
        Func<T, CancellationToken, Task> execute, 
        Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter is not T typedParameter)
        {
            return false;
        }
        return !_isExecuting && (_canExecute?.Invoke(typedParameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (parameter is not T typedParameter)
        {
            return;
        }

        _isExecuting = true;
        RaiseCanExecuteChanged();

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _execute(typedParameter, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            _isExecuting = false;
            _cancellationTokenSource = null;
            RaiseCanExecuteChanged();
        }
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    protected void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
