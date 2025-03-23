using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DownKyi.Commands;

public class AsyncDelegateCommand : ICommand
{
    private readonly Func<object, CancellationToken, Task> _execute;
    private readonly Func<object, bool> _canExecute;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isExecuting;

    public event EventHandler CanExecuteChanged;

    public AsyncDelegateCommand(Func<object, CancellationToken, Task> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }

    public async void Execute(object parameter)
    {
        _isExecuting = true;
        RaiseCanExecuteChanged();

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _execute(parameter, _cancellationTokenSource.Token);
        }
        finally
        {
            _isExecuting = false;
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